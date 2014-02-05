using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dumplib.Image
{
    public class NintendoVirtualBoy_ROM : MediaImage
    {
        public NintendoVirtualBoy_ROM(string Filepath)
            : base(Filepath)
        {
            base.MediaType = MediaTypes.ROM;
            base.ReadWholeFile();
            base.SoftwareTitle = GetText_SJIS((uint)base.Data.LongLength - 544, 20);
        }
    }
}
