using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dumplib.Layout;
using dumplib;

namespace dumplib.Text
{
 
    public static class ByteDump
    {
        /*
        // div = amount of complete lines of 16 bytes, mod = left over bytes
            long div16 = Addr.Length / 16;
            long mod16 = Addr.Length % 16;
            StringBuilder _out = new StringBuilder();

            // for each complete line, write out a full 16 column line of bytes
            if (div16 > 0)
            {
                for (int t = 0; t < div16; t++)
                {
                    _out.Append((Addr.StartOffset + (t * 16)).ToString("X8"));
                    for (int y = 0; y < 16; y++)
                        _out.Append(' ' + Data[Addr.StartOffset + (t * 16) + y].ToString("X2"));
                    _out.Append(Environment.NewLine);
                }
            }

            // if there are any bytes left over, write them out
            if (mod16 > 0)
            {
                _out.Append((Addr.StartOffset + (div16 * 16)).ToString("X8"));
                for (long y = Addr.StartOffset + (div16 * 16); y < Addr.StartOffset + (div16 * 16) + mod16; y++)
                    _out.Append(' ' + Data[y].ToString("X2"));
            }
            return _out.ToString();
*/
        public static string ToHex(DataChunk Chunk)
        {
            return MakeTable(Chunk, 16, "X8", "X2");
        }

        public static string ToDecimal(DataChunk Chunk)
        {
            return MakeTable(Chunk, 10, "D12", "D3");
        }

        private static string MakeTable(DataChunk Chunk, int Base, string IndexFormat, string ValueFormat)
        {
            // div = amount of complete lines, mod = left over bytes
            int div = Chunk.Info.Addr.Length / Base;
            int mod = Chunk.Info.Addr.Length % Base;
            StringBuilder _out = new StringBuilder();

            // for each complete line, write out a full line of bytes
            if (div > 0)
            {
                for (int t = 0; t < div; t++)
                {
                    _out.Append(((t * Base) + Chunk.Info.Addr.StartOffset).ToString(IndexFormat));
                    for (int y = 0; y < Base; y++)
                        //_out.Append(' ' + Chunk.Data[Chunk.Info.Addr.StartOffset + (t * Base) + y].ToString(ValueFormat));
                        _out.Append(' ' + Chunk.Data[(t * Base) + y].ToString(ValueFormat));
                    _out.Append(Environment.NewLine);
                }
            }

            // if there are any bytes left over, write them out
            if (mod > 0)
            {
                _out.Append((Chunk.Info.Addr.StartOffset + (div * Base)).ToString(IndexFormat));
                int modstart = div * Base;
                for (int y = modstart; y < modstart + mod; y++)
                    _out.Append(' ' + Chunk.Data[y].ToString(ValueFormat));
            }
            return _out.ToString();
        }
    }
}
