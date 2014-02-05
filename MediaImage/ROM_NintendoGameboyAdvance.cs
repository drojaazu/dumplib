using System;

namespace dumplib.Image
{
    public class NintendoGameboyAdv_ROM : MediaImage
    {
        public NintendoGameboyAdv_ROM(string Filepath)
            : base(Filepath)
        {
            base.MediaType = MediaTypes.ROM;
            base.ReadWholeFile();
            base.SoftwareTitle = Text.GetText.UsingASCII(GetBytes(0xa0, 12));
        }

    }
}