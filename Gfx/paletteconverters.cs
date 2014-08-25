using System;
using System.Drawing;
using System.Drawing.Imaging;
using dumplib.Gfx;

namespace dumplib.Gfx.PaletteConverters
{
    public class Sega_Megadrive_CRAM : IPaletteConverter
    {
        public string ID
        {
            get
            {
                return "smd_cram";
            }
        }

        public string Description
        {
            get
            {
                return "Palette data stored in the Megadrive color memory (CRAM)";
            }
        }

        private static ColorConverters.Sega_Megadrive ConvertColor = new ColorConverters.Sega_Megadrive();

        public ColorPalette GetPalette(byte[] Data)
        {
            if (Data.Length < 128) throw new ArgumentOutOfRangeException("Sega Megadrive CRAM palette must be at least 128 bytes");

            var _out = Gfx.GetPalette.New_8bit(true);

            for (int t = 0, entrycount = 0; t < 128; t += 2, entrycount++)
                _out.Entries[entrycount] = ConvertColor.GetColor(new byte[2] { Data[t], Data[t + 1] });
            return _out;
        }
    }

    public class Nintendo_SuperFamicom_CGRAM : IPaletteConverter
    {
        public string ID
        {
            get
            {
                return "sfc_cgram";
            }
        }

        public string Description
        {
            get
            {
                return "Palette data stored in the Super Famicom color memory (CGRAM)";
            }
        }

        private static ColorConverters.Nintendo_SuperFamicom ConvertColor = new ColorConverters.Nintendo_SuperFamicom();

        public ColorPalette GetPalette(byte[] Data)
        {
            if (Data.Length < 512) throw new ArgumentOutOfRangeException("Nintendo Super Famicom palette must be at least 512 bytes");

            var _out = Gfx.GetPalette.New_8bit(true);
            for (int t = 0, entrycount = 0; t < 512; t += 2, entrycount++)
                _out.Entries[entrycount] = ConvertColor.GetColor(new byte[2] { Data[t], Data[t + 1] });
            return _out;
        }
    }

    public class TileLayerPro : IPaletteConverter
    {
        public string ID
        {
            get
            {
                return "tlp";
            }
        }

        public string Description
        {
            get
            {
                return "Palette data stored in TPL (TileLayer Pro) files";
            }
        }
        private static ColorConverters.Nintendo_SuperFamicom ConvertColor = new ColorConverters.Nintendo_SuperFamicom();

        public ColorPalette GetPalette(byte[] Data)
        {
            if (Data.Length < 4 || Data[0] != 0x54 || Data[1] != 0x50 || Data[2] != 0x4c)
            {
                throw new ArgumentException("Invalid TileLayer palette data");
            }

            // mode: 0 = RGB, 1 = NES, 2 = SNES/GBC/GBA
            // to do - check that there are base-16 number of entries up to 256 in the tpl file

            var _out = Gfx.GetPalette.New_8bit();
            int palindex = 0;

            switch (Data[3])
            {
                case 0:
                    if ((Data.Length - 4) / 3 > 256)
                    {
                        throw new IndexOutOfRangeException("Invalid number of palette entries in TileLayer palette data (RGB format)");
                    }
                    for (int t = 4; t < Data.Length; t += 3)
                    {
                        _out.Entries[palindex] = Color.FromArgb((int)Data[t], (int)Data[t + 1], (int)Data[t + 2]);
                        palindex++;
                    }
                    break;
                case 1:
                    throw new NotImplementedException("NES formatted TileLayer palettes not supported right now");
                case 2:
                    if ((Data.Length - 4) / 2 > 256)
                    {
                        throw new IndexOutOfRangeException("Invalid number of palette entries in TileLayer palette data (SNES format)");
                    }
                    for (int t = 4; t < Data.Length; t += 2)
                    {
                        _out.Entries[palindex] = ConvertColor.GetColor(new byte[2] { Data[t], Data[t + 1] });
                        palindex++;
                    }
                    break;
            }
            return _out;
        }
    }
}