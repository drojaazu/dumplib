using System; 
using System.Drawing;
using System.Drawing.Imaging;

namespace dumplib.Gfx
{
    /// <summary>
    /// Interface for converting a graphic tile into a standard Bitmap (as a byte array)
    /// </summary>
    public interface ITileConverter
    {
        /// <summary>
        /// Description of this converter
        /// </summary>
        string Description
        {
            get;
        }

        /// <summary>
        /// Identifier for this converter
        /// </summary>
        string ID
        {
            get;
        }

        /// <summary>
        /// The graphics bitdepth (number of bits of data per pixel)
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
        /// The size of the expected chunk of raw data
        /// ( usually ((BitDepth * TileWidth) * TileHeight) / 8 )
        /// </summary>
        int ChunkSize
        {
            get;
        }

        /// <summary>
        /// Method to perform the conversion
        /// </summary>
        /// <param name="Data">Raw data to convert</param>
        /// <returns>Byte array to be converted to standard Bitmap object</returns>
        byte[] GetTile(byte[] Data);
    }

    public static class TileGfx
    {
        public static Bitmap GetTiles(byte[] TileData, ITileConverter Converter, ColorPalette Palette, int TilesPerRow)
        {
            //so when I first wrote this function I A) was significantly less experienced with C#/oop and
            // B) didn't comment it very well.
            // so here I am tearing it down to make it work with interfaces instead of the wacky sprawling delegate/multiple class solution from before
            // and I'm commenting it this time dammit!
            if (TileData == null || Palette == null || Converter == null)
                throw new ArgumentNullException();

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
}
