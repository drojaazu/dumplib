using System;
using System.IO;

namespace dumplib.Image
{
    public class UnknownImage : MediaImage
    {
        public UnknownImage(Stream Datastream, IDumpConverter Converter = null)
            : base(Datastream, Converter)
        {
            base.MediaType = MediaTypes.ROM;
            base.HardwareName = "Unknown";
            base.SoftwareTitle = "Unknown ROM";
        }
    }
}