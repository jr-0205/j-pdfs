// Derived from Apache PDFBox JBIG2 ImageIO Plugin and its C# port.
// Modified for the JPDFs engine.
#nullable disable

namespace JPDFs.Engine.Filters.Jbig2
{
    using System;

    internal sealed class InvalidHeaderValueException : Jbig2Exception
    {
        public InvalidHeaderValueException()
        {
        }

        public InvalidHeaderValueException(string message)
            : base(message)
        {
        }

        public InvalidHeaderValueException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
