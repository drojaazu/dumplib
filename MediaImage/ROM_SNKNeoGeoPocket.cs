using System;

namespace dumplib.Image
{
    public class SNKNeoGeoPocket_ROM : MediaImage
    {
        public SNKNeoGeoPocket_ROM(string Filepath)
            : base(Filepath)
        {
            base.MediaType = MediaTypes.ROM;
            base.ReadWholeFile();

            base.SoftwareTitle = GetText_ASCII(0x24, 12);
        }
    }
}
