using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace dumplib.Image
{
    public class Atari8bit_disk : DiskImage
    {
        public Atari8bit_disk(Stream DataStream, IDumpConverter Converter) : base(DataStream, Converter)
        {
            if (Converter == null) this.Datastream = Datastream;
            else this.Datastream = Converter.Normalize(DataStream);

            this.Volumes = new Volume[1];
            this.Volumes[0] = new Volume();
            this.Volumes[0].Root = new Directory();
            
            // check for the VTOC location first, to confirm if single or double density
            if (this.Datastream.Length < 0xb380)
            {
                // file isn't even long enough to look for the VTOC
                // set an empty root directory and get outta here
                this.Density = DensityType.Unknown;
                
                return;
            }
            this.Datastream.Seek(0xb381, SeekOrigin.Begin);
            if (this.Datastream.ReadByte() == 0xc3 && this.Datastream.ReadByte() == 2)
            {
                //most likely found the VTOC
                this.Density = DensityType.Single;
                this.Volumes[0].UnitSize = 128;
            }
            else
            {
                this.Datastream.Seek(0x16801, SeekOrigin.Begin);
                if (this.Datastream.ReadByte() == 0xc3 && this.Datastream.ReadByte() == 2)
                {
                    //most likely found the VTOC
                    this.Density = DensityType.Double;
                    this.Volumes[0].UnitSize = 256;
                }
                else
                {
                    // non-standard format or not an Atari disk
                    // set an empty root directory and get outta here
                    this.Density = DensityType.Unknown;
                    return;
                }
            }

            //our bearings our set, get the file list
            this.Datastream.Seek(360 * this.Volumes[0].UnitSize, SeekOrigin.Begin);
            int x = 0;
            byte[] buffer = new byte[16];
            byte[] temp = new byte[2];
            List<File> files = new List<File>();
            // max of 64 file entries
            while (x < 64)
            {
                this.Datastream.Read(buffer, 0, 16);
                if (buffer[0] == 0) break;
                var newfile = new AtariFile();
                newfile.Flags = buffer[0];
                // get sector count
                Buffer.BlockCopy(buffer, 1, temp, 0, 2);
                if (!BitConverter.IsLittleEndian) Array.Reverse(temp);
                newfile.Length = (ulong)BitConverter.ToInt16(temp, 0);
                // get sector start
                Buffer.BlockCopy(buffer, 3, temp, 0, 2);
                if (!BitConverter.IsLittleEndian) Array.Reverse(temp);
                newfile.Start = (ulong)BitConverter.ToInt16(temp, 0);
                newfile.Name = Encoding.ASCII.GetString(buffer, 5, 8).Trim() + "." + Encoding.ASCII.GetString(buffer, 13, 3);
                files.Add(newfile);
                x++;
            }
            Volumes[0].Root.Contents = files.ToArray();
            
        }

        public DensityType Density
        {
            get;
            set;
        }

        public enum DensityType
        {
            Single,
            Enhanced,
            Double,
            Unknown
        }
    }

    public class AtariFile : dumplib.Image.File
    {
        public enum FileFlags
        {
            Deleted = 0x80,
            InUse = 0x40,
            Locked = 0x20,
            Open = 1,
            New = 0
        }

        public byte Flags
        {
            get;
            set;
        }
    }

    public class Atari_ATR : IDumpConverter
    {
        public string Description
        {
            get
            {
                return "ATR disk image format";
            }
        }

        public MemoryStream Normalize(Stream DataStream)
        {
            var _out = new MemoryStream();
            DataStream.Seek(16, SeekOrigin.Begin);
            DataStream.CopyTo(_out);
            return _out;
        }
    }
}
