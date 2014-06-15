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
    public enum MediaTypes
    {
        ROM = 0, Disk, Tape
    }


    /// <summary>
    /// Represents the binary contents of a data storage medium or device
    /// </summary>
    public abstract class MediaImage : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (this._disposed) return;
            // free manager resources here

            this.Datastream.Dispose();

            this._disposed = true;
        }

        #region     CONSTRUCTOR -=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=- CONSTRUCTOR

        public MediaImage(string Filepath)
        {
            //this.File = new FileInfo(Filepath);
            //this.Comments = new List<string>();
            this.Datastream = System.IO.File.OpenRead(Filepath);
        }

        public MediaImage(Stream Datastream)
        {
            if (Datastream == null) throw new ArgumentNullException();
            this.Datastream = Datastream;
        }

        public MediaImage(Stream Datastream, IDumpConverter Converter)
        {
            if (Datastream == null) throw new ArgumentNullException();
            this.Datastream = Converter.Normalize(Datastream);
        }

        #endregion  CONSTRUCTOR -=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=- CONSTRUCTOR


        #region     PROTECTED MEMBERS -=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=- PROTECTED MEMBERS

        public Stream Datastream = null;

        /// <summary>
        /// The Data buffer representing the binary data from the medium
        /// </summary>
        //protected byte[] Data = null;

        /// <summary>
        /// Loads the entire file into memory
        /// </summary>
        /*protected void ReadWholeFile()
        {
            this.Data = System.IO.File.ReadAllBytes(this.File.FullName);
        }*/

        #endregion

        #region     PUBLIC MEMBERS -=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=- PUBLIC MEMBERS
        public MediaTypes MediaType
        {
            get;
            protected set;
        }

        /*public uint DataSize
        {
            get
            {
                return (uint)this.Data.LongLength;
            }
        }*/

        /// <summary>
        /// References the image file on disk
        /// </summary>
        /*public FileInfo File
        {
            get;
            private set;
        }*/
        
        public List<string> Comments
        {
            get;
            protected set;
        }

        /// <summary>
        /// Title of the software, if available.
        /// </summary>
        public string SoftwareTitle
        {
            get;
            protected set;
        }

        public string HardwareName
        {
            get;
            protected set;
        }

        /// <summary>
        /// Returns one byte from the specified offset
        /// </summary>
        /// <param name="offset">Start offset</param>
        /// <returns></returns>
        public byte GetByte(long offset)
        {
            this.Datastream.Seek(offset, SeekOrigin.Begin);
            var getbyte = this.Datastream.ReadByte();
            return (byte)getbyte;
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
        public byte[] GetBytes(long Offset, int Length)
        {
            byte[] data = new byte[Length];
            Datastream.Seek(Offset, SeekOrigin.Begin);
            Datastream.Read(data, 0, Length);
            return data;
        }


        /// <summary>
        /// Extracts a string of text from the Data buffer using ASCII encoding
        /// </summary>
        /// <param name="Offset">Starting offset in the Data buffer</param>
        /// <param name="Length">Number of bytes</param>
        /// <returns>Unicode formatted string</returns>
        public string GetText_ASCII(long Offset, int Length)
        {
            return dumplib.Text.Transcode.UsingASCII(GetBytes(Offset, Length));
        }

        /// <summary>
        /// Extracts a string of text from the Data buffer using ASCII encoding
        /// </summary>
        /// <param name="Addr">Chunk address of the text string</param>
        /// <returns>Unicode formatted string</returns>
        public string GetText_ASCII(Range Addr)
        {
            return dumplib.Text.Transcode.UsingASCII(GetBytes(Addr));
        }

        /// <summary>
        /// Extracts a string of text from the Data buffer using SJIS (Japanese) encoding
        /// </summary>
        /// <param name="Offset">Starting offset in the Data buffer</param>
        /// <param name="Length">Number of bytes</param>
        /// <returns>Unicode formatted string</returns>
        public string GetText_SJIS(long Offset, int Length)
        {
            return dumplib.Text.Transcode.UsingSJIS(GetBytes(Offset, Length));
        }

        /// <summary>
        /// Extracts a string of text from the Data buffer using SJIS (Japanese) encoding
        /// </summary>
        /// <param name="Addr">Chunk address of the text string</param>
        /// <returns>Unicode formatted string</returns>
        public string GetText_SJIS(Range Addr)
        {
            return dumplib.Text.Transcode.UsingSJIS(GetBytes(Addr));
        }

        /// <summary>
        /// Extracts a string of text from the Data buffer using the specified text table
        /// </summary>
        /// <param name="Addr">Chunk address of the text string</param>
        /// <returns>Unicode formatted string</returns>
        public string GetText_Table(Range Addr, dumplib.Text.Table Table, string StartTable = "{main}")
        {
            return dumplib.Text.Transcode.UsingTable(GetBytes(Addr), Table, StartTable);
        }

        /// <summary>
        /// Extracts a string of text from the Data buffer using the specified text table
        /// </summary>
        /// <param name="Addr">Chunk address of the text string</param>
        /// <returns>Unicode formatted string</returns>
        public string GetText_Table(Range Addr, dumplib.Text.Table Table, uint StartOffset, string StartTable = "{main}")
        {
            return dumplib.Text.Transcode.UsingTable(GetBytes(Addr), Table, StartTable, true, StartOffset);
        }

        /// <summary>
        /// Extracts a string of text from the Data buffer using the specified text encoding
        /// </summary>
        /// <param name="Addr">Chunk address of the text string</param>
        /// <param name="Encoding">Encoding of the source string</param>
        /// <returns>Unicode formatted string</returns>
        public string GetText_Encoding(Range Addr, Encoding Encoding)
        {
            return dumplib.Text.Transcode.UsingEncoding(GetBytes(Addr), Encoding);
        }

        /// <summary>
        /// Performs a pattern search (aka relative search) on the entire Data buffer
        /// </summary>
        /// <param name="Pattern">Variance pattern as an array of signed integers</param>
        /// <returns>A list of matches as chunk addresses</returns>
        public List<Range> Search_Pattern(int[] Pattern)
        {
            //return dumplib.Search.Pattern(this.Data, Pattern);
            return null;
        }

        /// <summary>
        /// Performs a pattern search (aka relative search) on the specified chunk
        /// </summary>
        /// <param name="Pattern">Array of signed integers representing the variance between byte values</param>
        /// <param name="Addr">Chunk address to search</param>
        /// <returns></returns>
        public List<Range> Search_Pattern(int[] Pattern, Range Addr)
        {
            return dumplib.Search.Pattern(GetBytes(Addr), Pattern);
        }

        public List<Range> Search_Sequence(byte[] Sequence)
        {
            //return dumplib.Search.Sequence(this.Data, Sequence);
            return null;
        }

        public List<Range> Search_Sequence(byte[] Sequence, Range Addr)
        {
            return dumplib.Search.Sequence(GetBytes(Addr), Sequence);
        }

        public Bitmap GetTileGfx(Range Addr, ITileConverter Converter, ColorPalette Palette, int TilesPerRow)
        {
            return dumplib.Gfx.TileGfx.GetTiles(GetBytes(Addr), Converter, Palette, TilesPerRow);
        }

        /// <summary>
        /// Generates a readable overview of information about the media image
        /// </summary>
        /// <returns>String containing the report</returns>
        virtual public string Report()
        {
            /*
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
             * */
            return null;
        }

        /// <summary>
        /// Generates a human-readable output of byte data
        /// </summary>
        /// <param name="Addr">Chunk address to display</param>
        /// <returns></returns>
        public string GetBytesReadable(Range Addr)
        {
            /*
            // div = amount of complete lines of 16 bytes, mod = left over bytes
            long div16 = Addr.Length / 16;
            long mod16 = Addr.Length % 16;
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
                for (long y = Addr.StartOffset + (div16 * 16); y < Addr.StartOffset + (div16 * 16) + mod16; y++)
                    _out.Append(' ' + Data[y].ToString("X2"));
            }
            return _out.ToString();
            */
            return null;
        }

        /// <summary>
        /// Generates a file map describing the Data buffer
        /// </summary>
        /// <returns>File map</returns>
        virtual public ImageMap AutoMap()
        {
            var _out = new ImageMap();
            _out.Description = "Auto-generated";
            _out.Add(new DataChunkInfo(new Range(0, (int)this.Datastream.Length), "Entire Image"));
            return _out;
        }
        
        #endregion

        

        /// <summary>
        /// Adds a string to the comment list
        /// </summary>
        /// <param name="Comment">The comment to add</param>
        internal void AddComment(string Comment)
        {
            this.Comments.Add(Comment);
        }
   
    }
}
