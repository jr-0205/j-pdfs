using JPDFs.Engine.Authoring;
using JPDFs.Engine.Documents;
using JPDFs.Engine.Objects;
using JPDFs.Engine.Rendering;
using JPDFs.Engine.Writing;
using Xunit;

namespace JPDFs.Engine.Tests.Writing;

public sealed class PdfSaveSanitizerIntersectionTests
{
    [Theory]
    [InlineData(50, 150)]
    [InlineData(99.75, 100.25)]
    public void RepairPreservesPartlyOverlappingCropBox(double lower, double upper)
    {
        PdfDocument original = PdfDocument.Open(new PdfDocumentBuilder().AddBlankPage(100, 100).Build());
        PdfPageTreeEntry page = PdfPageTree.Read(original).Pages[0];
        var cropName = new PdfName("CropBox"u8);
        var crop = new PdfArray([new PdfReal(lower), new PdfReal(lower),
            new PdfReal(upper), new PdfReal(upper)]);
        byte[] source = new PdfIncrementalUpdateBuilder(original)
            .ReplaceObject(page.Reference.ObjectNumber, new PdfDictionary(page.Dictionary.Append(
                new KeyValuePair<PdfName, PdfObject>(cropName, crop))))
            .Build();

        byte[] repaired = PdfSaveSanitizer.RepairHarmlessArtifacts(PdfDocument.Open(source));

        Assert.Equal(source, repaired);
        var options = new PdfRenderOptions(50, 50);
        Assert.Equal(
            new PdfPageRenderer(PdfDocument.Open(source)).Render(0, options).Pixels.ToArray(),
            new PdfPageRenderer(PdfDocument.Open(repaired)).Render(0, options).Pixels.ToArray());
        PdfPageTreeEntry reopened = PdfPageTree.Read(PdfDocument.Open(repaired)).Pages[0];
        Assert.Equal(4, Assert.IsType<PdfArray>(reopened.Dictionary[cropName]).Count);
    }
}
