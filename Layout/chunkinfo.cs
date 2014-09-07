using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace dumplib.Layout
{
    /// <summary>
    /// The expected type of information for a given chunk of data
    /// </summary>
    public enum DataTypes
    {
        Raw = 0,
        Text,
        Gfx,
        Audio,
        Other
    }

    /// <summary>
    /// Contains metadata about a chunk of data relative to its source
    /// </summary>
    public interface IChunkInfo
    {
        DataTypes DumpType
        {
            get;
        }

        string Description
        {
            get;
        }

        string ID
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

    public class chunkinfo2 : ChunkInfo
    {
        
    }

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

        virtual public DataTypes DumpType
        {
            get
            {
                return Layout.DataTypes.Raw;
            }
        }

        virtual public string ID
        {
            get
            {
                return "C";
            }
        }

        virtual public string Description
        {
            get;
            protected set;
        }

        virtual public Range Addr
        {
            get;
            protected set;
        }

        virtual public void ParseArgs(string[] Options)
        {

        }
    }

    /// <summary>
    /// Describes a file in a file syste,
    /// </summary>
    public class FileChunkInfo : ChunkInfo
    {
        public FileChunkInfo(Range Addr = null, string Description = null)
        {
            if (string.IsNullOrEmpty(Description)) Description = "File chunk";
            this.Description = Description;
            this.Addr = Addr;
        }

        public override string ID
        {
            get
            {
                return "F";
            }
        }
    }

    /// <summary>
    /// Describes a chunk of graphics data
    /// </summary>
    public class GfxChunkInfo : ChunkInfo
    {
        public GfxChunkInfo(Range Addr = null, string Description = null)
        {
            if (Description == null) Description = string.Format("Graphics chunk");
            this.Description = Description;
            this.Addr = Addr;
            this.TilesPerRow = null;
            this.Subpalette = null;
        }

        public override string ID
        {
            get
            {
                return "G";
            }
        }

        public override DataTypes DumpType
        {
            get
            {
                return Layout.DataTypes.Gfx;
            }
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

        public override void ParseArgs(string[] Options)
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
    public class TextChunkInfo : ChunkInfo
    {
        // Block of in-game text

        public TextChunkInfo(Range Addr = null, string Description = null)
        {
            this.UseTextTable = true;
            this.Addr = Addr;
            if (string.IsNullOrEmpty(Description)) Description = "Text chunk";
            this.Description = Description;
            this.Encoding = null;
            this.UseTextTable = null;
            this.TableID = null;
        }

        public override string ID
        {
            get
            {
                return "T";
            }
        }

        public override DataTypes DumpType
        {
            get
            {
                return Layout.DataTypes.Text;
            }
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

        public override void ParseArgs(string[] Options)
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
    /// Describes a chunk of compiled, machine language code
    /// </summary>
    public class CodeChunkInfo : ChunkInfo
    {
        // Block of compiled code

        public CodeChunkInfo(Range Addr = null, string Description = null)
        {
            this.Addr = Addr;
            if (string.IsNullOrEmpty(Description)) Description = "Machine code chunk";
            this.Description = Description;
        }

        public override string ID
        {
            get
            {
                return "P";
            }
        }

        public string Architecture
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Describes a chunk of compressed data
    /// (this will be further fleshed out as the Compression functions are added to dumpster)
    /// </summary>
    public class CompressedChunkInfo : ChunkInfo
    {
        public CompressedChunkInfo(Range Addr = null, string Description = null)
        {
            if (string.IsNullOrEmpty(Description)) Description = "Compressed chunk";
            this.Description = Description;
            this.Addr = Addr;

        }

        public IChunkInfo DecompressedType
        {
            get;
            private set;
        }

        public override string ID
        {
            get
            {
                return "C";
            }
        }
    }
}
