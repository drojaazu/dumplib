using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace dumplib.Gfx
{
    /// <summary>
    /// Interface for converting a unique color encoding to a standard system Color object
    /// </summary>
    public interface IColorConverter
    {
        /// <summary>
        /// Identifier for this converter
        /// </summary>
        string ID
        {
            get;
        }

        /// <summary>
        /// Description of this converter
        /// </summary>
        string Description
        {
            get;
        }

        /// <summary>
        /// Method to perform the conversion
        /// </summary>
        /// <param name="Data">Raw data to convert</param>
        /// <returns>Standard Color object</returns>
        Color GetColor(byte[] Data);
    }
}