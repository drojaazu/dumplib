using System;

namespace dumplib.Image
{

    // --------------------------------------------------- NGB_ROM (Nintendo GameBoy image)
    public class NintendoGameboy_ROM : MediaImage
    {
        private readonly static string HW_Worldwide = "Nintendo Game Boy";
        private readonly static string HW_JP = "任天堂　ゲームボーイ";

        public string HardwareName_Worldwide
        {
            get
            {
                return NintendoGameboy_ROM.HW_Worldwide;
            }
        }

        public string HardwareName_Japan
        {
            get
            {
                return NintendoGameboy_ROM.HW_JP;
            }
        }

        public string HardwareName_JapanRomaji
        {
            get
            {
                return NintendoGameboy_ROM.HW_Worldwide;
            }
        }

        public enum SoftwareRegions : byte
        {
            Japan = 0,
            International = 1
        }

        public enum SoftwareColorMode : byte
        {
            CGBandMono = 0x80,
            CGBonly = 0xc0
        }

        public SoftwareRegions SoftwareRegion
        {
            get;
            private set;
        }

        public NintendoGameboy_ROM(string Filepath)
            : base(Filepath)
        {
            base.MediaType = MediaTypes.ROM;
            base.HardwareName = NintendoGameboy_ROM.HW_Worldwide;
            //base.ReadWholeFile();
            //base.System = Systems.NGB;
            SetupHeader();
        }

        protected void SetupHeader()
        {
            // the software title is stored as an ASCII string, from 0x134-0x143 on older carts, and 0x134-0x13e on newer carts
            // if 0x143 contains 0x80 or 0xc0, which are common CGB flags, use the short length
            int titlelength = 16;
            if (GetByte(0x143) == 0x80 | GetByte(0x143) == 0xC0) titlelength = 10;
            this.SoftwareRegion = (SoftwareRegions)GetByte(0x14a);
            base.SoftwareTitle = Text.Transcode.UsingASCII(GetBytes(0x0134, titlelength)).Trim();
        }
    }
}