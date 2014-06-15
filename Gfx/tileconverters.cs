using System; 
using System.Drawing;
using System.Drawing.Imaging;
//using System.Text;
//using System.Windows.Forms;

namespace dumplib.Gfx.TileConverters
{
    public class Nintendo_VirtualBoy : ITileConverter
    {
        public string ID
        {
            get
            {
                return "nvb";
            }
        }
        public string Description
        {
            get
            {
                return "Nintendo VirtualBoy (2bpp)";
            }
        }

        public int Bitdepth
        {
            get
            {
                return 2;
            }
        }

        public int TileWidth
        {
            get
            {
                return 8;
            }
        }

        public int TileHeight
        {
            get
            {
                return 8;
            }
        }

        // the proper way to calculate chunk size:
        // ((BitDepth * TileWidth) * TileHeight) / 8
        public int ChunkSize {
            get
            {
                return 16;
            }
        }

        public byte[] GetTile(byte[] TileData)
        {
            if (TileData.Length != 16)
                throw new ArgumentOutOfRangeException("Data chunk is incorrect size. 2bpp tiles must be exactly 16 bytes.");

            var _out = new byte[64];
            int outptr = 0;

            for (int row = 0; row < 16; row += 2)
            {
                int j = outptr * 8;
                byte byte1 = TileData[row];
                byte byte2 = TileData[row + 1];

                _out[j] = (byte)(byte1 & 3);
                _out[j + 1] = (byte)((byte1 >> 2) & 3);
                _out[j + 2] = (byte)((byte1 >> 4) & 3);
                _out[j + 3] = (byte)((byte1 >> 6) & 3);
                _out[j + 4] = (byte)(byte2 & 3);
                _out[j + 5] = (byte)((byte2 >> 2) & 3);
                _out[j + 6] = (byte)((byte2 >> 4) & 3);
                _out[j + 7] = (byte)((byte2 >> 6) & 3);
                outptr++;
            }

            return _out;
        }
    }

    public class SNK_NeoGeoPocket : ITileConverter
    {
        public string ID
        {
            get
            {
                return "ngp";
            }
        }

        public string Description
        {
            get
            {
                return "NeoGeo Pocket + Color (2bpp)";
            }
        }

        public int Bitdepth
        {
            get
            {
                return 2;

            }
        }

        public int TileWidth
        {
            get
            {
                return 8;
            }
        }

        public int TileHeight
        {
            get
            {
                return 8;
            }
        }

        // the proper way to calculate chunk size:
        // ((BitDepth * TileWidth) * TileHeight) / 8
        public int ChunkSize
        {
            get
            {
                return 16;
            }
        }

        public byte[] GetTile(byte[] TileData)
        {
            if (TileData.Length != 16)
                throw new ArgumentOutOfRangeException("Data chunk is incorrect size. 2bpp tiles must be exactly 16 bytes.");

            var _out = new byte[64];
            int outptr = 0;

            for (int row = 0; row < 16; row += 2)
            {
                int j = outptr * 8;
                byte byte1 = TileData[row];
                byte byte2 = TileData[row + 1];

                _out[j + 3] = (byte)(byte2 & 3);
                _out[j + 2] = (byte)((byte2 >> 2) & 3);
                _out[j + 1] = (byte)((byte2 >> 4) & 3);
                _out[j] = (byte)((byte2 >> 6) & 3);
                _out[j + 7] = (byte)(byte1 & 3);
                _out[j + 6] = (byte)((byte1 >> 2) & 3);
                _out[j + 5] = (byte)((byte1 >> 4) & 3);
                _out[j + 4] = (byte)((byte1 >> 6) & 3);
                outptr++;
            }

            return _out;
        }
    }




    public class Monochrome : ITileConverter
    {
        public string ID
        {
            get
            {
                return "mono";
            }
        }

        public string Description
        {
            get
            {
                return "Monochrome (1bpp)";
            }
        }

        public int Bitdepth
        {
            get
            {
                return 1;
            }
        }

        public int TileWidth
        {
            get
            {
                return 8;
            }
        }

        public int TileHeight
        {
            get
            {
                return 8;
            }
        }

