using System;
using System.ComponentModel;
using System.Reflection;
using dumplib.Gfx;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.IO;
using System.Collections.Generic;
using dumplib.Layout;

namespace dumplib.Image
{

    /// <summary>
    /// Represents the binary contents of a data storage medium or device
    /// </summary>
    public abstract class MediaImage
    {
        public enum MediaTypes
        {
            ROM = 0, Disk, Tape
        }

        public MediaImage(string Filepath)
        {
            Data = null;
            this.File = new FileInfo(Filepath);
            this.Comments = new List<string>();
            this.GfxDefaultPixelFormat = TileFormats.Monochrome;
        }

        /// <summary>
        /// The Data buffer representing the binary data from the medium
        /// </summary>
        protected byte[] Data;

        public MediaTypes MediaType
        {
            get;
            protected set;
        }

        public uint DataSize
        {
            get
            {
                return (uint)this.Data.LongLength;
            }
        }

        /// <summary>
        /// References the image file on disk
        /// </summary>
        public FileInfo File
        {
            get;
            private set;
        }
        
        /// <summary>
        /// A collection of comments passed by dumplib functions as they encounter notable information about the iamge file
        /// </summary>
        public List<string> Comments
        {
            get;
            protected set;
        }

        /// <summary>
        /// Default pixel format for graphics
        /// </summary>
        public Gfx.TileFormats GfxDefaultPixelFormat
        {
            get;
            protected set;
        }

        /// <summary>
        /// Title from the ROM header, if available.
        /// </summary>
        public string SoftwareTitle
        {
            get;
            protected set;
        }

        /// <summary>
        /// Returns one byte from the specified offset
        /// </summary>
        /// <param name="offset">Start offset</param>
        /// <returns></returns>
        public byte GetByte(uint offset)
        {
            return Data[offset];
        }

        /// <summary>
        /// Returns an array of bytes from the specified offset
        /// </summary>
        /// <param name="ChunkAddr">Chunk address</param>
        /// <returns></returns>
        public byte[] GetBytes(Range ChunkAddr)
        {
            return this.GetBytes(ChunkAddr.StartOffset, ChunkAddr.Length);
        }

        /// <summary>
        /// Returns an aray of bytes from the specified offset
        /// </summary>
        /// <param name="Offset">Start offset</param>
        /// <param name="Length">Number of bytes to return</param>
        /// <returns></returns>
        public byte[] GetBytes(uint Offset, uint Length)
        {
            byte[] outBytes = new byte[Length];
            // Buffer.BlockCopy appears to be faster over lots of bytes
            // HOWEVER it uses an int (signed) for indexing
            // dumplib uses uint for 4gb addressing
            // this means this (rather important) function is a little slower than it should be, but it's the only solution for now
            Array.Copy(Data, Offset, outBytes, 0, Length);
            //Buffer.BlockCopy(Data, (int)Offset, outBytes, 0, (int)Length);
            return outBytes;
        }
        
        /// <summary>
        /// Loads the entire file into memory
        /// </summary>
        protected void ReadWholeFile()
        {
            this.Data = System.IO.File.ReadAllBytes(this.File.FullName);
        }

        /// <summary>
        /// Adds a string to the comment list
        /// </summary>
        /// <param name="Comment">The comment to add</param>
        internal void AddComment(string Comment)
        {
            this.Comments.Add(Comment);
        }
   
        /// <summary>
        /// Extracts a string of text from the Data buffer using ASCII encoding
        /// </summary>
        /// <param name="Offset">Starting offset in the Data buffer</param>
        /// <param name="Length">Number of bytes</param>
        /// <returns>Unicode formatted string</returns>
        public string GetText_ASCII(uint Offset, uint Length)
        {
            return dumplib.Text.GetText.UsingASCII(GetBytes(Offset,Length));
        }

        /// <summary>
        /// Extracts a string of text from the Data buffer using ASCII encoding
        /// </summary>
        /// <param name="Addr">Chunk address of the text string</param>
        /// <returns>Unicode formatted string</returns>
        public string GetText_ASCII(Range Addr)
        {
            return dumplib.Text.GetText.UsingASCII(GetBytes(Addr));
           //return Encoding.GetEncoding(437).GetString(GetBytes(Addr));
            //return Convert.ToBase64String(GetBytes(Addr));
        }

        /// <summary>
        /// Extracts a string of text from the Data buffer using SJIS (Japanese) encoding
        /// </summary>
        /// <param name="Offset">Starting offset in the Data buffer</param>
        /// <param name="Length">Number of bytes</param>
        /// <returns>Unicode formatted string</returns>
        public string GetText_SJIS(uint Offset, uint Length)
        {
            return dumplib.Text.GetText.UsingSJIS(GetBytes(Offset, Length));
        }

        /// <summary>
        /// Extracts a string of text from the Data buffer using SJIS (Japanese) encoding
        /// </summary>
        /// <param name="Addr">Chunk address of the text string</param>
        /// <returns>Unicode formatted string</returns>
        public string GetText_SJIS(Range Addr)
        {
            return dumplib.Text.GetText.UsingSJIS(GetBytes(Addr));
        }

