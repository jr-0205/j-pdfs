using JPDFs.Engine.Authoring;
using JPDFs.Engine.Documents;
using JPDFs.Engine.Editing;
using JPDFs.Engine.Parsing;
using JPDFs.Engine.Objects;
using JPDFs.Engine.Writing;
using Xunit;

namespace JPDFs.Engine.Tests.Editing;

public sealed class PdfFormFlattenerTests
{
    [Fact]
    public void FlattenGeneratesMissingTextAppearanceWithoutLosingTheValue()
    {
        PdfDocument source = PdfDocument.Open(new PdfDocumentBuilder().AddBlankPage()
            .AddTextField(0, "name", 20, 20, 160, 24, "Ada").Build());
        PdfFormWidgetInfo widget = Assert.Single(PdfFormWidgetReader.ReadPage(source, 0));
        PdfDictionary field = Assert.IsType<PdfDictionary>(source.Resolve(
            new PdfIndirectReference(widget.ObjectNumber, widget.Generation)));
        var update = new PdfIncrementalUpdateBuilder(source);
        update.ReplaceObject(widget.ObjectNumber, new PdfDictionary(field.Where(
            entry => entry.Key.ValueAsLatin1() != "AP")));
        source = PdfDocument.Open(update.Build());

        PdfDocument flattened = PdfDocument.Open(PdfFormFlattener.Flatten(source));

        Assert.Empty(PdfFormWidgetReader.ReadPage(flattened, 0));
        Assert.Contains("Ada", PdfStructuredExport.ToPlainText(flattened));
    }

    [Fact]
    public void FlattenPaintsSelectedWidgetAppearancesAndRemovesFields()
    {
        byte[] sourceBytes = new PdfDocumentBuilder().AddBlankPage(300, 300)
            .AddTextField(0, "name", 20, 220, 160, 24, "Ada")
            .AddCheckBox(0, "active", 20, 170, 20, 20, isChecked: true)
            .AddComboBox(0, "region", 20, 120, 120, 24,
                ["US", "CA"], "CA")
            .Build();
        PdfDocument source = PdfDocument.Open(sourceBytes);

        byte[] output = PdfFormFlattener.Flatten(source);
        PdfDocument reopened = PdfDocument.Open(output);

        Assert.Empty(PdfFormWidgetReader.ReadPage(reopened, 0));
        string text = PdfStructuredExport.ToPlainText(reopened);
        Assert.Contains("Ada", text);
        Assert.Contains("CA", text);
        Assert.True(output.AsSpan(0, sourceBytes.Length).SequenceEqual(sourceBytes));
        Assert.Contains(new PdfPageContentReader(reopened).ReadInstructions(0),
            instruction => instruction.Operator == "Do");
    }

    [Fact]
    public void FlattenWithoutWidgetsPreservesTheOriginalBytes()
    {
        byte[] source = new PdfDocumentBuilder().AddBlankPage().Build();

        byte[] output = PdfFormFlattener.Flatten(PdfDocument.Open(source));

        Assert.Equal(source, output);
    }
}
