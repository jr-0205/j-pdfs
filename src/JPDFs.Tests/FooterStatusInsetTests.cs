using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace JPDFs.Tests;

/// <summary>
/// JPDFs has no status light in the footer corner. 1.8.91 inset "Ready" by its 16px footer padding;
/// the kit's FooterPadding is 0, so the text touched the window edge. FooterStatusInset is
/// JPDFs's own key, completed in AppTheme for both Acrobat palettes.
/// </summary>
public sealed class FooterStatusInsetTests
{
    [Fact]
    public void RoundedThemesRestoreTheShippedSixteenPixelInset()
    {
        string source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "Services", "AppTheme.cs"));
        Assert.Matches(new Regex(@"Default\(""FooterStatusInset"",\s*new Thickness\(16,\s*0,\s*0,\s*0\)\)"), source);
    }

    [Fact]
    public void TheKitPaletteDoesNotDefineTheInset()
    {
        // A kit key would outrank JPDFs's value; the inset must stay an app key.
        string kit = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "Services", "ThemeManager.cs"));
        Assert.DoesNotContain("FooterStatusInset", kit);
    }

    [Fact]
    public void TheFooterStatusTextTakesTheInset()
    {
        string window = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "Shell", "MainWindow.xaml"));
        Match status = Regex.Match(window, @"<TextBlock x:Name=""StatusText""[^>]*>", RegexOptions.Singleline);
        Assert.True(status.Success, "MainWindow.xaml has no StatusText.");
        Assert.Contains(@"Margin=""{DynamicResource FooterStatusInset}""", status.Value);
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
