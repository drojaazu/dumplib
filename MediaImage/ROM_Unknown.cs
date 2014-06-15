using System;

namespace dumplib.Image
{
    public class UNK_ROM : MediaImage
    {
        public UNK_ROM(string _file)
            : base(_file)
        {
            base.MediaType = MediaTypes.ROM;
            base.HardwareName = "Unknown";
            //base.ReadWholeFile();
            base.SoftwareTitle = "Unknown ROM";
        }
    }
}