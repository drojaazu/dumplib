using System; 
//using System.Collections.Generic;
//using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
//using System.Linq;
//using System.Text;
//using System.Windows.Forms;

namespace dumplib.Gfx
{
    /*
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
    */

    public interface ITileConverter
    {
        /// <summary>
        /// A description of the tile format, such as its name and what systems implemented it
        /// </summary>
        string Description
        {
            get;
        }

        string ID
        {
            get;
        }

        /// <summary>
        /// The graphics bitdepth, which determines how many colors will be in the palette
        /// </summary>
        int Bitdepth
        {
            get;
        }

        /// <summary>
        /// The width of tile in pixels
        /// </summary>
        int TileWidth
        {
            get;
        }

        /// <summary>
        /// The height of the tile in pixels
        /// </summary>
        int TileHeight
        {
            get;
        }

        /// <summary>
        /// The size of the tile data in bytes
        /// </summary>
        int ChunkSize
        {
            get;
        }

        /// <summary>
        /// Method to convert the format
        /// </summary>
        /// <param name="TileData"></param>
        /// <returns></returns>
        byte[] GetTile(byte[] TileData);
    }

    public static class TileGfx
    {
        //private delegate byte[] TileMode(byte[] _in);

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
        /*public static Bitmap GetTiles(byte[] TileData, TileFormats TileFormat, ColorPalette Palette, int TilesPerRow)
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
        }*/

        public static Bitmap GetTiles(byte[] TileData, ITileConverter Converter, ColorPalette Palette, int TilesPerRow)
        {
            //so when I first wrote this function i was A) significantly less experienced with C#/oop and
            // B) I didn't comment it very well.
            // so here I am tearing it down to make it work with interfaces instead of the wacky sprawling delegate/multiple class solution from before
            // and I'm commenting it this time dammit!
            if (TileData == null || Palette == null || Converter == null)
                throw new ArgumentNullException();

            // delegate version is now obsolete with the interface method
            //TileMode GetTile = null;

            //GetTile = Converter.GetTile;

            //chunk = the byte array of source data
            byte[] chunk = new byte[Converter.ChunkSize];

            //we'll need to see if the supplied data is evenly divided by the chunksize
            // if it isn't, we'll pad it with 0's
            int leftover = TileData.Length % Converter.ChunkSize;
            if (leftover > 0)
            {
                byte[] newtiledata = new byte[TileData.Length + (Converter.ChunkSize - leftover)];
                Buffer.BlockCopy(TileData, 0, newtiledata, 0, TileData.Length);
                for (int c = TileData.Length; c < (newtiledata.Length - 1); c++)
                    newtiledata[c] = 0;
                TileData = newtiledata;
            }

            //array of output tile data
            // Data length / expected chunksize = total number of output tiles
            // 2nd dimension will hold the byte array for each individual tile
            byte[][] tiles = new byte[TileData.Length / Converter.ChunkSize][];

            // copy each chunk of tile data from the source data,
            // convert it, then put it into the tiles array
            for (int u = 0; u < tiles.Length; u++)
            {
                Buffer.BlockCopy(TileData, u * Converter.ChunkSize, chunk, 0, Converter.ChunkSize);
                //tiles[u] = GetTile(chunk);
                tiles[u] = Converter.GetTile(chunk);
            }

            // ? why did I null this?
            TileData = null;
            // so it clears up memory faster I guess?

            // determine the final width and height of the image, in pixels
            // this will be the width of each tile * tiles per row
            // therefore the output image will be the requested size even if there is not enough data to fill it
            int final_width = Converter.TileWidth * TilesPerRow;

            // is the number of tiles less than/eq to Tiles Per Row? then the height is the height of one tile (TileHeight)
            // more than/not eq to? height is number of rows (num tiles / Tiles Per Row) * height of a tile, + the height of one more row if there are extra tiles
            int final_height = tiles.Length <= TilesPerRow ? Converter.TileHeight : (tiles.Length / TilesPerRow) * Converter.TileHeight + ((tiles.Length % TilesPerRow) != 0 ? Converter.TileHeight : 0);


            //generate a new indexed bitmap then set the palette, which will be manually filled with our converted data
            var finalimg = new Bitmap(final_width, final_height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            finalimg.Palette = Palette;

            unsafe
            {
                // create BitmapData object by locking the final output data in memory for our use only
                BitmapData finalimgData = finalimg.LockBits(new Rectangle(0, 0, finalimg.Width, finalimg.Height), ImageLockMode.ReadWrite, finalimg.PixelFormat);
                IntPtr finalimgPtr = finalimgData.Scan0;
                
                // holds the output, standard bitmap data
                byte[] finalbytes = new byte[finalimgData.Height * finalimgData.Stride];
                for (int row = 0; row < finalbytes.Length; row++) finalbytes[row] = 0;

                // total number of rows
                int tilerows = finalimg.Height / Converter.TileHeight;
                
                // TileSize here is probably TileHeight
                //int tilerowbytesize = finalimgData.Stride * Format.TileSize;

                // size of each row in bytes
                // stride = size of a full scan line, in bytes
                int row_bytesize = finalimgData.Stride * Converter.TileHeight;
                int thisrow_byteoffset = 0;
                int tilecount = 0;

                if (TilesPerRow > tiles.Length) TilesPerRow = tiles.Length;
                int thisrow_numcolumns = TilesPerRow;

                // outer loop for counting rows
                for (int thisrow_num = 0; thisrow_num < tilerows; thisrow_num++)
                {
                    // loop for counting each scan line in the row
                    for (int thisscanline = 0; thisscanline < Converter.TileHeight; thisscanline++)
                    {
                        // loop for counting the line from each tile
                        for (int thiscolumn = 0; thiscolumn < thisrow_numcolumns; thiscolumn++)
                        {
                            // check tilecount here for left over
                            // /\ --- leftover what?
                            // ooooh probably leftover bytes, if a tile isn't 'complete'
                            Buffer.BlockCopy(tiles[TilesPerRow * thisrow_num + thiscolumn],
                                //thisscanline * Format.TileSize,
                                thisscanline * Converter.TileHeight,
                                finalbytes,
                                thisrow_byteoffset + (thisscanline * finalimgData.Stride) + (thiscolumn * Converter.TileWidth),
                                Converter.TileWidth);
                        }

                    }
                    tilecount += thisrow_numcolumns;
                    thisrow_byteoffset += row_bytesize;
                    if (tiles.Length - tilecount < TilesPerRow) thisrow_numcolumns = tiles.Length - tilecount;
                }

                System.Runtime.InteropServices.Marshal.Copy(finalbytes, 0, finalimgPtr, finalbytes.Length);
                finalimg.UnlockBits(finalimgData);
                return finalimg;
            }
        }


    }

    /*
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
    */

    /*
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
                case TileFormats.VirtualBoy:
                    this.TileSize = 8;
                    this.BitDepth = 2;
                    this.Description = "Used by the Nintendo VirtualBoy";
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
    }*/
}
