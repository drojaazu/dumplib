using System;
using InOut = System.IO;
using System.Text;
using dumplib.Gfx;
using dumplib.Layout;

namespace dumplib.Image
{
    /// <summary>
    /// Nintendo Super Famicom / Super NES
    /// </summary>
    public class NintendoSuperFamicom_ROM : MediaImage
    {
        /// <summary>
        /// The Super Famicom uses two addressing modes based on the size of each ROM bank: LoROM refers to bank size of 32k and HiROM to 64k
        /// </summary>
        public enum ROMBankSizes : uint
        {
            LoROM = 0x8000,
            HiROM = 0x10000
        }

        public CRTDisplayType SoftwareRegionVideoType
        {
            get;
            private set;
        }

        /// <summary>
        /// The size of each ROM bank
        /// </summary>
        public ROMBankSizes ROMBankSize
        {
            get;
            private set;
        }

        public byte[] SoftwareHeader
        {
            get;
            private set;
        }

        /// <summary>
        /// The format of the ROM dump
        /// </summary>
        public Dump.Formats ImageFormat
        {
            get;
            private set;
        }

        // --------------------------------------------------- constructor

        /// <summary>
        /// Creates a new Super Famicom ROM image
        /// </summary>
        /// <param name="Filepath">Location of the ROM file</param>
        public NintendoSuperFamicom_ROM(string Filepath)
            : base(Filepath)
        {
            base.MediaType = MediaTypes.ROM;
            ReadWholeFile();
            Setup(Filepath);
        }

        private void Setup(string Filepath)
        {
            base.GfxDefaultPixelFormat = TileFormats.SuperFamicom_4bpp;
            SoftwareHeader = new byte[0x40];

            this.ImageFormat = Dump.GetDumpType(this.Data);
            if (this.ImageFormat != Dump.Formats.RAW) base.Data = Dump.Standardize(base.Data, this.ImageFormat);

            CheckLayout();
            SetupHeader();

            //Log.Message("(SFC_ROM) Opened Nintendo Super Famicom ROM: " + this.Title + " (bank size $" + this.BankSize.ToString("X") + ")");
        }

        // --------------------------------------------------- Methods
        private void SetupHeader()
        {
            Buffer.BlockCopy(Data, (int)this.ROMBankSize - 0x40, this.SoftwareHeader, 0, 0x40);
            base.SoftwareTitle = ASCIIEncoding.ASCII.GetString(this.SoftwareHeader, 0, 21).Trim();
            if (this.GetByte(0xd9) > 1 & this.GetByte(0xd9) < 0x0d) this.SoftwareRegionVideoType = CRTDisplayType.PAL;
            else if (this.GetByte(0xd9) > 0x0d) this.SoftwareRegionVideoType = CRTDisplayType.Unknown;
            else this.SoftwareRegionVideoType = CRTDisplayType.NTSC;
        }

        private void CheckLayout()
        {
            // quick patch: if the file size is less than 64k, assume it is lo-rom
            if (this.Data.Length < 0x10000)
            {
                this.ROMBankSize = ROMBankSizes.LoROM;
                return;
            }
            // determines bank size of the ROM - 32k (LoROM) or 64k (HiROM)
            // copy 40 bytes from the two location candidates and test against these
            byte[] cand_lo = new byte[40], cand_hi = new byte[40];
            Buffer.BlockCopy(Data, 0x7fc0, cand_lo, 0, 40);
            Buffer.BlockCopy(Data, 0xffc0, cand_hi, 0, 40);

            // check the two locations for expected values for a Hi or Lo ROM and add points if they match
            int loScore = 0, hiScore = 0;
            // check layout flag
            if ((cand_lo[0x15] & 0x20) == 0x20 && cand_lo[0x15] <= 0x31) loScore += 2;
            if ((cand_hi[0x15] & 0x21) == 0x21 && cand_hi[0x15] <= 0x31) hiScore += 2;

            // Checksum + complement should equal $FFFF
            if ((cand_lo[0x1c] + (cand_lo[0x1d] << 8)) + (cand_lo[0x1e] + (cand_lo[0x1f] << 8)) == 0xffff) loScore += 2;
            if ((cand_hi[0x1c] + (cand_hi[0x1d] << 8)) + (cand_hi[0x1e] + (cand_hi[0x1f] << 8)) == 0xffff) hiScore += 2;

            // Check last character of title field, will usually be an ascii space (0x20)
            if (cand_lo[0x14] == 0x20) loScore++;
            if (cand_hi[0x14] == 0x20) hiScore++;

            //if (loScore >= hiScore) this.ROMBankSize = 0x8000;
            //else this.ROMBankSize = 0x10000;
            if (loScore >= hiScore) this.ROMBankSize = ROMBankSizes.LoROM;
            else this.ROMBankSize = ROMBankSizes.HiROM;
        }

