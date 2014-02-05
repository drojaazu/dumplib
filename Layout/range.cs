using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dumplib.Layout
{
    /// <summary>
    /// References a location inside a serial block of data
    /// Uses uint for addressing (max. 4gb of data)
    /// </summary>
    public class Range
    {
        public override string ToString()
        {
            return "Offset: " + this.StartOffset.ToString("X") + " Length: " + this.Length.ToString("X");
        }

        public uint StartOffset
        {
            get;
            private set;
        }

        public uint Length
        {
            get;
            private set;
        }

        public uint EndOffset
        {
            get
            {
                return (this.StartOffset + this.Length) - 1;
            }
        }

        /// <summary>
        /// Defines a chunk of data inside an sequential data structure
        /// </summary>
        /// <param name="Offset">Starting index of the data</param>
        /// <param name="Length">Number of bytes</param>
        public Range(uint Offset, uint Length)
        {
            if (Length < 1) throw new ArgumentException("Length cannot be zero");

            this.StartOffset = Offset;
            this.Length = Length;
        }

        /// <summary>
        /// Defines a chunk of data inside an sequential data structure
        /// </summary>
        /// <param name="Offset">Starting index of the data (Input must be a properly formatted hex string)</param>
        /// <param name="Length">Number of bytes (Input must be a properly formatted hex string)</param>
        public Range(string Offset, string Length)
        {
            if (Offset == string.Empty || Length == string.Empty)
                throw new ArgumentException("Argument is empty");

            if (IsValidHex(Offset) && IsValidHex(Length))
                {
                    this.StartOffset = uint.Parse(Offset, System.Globalization.NumberStyles.HexNumber);
                    this.Length = uint.Parse(Length, System.Globalization.NumberStyles.HexNumber);
                }
                else
                    // I told you...
                    throw new FormatException("Argument is not a properly formatted hexadecimal number");

            if (this.Length < 1) throw new ArgumentException("Length cannot be less than 1");
        }

        private bool IsValidHex(string HexString)
        {
            bool ishex;
            foreach (char c in HexString)
            {
                ishex = (c >= '0' && c <= '9') ||
                    (c >= 'a' && c <= 'f') ||
                    (c >= 'A' && c <= 'F');
                if (!ishex) return false;
            }
            return true;
        }
        
    }
}
