using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace dumplib.Image
{
    public interface IDumpConverter
    {
        string Description
        {
            get;
        }

        MemoryStream Normalize(Stream DataStream);
    }

    public class SuperFamicom_Dumpers : IDumpConverter
    {
        public string Description
        {
            get
            {
                return "Super Magicom, Super Wild Card, Pro Fighter";
            }
        }

        public MemoryStream Normalize(Stream Datastream)
        {
            if (Datastream.Length < 512) throw new ArgumentException("Stream size is too small to convert");
            byte[] data = new byte[(Datastream.Length - 512)];
            Datastream.Seek(512, SeekOrigin.Begin);
            Datastream.Read(data, 0, data.Length);
            return new MemoryStream(data);
        }
    }
    public class NintendoFamicom_iNES : IDumpConverter
    {
        public string Description
        {
            get
            {
                return "iNES";
            }
        }

        public MemoryStream Normalize(Stream Datastream)
        {
            if (Datastream.Length < 16) throw new ArgumentException("Stream size is too small to convert");
            byte[] data = new byte[(Datastream.Length - 16)];
            Datastream.Seek(16, SeekOrigin.Begin);
            Datastream.Read(data, 0, data.Length);
            return new MemoryStream(data);
        }
    }

    public class Nintendo64_CD64 : IDumpConverter
    {
        public string Description
        {
            get
            {
                return "CD64";
            }
        }

        public MemoryStream Normalize(Stream Datastream)
        {
            var _out = new MemoryStream();
            _out.SetLength(Datastream.Length);
            byte[] swap = new byte[4];
            Datastream.Seek(0, SeekOrigin.Begin);
            while (Datastream.Read(swap, 0, 4) > 0)
            {
                _out.WriteByte(swap[3]);
                _out.WriteByte(swap[2]);
                _out.WriteByte(swap[1]);
                _out.WriteByte(swap[0]);
            }
            /*for (int i = 0; i < _out.Length; i += 4)
            {
                for (int t = 0; t < 4; t++) swap[t] = _out[i + t];
                Image[i] = swap[3];
                Image[i + 1] = swap[2];
                Image[i + 2] = swap[1];
                Image[i + 3] = swap[0];
            }*/
            return _out;
        }
    }

    public class Nintendo64_DoctorV64 : IDumpConverter
    {
        public string Description
        {
            get
            {
                return "Doctor V64";
            }
        }

        public MemoryStream Normalize(Stream Datastream)
        {
            var _out = new MemoryStream();
            _out.SetLength(Datastream.Length);
            Datastream.Seek(0, SeekOrigin.Begin);

            var swap = new byte[2];
            while (Datastream.Read(swap, 0, 2) > 0)
            {
                _out.WriteByte(swap[1]);
                _out.WriteByte(swap[0]);
            }

            /*for (int i = 0; i < _out.Length; i += 2)
            {
                swap[0] = _out[i]; swap[1] = _out[i + 1];
                _out[i] = swap[1]; _out[i + 1] = swap[0];
            }*/
            return _out;
        }
    }

    public class SegaMegadrive_SuperMagicdrive : IDumpConverter
    {
        public string Description
        {
            get
            {
                return "Super Magic Drive";
            }
        }

        public MemoryStream Normalize(Stream Datastream)
        {
            // start by ignoring the 512 byte header, then divide the remainder by 16k chunks
            // (check for modulus to see if the rom is an odd size)
            //if(Data.Length % 0x4000 != 0)  //modulus is not zero, file is an unexpected size, add a comment
            //byte[] _out = new byte[Datastream.Length - 0x200];
            MemoryStream _out2 = new MemoryStream();
            _out2.SetLength(Datastream.Length - 512);
            Datastream.Seek(512, SeekOrigin.Begin);

            int totalchunks = (int)Datastream.Length / 0x4000;
            // outer loop through the total number of 16k chunks
            var thischunk = new byte[0x4000];
            //int outoffset = 0, outcount = 0;
            for (int chunkloop = 0; chunkloop < totalchunks; chunkloop++)
            {
                //Buffer.BlockCopy(Image, 0x200 + (chunkloop * 0x4000), thischunk, 0, 0x4000);
                Datastream.Read(thischunk, 0, 0x4000);
                //outoffset = chunkloop * 0x4000;
                //outcount = 0;
                // decode the 16k block: the odd digit is from the low end of the block, the even digit is from the high end
                for (int deint = 0; deint < 0x2000; deint++)
                {
                    _out2.WriteByte(thischunk[deint + 0x2000]);
                    _out2.WriteByte(thischunk[deint]);
                    //_out[outoffset + outcount + 1] = thischunk[deint];
                    //_out[outoffset + outcount] = thischunk[deint + 0x2000];
                    //outcount += 2;
                }

                
            }
            return _out2;
        }
    }
}
