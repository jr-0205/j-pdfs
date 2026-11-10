using JPDFs.Services;
using Xunit;

namespace JPDFs.Tests;

public sealed class BatchOutputPlanningTests
{
    [Fact]
    public void RenamedMacroOutputsUseTheChosenDirectoryAndCannotReplaceSources()
    {
        var macro = new JPDFs.Engine.Documents.PdfMacro("Rename", [
            JPDFs.Engine.Documents.PdfRenameMacro.RenameStep("{name}-{index}")]);
        string[] inputs = [@"C:\one\alpha.pdf", @"C:\two\beta.pdf"];

        Assert.Equal([@"C:\out\alpha-1.pdf", @"C:\out\beta-2.pdf"],
            PdfEngineIntegration.PlanMacroOutputs(macro, inputs, @"C:\out\result.pdf"));
        Assert.Throws<ArgumentException>(() => PdfEngineIntegration.PlanMacroOutputs(
            macro, inputs, @"C:\out\result.pdf", [@"C:\out\beta-2.pdf"]));
    }

    [Fact]
    public void BatchOutputsAreUniqueAndCannotReplaceAnySource()
    {
        string[] sources = [@"C:\one\same.pdf", @"C:\two\same.pdf"];
        var outputs = PdfEngineIntegration.PlanBatchOutputs(sources, @"C:\out\result.pdf");
        Assert.Equal(new[] { @"C:\out\result.pdf", @"C:\out\result-2.pdf" }, outputs);
        Assert.Throws<ArgumentException>(() => PdfEngineIntegration.PlanBatchOutputs(
            [@"C:\out\result-2.pdf", @"C:\other.pdf"], @"C:\out\result.pdf"));
    }
}
