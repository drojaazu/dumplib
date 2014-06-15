using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dumplib.Layout;
using dumplib;

namespace dumplib.Text
{
 /*
    public static class ByteDump
    {
        public static string ToHex(DataChunk Chunk)
        {
            // div = amount of complete lines of 16 bytes, mod = left over bytes
            int div16 = Chunk.Info.Addr.Length / 16;
            int mod16 = Chunk.Info.Addr.Length % 16;
            StringBuilder _out = new StringBuilder();

            // for each complete line, write out a full 16 column line of bytes
            if (div16 > 0)
            {
                for (int t = 0; t < div16; t++)
                {
                    _out.Append(((t * 16) + Chunk.Info.Addr.StartOffset).ToString("X8"));
                    for (int y = 0; y < 15; y++)
                        _out.Append(' ' + Chunk.Data[Chunk.Info.Addr.StartOffset + (t * 16) + y].ToString("X2"));
                    _out.Append(Environment.NewLine);
                }
            }

            // if there are any bytes left over, write them out
            if (mod16 > 0)
            {
                _out.Append((Chunk.Info.Addr.StartOffset + (div16 * 16)).ToString("X8"));
                for (long y = Chunk.Info.Addr.StartOffset + (div16 * 16); y < Chunk.Info.Addr.StartOffset + (div16 * 16) + mod16; y++)
                    _out.Append(' ' + Chunk.Data[y].ToString("X2"));
            }
            return _out.ToString();
        }

        public static string ToOctal(byte[] Data)
        {
            return null;
        }

        public static string ToDecimal(byte[] Data)
        {
            return null;
        }
    }*/
}