        /// <summary>
        /// Generates an overview of information about this Super Famicom ROM
        /// </summary>
        /// <returns>String containing the report</returns>
        public override string Report()
        {
            var _out = new StringBuilder(base.Report());
            _out.AppendLine("Super Famicom Information:");
            _out.AppendLine("Dump type: " + this.ImageFormat.GetEnumDesc());
            _out.AppendLine("Video type: " + this.SoftwareRegionVideoType.GetEnumDesc());
            return _out.ToString();
        }

        public override Layout.ImageMap AutoMap()
        {
            var _out = base.AutoMap();
            for (uint j = 0; j < this.File.Length / (uint)this.ROMBankSize; j++)
                _out.Add(new Chunk(new Range((j * (uint)this.ROMBankSize), (uint)this.ROMBankSize), ("ROM Bank " + j.ToString() + " [" + this.ROMBankSize.ToString() + "]")));
            return _out;
        }

        static public class Dump
        {
            /// <summary>
            /// The supported dump formats
            /// </summary>
            public enum Formats
            {
                RAW = 0,
                SMC,
                FIG,
                SWC,
            }

            public static string GetDumpInfo(Formats DumpFormat)
            {
                switch (DumpFormat)
                {
                    case Formats.FIG:
                        return "Pro Fighter format - 512 byte header";
                    case Formats.SMC:
                        return "Super Magicom format - 512 byte header";
                    case Formats.SWC:
                        return "Super Wild Card format - 512 byte header";
                    case Formats.RAW:
                        return "Raw format";
                    default:
                        return "Unknown dump format";
                }
            }


            public static Formats GetDumpType(byte[] Image)
            {
                // determine if the file has the 512 byte dump header
                // min file size is based on smallest possible ROM bank ($4000) + 512byte dump header ($200)
                if (Image.Length >= 0x8200 && Image.Length % 1024 == 512)
                {
                    // dump header detected, determine which device it is from
                    // detect SWC: 3 byte signature at $08: $aa $bb $04
                    if (Image[8] == 0xaa && Image[9] == 0xbb && Image[10] == 0x04)
                        return Formats.SWC;
                    // detect FIG: very smiliar to SMC, but has three extra bytes that contain settings, see if these bytes are NOT zero
                    // if any are not zero, it is NOT SMC (and therefore FIG)
                    if (Image[3] != 0 | Image[4] != 0 | Image[5] != 0)
                        return Formats.FIG;
                    // assume that it's SMC, which is most common anyway
                    return Formats.SMC;
                }
                // either the rom is a raw dump or some crazy dump format; return the type as Raw regardless
                // if the file size is fishy, add a comment to the ROM about it, but still return raw
                //if (Image.Length < 0x8000 | Image.Length % 1024 != 0)
                //    Image.AddComment("Warning: file size is non-standard (" + (Image.File.Length % 1024).ToString() + " extra bytes found)");
                return Formats.RAW;
            }

            public static byte[] Standardize(byte[] Image, Formats DumpFormat)
            {
                if (Image == null) throw new ArgumentNullException();
                byte[] _out;

                switch (DumpFormat)
                {
                    case Formats.FIG:
                    case Formats.SMC:
                    case Formats.SWC:
                        _out = new byte[Image.Length - 512];
                        Buffer.BlockCopy(Image, 512, _out, 0, _out.Length);
                        return _out;
                    default:
                        return Image;
                }
            }
        }
    }
}