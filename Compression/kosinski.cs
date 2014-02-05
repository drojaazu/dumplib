using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;

namespace dumplib.Compression
{
    public enum CompressionFormats
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("Kosinski (Sonic games)")]
        Kosinski = 1
    }

    public static class Kosinski
    {
        public static byte[] Decompress(byte[] Data)
        {
            var memstream = new MemoryStream();
            /*
             *  1. read two bytes from the data, this will be the description field
             *  2. break down bytes into bit values (little endian) 
             *  3. loop through the bits to construct the uncompressed chunk
             *  
             */

            // an array of binary values that act as commands
            bool[] Commands;
            short descfield;
            int dataloop = 0;
            bool foundend = false;
            
            for (; dataloop < Data.Length; )
            {
                //reverse the 4-bit endianness in each byte and get the final value
                /*var testing = new byte[2] { 
                    (byte)(((Data[dataloop+1] & 0xf) << 4) | ((Data[dataloop+1] & 0xf0) >> 4)),
                    (byte)(((Data[dataloop] & 0x0f) << 4) | ((Data[dataloop] & 0xf0) >> 4))
                };*/

                descfield = 0;
                descfield = (short)BitConverter.ToInt16(new byte[2] { Data[dataloop+1].ReverseBits(), Data[dataloop].ReverseBits() }, 0);
                
                Commands = new bool[16];
                // note: the LAST BIT of the desc field is carried to the start of the next desc, EVEN IF it's complete
                // i.e. last bit is 1: do not copy the uncompressed byte, instead start a new cmd stack with 1 at the bottom and read the next desc
                // or something like that...
                for (int cmdnum = 0, mask = 0x8000; cmdnum < 16; cmdnum++)
                {
                    Commands[cmdnum] = ((descfield & mask) == mask) ? true : false;
                    mask /= 2;
                }
                // should now have the Commands array full
                // increase dataloop pointer past the two description field bytes, will now begin pointing at the data field bytes
                 dataloop += 2;

                for (int cmdloop = 0; cmdloop < 15; cmdloop++)
                {
                    // uncompressed byte (command 1)
                    if (Commands[cmdloop])
                    {
                        memstream.Seek(0, SeekOrigin.End);
                        memstream.WriteByte(Data[dataloop]);
                        dataloop++;
                        continue;
                    }
                    // seperate run length encoding
                    else if (!Commands[cmdloop] & Commands[cmdloop + 1])
                    {
                        //increase loop by one to keep it in sync
                        cmdloop++;

                        // get a pointer to the location inside the uncompressed stream
                        //long offset = 0xFFFFE000 | ((long)(0xf8 & Data[dataloop + 1]) << 5) | Data[dataloop];
                        int offset = (-8192) + ((0xf8 & Data[dataloop + 1]) << 5) + Data[dataloop];
                        
                        int numtocopy;
                        // get the amount of bytes to copy from the pointer
                        // test the second byte to see if we need the third
                        // use 2 bytes...
                        if ((Data[dataloop + 1] & 7) == 7)
                        {
                            numtocopy = (Data[dataloop + 1] & 7) + 2;
                            dataloop += 2;
                        }
                        else
                            // use 3 bytes
                        {
                            if (Data[dataloop + 2] == 0)
                            {
                                foundend = true;
                                break;
                            }
                            numtocopy = Data[dataloop + 2] + 1;
                            dataloop += 3;
                        }
                        memstream.Seek((long)offset, SeekOrigin.Current);
                        var chunk = new byte[Math.Abs(offset)];
                        memstream.Read(chunk, 0, chunk.Length);
                        //memstream.CopyTo(memstream, numtocopy);
                        memstream.Seek(0, SeekOrigin.End);
                        for (int g = 0; g < numtocopy; g++)
                        {
                            memstream.Write(chunk, 0, chunk.Length);
                        }
                        continue;

                    }
                    else
                        // must be inline encoding
                    {
                        int numtocopy = 2;
                        if (Commands[cmdloop + 2]) numtocopy += 2;
                        if (Commands[cmdloop + 3]) numtocopy++;
                         cmdloop += 3;

                        int offset = (-256) + Data[dataloop];
                        dataloop++;

                        var chunk = new byte[Math.Abs(offset)];
                        memstream.Seek((long)offset, SeekOrigin.Current);
                        //memstream.CopyTo(memstream, numtocopy);
                        memstream.Read(chunk, 0, chunk.Length);
                        memstream.Seek(0, SeekOrigin.End);
                        for (int g = 0; g < numtocopy; g++)
                        {
                            memstream.Write(chunk, 0, chunk.Length);
                        }
                        continue;
                    }
                }
                if (foundend) break;
            }

            var _out = new byte[memstream.Length];
            using (BinaryReader br = new BinaryReader(memstream))
            {
                return br.ReadBytes((int)memstream.Length);
            }
        }
    }
}
