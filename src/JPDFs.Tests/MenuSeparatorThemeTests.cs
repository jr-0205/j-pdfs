using System;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Linq;
using Xunit;

namespace JPDFs.Tests;

public sealed class MenuSeparatorThemeTests
{
    private static readonly XNamespace Presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XNamespace Xaml = "http://schemas.microsoft.com/winfx/2006/xaml";

    [Fact]
    public void ThemesDeclareMenuDividersOrKeepTheirCardBorderFallback()
    {
        string root = FindRepositoryRoot();
        // The kit owns the divider's fallback; JPDFs no longer states its own.
        string manager = File.ReadAllText(Path.Combine(root, "Services", "ThemeManager.cs"));
        Assert.Contains("Mirror(\"MenuSeparatorBrush\", \"MenuBorderBrush\");", manager);
        Assert.DoesNotContain("MenuSeparatorBrush", File.ReadAllText(Path.Combine(root, "Services", "AppTheme.cs")));

        foreach (string themePath in Directory.EnumerateFiles(Path.Combine(root, "Themes"), "*.xaml"))
        {
            string theme = Path.GetFileNameWithoutExtension(themePath);
            var divider = XDocument.Load(themePath).Descendants()
                .SingleOrDefault(element => (string?)element.Attribute(Xaml + "Key") == "MenuSeparatorBrush");
            string color = divider is null
                ? BrushColor(root, theme, "MenuBorderBrush")
                : divider.Attribute("Color")!.Value;
            Assert.IsType<Color>(ColorConverter.ConvertFromString(color));
        }
    }

    [Theory]
    [InlineData("AcrobatLight")]
    [InlineData("AcrobatDark")]
    public void ExistingMenuAndDialogSeparatorsFollowResourceChanges(string nextTheme)
    {
        RunSta(() =>
        {
            string root = FindRepositoryRoot();
            var source = XDocument.Load(Path.Combine(root, "Shell", "MainWindow.xaml"));
            var styles = source.Descendants(Presentation + "Style")
                .Where(element => (string?)element.Attribute("TargetType") == "Separator")
                .Select(element => new XElement(element)).ToArray();
            Assert.Equal(2, styles.Length);
            var dictionaryXml = new XElement(Presentation + "ResourceDictionary",
                new XAttribute(XNamespace.Xmlns + "x", Xaml.NamespaceName), styles);
            var resources = (ResourceDictionary)XamlReader.Parse(dictionaryXml.ToString());
            var menuSeparator = new Separator
            {
                Style = (Style)resources[MenuItem.SeparatorStyleKey]
            };
            var dialogSeparator = new Separator
            {
                Style = (Style)resources[typeof(Separator)]
            };
            var gray = (Brush)new BrushConverter().ConvertFromString(
                BrushColor(root, "AcrobatLight", "MenuSeparatorBrush"))!;
            menuSeparator.Resources["MenuSeparatorBrush"] = gray;
            dialogSeparator.Resources["MenuSeparatorBrush"] = gray;
            menuSeparator.ApplyTemplate();
            var line = Assert.IsType<Border>(VisualTreeHelper.GetChild(menuSeparator, 0));
            Assert.Same(gray, line.Background);
            Assert.Same(gray, dialogSeparator.Background);

            var restored = (Brush)new BrushConverter().ConvertFromString(BrushColor(root, nextTheme, "MenuSeparatorBrush"))!;
            menuSeparator.Resources["MenuSeparatorBrush"] = restored;
            dialogSeparator.Resources["MenuSeparatorBrush"] = restored;
            Assert.Same(restored, line.Background);
            Assert.Same(restored, dialogSeparator.Background);
            Assert.Same(line, VisualTreeHelper.GetChild(menuSeparator, 0));
        });
    }

    private static string BrushColor(string root, string theme, string key) =>
        XDocument.Load(Path.Combine(root, "Themes", theme + ".xaml")).Descendants()
            .Single(element => (string?)element.Attribute(Xaml + "Key") == key)
            .Attribute("Color")!.Value;

    private static void RunSta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception exception) { error = exception; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (error is not null)
            ExceptionDispatchInfo.Capture(error).Throw();
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
