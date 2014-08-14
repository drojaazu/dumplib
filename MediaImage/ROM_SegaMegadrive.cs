using System;
using System.IO;

namespace dumplib.Image
{

    /// <summary>
    /// Sega Megadrive / Genesis
    /// </summary>
    public class SegaMegadrive_ROM : MediaImage
    {
        private readonly static string HW_Worldwide = "Sega Mega Drive";
        private readonly static string HW_NA = "Sega Genesis";
        private readonly static string HW_JP = "セガ　メガドライブ";

        public string HardwareName_Worldwide
        {
            get
            {
                return SegaMegadrive_ROM.HW_Worldwide;
            }
        }

        public string HardwareName_NorthAmerica
        {
            get
            {
                return SegaMegadrive_ROM.HW_NA;
            }
        }

        public string HardwareName_Japan
        {
            get
            {
                return SegaMegadrive_ROM.HW_JP;
            }
        }

        public string HardwareName_JapanRomaji
        {
            get
            {
                return SegaMegadrive_ROM.HW_Worldwide;
            }
        }

        public SegaMegadrive_ROM(Stream Datastream, IDumpConverter Converter = null)
            : base(Datastream, Converter)
        {
            this.Init();
        }

        private void Init()
        {
            base.HardwareName = SegaMegadrive_ROM.HW_Worldwide;
            base.MediaType = MediaTypes.ROM;
            Setup();
        }

        public string SoftwareTitle_Domestic
        {
            get;
            private set;
        }

        public string SoftwareTitle_International
        {
            get;
            private set;
        }

        public string SoftwareDeveloper
        {
            get;
            private set;
        }

        public Dump.Formats DumpFormat
        {
            get;
            private set;
        }

        private void Setup()
        {
            //base.ReadWholeFile();
            //this.DumpFormat = Dump.GetDumpFormat(base.Data);
            /*switch (this.DumpFormat)
            {
                case Dump.Formats.SMD:
                    base.Data = Dump.Standardize(base.Data, this.DumpFormat);
                    this.AddComment("Converted from interleaved Super MagicDrive format");
                    break;
            }*/

            this.SoftwareTitle_International = Text.Transcode.UsingASCII(GetBytes(0x150, 48)).Trim();
            this.SoftwareTitle_Domestic = Text.Transcode.UsingSJIS(GetBytes(0x120, 48)).Trim();
            base.SoftwareTitle = this.SoftwareTitle_International == string.Empty ? this.SoftwareTitle_Domestic : this.SoftwareTitle_International;
            this.SoftwareDeveloper = Text.Transcode.UsingASCII(GetBytes(0x113, 5));
        }

        static public class Dump
        {
            public enum Formats
            {
                RAW = 0,
                SMD
            }

            static public Formats GetDumpFormat(byte[] Image)
            {
                if (Image == null) throw new ArgumentNullException();
                // Megadrive ROM sizes vary from 1Mb (128kb) to ~40Mb (5MB), so we can't just check for an 'extra' 512k like the SFC
                // First we'll check for Super MagicDrive: check the three byte signature at 0x01 (03) 0x08 (AA) 0x09 (BB) 0x0A (06)
                if (Image[1] != 3 && Image[8] != 0xaa && Image[9] != 0xbb && Image[10] != 6) return Formats.RAW;
                // then, to be sure, check that all bytes from 0x0B to 0x01FF are zero
                for (uint t = 11; t < 0x200; t++)
                    if (Image[t] != 0) return Formats.RAW;

                // Appears to be an interleaved Super MagicDrive dump
                return Formats.SMD;
            }

            public static string GetDumpInfo(Formats DumpFormat)
            {
                switch (DumpFormat)
                {
                    case Formats.SMD:
                        return "Super MagicDrive format - Interleaved, 512 byte header";
                    case Formats.RAW:
                        return "Raw format";
                    default:
                        return "Unknown dump format";
                }
            }

            static public byte[] Standardize(byte[] Image, Formats DumpFormat)
            {
                if (Image == null) throw new ArgumentNullException();

                switch (DumpFormat)
                {
                    case Formats.SMD:
                        // Assume this data has already been verified as a super magicdrive dump
                        // so start by ignoring the 512 byte header, then divide the remainder by 16k chunks
                        // (check for modulus to see if the rom is an odd size)
                        //if(Data.Length % 0x4000 != 0)  //modulus is not zero, file is an unexpected size, add a comment
                        byte[] _out = new byte[Image.Length - 0x200];
                        int totalchunks = Image.Length / 0x4000;
                        // outer loop through the total number of 16k chunks
                        var thischunk = new byte[0x4000];
                        int outoffset = 0, outcount = 0;
                        for (int chunkloop = 0; chunkloop < totalchunks; chunkloop++)
                        {
                            Buffer.BlockCopy(Image, 0x200 + (chunkloop * 0x4000), thischunk, 0, 0x4000);
                            outoffset = chunkloop * 0x4000;
                            outcount = 0;
                            // decode the 16k block: the odd digit is from the low end of the block, the even digit is from the high end
                            for (int deint = 0; deint < 0x2000; deint++)
                            {
                                _out[outoffset + outcount + 1] = thischunk[deint];
                                _out[outoffset + outcount] = thischunk[deint + 0x2000];
                                outcount += 2;
                            }
                        }
                        return _out;
                    default:
                        return Image;
                }
            }
        }
    }
}