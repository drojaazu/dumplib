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
            // free managed resources here

            this.Datastream.Dispose();

            this._disposed = true;
        }

        #region     CONSTRUCTOR -=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=- CONSTRUCTOR

        protected MediaImage(Stream Datastream, IDumpConverter Converter = null)
        {
            if (Datastream == null) throw new ArgumentNullException();
            if (Converter == null) this.Datastream = Datastream;
            else this.Datastream = Converter.Normalize(Datastream);
        }

        #endregion  CONSTRUCTOR -=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=- CONSTRUCTOR


        #region     PROTECTED MEMBERS -=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=- PROTECTED MEMBERS

        

        #endregion

        #region     PUBLIC MEMBERS -=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=- PUBLIC MEMBERS
        public Stream Datastream
        {
            get;
            protected set;
        }
        
        public MediaTypes MediaType
        {
            get;
            protected set;
        }

        /// <summary>
        /// Title of the software, if available
        /// </summary>
        public string SoftwareTitle
        {
            get;
            protected set;
        }

        /// <summary>
        /// Name of the hardware this software is intended to run on, if available
        /// </summary>
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
        /// <param name="Addr">Chunk address</param>
        /// <returns></returns>
        public byte[] GetBytes(Range Addr)
        {
            return this.GetBytes(Addr.StartOffset, Addr.Length);
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

        public DataChunk GetChunk(Range Addr)
        {
            return this.GetChunk(new ChunkInfo(Addr));
        }

        public DataChunk GetChunk(long Offset, int Length)
        {
            return this.GetChunk(new Range(Offset, Length));
        }

        /// <summary>
        /// Returns a DataChunk containing the data from the specified address range, as the specified chunk type
        /// </summary>
        /// <param name="ChunkInfo"></param>
        /// <returns></returns>
        public DataChunk GetChunk(IChunkInfo ChunkInfo)
        {
            return new DataChunk(this.GetBytes(ChunkInfo.Addr), ChunkInfo);
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

        /// <summary>
        /// Extracts a graphics tile from the Data buffer
        /// </summary>
        /// <param name="Addr">Chunk address of the tile data</param>
        /// <param name="Converter">Converter to use</param>
        /// <param name="Palette">Color palette to apply</param>
        /// <param name="TilesPerRow">Number of tiles to render per row in the final image</param>
        /// <returns>Bitmapped image of all tiles extracted</returns>
        public Bitmap GetTileGfx(Range Addr, ITileConverter Converter, ColorPalette Palette, int TilesPerRow)
        {
            return dumplib.Gfx.TileGfx.GetTiles(GetBytes(Addr), Converter, Palette, TilesPerRow);
        }

        /// <summary>
        /// Generates a file map describing the Data buffer
        /// </summary>
        /// <returns>File map</returns>
        virtual public ImageMap AutoMap()
        {
            var _out = new ImageMap();
            _out.Description = "Auto-generated";
            _out.Add(new ChunkInfo(new Range(0, (int)this.Datastream.Length), "Entire Image"));
            return _out;
        }
        
        #endregion
        
    }
}
