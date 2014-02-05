using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace dumplib.Layout
{
    public enum ChunkTypes : byte
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("Text")]
        Text = 1,
        [Description("Pointer Table")]
        PointerTable = 2,
        [Description("Compressed Data")]
        Compressed = 3,
        [Description("Compiled Code")]
        CompiledCode = 4,
        [Description("Graphics")]
        Graphics = 5,
        [Description("Palette")]
        Palette = 6,
        [Description("ASCII Text")]
        ASCII = 7,
        [Description("SJIS Text")]
        SJIS = 8,
        [Description("File on disk")]
        File = 9,
        [Description("Generic")]
        Generic = 0xff
    }

    public abstract class ChunkEntry
    {
        public ChunkEntry(Range Addr, string Description, string[] Args)
        {
            this.Addr = Addr;
            this.Description = Description;
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

        public ChunkTypes Type
        {
            get;
            protected set;
        }
    }

    /// <summary>
    /// Describes a generic chunk of data
    /// </summary>
    public class Chunk : ChunkEntry
    {
        public Chunk(Range Addr, string Description, string[] Args = null) :
            base(Addr, Description, Args)
        {
            base.Type = ChunkTypes.Generic;
        }
    }

    /// <summary>
    /// Describes a file on a disk
    /// </summary>
    public class FileChunk : ChunkEntry
    {
        public FileChunk(Range Addr, string Description, string[] Args = null) :
            base(Addr, Description, Args)
        {
            base.Type = ChunkTypes.File;
        }
    }

    /// <summary>
    /// Describes a chunk of graphics data
    /// </summary>
    public class GfxChunk : ChunkEntry
    {
        public GfxChunk(Range Addr, string Description, string[] Args = null) :
            base(Addr, Description, Args)
        {
            base.Type = ChunkTypes.Graphics;
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
        }

        public Gfx.TileFormats Format
        {
            get;
            private set;
        }

        public int Subpalette
        {
            get;
            private set;
        }

        public string Title
        {
            get
            {
                return "Graphics @ " + this.Addr.StartOffset.ToString("X") + " " + this.Description;
            }
        }
    }

    /// <summary>
    /// Describes a chunk of text data
    /// </summary>
    public class TextChunk : ChunkEntry
    {
        // Block of in-game text

        public TextChunk(Range Addr, string Description, string[] Args = null) :
            base(Addr, Description, Args)
        {
            base.Type = ChunkTypes.Text;
            this.UsesTextTable = true;
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
        }

        public bool UsesTextTable
        {
            get;
            private set;
        }

        public Encoding Encoding
        {
            get;
            private set;
        }

        public string TableID
        {
            get;
            private set;
        }

        public string Title
        {
            get
            {
                return "Text @ " + this.Addr.StartOffset.ToString("X") + " " + this.Description;
            }
        }
    }

    /// <summary>
    /// Describes a chunk of pointer data
    /// </summary>
    public class PointerChunk : ChunkEntry
    {
        // Pointer table
        public PointerChunk(Range Addr, string Description, string[] Args = null) :
            base(Addr, Description, Args)
        {
            base.Type = ChunkTypes.PointerTable;
        }

        public string Title
        {
            get
            {
                return "Pointer Table @ " + this.Addr.StartOffset.ToString("X") + " " + this.Description;
            }
        }
    }

    /// <summary>
    /// Describes a chunk of machine language
    /// </summary>
    public class CodeChunk : ChunkEntry
    {
        // Block of compiled code

        public CodeChunk(Range Addr, string Description, string[] Args = null) :
            base(Addr, Description, Args)
        {
            base.Type = ChunkTypes.CompiledCode;
        }

        private string cpu;
        public string CPU
        {
            get { return this.cpu; }
        }

        public string Title
        {
            get
            {
                return "Code @ " + this.Addr.StartOffset.ToString("X") + " " + this.Description;
            }
        }
    }

    /// <summary>
    /// Describes a chunk of compressed data
    /// </summary>
    public class CompressedChunk : ChunkEntry
    {
        public ChunkTypes DecompressedType
        {
            get;
            private set;
        }

        public dumplib.Compression.CompressionFormats Compression
        {
            get;
            private set;
        }
        
        public CompressedChunk(Range Addr, string Description, string[] Args = null) :
            base(Addr, Description, Args)
        {
            base.Type = ChunkTypes.Compressed;
            this.DecompressedType = ChunkTypes.Unknown;
            this.Compression = dumplib.Compression.CompressionFormats.Unknown;

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
        }

        public string Title
        {
            get
            {
                return "Compressed Data @ " + this.Addr.StartOffset.ToString("X") + " " + this.Description;
            }
        }
    }
}
