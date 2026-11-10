// Derived from Apache PDFBox JBIG2 ImageIO Plugin and its C# port.
// Modified for the JPDFs engine.

#nullable disable

namespace JPDFs.Engine.Filters.Jbig2
{
    /// <summary>
    /// This segment flags an end of stripe (see JBIG2 ISO standard, 7.4.9).
    /// </summary>
    internal sealed class EndOfStripe : ISegmentData
    {
        private SubInputStream subInputStream;
        private int lineNumber;

        private void ParseHeader()
        {
            lineNumber = (int)(subInputStream.ReadBits(32) & 0xffffffff);
        }

        public void Init(SegmentHeader header, SubInputStream sis)
        {
            subInputStream = sis;
            ParseHeader();
        }

        public int GetLineNumber()
        {
            return lineNumber;
        }
    }
}
