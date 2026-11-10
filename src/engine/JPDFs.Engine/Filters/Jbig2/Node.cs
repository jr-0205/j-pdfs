// Derived from Apache PDFBox JBIG2 ImageIO Plugin and its C# port.
// Modified for the JPDFs engine.
#nullable disable

namespace JPDFs.Engine.Filters.Jbig2
{
    /// <summary>
    /// Base class for all nodes in a Huffman tree.
    /// </summary>
    internal abstract class Node
    {
        public abstract long Decode(IImageInputStream iis);
    }
}
