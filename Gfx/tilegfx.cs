using System; 
//using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dumplib.Gfx
{
    public enum TileFormats : byte
    {
        Monochrome = 0,
        Gameboy,
        SuperFamicom_3bpp,
        SuperFamicom_4bpp,
        Famicom,
        Megadrive,
        Sega_8bit,
        SuperFamicom_8bpp,
        SuperFamicom_Mode7,
        VirtualBoy,
        NeoGeoPocket
    }

    

    public static class TileGfx
    {
        private delegate byte[] TileMode(byte[] _in);

        /// <summary>
        /// Gets a block of tiles as a bitmap
        /// </summary>
        /// <param name="TileData"></param>
        /// <param name="Settings"></param>
        /// <returns>"Advanced" mode - returns a 32bit bitmap that contains the rendered output as well as optional formatting</returns>
        /*public static Bitmap GetTiles(byte[] TileData, TileRenderSettings Settings)
        {
            // verify and set up
            if (TileData == null || Settings == null) throw new ArgumentNullException();

            // get specifications about the selected tile format
            TileFormatInfo Format = new TileFormatInfo(Settings.Format);

            // didn't pass enough data...
            if (TileData.Length < Format.ChunkSize) throw new ArgumentOutOfRangeException("Array too short for the specified tile format");

            TileMode GetTile = null;
            
            // some internal variables for caching...
            // number of total tiles to render
            int TileCount = TileData.Length / Format.ChunkSize;
            // number of *complete* rows
            int FullRows = TileCount / Settings.TilesPerRow;
            // number of leftover tiles in the last row, if any
            int Remainder = TileCount % Settings.TilesPerRow;

            // image rendering algorithm psuedo code!
            // width: (tiles per row * tilesize) + ((tilemargin * tilesperrow) +  tilemargin) + address text width + (image margin * 2)
            // height: ((FullRows * tilesize) + (Remainder > 0 ? tilesize : 0)) + ((tilemargin * tilesperrow) + tilemargin) + header text height + (imagemargin * 2)

            int FinalImageWidth = (Settings.TilesPerRow * Format.TileSize) + ((Settings.TileMargin * Settings.TilesPerRow) + Settings.TileMargin);
        }*/

        /// <summary>
        /// Gets a block of tiles as a bitmap
        /// </summary>
        /// <param name="TileData">The encoded source data</param>
        /// <param name="TileFormat">The tile format to use</param>
        /// <param name="Palette">The color palette to use</param>
        /// <param name="TilesPerRow">The number of tiles to render per row</param>
        /// <returns>"Basic" mode - returns a bitmap with preserved bitdepth from the source; does not support extra formatting</returns>
        public static Bitmap GetTiles(byte[] TileData, TileFormats TileFormat, ColorPalette Palette, int TilesPerRow)
        {
            if (TileData == null || Palette == null)
                throw new ArgumentNullException();

            TileMode GetTile = null;

            // get specifications about the selected tile format
            TileFormatInfo Format = new TileFormatInfo(TileFormat);

            // validate that the palette is large enough
            if (Palette.Entries.Length < Format.PaletteSize)
                throw new ArgumentOutOfRangeException("Palette does not have enough entries");

            // set the delegate to the chosen format's method
            switch (TileFormat)
            {
                case TileFormats.Gameboy:
                    GetTile = From_NGB;
                    break;
                case TileFormats.SuperFamicom_4bpp:
                    GetTile = From_SFC;
                    break;
                case TileFormats.SuperFamicom_3bpp:
                    GetTile = From_SFC_3bpp;
                    break;
                case TileFormats.Monochrome:
                    GetTile = From_1bpp;
                    break;
                case TileFormats.SuperFamicom_Mode7:
                    GetTile = From_SFC_Mode7;
                    break;
                case TileFormats.SuperFamicom_8bpp:
                    GetTile = From_SFC_8bpp;
                    break;
                case TileFormats.Megadrive:
                    GetTile = From_SMD;
                    break;
                case TileFormats.Sega_8bit:
                    GetTile = From_SGG;
                    break;
                case TileFormats.Famicom:
                    GetTile = From_NFC;
                    break;
                case TileFormats.VirtualBoy:
                    GetTile = From_VB;
                    break;
                case TileFormats.NeoGeoPocket:
                    GetTile = From_NGP;
                    break;
            }
            byte[] chunk = new byte[Format.ChunkSize];
            byte[][] tiles = new byte[TileData.Length / Format.ChunkSize][];

            for (int u = 0; u < tiles.Length; u++)
            {
                Buffer.BlockCopy(TileData, u * Format.ChunkSize, chunk, 0, Format.ChunkSize);
                tiles[u] = GetTile(chunk);
            }

            TileData = null;
            int final_width = tiles.Length <= TilesPerRow ? Format.TileSize * tiles.Length : Format.TileSize * TilesPerRow;
            int final_height = tiles.Length <= TilesPerRow ? Format.TileSize : (tiles.Length / TilesPerRow) * Format.TileSize + ((tiles.Length % TilesPerRow) != 0 ? Format.TileSize : 0);


            //generate a new indexed bitmap then set the palette 
            var finalimg = new Bitmap(final_width, final_height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            finalimg.Palette = Palette;

            // create BitmapData object in memory to manually modify the byte values
            unsafe
            {
                BitmapData finalimgData = finalimg.LockBits(new Rectangle(0, 0, finalimg.Width, finalimg.Height), ImageLockMode.ReadWrite, finalimg.PixelFormat);
                IntPtr finalimgPtr = finalimgData.Scan0;
                byte[] finalbytes = new byte[finalimgData.Height * finalimgData.Stride];
                
                for (int row = 0; row < finalbytes.Length; row++) finalbytes[row] = 0;

                int tilerows = finalimg.Height / Format.TileSize;
                int tilerowbytesize = finalimgData.Stride * Format.TileSize;
                int startoffsetrows = 0;
                int tilecount = 0;
                int thisrownumcolumns = TilesPerRow;

                for (int thisrow = 0; thisrow < tilerows; thisrow++)
                {
                    startoffsetrows = tilerowbytesize * thisrow;
                    if (tiles.Length - tilecount < TilesPerRow) thisrownumcolumns = tiles.Length - tilecount;
                    for (int thisscanline = 0; thisscanline < Format.TileSize; thisscanline++)
                    {
                        
                        for (int thiscolumn = 0; thiscolumn < thisrownumcolumns; thiscolumn++)
                        {
                            // check tilecount here for left over
                            Buffer.BlockCopy(tiles[TilesPerRow * thisrow + thiscolumn],
                                thisscanline * Format.TileSize,
                                finalbytes,
                                startoffsetrows + (thisscanline * finalimgData.Stride) + (thiscolumn * Format.TileSize),
                                Format.TileSize);
                        }

                    }
                    tilecount += thisrownumcolumns;
                }

                System.Runtime.InteropServices.Marshal.Copy(finalbytes, 0, finalimgPtr, finalbytes.Length);
                finalimg.UnlockBits(finalimgData);
                return finalimg;
            }
        }

        public static byte[] From_VB(byte[] TileData)
        {
            if(TileData.Length != 16)
                throw new ArgumentOutOfRangeException("Data chunk is incorrect size. 2bpp tiles must be exactly 16 bytes.");

            var _out = new byte[64];
            int outptr = 0;

            for (int row = 0; row < 16; row += 2)
            {
                int j = outptr * 8;
                byte byte1 = TileData[row];
                byte byte2 = TileData[row +1];

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

        public static byte[] From_NGP(byte[] TileData)
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

        /// <summary>
        /// Transcodes a 1 bit per pixel tile into standard bitmap data
        /// </summary>
        /// <param name="TileData">Array of bytes to decode. The array MUST contain exactly 8 bytes.</param>
        /// <returns>Array of bytes in standard bitmap format</returns>
        public static byte[] From_1bpp(byte[] TileData)
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

        /// <summary>
        /// Transcodes a Nintendo Gameboy tile into standard bitmap data
        /// </summary>
        /// <param name="TileData">Array of bytes to decode. The array MUST contain exactly 16 bytes.</param>
        /// <returns>Array of bytes in standard bitmap format</returns>
        public static byte[] From_NGB(byte[] TileData)
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

        public static byte[] From_SFC_3bpp(byte[] TileData)
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
                    if (((TileData[(row /2 ) + 16] >> shift - 1) & 1) == 1) _out[count] += 4;
                    count++;
                }
            }
            return _out;
        }

        /// <summary>
        /// Transcodes a Nintendo Famicom tile into standard bitmap data
        /// </summary>
        /// <param name="TileData">Array of bytes to decode. The array MUST contain exactly 16 bytes.</param>
        /// <returns>Array of bytes in standard bitmap format</returns>
        public static byte[] From_NFC(byte[] TileData)
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

        /// <summary>
        /// Transcodes a 4-bit per pixel Nintendo Super Famicom tile into standard bitmap data
        /// </summary>
        /// <param name="TileData">Array of bytes to decode. The array MUST contain exactly 16 bytes.</param>
        /// <returns>Array of bytes in standard bitmap format</returns>
        public static byte[] From_SFC(byte[] TileData)
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

        /// <summary>
        /// Transcodes a Sega GameGear/Master System tile into standard bitmap data
        /// </summary>
        /// <param name="TileData">Array of bytes to decode. The array MUST contain exactly 16 bytes.</param>
        /// <returns>Array of bytes in standard bitmap format</returns>
        public static byte[] From_SGG(byte[] TileData)
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

        /// <summary>
        /// Transcodes a Sega Megadrive tile into standard bitmap data
        /// </summary>
        /// <param name="TileData">Array of bytes to decode. The array MUST contain exactly 16 bytes.</param>
        /// <returns>Array of bytes in standard bitmap format</returns>
        public static byte[] From_SMD(byte[] TileData)
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


    public class TileRenderSettings
    {
        public TileRenderSettings(TileFormats Format, ColorPalette Palette)
        {
            TileFormatInfo FormatInfo = new TileFormatInfo(Format);
            if (Palette.Entries.Length < FormatInfo.PaletteSize) throw new ArgumentException("Palette is too small for the specified tile format");

            this.Format = Format;
            this.Palette = Palette;

            // set defaults
            this.tilesperrow = 6;

            this.tilemargin = 0;
            this.imagemargin = 0;

            this.ShowAddress = false;
            this.StartOffset = 0;
        }

        private int tilesperrow;
        public int TilesPerRow
        {
            get
            {
                return this.tilesperrow;
            }
            set
            {
                if (value < 1) value = 1;
                this.tilesperrow = value;
            }
        }

        private int tilemargin;
        public int TileMargin
        {
            get
            {
                return this.tilemargin;
            }
            set
            {
                if (value < 0) value = 0;
                this.tilemargin = value;
            }
        }

        public bool ShowAddress
        {
            get;
            set;
        }

        public uint StartOffset
        {
            get;
            set;
        }

        private int imagemargin;
        public int ImageMargin
        {
            get
            {
                return this.imagemargin;
            }
            set
            {
                if (value < 0) value = 0;
                this.imagemargin = value;
            }
        }

        public ColorPalette Palette
        {
            get;
            private set;
        }

        public TileFormats Format
        {
            get;
            private set;
        }
    }

    public class TileFormatInfo
    {
        public TileFormatInfo(TileFormats Format)
        {
            switch (Format)
            {
                case TileFormats.Famicom:
                    this.TileSize = 8;
                    this.BitDepth = 2;
                    this.Description = "Used by Nintendo Famicom";
                    break;
                case TileFormats.Gameboy:
                    this.TileSize = 8;
                    this.BitDepth = 2;
                    this.Description = "Used by Nintendo Gameboy and Super Famicom";
                    break;
                case TileFormats.Megadrive:
                    this.TileSize = 8;
                    this.BitDepth = 4;
                    this.Description = "Used by Sega MegaDrive and X68000 computer system";
                    break;
                case TileFormats.Monochrome:
                    this.TileSize = 8;
                    this.BitDepth = 1;
                    this.Description = "Used by many 2D based systems";
                    break;
                case TileFormats.NeoGeoPocket:
                    this.TileSize = 8;
                    this.BitDepth = 2;
                    this.Description = "Used by NeoGeo Pocket + Color";
                    break;
                case TileFormats.Sega_8bit:
                    this.TileSize = 8;
                    this.BitDepth = 4;
                    this.Description = "Used by Sega GameGear and Master System, and Bandai Wonderswan Color";
                    break;
                case TileFormats.SuperFamicom_3bpp:
                    this.TileSize = 8;
                    this.BitDepth = 3;
                    this.Description = "Used by Nintendo Super Famicom; a form of compression and not a native hardware format";
                    break;
                case TileFormats.SuperFamicom_4bpp:
                    this.TileSize = 8;
                    this.BitDepth = 4;
                    this.Description = "Used by Nintendo Super Famicom and NEC PC Engine";
                    break;
                case TileFormats.SuperFamicom_8bpp:
                    this.TileSize = 8;
                    this.BitDepth = 8;
                    this.Description = "Used by Nintendo Super Famicom";
                    break;
                case TileFormats.SuperFamicom_Mode7:
                    this.TileSize = 8;
                    this.BitDepth = 8;
                    this.Description = "Used by Nintendo Super Famicom";
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }
            this.Format = Format;
            this.ChunkSize = this.BitDepth * this.TileSize;
            this.PaletteSize = 255 >> (8 - this.BitDepth);
        }

        public int BitDepth
        {
            get;
            private set;
        }

        public int TileSize
        {
            get;
            private set;
        }

        public int PaletteSize
        {
            get;
            private set;
        }

        public TileFormats Format
        {
            get;
            private set;
        }

        public string Description
        {
            get;
            private set;
        }

        public int ChunkSize
        {
            get;
            private set;
        }
    }
}
