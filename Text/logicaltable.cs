using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dumplib.Text
{
    /// <summary>
    /// Represents logical table sections from table files
    /// </summary>
    public class LogicalTable
    {
        public Dictionary<byte[], string> StdDict
        {
            get;
            private set;
        }

        public Dictionary<byte[], EndToken> EndTokens
        {
            get;
            private set;
        }

        public Dictionary<byte[], ControlCode> ControlCodes
        {
            get;
            private set;

        }
        public Dictionary<byte[], TableSwitch> TableSwitches
        {
            get;
            private set;
        }
        
        public int ByteWidth
        {
            get;
            set;
        }

        public string ID
        {
            get;
            private set;
        }

        public LogicalTable(string ID)
        {
            if (ID.Contains(',')) throw new ArgumentException("Table IDs cannot contain commas");
            this.ID = ID;
            this.ByteWidth = 1;
            this.StdDict = new Dictionary<byte[], string>(new ByteArrayComparer());
            this.ControlCodes = new Dictionary<byte[], ControlCode>(new ByteArrayComparer());
            this.EndTokens = new Dictionary<byte[], EndToken>(new ByteArrayComparer());
            this.TableSwitches = new Dictionary<byte[], TableSwitch>(new ByteArrayComparer());
        }

        public LogicalTable(string ID, LogicalTable Template)
        {
            if (ID.Contains(',')) throw new ArgumentException("Table IDs cannot contain commas");
            this.ID = ID;
            this.ByteWidth = Template.ByteWidth;
            this.StdDict = new Dictionary<byte[], string>(Template.StdDict, new ByteArrayComparer());
            this.ControlCodes = new Dictionary<byte[], ControlCode>(Template.ControlCodes, new ByteArrayComparer());
            this.EndTokens = new Dictionary<byte[], EndToken>(Template.EndTokens, new ByteArrayComparer());
            this.TableSwitches = new Dictionary<byte[], TableSwitch>(Template.TableSwitches, new ByteArrayComparer());
        }

        public void AddEntry(byte[] Identifier, ControlCode NewEntry)
        {
            if (this.ControlCodes.ContainsKey(Identifier))
                this.ControlCodes[Identifier] = NewEntry;
            else
                this.ControlCodes.Add(Identifier, NewEntry);
        }
        
        public void AddEntry(byte[] Identifier, TableSwitch NewEntry)
        {
            if (this.TableSwitches.ContainsKey(Identifier))
                this.TableSwitches[Identifier] = NewEntry;
            else
                this.TableSwitches.Add(Identifier, NewEntry);
        }

        public void AddEntry(byte[] Identifier, EndToken NewEntry)
        {
            if (this.EndTokens.ContainsKey(Identifier))
                this.EndTokens[Identifier] = NewEntry;
            else
                this.EndTokens.Add(Identifier, NewEntry);
        }

        public void AddEntry(byte[] Identifier, string NewEntry)
        {
            if (this.StdDict.ContainsKey(Identifier))
                this.StdDict[Identifier] = NewEntry;
            else
                this.StdDict.Add(Identifier, NewEntry);
        }

        public class EndToken
        {
            public string Label
            {
                get;
                private set;
            }

            public string Formatting
            {
                get;
                private set;
            }

            public EndToken(string Label, string Formatting = null)
            {
                if (Label == null) throw new ArgumentException("Label cannot be null");
                this.Label = Label;
                if (Formatting == null) this.Formatting = string.Empty;
                else this.Formatting = Formatting;
            }
        }

        public class TableSwitch
        {
            public int Matches
            {
                get;
                private set;
            }

            public string TableID
            {
                get;
                private set;
            }

            public TableSwitch(string TableID, int Matches)
            {
                if (TableID == null) throw new ArgumentException("TableID cannot be null");
                if (Matches < -1) throw new ArgumentException("Invalid matches value");

                this.Matches = Matches;
                this.TableID = TableID;
            }
        }

        public class ControlCode
        {
            public string Label
            {
                get;
                private set;
            }

            public string Formatting
            {
                get;
                private set;
            }

            public Parameter[] Params
            {
                get;
                private set;
            }

            public ControlCode(string Label, string Formatting = null, Parameter[] Params = null)
            {
                if (Label == null) throw new ArgumentException("Label cannot be null");
                this.Label = Label;
                this.Params = Params;
                if (Formatting == null) this.Formatting = string.Empty;
                else this.Formatting = Formatting;
            }

            public class Parameter
            {
                public enum NumberType : byte
                {
                    Hex = 0,
                    Decimal,
                    Binary
                }

                public NumberType Type
                {
                    get;
                    private set;
                }

                public string Label
                {
                    get;
                    private set;
                }

                public Parameter(NumberType Type, string Label)
                {
                    if (Label == null) throw new ArgumentException("Label cannot be null");
                    this.Type = Type;
                    this.Label = Label;
                }
            }
        }
    }
}
