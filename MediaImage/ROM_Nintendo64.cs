using System;
using System.IO;

namespace dumplib.Image
{
    /// <summary>
    /// Nintendo 64
    /// </summary>
    public class Nintendo64_ROM : MediaImage
    {
        private readonly static string HW_Worldwide = "Nintendo 64";
        private readonly static string HW_JP = "ニンテンドー64";

        public string HardwareName_Worldwide
        {
            get
            {
                return Nintendo64_ROM.HW_Worldwide;
            }
        }

        public string HardwareName_Japan
        {
            get
            {
                return Nintendo64_ROM.HW_JP;
            }
        }

        public string HardwareName_JapanRomaji
        {
            get
            {
                return Nintendo64_ROM.HW_Worldwide;
            }
        }

        public Nintendo64_ROM(string _file)
            : base(_file)
        {
            base.MediaType = MediaTypes.ROM;
            base.HardwareName = Nintendo64_ROM.HW_Worldwide;
            //base.ReadWholeFile();
            //this.DumpType = Dump.GetDumpFormat(this.Data);
            // dumplib uses the Z64 format as 'baseline' as it is PC readable (little endian)
            /*switch (this.DumpType)
            {
                case Dump.Formats.N64:
                    this.Data = Dump.Standardize(this.Data, this.DumpType);
                    break;
                case Dump.Formats.V64:
                    this.Data = Dump.Standardize(this.Data, this.DumpType);
                    break;
            }*/
            base.SoftwareTitle = Text.Transcode.UsingASCII(GetBytes(0x20, 20)).Trim();
        }

        public Dump.Formats DumpType
        {
            get;
            private set;
        }


        public static class Dump
        {
            public enum Formats
            {
                V64 = 1,
                Z64,
                N64
            }

            public static string GetDumpInfo(Formats DumpFormat)
            {
                switch (DumpFormat)
                {
                    case Formats.V64:
                        return "Doctor V64 format - Big-endian, byte-swapped, no header";
                    case Formats.N64:
                        return "CD64 format - Little-endian, no header";
                    case Formats.Z64:
                        return "Mr. Backup Z64 format - Big-endian, no header";
                    default:
                        return "Unknown dump format";
                }
            }

            public static Formats GetDumpFormat(byte[] Image)
            {
                if (Image == null) throw new ArgumentNullException();
                if (Image[0] == 0x37) return Formats.V64;
                if (Image[0] == 0x40) return Formats.N64;
                //if (Data.GetByte(0) == 0x80) return N64_CopierFormats.Z64;
                return Formats.Z64;
            }

            public static byte[] Standardize(byte[] Image, Formats DumpFormat)
            {
                if (Image == null) throw new ArgumentNullException();
                byte[] _out;
                byte[] swap;
                switch (DumpFormat)
                {
                    case Formats.N64:
                        _out = Image;
                        swap = new byte[4];
                        for (int i = 0; i < _out.Length; i += 4)
                        {
                            for (int t = 0; t < 4; t++) swap[t] = _out[i + t];
                            Image[i] = swap[3];
                            Image[i + 1] = swap[2];
                            Image[i + 2] = swap[1];
                            Image[i + 3] = swap[0];
                        }
                        return _out;
                    case Formats.V64:
                        _out = Image;
                        swap = new byte[2];
                        for (int i = 0; i < _out.Length; i += 2)
                        {
                            swap[0] = _out[i]; swap[1] = _out[i + 1];
                            _out[i] = swap[1]; _out[i + 1] = swap[0];
                        }
                        return _out;
                    default:
                        return Image;
                }
            }
        }
    }
}