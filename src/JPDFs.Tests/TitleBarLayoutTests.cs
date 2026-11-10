using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace JPDFs.Tests;

public sealed class TitleBarLayoutTests
{
    [Fact]
    public void PlainAppTitleUsesThemeAlignmentOffset()
    {
        string root = FindRepositoryRoot();
        var document = XDocument.Load(Path.Combine(root, "Shell", "MainWindow.xaml"));

        XElement title = document.Descendants()
            .Single(element => element.Name.LocalName == "TextBlock" &&
                (string?)element.Attribute("Text") == "J-PDF's" &&
                (string?)element.Attribute("Visibility") == "{DynamicResource PlainTitleVisibility}");
        Assert.Equal("{DynamicResource TitleTextMargin}", (string?)title.Attribute("Margin"));

        // The kit supplies the neutral offset for both Acrobat palettes.
        string engine = File.ReadAllText(Path.Combine(root, "Services", "ThemeManager.cs"));
        Assert.Contains("SetIfAbsent(\"TitleTextMargin\", new Thickness(0));", engine);
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
