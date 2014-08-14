using System;
using System.IO;

namespace dumplib.Image
{
    public class NintendoGameboyAdv_ROM : MediaImage
    {
        private readonly static string HW_Worldwide = "Nintendo Game Boy Advance";
        private readonly static string HW_JP = "ニンテンドー　ゲームボーイアドバンス";

        public string HardwareName_Worldwide
        {
            get
            {
                return NintendoGameboyAdv_ROM.HW_Worldwide;
            }
        }

        public string HardwareName_Japan
        {
            get
            {
                return NintendoGameboyAdv_ROM.HW_JP;
            }
        }

        public string HardwareName_JapanRomaji
        {
            get
            {
                return NintendoGameboyAdv_ROM.HW_Worldwide;
            }
        }

        public NintendoGameboyAdv_ROM(Stream Datastream, IDumpConverter Converter = null)
            : base(Datastream, Converter)
        {
            this.Init();
        }

        private void Init()
        {
            base.MediaType = MediaTypes.ROM;
            base.HardwareName = NintendoGameboyAdv_ROM.HW_Worldwide;
            base.SoftwareTitle = Text.Transcode.UsingASCII(GetBytes(0xa0, 12));
        }
    }
}