        /// <summary>
        /// Extracts a string of text from the Data buffer using the specified text table
        /// </summary>
        /// <param name="Addr">Chunk address of the text string</param>
        /// <returns>Unicode formatted string</returns>
        public string GetText_Table(Range Addr, dumplib.Text.Table Table, string StartTable = "{main}")
        {
            return dumplib.Text.GetText.UsingTable(GetBytes(Addr), Table, StartTable);
        }

        /// <summary>
        /// Extracts a string of text from the Data buffer using the specified text table
        /// </summary>
        /// <param name="Addr">Chunk address of the text string</param>
        /// <returns>Unicode formatted string</returns>
        public string GetText_Table(Range Addr, dumplib.Text.Table Table, uint StartOffset, string StartTable = "{main}")
        {
            return dumplib.Text.GetText.UsingTable(GetBytes(Addr), Table, StartTable, true, StartOffset);
        }

        /// <summary>
        /// Extracts a string of text from the Data buffer using the specified text encoding
        /// </summary>
        /// <param name="Addr">Chunk address of the text string</param>
        /// <param name="Encoding">Encoding of the source string</param>
        /// <returns>Unicode formatted string</returns>
        public string GetText_Encoding(Range Addr, Encoding Encoding)
        {
            return dumplib.Text.GetText.UsingEncoding(GetBytes(Addr), Encoding);
        }

        /// <summary>
        /// Performs a pattern search (aka relative search) on the entire Data buffer
        /// </summary>
        /// <param name="Pattern">Variance pattern as an array of signed integers</param>
        /// <returns>A list of matches as chunk addresses</returns>
        public List<Range> Search_Pattern(int[] Pattern)
        {
            return dumplib.Search.Search.Pattern(this.Data, Pattern);
        }

        /// <summary>
        /// Performs a pattern search (aka relative search) on the specified chunk
        /// </summary>
        /// <param name="Pattern">Array of signed integers representing the variance between byte values</param>
        /// <param name="Addr">Chunk address to search</param>
        /// <returns></returns>
        public List<Range> Search_Pattern(int[] Pattern, Range Addr)
        {
            return dumplib.Search.Search.Pattern(GetBytes(Addr), Pattern);
        }

        public List<Range> Search_Sequence(byte[] Sequence)
        {
            return dumplib.Search.Search.Sequence(this.Data, Sequence);
        }

        public List<Range> Search_Sequence(byte[] Sequence, Range Addr)
        {
            return dumplib.Search.Search.Sequence(GetBytes(Addr), Sequence);
        }

        /// <summary>
        /// Extracts a bitmapped picture from the Data buffer using the specified format and palette
        /// </summary>
        /// <param name="Addr">Chunk address of the source graphics</param>
        /// <param name="PixelFormat">Pixel format to decode the source graphics</param>
        /// <param name="Palette">Color palette to apply to the pixel data</param>
        /// <param name="TilesPerRow">Number of tiles to render per row in the final bitmap</param>
        /// <returns>Standard bitmapped image</returns>
        public Bitmap GetGfx(Range Addr, TileFormats PixelFormat, ColorPalette Palette, int TilesPerRow)
        {
            return dumplib.Gfx.TileGfx.GetTiles(GetBytes(Addr), PixelFormat, Palette, TilesPerRow);
        }

        /// <summary>
        /// Generates a readable overview of information about the media image
        /// </summary>
        /// <returns>String containing the report</returns>
        virtual public string Report()
        {
            var _out = new StringBuilder();
            _out.AppendLine("Filename: " + this.File.Name);
            _out.AppendLine("Location: " + this.File.DirectoryName);
            if (this.Comments.Count != 0)
            {
                _out.AppendLine("Comments:");
                foreach (string s in this.Comments)
                    _out.AppendLine("-- " + s);
            }
            return _out.ToString();
        }

        /// <summary>
        /// Generates a human-readable output of byte data
        /// </summary>
        /// <param name="Addr">Chunk address to display</param>
        /// <returns></returns>
        public string GetBytesReadable(Range Addr)
        {
            // div = amount of complete lines of 16 bytes, mod = left over bytes
            uint div16 = Addr.Length / 16;
            uint mod16 = Addr.Length % 16;
            StringBuilder _out = new StringBuilder();

            // for each complete line, write out a full 16 column line of bytes
            if (div16 > 0)
            {
                for (int t = 0; t < div16; t++)
                {
                    _out.Append((Addr.StartOffset + (t * 16)).ToString("X8"));
                    for (int y = 0; y < 16; y++)
                        _out.Append(' ' + Data[Addr.StartOffset + (t * 16) + y].ToString("X2"));
                    _out.Append(Environment.NewLine);
                }
            }

            // if there are any bytes left over, write them out
            if (mod16 > 0)
            {
                _out.Append((Addr.StartOffset + (div16 * 16)).ToString("X8"));
                for (uint y = Addr.StartOffset + (div16 * 16); y < Addr.StartOffset + (div16 * 16) + mod16; y++)
                    _out.Append(' ' + Data[y].ToString("X2"));
            }
            return _out.ToString();
        }

        /// <summary>
        /// Generates a file map describing the Data buffer
        /// </summary>
        /// <returns>File map</returns>
        virtual public ImageMap AutoMap()
        {
            var _out = new ImageMap();
            _out.Description = "Auto-generated [" + this.File.Name + "]";
            _out.Add(new Chunk(new Range(0, (uint)this.Data.Length), "Entire Image"));
            return _out;
        }
    }
}
