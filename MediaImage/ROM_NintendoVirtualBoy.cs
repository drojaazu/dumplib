using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dumplib.Image
{
    public class NintendoVirtualBoy_ROM : MediaImage
    {
        private readonly static string HW_Worldwide = "Nintendo Virtual Boy";
        private readonly static string HW_JP = "任天堂　バーチャルボーイ";

        public string HardwareName_Worldwide
        {
            get
            {
                return NintendoVirtualBoy_ROM.HW_Worldwide;
            }
        }

        public string HardwareName_Japan
        {
            get
            {
                return NintendoVirtualBoy_ROM.HW_JP;
            }
        }

        public string HardwareName_JapanRomaji
        {
            get
            {
                return NintendoVirtualBoy_ROM.HW_Worldwide;
            }
        }

        public NintendoVirtualBoy_ROM(string Filepath)
            : base(Filepath)
        {
            base.MediaType = MediaTypes.ROM;
            base.HardwareName = NintendoVirtualBoy_ROM.HW_Worldwide;
            //base.ReadWholeFile();
            base.SoftwareTitle = GetText_SJIS(base.Datastream.Length - 544, 20);
        }
    }
}
