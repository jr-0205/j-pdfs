using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace JPDFs.Tests;

/// <summary>
/// Values the family theme contract fixes. Left to the theme engine's derivation,
/// ContentPaneBrush becomes PaneBrush and FilenameBrush becomes MutedTextBrush; both Acrobat
/// palettes state the two source values those derivations rely on.
/// </summary>
public sealed class ThemeContractValueTests
{
    private static readonly XNamespace Xaml = "http://schemas.microsoft.com/winfx/2006/xaml";

    [Theory]
    [InlineData("AcrobatLight.xaml")]
    [InlineData("AcrobatDark.xaml")]
    public void AcrobatPalettesStateTheSourcesForTheCompletedContract(string themeFile)
    {
        // ContentPaneBrush and FilenameBrush are completed by the engine; the palette must state
        // the two colours those mirrors read.
        Assert.NotEqual(BrushColor(themeFile, "PaneBrush"), BrushColor(themeFile, "MutedTextBrush"));
        Assert.NotEqual(BrushColor(themeFile, "TextBrush"), BrushColor(themeFile, "MutedTextBrush"));
    }

    [Fact]
    public void ThemeEngineMirrorsTheSourceValues()
    {
        string manager = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "Services", "ThemeManager.cs"));
        Assert.Contains("Mirror(d, \"ContentPaneBrush\", \"PaneBrush\");", manager);
        Assert.Contains("Mirror(d, \"FilenameBrush\", \"MutedTextBrush\");", manager);
    }

    private static string BrushColor(string themeFile, string key)
    {
        var document = XDocument.Load(Path.Combine(FindRepositoryRoot(), "Themes", themeFile));
        var brush = document.Descendants()
            .SingleOrDefault(element => (string?)element.Attribute(Xaml + "Key") == key);
        Assert.True(brush is not null, $"{themeFile} does not declare {key}.");
        return brush!.Attribute("Color")!.Value;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "JPDFs.csproj"))) return directory.FullName;
            directory = directory.Parent;
        }
        throw new InvalidOperationException("JPDFs.csproj was not found above the test output.");
    }
}
