using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.ComponentModel;

namespace dumplib
{
    /// <summary>
    /// Display types for older CRTs, often marked as a setting in the software header
    /// </summary>
    public enum CRTDisplayType
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("NTSC")]
        NTSC,
        [Description("PAL")]
        PAL
    }

    internal class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] left, byte[] right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }
            return left.SequenceEqual(right);
        }
        public int GetHashCode(byte[] key)
        {
            if (key == null) throw new ArgumentNullException("ByteArrayComparer: key is null!");
            return key.Sum(b => b);
        }
    }

    public class FileParseException : Exception
    {
        public int Line
        {
            get;
            private set;
        }

        public string Filepath
        {
            get;
            private set;
        }

        public FileParseException(string Message, string Filepath, int Line, Exception InnerException)
            : base(Message, InnerException)
        {
            this.Line = Line;
            this.Filepath = Filepath;
        }

        public FileParseException(string Message, string Filepath, int Line)
            : base(Message)
        {
            this.Line = Line;
            this.Filepath = Filepath;
        }

        public FileParseException(string Message, string Filepath)
            : base(Message)
        {
            this.Filepath = Filepath;
        }

        public FileParseException(string Message, string Filepath, Exception InnerException)
            : base(Message, InnerException)
        {
            this.Filepath = Filepath;
        }
    }

    internal static class ExtensionMethods
    {
        internal static string GetEnumDesc(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }

        /*public static string ConvertLF(this string text)
        {
            if (!text.Contains("\\n"))
            {
                // no codes found in the string, return it
                return text;
            }
            int n = text.IndexOf("\\n");
            while (n >= 0)
            {
                text = text.Substring(0, n) + Environment.NewLine + text.Substring(n + 2, text.Length - (n + 2));
                n = text.IndexOf("\\n");
            }
            return text;
        }*/

        internal static byte ReverseBits(this byte thisbyte)
        {
            byte _out = thisbyte;
            byte shift = 0;
            for (thisbyte >>= 1; shift < 7; thisbyte >>= 1)
            {
                _out <<= 1;
                _out |= (byte)(thisbyte & 1);
                shift++;
            }
            return _out;
        }

        internal static byte[] HexStringToByteArray(this string hex)
        {
            // if string is not even length, prepend a 0
            if ((hex.Length % 2) != 0)
                hex = "0" + hex;

            byte[] _out = new byte[(hex.Length / 2)];
            for (int i = 0; i < _out.Length; i++)
                _out[i] = byte.Parse(hex.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            return _out;
        }
    }
}
