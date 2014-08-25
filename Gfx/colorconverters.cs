using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace dumplib.Gfx.ColorConverters
{
    public class Sega_Megadrive : IColorConverter
    {

        public string ID
        {
            get
            {
                return "smd";
            }
        }

        public string Description
        {
            get
            {
                return "Sega Megadrive native color format";
            }
        }
        
        public Color GetColor(byte[] Data)
        {
            // 0000BBBBGGGGRRRR, linear
            int b = Data[0] & 0xf;
            int g = (Data[1] & 0xf0) >> 4;
            int r = Data[1] & 0xf;
            return Color.FromArgb(
                (r << 4) + r,
                (g << 4) + g,
                (b << 4) + b);
        }
    }

    public class Nintendo_SuperFamicom : IColorConverter
    {
        public string ID
        {
            get
            {
                return "sfc";
            }
        }

        public string Description
        {
            get
            {
                return "Nintendo Super Famicom native color format";
            }
        }

        public Color GetColor(byte[] Data)
        {
            // 0BBBBBGGGGGRRRRR, planar
            // and little endian! so in memory as:
            // GGGRRRRR0BBBBBGG
            // |||G lo bits  ||G hi bits
            return Color.FromArgb(
                ((Data[0] & 0x1f) * 255) / 31,
                ((((Data[0] & 0xe0) >> 5) + ((Data[1] & 0x3) << 3)) * 255) / 31,
                (((Data[1] & 0x7c) >> 2) * 255) / 31);
        }
    }

    public class Sega_MasterSystem : IColorConverter
    {
        public string ID
        {
            get
            {
                return "sms";
            }
        }

        public string Description
        {
            get
            {
                return "Sega Master System native color format";
            }
        }

        public Color GetColor(byte[] Data)
        {
            // 00BBGGRR
            //little endian but that don't matter with one byte now does it.. :V
            return Color.FromArgb(
                ((Data[0] & 3) * 255) / 3,
                (((Data[0] & 12) >> 2) * 255) / 3,
                (((Data[0] & 48) >> 4) * 255) / 3);
        }
    }

    public class Sega_GameGear : IColorConverter
    {
        public string ID
        {
            get
            {
                return "sgg";
            }
        }

        public string Description
        {
            get
            {
                return "Sega GameGear native color format";
            }
        }

        public Color GetColor(byte[] Data)
        {
            // 0000GGGGRRRRBBBB
            // little endian, so:
            // RRRRBBBB0000GGGG
            return Color.FromArgb(
                (((Data[0] & 0xf0) >> 4) * 255) / 7,
                ((Data[1] & 0xf) * 255) / 7,
                ((Data[0] & 0xf) * 255) / 7);
        }
    }
}