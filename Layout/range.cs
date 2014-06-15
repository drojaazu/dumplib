using System;
using System.Collections.Generic;
using System.Text;

namespace dumplib.Layout
{
    /// <summary>
    /// References a location inside a serial block of data
    /// </summary>
    public class Range
    {
        public override string ToString()
        {
            return "Offset: " + this.StartOffset.ToString("X") + " Length: " + this.Length.ToString("X");
        }

        public long StartOffset
        {
            get;
            private set;
        }

        public int Length
        {
            get;
            private set;
        }

        public long EndOffset
        {
            get
            {
                return this.StartOffset + this.Length;
            }
        }

        /// <summary>
        /// Defines the boundaries of a section of data inside a sequential data structure
        /// </summary>
        /// <param name="Offset">Starting index of the data</param>
        /// <param name="Length">Number of bytes</param>
        public Range(long Offset, int Length)
        {
            if (Length < 1) throw new ArgumentException("Length must be at least 1");

            this.StartOffset = Offset;
            this.Length = Length;
        }

        /// <summary>
        /// Defines the boundaries of a section of data inside a sequential data structure
        /// </summary>
        /// <param name="Offset">Starting index of the data (Input must be a properly formatted hex string)</param>
        /// <param name="Length">Number of bytes (Input must be a properly formatted hex string)</param>
        public Range(string Offset, string Length)
        {
            if(string.IsNullOrEmpty(Offset) || string.IsNullOrEmpty(Length))
                throw new ArgumentException();

            long tempoffset; int templength;
            if (!long.TryParse(Offset, System.Globalization.NumberStyles.HexNumber, null, out tempoffset))
                throw new FormatException("Offset is not a properly formatted hex string");

            if (!int.TryParse(Offset, System.Globalization.NumberStyles.HexNumber, null, out templength))
                throw new FormatException("Length is not a properly formatted hex string");

            if (templength < 1) throw new ArgumentException("Length cannot be less than 1");

            this.StartOffset = tempoffset;
            this.Length = templength;
        }
    }
}
