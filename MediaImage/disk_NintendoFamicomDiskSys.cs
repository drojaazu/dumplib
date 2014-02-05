using System;
using dumplib.Layout;
//using dumplib.Image.Formats;

namespace dumplib.Image
{
    public class NintendoFamicomDiskSys_disk : MediaImage
    {

        public static class Dump
        {
            public enum Formats
            {
                RAW = 0,
                FDS,
                FAM
            }

            public static Formats GetDumpFormat(byte[] Image)
            {
                if (Image == null) throw new ArgumentNullException();
                // check for FDS first
                if (Image[0] == 0x46 && Image[1] == 0x44 && Image[2] == 0x53 && Image[3] == 0x1a) return Formats.FDS;
                // the FAM format doesn't seem to be well understood
                // There is always a 0xf180 length header followed by standard FDS data
                // check for the block markers after 0xf180
                if (Image[0xf180] == 1 && Image[0xf1b8] == 2 && Image[0xf1ba] == 3) return Formats.FAM;

                return Formats.RAW;
            }

            public static string GetDumpInfo(Formats DumpFormat)
            {
                switch (DumpFormat)
                {
                    case Formats.FAM:
                        return "Pasofami format - 0xF180 byte header";
                    case Formats.FDS:
                        return "fwNES format - 16 byte header";
                    case Formats.RAW:
                        return "Raw format";
                    default:
                        return "Unknown dump format";
                }
            }

            public static byte[] Standardize(byte[] Image, Formats DumpFormat)
            {
                if (Image == null) throw new ArgumentNullException();
                byte[] _out;
                switch (DumpFormat)
                {
                    case Dump.Formats.FDS:
                        _out = new byte[Image.Length - 16];
                        Buffer.BlockCopy(Image, 16, _out, 0, _out.Length);
                        return _out;
                    case Dump.Formats.FAM:
                        _out = new byte[Image.Length - 0xf180];
                        Buffer.BlockCopy(Image, 0xf180, _out, 0, _out.Length);
                        return _out;
                    default:
                        return Image;
                }

            }
        }

        

        public Dump.Formats FileFormat
        {
            get;
            private set;
        }

        public FDS_Side[] Sides
        {
            get;
            private set;
        }

        public NintendoFamicomDiskSys_disk(string Filepath)
            : base(Filepath)
        {
            base.MediaType = MediaTypes.Disk;
            base.ReadWholeFile();
            this.FileFormat = Dump.GetDumpFormat(this.Data);
            switch (this.FileFormat)
            {
                case Dump.Formats.FDS:
                    this.Sides = new FDS_Side[GetByte(4)];
                    Dump.Standardize(base.Data, this.FileFormat);
                    break;
                case Dump.Formats.FAM:
                    this.Sides = new FDS_Side[GetByte(0)];
                    Dump.Standardize(base.Data, this.FileFormat);
                    break;
                default:
                    this.Sides = new FDS_Side[1];
                    break;
            }
            base.SoftwareTitle = this.GetText_ASCII(new Layout.Range(16, 4));
            base.GfxDefaultPixelFormat = Gfx.TileFormats.Famicom;
            for (uint h = 0; h < this.Sides.Length; h++)
            {
                this.Sides[h] = new FDS_Side(this, h * 65500);
            }
        }

        /// <summary>
        /// Generates an overview of information about this Famicom Disk System software
        /// </summary>
        /// <returns>String containing the report</returns>
        public override string Report()
        {
            var _out = new System.Text.StringBuilder(base.Report());
            _out.AppendLine("Nintendo Famicom Disk System information:");
            _out.AppendLine("-- Number of sides: " + this.Sides.Length.ToString());
            _out.AppendLine("(Following information extracted from disk 1 side A of the image)");
            _out.AppendLine("-- Software code: " + this.SoftwareTitle);
            return _out.ToString();
        }

        /// <summary>
        /// Generates a file map describing the Famicom Disk System image
        /// </summary>
        /// <returns>File map</returns>
        override public ImageMap AutoMap()
        {
            var _out = base.AutoMap();
            //_out.Add(new Chunk(new OffsetLengthPair(0, (uint)this.File.Length), "Entire File"));
            foreach (FDS_Side s in this.Sides)
            {
                _out.Add(new Chunk(s.EntireSide, "Disk " + s.SideNumber + " " + s.DiskSide.ToString() + " (Entire Side)"));
                foreach (FDS_File f in s.Files)
                {
                    _out.Add(new FileChunk(f.FileData, "Disk" + (s.SideNumber+1).ToString() + " " + s.DiskSide.ToString() + " - " + f.FileName + " [" + f.FileType.ToString() + "]"));
                }
            }
            return _out;
        }

        public class FDS_File
        {
            public enum Type : byte
            {
                Program = 0,
                Character,
                NameTable
            }

            /// <summary>
            /// Outlines the entire file (header + data) inside the disk image
            /// </summary>
            public Range EntireFile
            {
                get;
                private set;
            }

            /// <summary>
            /// Outlines just the file data
            /// </summary>
            public Range FileData
            {
                get;
                private set;
            }

            public Type FileType
            {
                get;
                private set;
            }

            public short FileSize
            {
                get;
                private set;
            }

            public string FileName
            {
                get;
                private set;
            }

            public uint NextFileOffset
            {
                get
                {
                    return this.EntireFile.EndOffset + 1;
                }
            }

            public FDS_File(NintendoFamicomDiskSys_disk Image, uint Offset)
            {
                if (Image.GetByte(Offset) != 3) Image.AddComment("Header block may be corrupt/incorrect [FDS Create File Entry]");
                this.FileName = Image.GetText_SJIS(Offset + 3, 8);
                byte[] temp = Image.GetBytes(Offset + 13, 2);
                //Array.Reverse(temp);
                this.FileSize = BitConverter.ToInt16(temp, 0);
                this.FileType = (Type)Image.GetByte(Offset + 15);
                
                this.EntireFile = new Range(Offset, (uint)this.FileSize + 17);
                this.FileData = new Range(Offset + 17, (uint)this.FileSize);
            }
        }

        public class FDS_Side
        {
            public enum SideLabel : byte
            {
                SideA = 0,
                SideB
            }

            public FDS_Side(NintendoFamicomDiskSys_disk Image, uint Offset)
            {
                // will probably want to rewrite this to pull the header as a chunk and the file as a chunk
                // to stop using so many GetByte calls
                if (Offset > Image.DataSize) throw new ArgumentOutOfRangeException("Offset goes past end of file");
                if ((Image.DataSize - Offset) < 65500) throw new ArgumentOutOfRangeException("Not enough bytes for a Disk Side from the offset specified");

                this.EntireSide = new Range(Offset, 65500);
                this.DiskSide = (SideLabel)Image.GetByte(Offset + 21);
                this.SideNumber = Image.GetByte(Offset+22);
                int NumFiles = Image.GetByte(Offset + 57);
                if (NumFiles < 1) throw new Exception("Files on disk cannot be zero. Is this a proper FDS image?");
                this.Files = new FDS_File[NumFiles];

                uint FileStartOffset = Offset + 58;
                Files[0] = new FDS_File(Image, FileStartOffset);
                if (NumFiles > 1)
                {
                    for (uint y = 1; y < NumFiles; y++)
                        Files[y] = new FDS_File(Image, Files[y-1].NextFileOffset);
                }
            }

            public SideLabel DiskSide
            {
                get;
                private set;
            }

            public byte SideNumber {
                get;
                private set;
            }

            public FDS_File[] Files
            {
                get;
                private set;
            }

            public Range EntireSide
            {
                get;
                private set;
            }
        }
    }
}
