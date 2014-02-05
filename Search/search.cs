using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using dumplib.Layout;

namespace dumplib.Search
{
    public static class Search
    {
        /// <summary>
        /// Searches for byte patterns
        /// </summary>
        /// <param name="Data">The data chunk to search</param>
        /// <param name="Pattern"></param>
        /// <returns>List of locations that matched the pattern</returns>
        public static List<Range> Pattern(byte[] Data, int[] Pattern)
        {
            var MatchList = new List<Range>();
            // outler loop, length of the Data, increases by 1 each time unless a pattern was found
            uint outerptr; // this will be our outer loop counter
            uint innerptr; // for the pattern loop; we want to keep the scope larger so we can track it after the loop
            byte baseline; // the baseline byte to use with the pattern
            uint patlen = (uint)Pattern.Length;
            uint finalbyte = (uint)Data.LongLength - patlen;
            for (outerptr = 0; outerptr < finalbyte; )
            {
                // get a byte, this will be the baseline for this iteration
                baseline = Data[outerptr];
                // need another loop to cycle through the Pattern
                for (innerptr = 0; innerptr < patlen; innerptr++)
                    // check that the next bytes after the outer pointer match the pattern relative to the baseline
                    // if any do not match, break the loop
                    if (Data[(outerptr + 1) + innerptr] != (baseline + Pattern[innerptr])) break;
                
                // out of the loop, let's see if the innerptr made it to the end of the pattern (i.e. found a match)
                if (innerptr == patlen)
                {
                    MatchList.Add(new Range(outerptr, patlen));
                    // move the outer pointer by the whole pattern, since we'll not need to check those bytes
                    outerptr += patlen;
                }
                else
                    outerptr++;
                //if (Pattern.Length > Data.Length - outerptr) break;
            }
            return MatchList;
        }

        /// <summary>
        /// Searches for sequences of bytes
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="Sequence"></param>
        /// <returns></returns>
        public static List<Range> Sequence(byte[] Data, byte[] Sequence)
        {
            var MatchList = new List<Range>();

            uint outerptr;
            uint seqlen = (uint)Sequence.Length;
            uint finalbyte = (uint)(Data.LongLength) - seqlen;
            
            for (outerptr = 0; outerptr < finalbyte; )
            {
                // copy a chunk from the data
                byte[] tempseq = new byte[seqlen];
                // argh damn you bockcopy and your signed int addressing!!!!!
                //Buffer.BlockCopy(Data, (int)outerptr, tempseq, 0, Sequence.Length);
                Array.Copy(Data, outerptr, tempseq, 0, seqlen);
                // test if that sequence is equal to the test sequence
                if (tempseq.SequenceEqual<byte>(Sequence))
                {
                    // it's equal, count it as a match
                    MatchList.Add(new Range(outerptr, seqlen));
                    outerptr += seqlen;
                }
                else
                    outerptr++;
                //if (Sequence.Length > Data.Length - outerptr) break;
            }
            return MatchList;
        }
    }
}
