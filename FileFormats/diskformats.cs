using System;
using ROMlib.Image;

namespace ROMlib.Image.Formats
{
    static public class FDS_CopyDevices
    {
        public enum Formats
        {
            RAW = 0,
            FDS,
            FAM
        }

        public static Formats GetDumpFormat(byte[] Image)
        {
            // check for FDS first
            if (Image[0] == 0x46 && Image[1] == 0x44 && Image[2] == 0x53 && Image[3] == 0x1a) return Formats.FDS;
            // the FAM format doesn't seem to be well understood
            // There is always a 0xf180 length header followed by standard FDS data
            // check for the block markers after 0xf180
            if (Image[0xf180] == 1 && Image[0xf1b8] == 2 && Image[0xf1ba] == 3) return Formats.FAM;

            return Formats.RAW;
        }
    }
}