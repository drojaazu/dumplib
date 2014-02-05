using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dumplib.Layout
{
    public class ImageMap
    {
        public ImageMap(string Filepath)
        {
            if (Filepath == null) throw new ArgumentNullException("Filepath cannot be null");

            this.Entries = new List<ChunkEntry>();
            int loaderrors = 0;
            var errors = new StringBuilder();

            using (var _s = new System.IO.StreamReader(Filepath))
            {
                int line = 1;
                string inBuffer = null;

                while (!_s.EndOfStream)
                {
                    inBuffer = _s.ReadLine();
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
            }
            if (loaderrors > 0) throw new FileParseException(errors.ToString(), Filepath);
            this.Description = System.IO.Path.GetFileName(Filepath);
        }

        public ImageMap()
        {
            this.Entries = new List<ChunkEntry>();
            this.Description = "Blank file map";
            this.Filepath = null;
        }

        public ImageMap(List<ChunkEntry> InitList, string Description = null)
        {
            if (InitList == null) throw new ArgumentNullException("Initial list cannot be null.");
            this.Entries = InitList;
        }

        public string Description
        {
            get;
            set;
        }

        public string Filepath
        {
            get;
            private set;
        }

        public List<ChunkEntry> Entries
        {
            get;
            private set;
        }

        public void Add(ChunkEntry NewEntry)
        {
            if (NewEntry == null) throw new ArgumentNullException("The entry cannot be null");
            this.Entries.Add(NewEntry);
        }

        private bool ValidateAddrEntry(string _input)
        {
            if (!_input.Contains('[') || !_input.Contains(']') ||
                !(_input.Contains('-') || _input.Contains('+')) ||
                _input.Length < 6)
                return false;

            return true;
        }

        // returns a ChunkEntry object from a line in the definition file
        private Layout.ChunkEntry ParseLine(string text)
        {
            if (!ValidateAddrEntry(text)) throw new FormatException("Entry not properly formatted");

            string offsets = dumplib.Text.Table.GetLabel(text);
            string[] args = null;
            int d1 = offsets.IndexOf('-'); int d2 = offsets.IndexOf('+');
            uint first; uint second;
            
            // a comma delimits the address from the options
            if (offsets.Contains(','))
            {
                string[] temp = offsets.Split(',');
                args = new string[temp.Length-1];
                offsets = temp[0];
                for (int t = 0; t < args.Length; t++) args[t] = temp[t + 1];
            }
            if (d1 == -1)
            {
                string[] offsetsplit = offsets.Split('+');
                first = uint.Parse(offsetsplit[0], System.Globalization.NumberStyles.HexNumber);
                second = uint.Parse(offsetsplit[1], System.Globalization.NumberStyles.HexNumber);
                if (second < 1) throw new ArgumentOutOfRangeException("Length cannot be less than 1");
            }
            else
            {
                string[] offsetsplit = offsets.Split('-');
                first = uint.Parse(offsetsplit[0], System.Globalization.NumberStyles.HexNumber);
                second = uint.Parse(offsetsplit[1], System.Globalization.NumberStyles.HexNumber);
                if (second < first) throw new ArgumentOutOfRangeException("Ending offset cannot be less than start offset");
                second = (second - first) + 1;
            }

            string desc = text.Substring(text.IndexOf(']') + 1);
            
            switch (text.Substring(0,1).ToLower())
            {
                case "t":
                    return new TextChunk(new Range(first, second), desc, args);
                case "p":
                    return new PointerChunk(new Range(first, second), desc, args);
                case "g":
                    return new GfxChunk(new Range(first, second), desc, args);
                case "c":
                    return new CodeChunk(new Range(first, second), desc, args);
                default:
                    throw new ArgumentException("Invalid BlockType code");
            }
        }

    }
}