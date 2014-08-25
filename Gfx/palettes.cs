using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace dumplib.Gfx
{
    /// <summary>
    /// Interface for converting a unique color palette to a standard system ColorPalette object
    /// </summary>
    public interface IPaletteConverter
    {
        /// <summary>
        /// Identifier for this converter
        /// </summary>
        string ID
        {
            get;
        }

        /// <summary>
        /// Description of this converter
        /// </summary>
        string Description
        {
            get;
        }

        /// <summary>
        /// Method to perform the conversion
        /// </summary>
        /// <param name="Data">Raw data to convert</param>
        /// <returns>Standard ColorPalette object</returns>
        ColorPalette GetPalette(byte[] Data);
    }

    public static class GetPalette
    {

        /// <summary>
        /// Gets a new 1-bit palette (2 colors)
        /// </summary>
        /// <param name="Blank">If true, the final palette entries will have 0 opacity</param>
        /// <returns>Standard color palette</returns>
        public static ColorPalette New_1bit(bool Blank = false)
        {
            var _out = new Bitmap(1, 1, PixelFormat.Format1bppIndexed).Palette;
            if (!Blank) return _out;
            _out.Entries[0] = Color.FromArgb(0, 0, 0, 0);
            _out.Entries[1] = Color.FromArgb(0, 0, 0, 0);
            return _out;
        }

        /// <summary>
        /// Gets a new 4-bit palette (16 colors)
        /// </summary>
        /// <param name="Blank">If true, the final palette entries will have 0 opacity</param>
        /// <returns></returns>
        public static ColorPalette New_4bit(bool Blank = false)
        {
            var _out = new Bitmap(1, 1, PixelFormat.Format4bppIndexed).Palette;
            if (!Blank) return _out;
            for (int t = 0; t < 16; t++) _out.Entries[t] = Color.FromArgb(0, 0, 0, 0);
            return _out;
        }

        /// <summary>
        /// Gets a new 8-bit palette (256 colors)
        /// </summary>
        /// <param name="Blank">If true, the final palette entries will have 0 opacity</param>
        /// <returns></returns>
        public static ColorPalette New_8bit(bool Blank = false)
        {
            var _out = new Bitmap(1, 1, PixelFormat.Format8bppIndexed).Palette;
            if (!Blank) return _out;
            for (int t = 0; t < 256; t++) _out.Entries[t] = Color.FromArgb(0, 0, 0, 0);
            return _out;
        }

        /// <summary>
        /// Gets a palette from a ZSNES savestate
        /// </summary>
        /// <param name="Filepath">Location of the savestate file</param>
        /// <returns>Standard color palette</returns>
        public static ColorPalette From_Savestate_ZST(string Filepath)
        {
            if (string.IsNullOrEmpty(Filepath)) throw new ArgumentException("Invalid filepath");
            using (var fs = new System.IO.FileStream(Filepath, System.IO.FileMode.Open))
            {
                return From_Savestate_ZST(fs);
            }
        }

        /// <summary>
        /// Gets a palette from a ZSNES savestate
        /// </summary>
        /// <param name="DataStream">Stream containing the savestate data</param>
        /// <returns>Standard color palette</returns>
        public static ColorPalette From_Savestate_ZST(Stream DataStream)
        {
            if (DataStream == null) throw new ArgumentNullException();
            var cgram = new byte[512];
            var converter = new PaletteConverters.Nintendo_SuperFamicom_CGRAM();

            DataStream.Seek(0x618, System.IO.SeekOrigin.Begin);
            DataStream.Read(cgram, 0, 512);
            return converter.GetPalette(cgram);
        }

        /// <summary>
        /// Gets a palette from a Gens savestate
        /// </summary>
        /// <param name="Filepath">Location of the savestate file</param>
        /// <returns>Standard color palette</returns>
        public static ColorPalette From_Savestate_GSX(string Filepath)
        {
            if (string.IsNullOrEmpty(Filepath)) throw new ArgumentException("Invalid filepath");
            using (var fs = new System.IO.FileStream(Filepath, System.IO.FileMode.Open))
            {
                return From_Savestate_GSX(fs);
            }
        }

        /// <summary>
        /// Gets a palette from a Gens savestate
        /// </summary>
        /// <param name="DataStream">Stream containing the savestate data</param>
        /// <returns>Standard color palette</returns>
        public static ColorPalette From_Savestate_GSX(Stream DataStream)
        {
            if (DataStream == null) throw new ArgumentNullException();
            var cram = new byte[128];
            var converter = new PaletteConverters.Sega_Megadrive_CRAM();
            DataStream.Seek(0x11f78, System.IO.SeekOrigin.Begin);
            DataStream.Read(cram, 0, 128);
            return converter.GetPalette(cram);
        }

        /// <summary>
        /// Gets a palette from a TileLayer palette
        /// </summary>
        /// <param name="Filepath">Location of the savestate file</param>
        /// <returns>Standard color palette</returns>
        public static ColorPalette From_TilelayerPalette(string Filepath)
        {
            if (string.IsNullOrEmpty(Filepath)) throw new ArgumentException("Invalid filepath");
            var converter = new PaletteConverters.TileLayerPro();
            return converter.GetPalette(System.IO.File.ReadAllBytes(Filepath));
        }

        /// <summary>
        /// Gets a palette from a TileLayer palette
        /// </summary>
        /// <param name="DataStream">Stream containing the palette data</param>
        /// <returns>Standard color palette</returns>
        public static ColorPalette From_TilelayerPalette(Stream DataStream)
        {
            if (DataStream == null) throw new ArgumentNullException();
            var converter = new PaletteConverters.TileLayerPro();
            var alldata = new byte[DataStream.Length];
            DataStream.Read(alldata, 0, alldata.Length);
            return converter.GetPalette(alldata);
        }
    }
}
