using System;
using dumplib.Layout;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dumplib.Image
{
    public class NintendoFamicomDiskSys_disk : DiskImage
    {
        private readonly static string HW_JP = "ニンテンド　ファミコン　ディスクシステム";
        private readonly static string HW_JP_R = "Nintendo Famicom Disk System";

        public string HardwareName_Japan
        {
            get
            {
                return NintendoFamicomDiskSys_disk.HW_JP;
            }
        }

        public string HardwareName_JapanRomaji
        {
            get
            {
                return NintendoFamicomDiskSys_disk.HW_JP_R;
            }
        }

        /*public static class Dump
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
        }*/

        public NintendoFamicomDiskSys_disk(Stream Datastream, IDumpConverter Converter = null)
            : base(Datastream, Converter)
        {
            base.MediaType = MediaTypes.Disk;
            base.HardwareName = NintendoFamicomDiskSys_disk.HW_JP_R;
            
            // this needs to be dealt with more gracefully:
            // FDS/FAM images are dumps of multiple disk sides concatenated into one file
            // the dump header indicates how many sides there are
            // in the current methodology, the dump headers are disposed
            // so, for now, the FDS converters have an extra property, Sides, that preserves this value
            // we can either look into a more robust dump solution that keeps a copy of the header
            // or just search through the stream and manualy find each disk side (i.e. the "NINTENDO-HVC" string)
            
            /*if (Converter != null)
            {
                if (Converter is NintendoFDS_FAM) this.Sides = new FamicomDiskSystem_Side[(Converter as NintendoFDS_FAM).Sides];
                else if (Converter is NintendoFDS_FDS) this.Sides = new FamicomDiskSystem_Side[(Converter as NintendoFDS_FDS).Sides];
            }
            else this.Sides = new FamicomDiskSystem_Side[1];*/

            base.SoftwareTitle = this.GetText_ASCII(new Layout.Range(16, 4));
            //byte[] thisside = new byte[65500];
            //this.Datastream.Seek(0, SeekOrigin.Begin);
            this.Volumes = new FamicomDiskSystem_Side[this.Datastream.Length / 65500];
            for (uint h = 0; h < this.Volumes.Length; h++)
            {
                //this.Datastream.Read(thisside, 0, 65500);
                base.Volumes[h] = new FamicomDiskSystem_Side(this.Datastream, (h * 65500));
            }
        }

        /// <summary>
        /// Generates a file map describing the Famicom Disk System image
        /// </summary>
        /// <returns>File map</returns>
        /*override public ImageMap AutoMap()
        {
            var _out = base.AutoMap();
            //_out.Add(new Chunk(new OffsetLengthPair(0, (uint)this.File.Length), "Entire File"));
            foreach (FamicomDiskSystem_Side s in this.Sides)
            {
                _out.Add(new ChunkInfo(s.EntireSide, "Disk " + s.SideNumber + " " + s.DiskSide.ToString() + " (Entire Side)"));
                foreach (FDS_File f in s.Files)
                {
                    if (f.FileType == FDS_File.FileTypes.Character)
                    {
                        var newchunk = new GfxChunkInfo(f.FileData, "Disk" + (s.SideNumber + 1).ToString() + " " + s.DiskSide.ToString() + " - " + f.FileName + " [" + f.FileType.ToString() + "]");
                        
                        //newchunk.TileConverter = new dumplib.Gfx.TileConverters.Nintendo_Famicom();

                        newchunk.ParseArgs(new string[] {"format=nfc"});
                        _out.Add(newchunk);
                    }
                    else
                        _out.Add(new FileChunkInfo(f.FileData, "Disk" + (s.SideNumber + 1).ToString() + " " + s.DiskSide.ToString() + " - " + f.FileName + " [" + f.FileType.ToString() + "]"));
                }
            }
            return _out;
        }*/

        public class FamicomDiskSystem_Side : IVolume
        {
            public string ID
            {
                get
                {
                    return "FDS_FileSystem";
                }
            }

            public FilesystemObject GetObject(string Name)
            {
                for (int x = 0; x < this.Root.Contents.Length; x++)
                {
                    if (this.Root.Contents[x].Name == Name) return this.Root.Contents[x];
                }
                throw new FileNotFoundException();
            }

            public byte[] GetFile(string Name)
            {
                var obj = this.GetObject(Name) as FamicomDiskSystem_File;
                this.Image.Position = this.StartOffset + (long)obj.Offset;
                byte[] _out = new byte[obj.Length];
                this.Image.Read(_out, 0, obj.Length);
                return _out;
            }

            //FDS specific data from the disk header

            /// <summary>
            /// Manufacturer code - a single-byte value representing the producer of the software
            /// </summary>
            public byte ManufacturerCode
            {
                get;
                private set;
            }

            /// <summary>
            /// Game name - a three byte value representing the name of the game
            /// </summary>
            public string GameName
            {
                get;
                private set;
            }

            /// <summary>
            /// Game type - a single byte value indicating the type of software
            /// (Normal disk, Event, Price reduction)
            /// </summary>
            public byte GameType
            {
                get;
                private set;
            }

            public byte GameVersion
            {
                get;
                private set;
            }

            public byte SideNumber
            {
                get;
                private set;
            }

            public byte DiskNumber
            {
                get;
                private set;
            }

            private Stream Image;

            public uint StartOffset
            {
                get;
                private set;
            }

            public Directory Root
            {
                get;
                private set;
            }

            public string Label
            {
                get
                {
                    return string.Format("{0} - Disk {1} Side {2}", this.GameName, this.DiskNumber, this.SideNumber);
                }
            }

            public FamicomDiskSystem_Side(Stream Image, uint Offset)
            {
                this.Image = Image;
                this.StartOffset = Offset;

                // Step 1: confirm the data contains valid FDS file system data
                this.Image.Position = this.StartOffset;
                // read first byte, confirm it is 0x01
                // read the next 14 bytes, confirm it is ASCII for "*NINTENDO-HVC*"
                byte[] verify = new byte[15];
                this.Image.Read(verify, 0, 15);
                if (verify[0] != 1 || Encoding.ASCII.GetString(verify, 1, 14) != "*NINTENDO-HVC*") throw new InvalidDataException("Not a valid Famicom Disk System file system (invalid hardware identifier in header)");

                //next byte should be the manufacturer code
                this.ManufacturerCode = (byte)Image.ReadByte();

                verify = new byte[3];
                this.Image.Read(verify, 0, 3);
                this.GameName = Encoding.ASCII.GetString(verify);

                this.GameType = (byte)this.Image.ReadByte();
                this.GameVersion = (byte)this.Image.ReadByte();

                this.SideNumber = (byte)this.Image.ReadByte();

                this.DiskNumber = (byte)this.Image.ReadByte();

                // Step 2: set up files
                // check number of files
                this.Image.Position = this.StartOffset + 0x38;
                if (this.Image.ReadByte() != 2) throw new InvalidDataException("Not a valid Famicom Disk System file system (invalid file count block)");
                int numfiles = this.Image.ReadByte();
                if (numfiles == 0) Console.WriteLine("File count value is zero; may be invalid data or 'copy protection'");
                this.Root = new Directory();
                this.Root.Contents = new FamicomDiskSystem_File[numfiles];

                byte[] filename = new byte[8];
                byte[] size = new byte[2];

                for (int allfiles = 0; allfiles < numfiles; allfiles++)
                {
                    // assume the stream pointer is set properly...
                    if (this.Image.ReadByte() != 3) if (this.Image.ReadByte() != 2) throw new InvalidDataException("Not a valid Famicom Disk System file system (invalid beginning of file metadata byte while trying file #" + allfiles + ")");
                    var props = new FamicomDiskSystem_File.Properties();
                    props.Number = (byte)this.Image.ReadByte();
                    props.ID = (byte)this.Image.ReadByte();
                    this.Image.Read(filename, 0, 8);
                    props.Name = String.Format("{0}:{1}", props.ID.ToString(), Encoding.ASCII.GetString(filename));
                    string test = Text.Transcode.UsingSJIS(filename);

                    //ignore the destination address for now..
                    this.Image.ReadByte();
                    this.Image.ReadByte();

                    this.Image.Read(size, 0, 2);
                    if (!BitConverter.IsLittleEndian) Array.Reverse(size);
                    props.Length = (ushort)BitConverter.ToInt16(size, 0);

                    props.Type = (FamicomDiskSystem_File.FileTypes)this.Image.ReadByte();
                    if (this.Image.ReadByte() != 4) throw new InvalidDataException("Not a valid Famicom Disk System file system (invalid beginning of file data byte while trying file #" + allfiles + ")");
                    props.Offset = (ulong)this.Image.Position;
                    //this.objects.Add(props.Name, new FamicomDiskSystem_File(props));
                    this.Root.Contents[allfiles] = new FamicomDiskSystem_File(props);
                    this.Image.Seek(props.Length, SeekOrigin.Current);
                }
            }
        }
    }

    public class FamicomDiskSystem_File : File
    {
        public enum FileTypes : byte
        {
            Program = 0,
            Character,
            NameTable
        }

        public struct Properties
        {
            public byte Number
            {
                get;
                set;
            }

            public byte ID
            {
                get;
                set;
            }

            public FileTypes Type
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }

            public ushort Length
            {
                get;
                set;
            }

            public ulong Offset
            {
                get;
                set;
            }
        }

        public FamicomDiskSystem_File(Properties Properties) : base(Properties.Name, Properties.Offset)
        {
            this.Name = Properties.Name;
            this.Offset = Properties.Offset;
            this.Length = Properties.Length;

            this.Number = Properties.Number;
            this.Type = Properties.Type;
            this.ID = Properties.ID;
        }
        
        public byte Number
        {
            get;
            private set;
        }

        public byte ID
        {
            get;
            private set;
        }

        public FileTypes Type
        {
            get;
            private set;
        }

        public ushort Length
        {
            get;
            private set;
        }

    }

}
