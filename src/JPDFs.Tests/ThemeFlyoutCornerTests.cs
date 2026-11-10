using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace JPDFs.Tests;

public sealed class ThemeFlyoutCornerTests
{
    [Fact]
    public void AcrobatThemesRoundEveryFlyoutCorner()
    {
        string root = FindRepositoryRoot();
        string themeDirectory = Path.Combine(root, "Themes");
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";

        string[] themes = Directory.GetFiles(themeDirectory, "*.xaml");
        Assert.Equal(2, themes.Length);
        foreach (string themePath in themes)
        {
            var document = XDocument.Load(themePath);
            var radius = document.Descendants()
                .Single(element => (string?)element.Attribute(x + "Key") == "FlyoutCornerRadius")
                .Value.Trim();
            Assert.Equal("6", radius);
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "JPDFs.csproj")))
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate the JPDFs repository root.");
    }
}
