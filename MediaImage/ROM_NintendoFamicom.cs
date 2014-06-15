using System;
using dumplib.Layout;

namespace dumplib.Image
{
    /// <summary>
    /// Nintendo Famicom / NES
    /// </summary>
    public class NintendoFamicom_ROM : MediaImage
    {

        private readonly static string HW_Worldwide = "Nintendo Entertainment System (NES)";
        private readonly static string HW_JP_R = "Nintendo Family Computer (Famicom)";
        private readonly static string HW_JP = "ニンテンドー　ファミリーコンピュータ(ファミコン)";

        public string HardwareName_Worldwide
        {
            get
            {
                return NintendoFamicom_ROM.HW_Worldwide;
            }
        }

        public string HardwareName_Japan
        {
            get
            {
                return NintendoFamicom_ROM.HW_JP;
            }
        }

        public string HardwareName_JapanRomaji
        {
            get
            {
                return NintendoFamicom_ROM.HW_JP_R;
            }
        }

        /*public Dump.Formats Format
        {
            get;
            private set;
        }*/

        private Dump.iNESHeader inesheader = null;

        public NintendoFamicom_ROM(string _file)
            : base(_file)
        {
            base.MediaType = MediaTypes.ROM;
            base.HardwareName = NintendoFamicom_ROM.HW_Worldwide;
            //base.ReadWholeFile();
            //this.Format = Dump.GetDumpType(base.Data);
            /*switch (this.Format)
            {
                case Dump.Formats.iNES:
                    this.inesheader = new Dump.iNESHeader(base.Data);
                    base.Data = Dump.Standardize(base.Data, this.Format);
                    break;
            }*/
            base.SoftwareTitle = "[Nintendo Famicom software]";
        }

        // function to add 'proper' iNES header

        /*public override ImageMap AutoMap()
        {
            var _out = base.AutoMap();
            if(this.inesheader != null){
                _out.Add(new DataChunkInfo(new Range(0,(uint)(16384*this.inesheader.PRG_Chunks)),"PRG ROM"));
                if (this.inesheader.CHR_Chunks != 0)
                    _out.Add(new DataChunkInfo(new Range((uint)(16384 * this.inesheader.PRG_Chunks), (uint)(8192 * this.inesheader.CHR_Chunks)), "CHR ROM"));
            }
            return _out;
        }*/

        static public class Dump
        {
            public class iNESHeader
            {
                public int PRG_Chunks
                {
                    get;
                    private set;
                }

                public int CHR_Chunks
                {
                    get;
                    private set;
                }

                public iNESHeader(byte[] Data)
                {
                    if (Data.Length < 16) throw new ArgumentOutOfRangeException("Data chunk cannot be less than 16 bytes");
                    this.PRG_Chunks = Data[4];
                    this.CHR_Chunks = Data[5];
                }
            }

            public enum Formats
            {
                RAW = 0,
                iNES
            }

            public static string GetDumpInfo(Formats DumpFormat)
            {
                switch (DumpFormat)
                {
                    case Formats.iNES:
                        return "iNES format - 16 byte header";
                    case Formats.RAW:
                        return "Raw format";
                    default:
                        return "Unknown dump format";
                }
            }

            public static Formats GetDumpType(byte[] Image)
            {
                // check for standard iNES header
                if (Image[0] == 0x4e && Image[1] == 0x45 && Image[2] == 0x53 && Image[3] == 0x1a) return Formats.iNES;
                return Formats.RAW;
            }

            public static byte[] Standardize(byte[] Image, Formats DumpFormat)
            {
                if (Image == null) throw new ArgumentNullException();
                byte[] _out;

                switch (DumpFormat)
                {
                    case Formats.iNES:
                        _out = new byte[Image.Length - 16];
                        Buffer.BlockCopy(Image, 16, _out, 0, _out.Length);
                        return _out;
                    default:
                        return Image;
                }
            }
        }
    }
}