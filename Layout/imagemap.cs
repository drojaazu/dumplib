using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace dumplib.Layout
{
    /// <summary>
    /// A collection of address ranges that map to a file with supporting metadata to describe each range.
    /// </summary>
    public class ImageMap
    {
        public static Dictionary<string, Type> ChunkTypes = new Dictionary<string, Type>()
        {
            {"D",typeof(DataChunkInfo)},    
            {"T",typeof(TextChunkInfo)},
            {"G",typeof(GfxChunkInfo)},
            {"C",typeof(CompressedChunkInfo)},
            {"P",typeof(CodeChunkInfo)},
            {"F",typeof(FileChunkInfo)}
        };

        #region     CONSTRUCTOR -=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=- CONSTRUCTOR
        public ImageMap(Stream Datastream, string Description = null)
        {
            if (Datastream == null) throw new ArgumentNullException();
            
            Datastream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(Datastream);

            this.Entries = new List<IChunkInfo>();
            int loaderrors = 0, line = 1;
            var errors = new StringBuilder();
            string inBuffer = null;
            
            while (!reader.EndOfStream)
            {
                inBuffer = reader.ReadLine();
                //check for blank lines and comments and ignore
                if (inBuffer == "" || inBuffer.Substring(0, 1) == "#")
                {
                    line++;
                    continue;
                }

                try
                {
                    Entries.Add(ParseLine(inBuffer));
                }
                catch (Exception ex)
                {
                    loaderrors++;
                    errors.Append("Line ").Append(line.ToString()).Append(": ").AppendLine(ex.Message);
                    if (loaderrors >= 5)
                    {
                        errors.Append("At least 5 syntax errors detected, loading aborted");
                        break;
                    }
                }
                finally
                {
                    line++;
                }
            }
            
            if (loaderrors > 0) throw new MapParseException(errors.ToString());

            if (string.IsNullOrEmpty(Description))
            {
                if (Datastream is FileStream) Description = (Datastream as FileStream).Name;
                else Description = "Map from data stream";
            }
            this.Description = Description;
        }

        public ImageMap(string Description = null)
        {
            this.Entries = new List<IChunkInfo>();
            if (string.IsNullOrEmpty(Description)) Description = "Blank map";
            this.Description = Description;
        }

        /*public ImageMap(IList<IChunkInfo> InitList, string Description = null)
        {
            if (InitList == null) throw new ArgumentNullException("Initial list cannot be null.");
            this.Entries = InitList;
            this.Description = Description;
        }*/

        #endregion  CONSTRUCTOR -=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=- CONSTRUCTOR

        #region     PUBLIC MEMBERS -=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=- PUBLIC MEMBERS

        /// <summary>
        /// Short description of the image map
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// List of map entries in the file
        /// </summary>
        public List<IChunkInfo> Entries
        {
            get;
            private set;
        }

        /// <summary>
        /// Adds a ChunkEntry object to the image map
        /// </summary>
        /// <param name="NewEntry"></param>
        public void Add(IChunkInfo NewEntry)
        {
            if (NewEntry == null) throw new ArgumentNullException();
            this.Entries.Add(NewEntry);
        }

        #endregion  PUBLIC MEMBERS -=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=- PUBLIC MEMBERS

        #region     PRIVATE MEMBERS -=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=- PRIVATE MEMBERS

        // returns a ChunkEntry object from a line in the definition file
        // May 2014 - oh boy more uncommented code. At least this is relatively simple
        // time to rewrite and comment!
        private Layout.IChunkInfo ParseLine(string text)
        {
            // do some simple checks to make sure the line is valid
            if (!text.Contains("[") || !text.Contains("]") ||
                !(text.Contains("-") || text.Contains("+")) ||
                text.Length < 6)
                throw new FormatException("Entry not properly formatted");

            //get the text between the [] brackets, which should be the address and any arguments
            string offsets = dumplib.Text.Table.GetLabel(text);
            string[] args = null;
            
            // a comma delimits the address and all args, so if there's a comma in the text between []...
            if (offsets.Contains(","))
            {
                //... then split by args, set the first result to offsets and the rest to the args array
                string[] temp = offsets.Split(',');
                args = new string[temp.Length-1];
                offsets = temp[0];
                //Buffer.BlockCopy(temp, 1, args, 0, args.Length);
                Array.Copy(temp, 1, args, 0, args.Length);
            }

            // the offset string can have a - or + to delimit between the two numbers
            long first; int second;
            if (offsets.IndexOf('-') > 0)
            {
                string[] offsetsplit = offsets.Split('-');
                first = long.Parse(offsetsplit[0], System.Globalization.NumberStyles.HexNumber);
                second = int.Parse(offsetsplit[1], System.Globalization.NumberStyles.HexNumber);
                if (second < first) throw new ArgumentOutOfRangeException("Ending offset cannot be less than start offset");
                second = (second - (int)first) + 1;
            }
            else if (offsets.IndexOf('+') > 0)
            {
                string[] offsetsplit = offsets.Split('+');
                first = long.Parse(offsetsplit[0], System.Globalization.NumberStyles.HexNumber);
                second = int.Parse(offsetsplit[1], System.Globalization.NumberStyles.HexNumber);
                if (second < 1) throw new ArgumentOutOfRangeException("Length cannot be less than 1");
            }
            else throw new FormatException("Offsets incorrectly formatted");
                
            string desc = text.Substring(text.IndexOf(']') + 1);
            //if (!ChunkTypes.ContainsKey(text.Substring(0, 1).ToUpper())) throw new ArgumentException("Specified chunk type not found");
            object[] thisinfo = new object[2];
            thisinfo[0] = new Range(first, second);
            thisinfo[1] = desc;
            IChunkInfo _out = Activator.CreateInstance(ChunkTypes[text.Substring(0, 1).ToUpper()],thisinfo) as IChunkInfo;

            if(args != null && args.Length > 0) _out.ParseArgs(args);

            return _out;
        }
        #endregion PRIVATE MEMBERS -=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=-=:=-=:=--=:=- PRIVATE MEMBERS
    }
}