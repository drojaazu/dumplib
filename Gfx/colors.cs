using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;

namespace dumplib.Gfx
{
    public interface IColorConverter
    {
        string ID
        {
            get;
        }

        string Description
        {
            get;
        }

        Color GetColor(byte[] Data);
    }
}