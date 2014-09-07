using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using dumplib.Layout;

namespace dumplib.Image
{
    public class DiskImage : MediaImage
    {
        public DiskImage(Stream DataStream, IDumpConverter Converter = null) : base(DataStream, Converter)
        {

        }

        public IVolume[] Volumes
        {
            get;
            protected set;
        }
    }

    public interface IVolume
    {
        string ID
        {
            get;
        }

        string Label
        {
            get;
        }

        Directory Root
        {
            get;
        }

        FilesystemObject GetObject(string Name);

        byte[] GetFile(string Name);
    }

    public class Directory : FilesystemObject
    {
        public FilesystemObject[] Contents
        {
            get;
            set;
        }
    }

    public class File : FilesystemObject
    {
        /// <summary>
        /// Generic description of the type of data in this file
        /// </summary>
        public DataTypes DataType
        {
            get;
            set;
        }

        public byte[] Data
        {
            get;
            protected set;
        }

        public File(string Name, ulong Offset)
        {
            this.Name = Name;
            this.Offset = Offset;
        }
    }

    /// <summary>
    /// Represents an object in a file system
    /// </summary>
    public class FilesystemObject
    {
        public virtual string Name
        {
            get;
            protected set;
        }

        /// <summary>
        /// Starting offset of the file in the image
        /// (note: can't use Range because not all files are contiguous)
        /// </summary>
        public ulong Offset
        {
            get;
            protected set;
        }
    }
}
