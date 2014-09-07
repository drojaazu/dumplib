using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dumplib.Layout
{
    /// <summary>
    /// Represents a chunk of dumped data
    /// </summary>
    public class DataChunk
    {
        /// <summary>
        /// Information about the data relative to its source
        /// </summary>
        public IChunkInfo Info
        {
            get;
            protected set;
        }

        /// <summary>
        /// The chunk of raw data
        /// </summary>
        public byte[] Data
        {
            get;
            protected set;
        }

        public DataChunk(byte[] Data, IChunkInfo Info = null)
        {
            if (Data == null) throw new ArgumentNullException("Data cannot be null");
            this.Data = Data;
            this.Info = Info;
        }
    }
}
