using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dumplib.Layout
{
    public class DataChunk
    {
        public IChunkInfo Info
        {
            get;
            private set;
        }

        public byte[] Data
        {
            get;
            private set;
        }

        public DataChunk(byte[] Data, IChunkInfo Info = null)
        {
            if (Data == null) throw new ArgumentNullException("Data cannot be null");
            this.Data = Data;
            this.Info = Info;
        }
    }
}
