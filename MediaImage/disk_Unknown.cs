using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dumplib.Image
{
    class UnknownDisk : MediaImage
    {
        public UnknownDisk(string Filepath)
            : base(Filepath)
        {
            base.MediaType = MediaTypes.Disk;
            base.ReadWholeFile();
            base.SoftwareTitle = "Unknown disk";
        }
    }
}