        // the proper way to calculate chunk size:
        // ((BitDepth * TileWidth) * TileHeight) / 8
        public int ChunkSize
        {
            get
            {
                return 8;
            }
        }

        /// <summary>
        /// Transcodes a 1 bit per pixel tile into standard bitmap data
        /// </summary>
        /// <param name="TileData">Array of bytes to decode. The array MUST contain exactly 8 bytes.</param>
        /// <returns>Array of bytes in standard bitmap format</returns>
        public byte[] GetTile(byte[] TileData)
        {
            if (TileData.Length != 8)
                throw new ArgumentOutOfRangeException("Data chunk is incorrect size. 1bpp tiles must be exactly 8 bytes.");

            int count = 0;
            byte bit = 0;
            var _out = new byte[64];

            for (int row = 0; row < 8; row++)
            {
                for (int shift = 8; shift > 0; shift--)
                {
                    bit = 0;
                    if (((TileData[row] >> shift - 1) & 1) == 1) bit++;
                    _out[count] = bit;
                    count++;
                }
            }
            return _out;
        }
    }

    public class Nintendo_GameBoy : ITileConverter
    {
        public string ID
        {
            get
            {
                return "ngb_2bpp";
            }
        }

        public string Description
        {
            get
            {
                return "Nintendo Gameboy/Super Famicom (2bpp)";
            }

        }

        public int Bitdepth
        {
            get
            {
                return 2;
            }
        }

        public int TileWidth
        {
            get
            {
                return 8;
            }
        }

        public int TileHeight
        {
            get
            {
                return 8;
            }
        }

        // the proper way to calculate chunk size:
        // ((BitDepth * TileWidth) * TileHeight) / 8
        public int ChunkSize
        {
            get
            {
                return 16;
            }
        }

        /// <summary>
        /// Transcodes a Nintendo Gameboy tile into standard bitmap data
        /// </summary>
        /// <param name="TileData">Array of bytes to decode. The array MUST contain exactly 16 bytes.</param>
        /// <returns>Array of bytes in standard bitmap format</returns>
        public byte[] GetTile(byte[] TileData)
        {
            if (TileData.Length != 16)
                throw new ArgumentOutOfRangeException("Data chunk is incorrect size. 2bpp tiles must be exactly 16 bytes.");

            var _out = new byte[64];
            int count = 0;

            for (int row = 0; row < 16; row += 2)
            {
                for (int shift = 8; shift > 0; shift--)
                {
                    _out[count] = 0;
                    if (((TileData[row] >> shift - 1) & 1) == 1) _out[count]++;
                    if (((TileData[row + 1] >> shift - 1) & 1) == 1) _out[count] += 2;
                    count++;
                }
            }
            return _out;
        }
    }

    public class Nintendo_SuperFamicom3BPP : ITileConverter
    {
        public string ID
        {
            get
            {
                return "sfc_3bpp";
            }
        }

        public string Description
        {
            get
            {
                return "Nintendo Super Famicom (3bpp)";
            }
        }

        public int Bitdepth
        {
            get
            {
                return 3;
            }
        }

        public int TileWidth
        {
            get
            {
                return 8;
            }
        }

        public int TileHeight
        {
            get
            {
                return 8;
            }
        }

        // the proper way to calculate chunk size:
        // ((BitDepth * TileWidth) * TileHeight) / 8
        public int ChunkSize
        {
            get
            {
                return 24;
            }
        }

        public byte[] GetTile(byte[] TileData)
        {
            if (TileData.Length != 24)
                throw new ArgumentOutOfRangeException("Data chunk is incorrect size. 3bpp tiles must be exactly 24 bytes.");

            var _out = new byte[96];
            int count = 0;

            for (int row = 0; row < 16; row += 2)
            {
                for (int shift = 8; shift > 0; shift--)
                {
                    _out[count] = 0;
                    if (((TileData[row] >> shift - 1) & 1) == 1) _out[count]++;
                    if (((TileData[row + 1] >> shift - 1) & 1) == 1) _out[count] += 2;
                    if (((TileData[(row / 2) + 16] >> shift - 1) & 1) == 1) _out[count] += 4;
                    count++;
                }
            }
            return _out;
        }
    }


