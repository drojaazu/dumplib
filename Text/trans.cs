using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;


// TO DO (1/30)!!!: Work on table switch nest

namespace romtool.tables
{
    /// <summary>
    /// Contains algorithm to convert text encoding (Unique game encoding -> Unicode)
    /// </summary>
    static class Transcode
    {
        public delegate void Callback(RunWorkerCompletedEventArgs e);

        static public BackgroundWorker bgTBlock = null;
        static public Table T = null;
        static public Callback _callback = null;

        static private void bgTBlock_DoWork(object sender, DoWorkEventArgs e)
        {
            byte[] _in = (byte[])e.Argument;
            string _out = "", chunk = "";
            byte[] TestSeq;

            string currL = T.Initial;
            
            int j; // loop pointer for test sequence
            int remaining = 0;  // bytes remaining in _in array
            int CIndex;

            // begin loop for all bytes in array passed to this method
            for (int i = 0; i < _in.Length; )
            {
                bgTBlock.ReportProgress((i * 100)  / _in.Length);

                remaining = _in.Length - i;

                /*      Search Loop
                 * 1. Make a byte array beginning with the current byte from the for loop
                 * 2. Add x more elements of bytes ahead of current position, where x is the Byte Width (max number of hex digits in logical table)
                 * 3. Check if this byte sequence is in the control code/table switch/end token lists for this logical table
                 * 4. If not, check if this byte sequence is in the dictionary
                 * 5. If not, go back to 1 with using Byte Width - 1
                 * 6. When back down to the original single byte, if it is still not found in the lists of the dict, it is not in the table
                 */
                
                // ensure that the length of the testseq does not go past the upper bound of _in
                j = T.Logical[currL].ByteWidth > remaining ? remaining : T.Logical[currL].ByteWidth;

                // Loop for test byte sequence
                for (; j > 0; j--)
                {
                    // make the testseq as long as j and copy that many bytes from _in @ current position (i)
                    TestSeq = new byte[j];
                    Buffer.BlockCopy(_in, i, TestSeq, 0, j);


                    // check if the testseq is an end token
                    T.Logical[currL].TryGetEndTokens(TestSeq, out CIndex);
                    if (CIndex > -1)
                    {
                        // found this test byte sequence in the end token list
                        chunk = T.Logical[currL].EndTokenArgs[CIndex];
                        i += j;
                        break;
                    }
                    
                    // check if the testseq is a table switch
                    if (T.Logical[currL].Switches.ContainsKey(TestSeq))
                    {
                        //Program.log.o("(table switch)");
                        Tuple<string,int> temp2 = T.Logical[currL].Switches[TestSeq];
                        currL = temp2.Item1;
                        //var args = T.Logical[currL].SwitchArgs[currL];
                        //currL = T.logTables[
                        //currL = args.Item1;
                        i += j;
                        break;
                    }

                    // check if testseq is a control code
                    T.Logical[currL].TryGetControlCodes(TestSeq, out CIndex);
                    if (CIndex > -1)
                    {
                        // Found the test sequence in the control code list
                        if (T.Logical[currL].ControlCodeArgs[CIndex].Length > 1)
                        {
                            // if there is more than 1 element in the control code arg array, use them for formatting
                            chunk = "[" + T.Logical[currL].ControlCodeArgs[CIndex][0];
                            for (int k = 1; k < T.Logical[currL].ControlCodeArgs.Count; k++)
                            {
                                chunk += " " + T.Logical[currL].ControlCodeArgs[CIndex][k];
                            }
                            chunk += "]";
                            i += j;
                            break;
                            // when control codes are properly implemented, it will need to increment i by j plus the number of bytes read past the control code per the formatting
                        }
                        else
                        {
                            chunk = "[" + T.Logical[currL].ControlCodeArgs[CIndex][0] + "]";
                            i += j;
                            break;
                        }

                    }

                    // check if the testseq is in the main dict
                    if (T.Logical[currL].Dict.TryGetValue(TestSeq, out chunk))
                    {
                        // Found the test sequence in the dictionary keys
                        i += j;  // move the pointer in the byte array ahead by however many bytes the key is
                        break;
                    }

                    // if we are down to the last iteration (original byte) and still haven't found anything, the byte isn't found
                    if (j == 1)
                    {
                        chunk = ("[" + TestSeq[0].ToString("X2") + "]");
                        i++;
                    }
                }
                // append our chunk to the out string
                _out += chunk;
                chunk = "";

            }
            e.Result = _out;
        }

        static private void bgTBlock_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Program.MainForm.prog.Value = e.ProgressPercentage;
        }

        static private void bgTBlock_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_callback == null)
            {
                // no call back function defined, just dump to console.
                Console.WriteLine((string)e.Result);
                return;
            }
            _callback(e);
        }

        /// <summary>
        /// Wrapper for async transcoding algorithm
        /// </summary>
        /// <param name="_in">Byte array to be transcoded</param>
        static public void TBlock(byte[] _in)
        {
            if (T == null) throw new NullReferenceException("TBlock: Table is null (Did you forget to load a table file?)");
            if (bgTBlock == null) throw new NullReferenceException("TBlock: BackgroundWorker is null");
            bgTBlock.RunWorkerAsync(_in);
        }

        /// <summary>
        /// Initializes the BackgroundWorker.
        /// </summary>
        static public void SetupTranscode()
        {
            bgTBlock = new BackgroundWorker();
            bgTBlock.WorkerReportsProgress = true;
            bgTBlock.WorkerSupportsCancellation = false;
            bgTBlock.DoWork += new DoWorkEventHandler(bgTBlock_DoWork);
            bgTBlock.ProgressChanged += new ProgressChangedEventHandler(bgTBlock_ProgressChanged);
            bgTBlock.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgTBlock_RunWorkerCompleted);
        }
    }
}
