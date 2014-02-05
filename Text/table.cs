using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace dumplib.Text
{
    public class Table
    {
        // dictionary of logical tables
        public Dictionary<string, LogicalTable> LogicalTables
        {
            get;
            private set;
        }

        public FileInfo File
        {
            get;
            private set;
        }

        public string Title
        {
            get
            {
                return this.File.Name;
            }
        }

        public Table(string Filepath)
        {
            int loaderrors = 0;
            var errors = new StringBuilder();

            this.LogicalTables = new Dictionary<string, LogicalTable>();
            
            // add first logtable as the nameless 'main' table
            this.LogicalTables.Add("{main}", new LogicalTable("{main}"));
            
            LogicalTable WorkTable = this.LogicalTables["{main}"];
            this.File = new FileInfo(Filepath);
            using (var fstream = new System.IO.StreamReader(Filepath))
            {
                int line = 1;
                string inBuffer;
                byte[] identifier = null;

                while (!fstream.EndOfStream)
                {
                    inBuffer = fstream.ReadLine();

                    if (inBuffer == "" || inBuffer.Substring(0, 1) == "#")
                    {
                        line++;
                        continue;
                    }

                    try
                    {
                        switch (inBuffer.Substring(0, 1))
                        {
                            case "@":
                                // table ID
                                var newtable = inBuffer.Substring(1).Split(' ');
                                string newname = newtable[0];
                                if (this.LogicalTables.ContainsKey(newname)) throw new ArgumentException("A logical table with the name " + newname + " already exists in this table file");
                                // if there is more than 1 element (i.e. there was a space followed by text)
                                // check if that text is a directive
                                // if not, ignore it
                                if (newtable.Length > 1)
                                {
                                    // inherit directive to use the root table as a template when creating a new logical table
                                    if (newtable[1].ToLower() == "inherit")
                                        this.LogicalTables.Add(newname, new LogicalTable(newname, this.LogicalTables["{main}"]));
                                } else
                                    this.LogicalTables.Add(newname, new LogicalTable(newname));
                                WorkTable = this.LogicalTables[newname];
                                break;
                            case "$":
                                //control code
                                identifier = inBuffer.Substring(1, (inBuffer.IndexOf('=') - 1)).HexStringToByteArray();
                                WorkTable.AddEntry(identifier, ParseLine_ControlCode(inBuffer));
                                break;
                            case "/":
                                // end token
                                identifier = inBuffer.Substring(1, (inBuffer.IndexOf('=') - 1)).HexStringToByteArray();
                                WorkTable.AddEntry(identifier, ParseLine_EndToken(inBuffer));
                                break;
                            case "!":
                                // table switch
                                identifier = inBuffer.Substring(1, (inBuffer.IndexOf('=') - 1)).HexStringToByteArray();
                                WorkTable.AddEntry(identifier, ParseLine_TableSwitch(inBuffer));
                                break;
                            default:
                                //standard dictionary entry
                                identifier = inBuffer.Substring(0, (inBuffer.IndexOf('='))).HexStringToByteArray();
                                WorkTable.AddEntry(identifier, ParseLine(inBuffer));
                                break;
                        }
                        if (identifier != null && identifier.Length > WorkTable.ByteWidth) WorkTable.ByteWidth = identifier.Length;
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
            if (loaderrors > 0)
                throw new FileParseException(errors.ToString(), Filepath);
        }

        private bool ValidateStdEntry(string Input, bool NonNormalEntry = false)
        {
            // level 1 - check for =
            if (Input.Contains('='))
            {
                // level 2 - check total length, and length of key and value
                // (check that it's not at the beginning or the end of the string)
                int e = Input.IndexOf('=');
                if (Input.Length > 1 && e > 0 && e != Input.Length - 1)
                    return true;
            }
            return false;
        }

        private bool ValidateNonStdEntry(string Input)
        {
            // level 1 - check that there is an = and enclosing brackets
            if (Input.Contains('=') && Input.Contains('[') && Input.Contains(']'))
            {
                // level 2 - check that the text between entries is not blank and that the label is alphanumeric only
                int b1 = Input.IndexOf('['); int b2 = Input.IndexOf(']');
                string label = Input.Substring(b1 + 1, (b2 - 1) - b1);
                string p = "^[a-zA-Z0-9]*$";
                if (!(b2 == b1 + 1) && (System.Text.RegularExpressions.Regex.IsMatch(label, p)))
                    return true;
            }
            return false;
        }

        private LogicalTable.ControlCode ParseLine_ControlCode(string Input)
        {
            if (!ValidateNonStdEntry(Input)) throw new FormatException("Malformed entry");

            string[] entrysplit = Input.Split(new char[] { '=' }, 2);
            if (entrysplit[1].Contains(','))
            {
                // has arguments, parse them
                var paramsplit = entrysplit[1].Split(',');
                var paramlist = new LogicalTable.ControlCode.Parameter[paramsplit.Length - 1];
                for (int t = 1; t < paramsplit.Length; t++)
                {
                    if (!paramsplit[t].Contains('='))
                        throw new ArgumentException("Malformed parameter");
                    var paramdefsplit = paramsplit[t].Split('=');
                    switch (paramdefsplit[1].ToUpper())
                    {
                        case "%X":
                            paramlist[t-1] =
                                new LogicalTable.ControlCode.Parameter(
                                    LogicalTable.ControlCode.Parameter.NumberType.Hex, paramdefsplit[0]);
                            break;
                        case "%B":
                            paramlist[t-1] =
                                new LogicalTable.ControlCode.Parameter(
                                    LogicalTable.ControlCode.Parameter.NumberType.Binary, paramdefsplit[0]);
                            break;
                        case "%D":
                            paramlist[t-1] =
                                new LogicalTable.ControlCode.Parameter(
                                    LogicalTable.ControlCode.Parameter.NumberType.Decimal, paramdefsplit[0]);
                            break;
                        default:
                            throw new ArgumentException("Invalid placeholder type");
                    }

                }

                // param list object has been created
                if (paramsplit[0].Substring(paramsplit[0].Length - 1) != "]")
                {
                    var formatsplit = paramsplit[0].Split(']');
                    formatsplit[1] = formatsplit[1].Replace("\\n", Environment.NewLine);
                    return new LogicalTable.ControlCode(formatsplit[0].Substring(1), formatsplit[1], paramlist);
                }
                else
                    return new LogicalTable.ControlCode(GetLabel(paramsplit[0]), null, paramlist);
            }
            else  // no parameters
            {
                if (entrysplit[1].Substring(entrysplit[1].Length) != "]")
                {
                    var formatsplit = entrysplit[1].Split(']');
                    formatsplit[1] = formatsplit[1].Replace("\\n", Environment.NewLine);
                    return new LogicalTable.ControlCode(formatsplit[0].Substring(1), formatsplit[1]);
                }
                else
                    return new LogicalTable.ControlCode(GetLabel(entrysplit[1]));
            }

        }

        private LogicalTable.TableSwitch ParseLine_TableSwitch(string Input)
        {
            if (!ValidateNonStdEntry(Input)) throw new FormatException("Malformed entry");

            string[] entrysplit = Input.Split(new char[] { '=' }, 2);
            entrysplit = entrysplit[1].Split(new char[] { ',' }, 2);
            return new LogicalTable.TableSwitch(GetLabel(entrysplit[0]), int.Parse(entrysplit[1]));
        }

        private LogicalTable.EndToken ParseLine_EndToken(string Input)
        {
            if (!ValidateStdEntry(Input, true)) throw new FormatException("Malformed entry");

            var entrysplit = Input.Split('=');
            if (entrysplit[1].Substring(entrysplit[1].Length) != "]")
            {
                var formatsplit = entrysplit[1].Split(']');
                formatsplit[1] = formatsplit[1].Replace("\\n", Environment.NewLine);
                return new LogicalTable.EndToken(formatsplit[0].Substring(1), formatsplit[1]);
            }
            else
                return new LogicalTable.EndToken(GetLabel(entrysplit[1]));
        }

        private string ParseLine(string Input)
        {
            var entrysplit = Input.Split('=');
            return entrysplit[1];
        }

        /// <summary>
        /// Returns the text inside [ ] brackets
        /// </summary>
        internal static string GetLabel(string text)
        {
            int b1 = text.IndexOf('[') + 1;
            int b2 = text.IndexOf(']') - 1;
            return text.Substring(b1, ((text.Length - b1) - (text.Length - b2)) + 1);
        }
    }

    
}