    public class Nintendo_Famicom : ITileConverter
    {
        public string ID
        {
            get
            {
                return "nfc";
            }
        }

        public string Description
        {
            get
            {
                return "Nintendo Famicom (2bpp)";
            }
        }

        public int Bitdepth
        {
            get
            {
                return 2;
            }
        }

        public int TileWidth
        {
            get
            {
                return 8;
            }
        }

        public int TileHeight
        {
            get
            {
                return 8;
            }
        }

        public int ChunkSize
        {
            get
            {
                return 16;
            }
        }

        /// <summary>
        /// Transcodes a Nintendo Famicom tile into standard bitmap data
        /// </summary>
        /// <param name="TileData">Array of bytes to decode. The array MUST contain exactly 16 bytes.</param>
        /// <returns>Array of bytes in standard bitmap format</returns>
        public byte[] GetTile(byte[] TileData)
        {
            if (TileData.Length != 16)
                throw new ArgumentOutOfRangeException("Data chunk is incorrect size. 2bpp tiles must be exactly 16 bytes.");

            var _out = new byte[64];
            int count = 0;

            for (int row = 0; row < 8; row++)
            {
                for (int shift = 7; shift >= 0; shift--)
                {
                    _out[count] = 0;
                    if (((TileData[row] >> shift) & 1) == 1) _out[count] += 1;
                    if (((TileData[row + 8] >> shift) & 1) == 1) _out[count] += 2;
                    count++;
                }
            }
            return _out;
        }
    }

    public class Nintendo_SuperFamicom : ITileConverter
    {
        public string ID
        {
            get
            {
                return "sfc_4bpp";
            }
        }

        public string Description
        {
            get
            {
                return "Nintendo Super Famicom; NEC PC Engine (4bpp)";
            }
        }

        public int Bitdepth
        {
            get
            {
                return 4;
            }
        }

        public int TileWidth
        {
            get
            {
                return 8;
            }
        }

        public int TileHeight
        {
            get
            {
                return 8;
            }
        }

        public int ChunkSize
        {
            get
            {
                return 32;
            }
        }

        /// <summary>
        /// Transcodes a 4-bit per pixel Nintendo Super Famicom tile into standard bitmap data
        /// </summary>
        /// <param name="TileData">Array of bytes to decode. The array MUST contain exactly 16 bytes.</param>
        /// <returns>Array of bytes in standard bitmap format</returns>
        public byte[] GetTile(byte[] TileData)
        {
            if (TileData.Length != 32)
                throw new ArgumentOutOfRangeException("Data chunk is incorrect size. 4bpp tiles must be exactly 32 bytes.");

            var _out = new byte[64];
            int count = 0;

            for (int row = 0; row < 16; row += 2)
            {
                for (int shift = 7; shift >= 0; shift--)
                {
                    _out[count] = 0;
                    if (((TileData[row] >> shift) & 1) == 1) _out[count] += 1;
                    if (((TileData[row + 1] >> shift) & 1) == 1) _out[count] += 2;
                    if (((TileData[row + 16] >> shift) & 1) == 1) _out[count] += 4;
                    if (((TileData[row + 17] >> shift) & 1) == 1) _out[count] += 8;
                    count++;
                }
            }
            return _out;
        }
    }

    public class Sega_8bit : ITileConverter
    {
        public string ID
        {
            get
            {
                return "sega_8bit";
            }
        }
        public string Description
        {
            get
            {
                return "Sega GameGear/Master System; Bandai Wonderswan Color (4bpp)";
            }
        }

        public int Bitdepth
        {
            get
            {
                return 4;
            }
        }

        public int TileWidth
        {
            get
            {
                return 8;
            }
        }

        public int ChunkSize
        {
            get
            {
                return 32;
            }
        }

        public int TileHeight
        {
            get
            {
                return 8;
            }
        }


