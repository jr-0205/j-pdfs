// Derived from Apache PDFBox JBIG2 ImageIO Plugin and its C# port.
// Modified for the JPDFs engine.
#nullable disable

namespace JPDFs.Engine.Filters.Jbig2
{
    using System;

    internal class Jbig2Exception : Exception
    {
        public Jbig2Exception()
        {
        }

        public Jbig2Exception(string message)
            : base(message)
        {
        }

        public Jbig2Exception(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
