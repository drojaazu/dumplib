using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace dumplib.Image
{
    class UnknownDisk : MediaImage
    {
        public UnknownDisk(Stream Datastream)
            : base(Datastream)
        {
            base.MediaType = MediaTypes.Disk;
            base.SoftwareTitle = "Unknown disk";
        }
    }
}
