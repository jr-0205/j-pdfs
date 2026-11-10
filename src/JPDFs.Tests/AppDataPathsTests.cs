using System.IO;
using System.Text.Json;
using JPDFs.Services;
using Xunit;

namespace JPDFs.Tests;

[Collection("Environment variables")]
public sealed class AppDataPathsTests
{
    [Fact]
    public void StartupSmokeDataDoesNotUseInheritedPortableSettings()
    {
        string root = Path.Combine(Path.GetTempPath(), $"jpdfs-smoke-isolation-{Guid.NewGuid():N}");
        string launcher = Path.Combine(root, "JPDFs-Portable.exe");
        string isolated = Path.Combine(root, "isolated");
        string? previousLauncher = Environment.GetEnvironmentVariable("JPDFS_LAUNCHER_PATH");
        string? previousSmokeRoot = AppDataPaths.StartupSmokeRoot;
        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllBytes(launcher, []);
            Environment.SetEnvironmentVariable("JPDFS_LAUNCHER_PATH", launcher);
            AppDataPaths.StartupSmokeRoot = isolated;

            Assert.Null(AppDataPaths.GetPortableSetting("LastFile"));
            Assert.Equal(isolated, AppDataPaths.LocalRoot);
            Assert.Equal(isolated, AppDataPaths.UserRoot);
            Assert.Equal(Path.Combine(isolated, "signatures.json"), AppDataPaths.SignaturesFile);
            AppDataPaths.SetPortableSetting("Locale", "en-US");
            Assert.True(File.Exists(Path.Combine(isolated, "settings.json")));
            Assert.False(Directory.Exists(Path.Combine(root, "JPDFs-Data")));
        }
        finally
        {
            AppDataPaths.StartupSmokeRoot = previousSmokeRoot;
            Environment.SetEnvironmentVariable("JPDFS_LAUNCHER_PATH", previousLauncher);
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void PortableSettingsAreNotReplacedWhenTheExistingFileCannotBeRead()
    {
        string root = Path.Combine(Path.GetTempPath(), $"jpdfs-portable-damaged-{Guid.NewGuid():N}");
        string launcher = Path.Combine(root, "JPDFs-Portable.exe");
        string? previous = Environment.GetEnvironmentVariable("JPDFS_LAUNCHER_PATH");
        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllBytes(launcher, []);
            Environment.SetEnvironmentVariable("JPDFS_LAUNCHER_PATH", launcher);
            string dataRoot = Path.Combine(root, "JPDFs-Data");
            Directory.CreateDirectory(dataRoot);
            string settingsFile = Path.Combine(dataRoot, "settings.json");
            // A half-written or locked file reads as garbage; the other settings in it must survive.
            File.WriteAllText(settingsFile, "{\"Locale\":\"ja-JP\",\"Theme\":\"Dark\"");
            string damaged = File.ReadAllText(settingsFile);

            AppDataPaths.SetPortableSetting("Accent", "Red");

            Assert.Equal(damaged, File.ReadAllText(settingsFile));
            Assert.Empty(Directory.GetFiles(dataRoot, "*.tmp"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("JPDFS_LAUNCHER_PATH", previous);
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void PortableSettingsAreStoredBesideTheLauncher()
    {
        string root = Path.Combine(Path.GetTempPath(), $"jpdfs-portable-data-{Guid.NewGuid():N}");
        string launcher = Path.Combine(root, "JPDFs-Portable.exe");
        string? previous = Environment.GetEnvironmentVariable("JPDFS_LAUNCHER_PATH");
        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllBytes(launcher, []);
            Environment.SetEnvironmentVariable("JPDFS_LAUNCHER_PATH", launcher);

            AppDataPaths.SetPortableSetting("Locale", "ja-JP");

            string dataRoot = Path.Combine(root, "JPDFs-Data");
            Assert.Equal(dataRoot, AppDataPaths.PortableRoot);
            Assert.Equal("ja-JP", AppDataPaths.GetPortableSetting("Locale"));
            Dictionary<string, string>? settings = JsonSerializer.Deserialize<Dictionary<string, string>>(
                File.ReadAllText(Path.Combine(dataRoot, "settings.json")));
            Assert.Equal("ja-JP", settings?["Locale"]);
        }
        finally
        {
            Environment.SetEnvironmentVariable("JPDFS_LAUNCHER_PATH", previous);
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}

[CollectionDefinition("Environment variables", DisableParallelization = true)]
public sealed class EnvironmentVariableCollection;
