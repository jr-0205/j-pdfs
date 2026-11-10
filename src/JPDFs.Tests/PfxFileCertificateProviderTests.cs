using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using JPDFs.Engine.Authoring;
using JPDFs.Engine.Documents;
using JPDFs.Engine.Signing;
using JPDFs.Services.Signing;
using Xunit;

namespace JPDFs.Tests;

public sealed class PfxFileCertificateProviderTests
{
    [Theory]
    [InlineData(".p12", "test-password")]
    [InlineData(".pfx", "")]
    public void ImportedPrivateKeySignsWithoutPersistingInTheKeyStore(string extension, string password)
    {
        string directory = Path.Combine(Path.GetTempPath(), $"jpdfs-pfx-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            using RSA originalKey = RSA.Create(2048);
            using X509Certificate2 original = new CertificateRequest("CN=JPDFs PFX Test", originalKey,
                HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
                .CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1));
            string pfx = Path.Combine(directory, "signer" + extension);
            File.WriteAllBytes(pfx, original.Export(X509ContentType.Pkcs12, password));
            using X509Certificate2 imported = new PfxFileCertificateProvider(pfx, password).GetCertificate();
            using RSA? importedKey = imported.GetRSAPrivateKey();
            try
            {
                string input = Path.Combine(directory, "input.pdf");
                string output = Path.Combine(directory, "signed.pdf");
                File.WriteAllBytes(input, new PdfDocumentBuilder().AddBlankPage().Build());
                new PdfSigner().Sign(input, output, imported, new PdfSigner.SignInfo("Approved", "", ""));
                byte[] bytes = File.ReadAllBytes(output);
                PdfSignatureInfo signature = Assert.Single(PdfSignatureReader.Read(PdfDocument.Open(bytes)));
                byte[] signedBytes = signature.ByteRange!.Chunk(2).SelectMany(range =>
                    bytes.AsSpan(checked((int)range[0]), checked((int)range[1])).ToArray()).ToArray();
                var cms = new SignedCms(new ContentInfo(signedBytes), detached: true);
                cms.Decode(signature.Cms.ToArray());
                cms.CheckSignature(verifySignatureOnly: true);

                Assert.True(Assert.IsType<RSACng>(importedKey).Key.IsEphemeral,
                    "Importing a PFX for one signing operation persisted its private key.");
            }
            finally
            {
                // A failing regression must remove only the key imported from this generated fixture.
                if (importedKey is RSACng cng && !cng.Key.IsEphemeral) cng.Key.Delete();
                if (importedKey is RSACryptoServiceProvider csp) csp.PersistKeyInCsp = false;
            }
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
