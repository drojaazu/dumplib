using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;

// CGRAM begins at 0x618 in ZST files

namespace dumplib.Gfx
{
    /// <summary>
    /// Utilities to convert color formats
    /// </summary>
    public static class GetColor
    {
        /// <summary>
        /// Converts a Sega Megadrive color
        /// </summary>
        /// <param name="Data">Source color value</param>
        /// <returns>Standard color</returns>
        public static Color From_SMD(short Data)
        {
            // 0000BBBBGGGGRRRR, linear
            int b = (Data & 0xf00) >> 8;
            int g = (Data & 0xf0) >> 4;
            int r = Data & 0xf;
            return Color.FromArgb(
                (r << 4) + r,
                (g << 4) + g,
                (b << 4) + b);
        }

        /// <summary>
        /// Converts a Nintendo Super Famicom color
        /// </summary>
        /// <param name="Data">Source color value</param>
        /// <returns>Standard color</returns>
        public static Color From_SFC(short Data)
        {
            // 0BBBBBGGGGGRRRRR, planar
            return Color.FromArgb(
                ((Data & 0x1f) * 255) / 31,
                (((Data & 0x3e0) >> 5) * 255) / 31,
                (((Data & 0x7c00) >> 10) * 255) / 31);
        }

        /// <summary>
        /// Converts a Sega Master System color
        /// </summary>
        /// <param name="Data">Source color value</param>
        /// <returns>Standard color</returns>
        public static Color From_SMS(byte Data)
        {
            return Color.FromArgb(
                ((Data & 3) * 255) / 3,
                (((Data & 12) >> 2) * 255) / 3,
                (((Data & 48) >> 4) * 255) / 3);
        }

        /// <summary>
        /// Converts a Sega GameGear color
        /// </summary>
        /// <param name="Data">Source color value</param>
        /// <returns>Standard color</returns>
        public static Color From_SGG(short Data)
        {
            return Color.FromArgb(
                (((Data & 0xf0) >> 4) * 255) / 7,
                (((Data & 0xf00) >> 8) * 255) / 7,
                ((Data & 0xf) * 255) / 7);
        }
    }

    public static class GetPalette
    {
        /// <summary>
        /// Gets a palette from a ZSNES savestate
        /// </summary>
        /// <param name="Filepath">Location of the savestate file</param>
        /// <returns>Standard color palette</returns>
        public static ColorPalette From_Savestate_ZST(string Filepath)
        {
            var cgram = new byte[512];
            using (var _s = new System.IO.FileStream(Filepath, System.IO.FileMode.Open))
            {
                _s.Seek(0x618,System.IO.SeekOrigin.Begin);
                _s.Read(cgram,0,512);
            }
            return From_SFC_CGRAM(cgram);
        }

        /// <summary>
        /// Gets a palette from a Gens savestate
        /// </summary>
        /// <param name="Filepath">Location of the savestate file</param>
        /// <returns>Standard color palette</returns>
        public static ColorPalette From_Savestate_GSX(string Filepath)
        {
            var cram = new byte[128];
            using (var _s = new System.IO.FileStream(Filepath, System.IO.FileMode.Open))
            {
                _s.Seek(0x11f78, System.IO.SeekOrigin.Begin);
                _s.Read(cram, 0, 128);
            }
            return From_MD_CRAM(cram);
        }

        /// <summary>
        /// Gets a palette from a TileLayer palette
        /// </summary>
        /// <param name="Filepath">Location of the savestate file</param>
        /// <returns>Standard color palette</returns>
        public static ColorPalette From_TilelayerPalette(string Filepath)
        {
            var data = System.IO.File.ReadAllBytes(Filepath);
            if (data.Length < 4 || data[0] != 0x54 || data[1] != 0x50 || data[2] != 0x4c)
            {
                //Log.Error("(LoadFromTPL) File is not a valid TileLayer palette");
                return null;
            }
            
            // mode: 0 = RGB, 1 = NES, 2 = SNES/GBC/GBA
            // to do - check that there are base-16 number of entries up to 256 in the tpl file

            var _out = New_8bit();
            int palindex = 0;

            switch (data[3])
            {
                case 0:
                    if ((data.Length - 4) / 3 > 256)
                    {
                        //Log.Error("(LoadFromTPL) Incorrect number of palette entries (RGB format)");
                        return null;
                    }
                    for (int t = 4; t < data.Length; t += 3)
                    {
                        _out.Entries[palindex] = Color.FromArgb((int)data[t], (int)data[t + 1], (int)data[t + 2]);
                        palindex++;
                    }
                    break;
                case 1:
                    throw new Exception("LoadTPL: no NES format support right now");
                case 2:
                    if ((data.Length - 4) / 2 > 256)
                    {
                        //Log.Error("(LoadFromTPL) Incorrect number of palette entries (SFC format)");
                        return null;
                    }
                    for (int t = 4; t < data.Length; t += 2)
                    {
                        _out.Entries[palindex] = GetColor.From_SFC(BitConverter.ToInt16(data, t));
                        palindex++;
                    }
                    break;
            }
            return _out;
        }

        /// <summary>
        /// Gets a palette from Sega Megadrive CRAM
        /// </summary>
        /// <param name="Data">Array of bytes from CRAM</param>
        /// <returns>Standard color palette</returns>
        public static ColorPalette From_MD_CRAM(byte[] Data)
        {
            if (Data.Length != 128)
            {
                //Log.Error("(LoadMD_CRAM) Megadrive CRAM chunk must be 128 bytes");
                return null;
            }

            var _out = New_8bit(true);
            
            for (int t = 0; t < 64; t++)
                _out.Entries[t] = GetColor.From_SMD((short)((Data[t * 2] << 8) + Data[(t * 2) + 1]));
            return _out;
        }

        /// <summary>
        /// Gets a palette from Super Famicom CGRAM
        /// </summary>
        /// <param name="Data">Array of bytes from CGRAM</param>
        /// <returns>Standard color palette</returns>
        public static ColorPalette From_SFC_CGRAM(byte[] Data)
        {
            if (Data.Length != 512)
            {
                //Log.Error("Super Famicom CGRAM chunk must be 512 bytes");
                return null;
            }
            
            var _out = New_8bit();

            for (int t = 0; t < 256; t++)
                _out.Entries[t] = GetColor.From_SFC((short)(Data[t * 2] + (Data[(t * 2) + 1] << 8)));
            return _out;
        }

        /// <summary>
        /// Gets a new 1-bit palette (2 colors)
        /// </summary>
        /// <param name="Blank">If true, the final palette entries will have 0 opacity</param>
        /// <returns>Standard color palette</returns>
        public static ColorPalette New_1bit(bool Blank=false)
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
        public static ColorPalette New_4bit(bool Blank=false)
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
        public static ColorPalette New_8bit(bool Blank=false)
        {
            var _out = new Bitmap(1, 1, PixelFormat.Format8bppIndexed).Palette;
            if(!Blank) return _out;
            for (int t = 0; t < 256; t++) _out.Entries[t] = Color.FromArgb(0,0,0,0);
            return _out;
        }
    }
}
