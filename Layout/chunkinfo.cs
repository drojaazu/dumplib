using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace dumplib.Layout
{
    public enum DumpTypes
    {
        Raw = 0,
        Text,
        Gfx,
        Audio
    }

    public interface IChunkInfo
    {
        DumpTypes DumpType
        {
            get;
        }

        string Description
        {
            get;
        }

        Range Addr
        {
            get;
        }

        // passed a line from the image map, will parse it and apply all the options
        void ParseArgs(string[] Options);
    }

    /*public abstract class ChunkInfo
    {
        public ChunkInfo(Range Addr, string Description)
        {
            if (Addr == null) throw new ArgumentNullException();
            this.Addr = Addr;
            if (string.IsNullOrEmpty(Description)) Description = string.Format("{0} bytes of data", Addr.Length);
            this.Description = Description;
        }

        public abstract Range Addr
        {
            get;
            protected set;
        }

        public abstract string Description
        {
            get;
            protected set;
        }
    }*/

    /// <summary>
    /// Describes a generic chunk of data
    /// </summary>
    public class ChunkInfo : IChunkInfo
    {
        public ChunkInfo(Range Addr = null, string Description = null)
        {
            if(string.IsNullOrEmpty(Description)) Description = "Data chunk";
            this.Description = Description;
            this.Addr = Addr;
        }

        public DumpTypes DumpType
        {
            get
            {
                return Layout.DumpTypes.Raw;
            }
        }

        public string Description
        {
            get;
            protected set;
        }

        public Range Addr
        {
            get;
            protected set;
        }

        public void ParseArgs(string[] Options)
        {

        }
    }

    /// <summary>
    /// Describes a file on a disk
    /// </summary>
    public class FileChunkInfo : IChunkInfo
    {
        public FileChunkInfo(Range Addr = null, string Description = null)
        {
            if (string.IsNullOrEmpty(Description)) Description = "File on disk";
            this.Description = Description;
            this.Addr = Addr;
        }

        public DumpTypes DumpType
        {
            get
            {
                return Layout.DumpTypes.Raw;
            }
        }

        public string Description
        {
            get;
            protected set;
        }

        public Range Addr
        {
            get;
            protected set;
        }

        public void ParseArgs(string[] Options)
        {

        }
    }

    /// <summary>
    /// Describes a chunk of graphics data
    /// </summary>
    public class GfxChunkInfo : IChunkInfo
    {
        public GfxChunkInfo(Range Addr = null, string Description = null)
        {
            if (Description == null) Description = string.Format("Graphics chunk");
            this.Description = Description;
            this.Addr = Addr;
            this.TilesPerRow = null;
            this.Subpalette = null;
            

            /*
            // if there is an argument array, cycle through them
            if (Args != null)
            {
                string[] argsplit;
                for (int t = 0; t < Args.Length; t++)
                {
                    argsplit = Args[t].Split('=');
                    switch (argsplit[0].ToLower())
                    {
                        case "format":
                            switch (argsplit[1].ToLower())
                            {
                                case "1bpp":
                                    this.Format = Gfx.TileFormats.Monochrome;
                                    break;
                                case "sfc_4bpp":
                                    this.Format = Gfx.TileFormats.SuperFamicom_4bpp;
                                    break;
                                case "smd_4bpp":
                                    this.Format = Gfx.TileFormats.Megadrive;
                                    break;
                                case "ngb_2bpp":
                                    this.Format = Gfx.TileFormats.Gameboy;
                                    break;
                                case "sgg_4bpp":
                                    this.Format = Gfx.TileFormats.Sega_8bit;
                                    break;
                                default:
                                    throw new ArgumentException("Unknown argument value " + argsplit[1]);
                            }
                            break;
                        case "subpalette":
                            this.Subpalette = int.Parse(argsplit[1]);
                            break;
                        default:
                            throw new ArgumentException("Unknown argument " + argsplit[0]);
                    }
                }
            }
             * */
        }

        public DumpTypes DumpType
        {
            get
            {
                return Layout.DumpTypes.Gfx;
            }
        }

        public string Description
        {
            get;
            protected set;
        }

        public Range Addr
        {
            get;
            protected set;
        }

        public string TileConverter
        {
            get;
            protected set;
        }

        public int? Subpalette
        {
            get;
            set;
        }


        private int? tilesperrow;
        public int? TilesPerRow
        {
            get
            {
                return this.tilesperrow;
            }
            set
            {
                if (value < 1) value = 1;
                this.tilesperrow = value;
            }
        }

        public void ParseArgs(string[] Options)
        {
            if (Options != null && Options.Length > 0)
            {
                string[] argsplit;
                for (int argloop = 0; argloop < Options.Length; argloop++)
                {
                    argsplit = Options[argloop].Split('=');
                    switch (argsplit[0].ToLower())
                    {
                        case "format":
                            this.TileConverter = argsplit[1];
                            break;
                        case "perrow":
                            int perrow;
                            int.TryParse(argsplit[1], out perrow);
                            this.TilesPerRow = perrow;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Describes a chunk of text data
    /// </summary>
    public class TextChunkInfo : IChunkInfo
    {
        // Block of in-game text

        public TextChunkInfo(Range Addr = null, string Description = null)
        {
            this.UseTextTable = true;
            this.Addr = Addr;
            if (string.IsNullOrEmpty(Description)) Description = "Text chunk";
            this.Description = Description;
            /*
            if (Args != null)
            {
                string[] argsplit;
                for (int t = 0; t < Args.Length; t++)
                {
                    argsplit = Args[t].Split('=');
                    switch (argsplit[0].ToLower())
                    {
                        case "enc":
                            switch (argsplit[1].ToLower())
                            {
                                case "sjis":
                                    this.Encoding = Encoding.GetEncoding(932);
                                    break;
                                case "ascii":
                                    this.Encoding = ASCIIEncoding.ASCII;
                                    break;
                                default:
                                    int codepage;
                                    if (!int.TryParse(argsplit[1], out codepage))
                                        if (!int.TryParse(argsplit[1], System.Globalization.NumberStyles.HexNumber,System.Globalization.CultureInfo.InvariantCulture,out codepage))
                                            throw new ArgumentException("Text -> Encoding -> Value is not a number: " + argsplit[1]);
                                    this.Encoding = Encoding.GetEncoding(codepage);
                                    break;
                            }
                            this.UsesTextTable = false;
                            break;
                        case "tableid":
                            this.TableID = argsplit[1].ToLower();
                            this.UsesTextTable = true;

                            break;

                    }
                }
            }
            */

            this.Encoding = null;
            this.UseTextTable = null;
            this.TableID = null;
        }

        public DumpTypes DumpType
        {
            get
            {
                return Layout.DumpTypes.Text;
            }
        }

        public Range Addr
        {
            get;
            private set;
        }

        public string Description
        {
            get;
            private set;
        }

        public bool? UseTextTable
        {
            get;
            set;
        }

        public Encoding Encoding
        {
            get;
            set;
        }

        public string TableID
        {
            get;
            set;
        }

        public void ParseArgs(string[] Options)
        {
            if (Options != null && Options.Length > 0)
            {
                string[] argsplit;
                for (int argloop = 0; argloop < Options.Length; argloop++)
                {
                    argsplit = Options[argloop].Split('=');
                    switch (argsplit[0].ToLower())
                    {
                        case "enc":
                            switch (argsplit[1].ToLower())
                            {
                                case "sjis":
                                    this.Encoding = Encoding.GetEncoding(932);
                                    break;
                                case "ascii":
                                    this.Encoding = ASCIIEncoding.ASCII;
                                    break;
                                default:
                                    int codepage;
                                    if (!int.TryParse(argsplit[1], out codepage))
                                        if (!int.TryParse(argsplit[1], System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out codepage))
                                            throw new ArgumentException("Text -> Encoding -> Value is not a number: " + argsplit[1]);
                                    this.Encoding = Encoding.GetEncoding(codepage);
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Describes a chunk of pointer data
    /// </summary>
    /*public class PointerChunkInfo : ChunkInfo
    {
        // Pointer table
        public PointerChunkInfo(Range Addr, string Title = null) :
            base(Addr, Title)
        {
            base.Type = ChunkTypes.PointerTable;
            base.Title = Title;
        }

    }*/

    /// <summary>
    /// Describes a chunk of machine language
    /// </summary>
    public class CodeChunkInfo : IChunkInfo
    {
        // Block of compiled code

        public CodeChunkInfo(Range Addr = null, string Description = null)
        {
            this.Addr = Addr;
            if (string.IsNullOrEmpty(Description)) Description = "Code chunk";
            this.Description = Description;
        }

        public DumpTypes DumpType
        {
            get
            {
                return Layout.DumpTypes.Raw;
            }
        }

        public string Description
        {
            get;
            private set;
        }

        public Range Addr
        {
            get;
            private set;
        }

        private string cpu;
        public string CPU
        {
            get { return this.cpu; }
        }

        public void ParseArgs(string[] Options)
        {

        }
    }

    /// <summary>
    /// Describes a chunk of compressed data
    /// </summary>
    public class CompressedChunkInfo : IChunkInfo
    {
        public IChunkInfo DecompressedType
        {
            get;
            private set;
        }

        public DumpTypes DumpType
        {
            get
            {
                return Layout.DumpTypes.Raw;
            }
        }

        public Range Addr
        {
            get;
            private set;
        }

        public string Description
        {
            get;
            private set;
        }

        public void ParseArgs(string[] Options)
        {

        }

        public dumplib.Compression.CompressionFormats Compression
        {
            get;
            private set;
        }
        
        public CompressedChunkInfo(Range Addr = null, string Description = null)
        {
            if (string.IsNullOrEmpty(Description)) Description = "Compressed chunk";
            this.Description = Description;
            this.Addr = Addr;
            /*
            if (Args != null)
            {
                string[] tempsplit;
                for (int t = 0; t < Args.Length; t++)
                {
                    tempsplit = Args[t].Split('=');
                    switch (tempsplit[0].ToLower())
                    {
                        case "cmp":
                            switch (tempsplit[1].ToLower())
                            {
                                case "unknown":
                                    this.Compression = dumplib.Compression.CompressionFormats.Unknown;
                                    break;
                                case "kosinki":
                                    this.Compression = dumplib.Compression.CompressionFormats.Kosinski;
                                    break;
                                default:
                                    throw new ArgumentException("Unknown argument value " + tempsplit[1]);
                            }
                            break;
                    }
                }
            }
             */
        }
    }
}
