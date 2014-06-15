using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;

// CGRAM begins at 0x618 in ZST files

namespace dumplib.Gfx
{
    public interface IPaletteConverter
    {
        string ID
        {
            get;
        }

        string Description
        {
            get;
        }

        ColorPalette GetPalette(byte[] Data);
    }

    public static class CreatePalette
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
    }


    public static class LoadPalette
    {
        /// <summary>
        /// Gets a palette from a ZSNES savestate
        /// </summary>
        /// <param name="Filepath">Location of the savestate file</param>
        /// <returns>Standard color palette</returns>
        /// 

        public static ColorPalette From_Savestate_ZST(string Filepath)
        {
            if (string.IsNullOrEmpty(Filepath)) throw new ArgumentException("Invalid filepath");
            var cgram = new byte[512];
            var converter = new PaletteConverters.Nintendo_SuperFamicom_CGRAM();
            using (var _s = new System.IO.FileStream(Filepath, System.IO.FileMode.Open))
            {
                _s.Seek(0x618, System.IO.SeekOrigin.Begin);
                _s.Read(cgram, 0, 512);
            }
            return converter.GetPalette(cgram);
        }
/*
        public static ColorPalette From_Savestate_ZST(string Filepath)
        {
            var cgram = new byte[512];
            var converter = new PaletteConverters.Nintendo_SuperFamicom_CGRAM();
            using (var _s = new System.IO.FileStream(Filepath, System.IO.FileMode.Open))
            {
                _s.Seek(0x618,System.IO.SeekOrigin.Begin);
                _s.Read(cgram,0,512);
            }
            return converter.GetPalette(cgram);
        }*/

        /// <summary>
        /// Gets a palette from a Gens savestate
        /// </summary>
        /// <param name="Filepath">Location of the savestate file</param>
        /// <returns>Standard color palette</returns>
        public static ColorPalette From_Savestate_GSX(string Filepath)
        {
            if (string.IsNullOrEmpty(Filepath)) throw new ArgumentException("Invalid filepath");
            var cram = new byte[128];
            var converter = new PaletteConverters.Sega_Megadrive_CRAM();
            using (var _s = new System.IO.FileStream(Filepath, System.IO.FileMode.Open))
            {
                _s.Seek(0x11f78, System.IO.SeekOrigin.Begin);
                _s.Read(cram, 0, 128);
            }
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


    }
}
