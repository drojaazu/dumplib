using System;

namespace dumplib.Image
{

    // --------------------------------------------------- NGB_ROM (Nintendo GameBoy image)
    public class NGB_ROM : MediaImage
    {
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

        public NGB_ROM(string Filepath)
            : base(Filepath)
        {
            base.MediaType = MediaTypes.ROM;
            base.ReadWholeFile();
            //base.System = Systems.NGB;
            SetupHeader();
        }

        protected void SetupHeader()
        {
            // the software title is stored as an ASCII string, from 0x134-0x143 on older carts, and 0x134-0x13e on newer carts
            // if 0x143 contains 0x80 or 0xc0, which are common CGB flags, use the short length
            uint titlelength = 16;
            if (GetByte(0x143) == 0x80 | GetByte(0x143) == 0xC0) titlelength = 10;
            this.SoftwareRegion = (SoftwareRegions)GetByte(0x14a);
            base.SoftwareTitle = Text.GetText.UsingASCII(GetBytes(0x0134, titlelength)).Trim();
        }
    }
}