        /// <summary>
        /// Transcodes a Sega GameGear/Master System tile into standard bitmap data
        /// </summary>
        /// <param name="TileData">Array of bytes to decode. The array MUST contain exactly 16 bytes.</param>
        /// <returns>Array of bytes in standard bitmap format</returns>
        public byte[] GetTile(byte[] TileData)
        {
            if (TileData.Length != 32)
                throw new ArgumentOutOfRangeException("Data chunk is incorrect size. 4bpp tiles must be exactly 32 bytes.");
            var _out = new byte[64];
            int count = 0;

            for (int row = 0; row < 32; row += 4)
            {
                for (int shift = 7; shift >= 0; shift--)
                {
                    _out[count] = 0;
                    if (((TileData[row] >> shift) & 1) == 1) _out[count] += 1;
                    if (((TileData[row + 1] >> shift) & 1) == 1) _out[count] += 2;
                    if (((TileData[row + 2] >> shift) & 1) == 1) _out[count] += 4;
                    if (((TileData[row + 3] >> shift) & 1) == 1) _out[count] += 8;
                    count++;
                }
            }
            return _out;
        }
    }

    public class Sega_Megadrive : ITileConverter
    {
        public string ID {
            get
            {
                return "smd_4bpp";
            }
        }

        public string Description
        {
            get
            {
                return "Sega MegaDrive; X68000 computer system (4bpp)";
            }
        }

        public int Bitdepth
        {
            get
            {
                return 4;
            }
        }

        public int TileWidth
        {
            get
            {
                return 8;
            }
        }

        public int TileHeight
        {
            get
            {
                return 8;
            }
        }

        public int ChunkSize
        {
            get
            {
                return 32;
            }
        }

        /// <summary>
        /// Transcodes a Sega Megadrive tile into standard bitmap data
        /// </summary>
        /// <param name="TileData">Array of bytes to decode. The array MUST contain exactly 16 bytes.</param>
        /// <returns>Array of bytes in standard bitmap format</returns>
        public byte[] GetTile(byte[] TileData)
        {
            if (TileData.Length != 32)
                throw new ArgumentOutOfRangeException("Data chunk is incorrect size. 4bpp tiles must be exactly 32 bytes.");

            int count = 0;
            var _out = new byte[64];

            for (int t = 0; t < 32; t++)
            {
                _out[count] = (byte)((TileData[t] & 0xf0) >> 4);
                _out[count + 1] = (byte)(TileData[t] & 0xf);
                count += 2;
            }
            return _out;
        }
    }
    /*
        public static byte[] From_SFC_8bpp(byte[] TileData)
        {
            if (TileData.Length != 64)
                throw new ArgumentOutOfRangeException("Data chunk is incorrect size. 8bpp tiles must be exactly 64 bytes.");

            var _out = new byte[64];
            int count = 0;

            for (int row = 0; row < 16; row += 2)
            {
                for (int shift = 7; shift >= 0; shift--)
                {
                    _out[count] = 0;
                    if (((TileData[row] >> shift) & 1) == 1) _out[count] += 1;
                    if (((TileData[row + 1] >> shift) & 1) == 1) _out[count] += 2;
                    if (((TileData[row + 16] >> shift) & 1) == 1) _out[count] += 4;
                    if (((TileData[row + 17] >> shift) & 1) == 1) _out[count] += 8;
                    if (((TileData[row + 32] >> shift) & 1) == 1) _out[count] += 16;
                    if (((TileData[row + 33] >> shift) & 1) == 1) _out[count] += 32;
                    if (((TileData[row + 48] >> shift) & 1) == 1) _out[count] += 64;
                    if (((TileData[row + 49] >> shift) & 1) == 1) _out[count] += 128;
                    count++;
                }
            }
            return _out;
        }

        /// <summary>
        /// Transcodes a Nintendo Super Famicom Mode 7 tile into standard bitmap data
        /// </summary>
        /// <param name="TileData">Array of bytes to decode. The array MUST contain exactly 16 bytes.</param>
        /// <returns>Array of bytes in standard bitmap format</returns>
        public static byte[] From_SFC_Mode7(byte[] TileData)
        {
            if (TileData.Length != 64)
                throw new ArgumentOutOfRangeException("Data chunk is incorrect size. Mode 7 tiles must be exactly 64 bytes.");
            byte[] _out = new byte[TileData.Length];
            Array.Copy(TileData,_out,TileData.Length);
            return _out;
        }

        */
}