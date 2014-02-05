using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Collections.Generic;

namespace dumplib.Gfx
{
    public static class Misc
    {
        public static List<KeyValuePair<string,TileFormats>> GetTileFormatsList()
        {
            var _out = new List<KeyValuePair<string,TileFormats>>();
            TileFormats temp;
            for (byte d=0; d < 11; d++)
            {
                temp=(TileFormats)d;
                _out.Add(new KeyValuePair<string, TileFormats>(GetTileFormatInfo(temp), temp));
            }
            
            return _out;
        }

        public static string GetTileFormatInfo(TileFormats Format)
        {
            switch (Format)
            {
                case TileFormats.Famicom:
                    return "2bpp Nintendo Famicom";
                case TileFormats.Gameboy:
                    return "2bpp Nintendo Gameboy + Color, Super Famicom";
                case TileFormats.Sega_8bit:
                    return "4bpp Sega GameGear, Master System; Bandai Wonderswan Color";
                case TileFormats.Megadrive:
                    return "4bpp Sega Megadrive; X68000 computer system";
                case TileFormats.SuperFamicom_3bpp:
                    return "3bpp Nintendo Super Famicom";
                case TileFormats.SuperFamicom_4bpp:
                    return "4bpp Nintendo Super Famicom; NEC PC Engine";
                case TileFormats.SuperFamicom_8bpp:
                    return "8bpp Nintendo Super Famicom";
                case TileFormats.SuperFamicom_Mode7:
                    return "8bpp Super Famicom Mode 7";
                case TileFormats.Monochrome:
                    return "1bpp Monochrome; used in many systems";
                case TileFormats.VirtualBoy:
                    return "2bpp Nintendo Virtual Boy";
                case TileFormats.NeoGeoPocket:
                    return "2bpp SNK NeoGeo Pocket Color";
                default:
                    return "No extended information available";
            }

        }
        /// <summary> 
        /// A simple zoom resize of a bitmap
        /// </summary> 
        /// <param name="Image">The image to resize</param> 
        /// <param name="Zoom">The size multiplier</param> 
        /// <returns>The resized image</returns> 
        public static Bitmap ZoomImage(Bitmap Image, int Zoom)
        {
            //validation
            //if (Image.PixelFormat != PixelFormat.Format8bppIndexed) throw new ArgumentException("Bitmap image must be indexed");
            if (Zoom < 1) throw new ArgumentOutOfRangeException("Zoom must be a positive value");
            if (Zoom == 1) return Image;

            // lets cache some variables to makes things a tiny bit faster
            int outW = Image.Width * Zoom;
            int outH = Image.Height * Zoom;
            byte[] outData; byte[] srcData;
            int outBytes; int srcBytes;
            int outScanlnBytes, outRowBytes;
            int srcStride;

            Bitmap _out;

            unsafe
            {
                // lock the image data in memory to ward off the GC and get a pointer to the bytes
                BitmapData srcBmpData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadOnly, Image.PixelFormat);

                // stride is the length of the line in bytes (width * bytes per pixel) - Bpp SHOULD be 1, so this should just be equal to Image.Width
                srcStride = srcBmpData.Stride;

                srcBytes = srcStride * Image.Height;
                outScanlnBytes = srcStride * Zoom;
                outRowBytes = outScanlnBytes * Zoom;
                outBytes = outScanlnBytes * outH;

                outData = new byte[outBytes];
                srcData = new byte[srcBytes];
                // copy the bytes from the source bitmap to the source array
                System.Runtime.InteropServices.Marshal.Copy(srcBmpData.Scan0, srcData, 0, srcBytes);

                // okay, we FINALLY have an array of bytes from the source image
                // outler loop is the height of the image
                for (int srcLoopH = 0; srcLoopH < Image.Height; srcLoopH++)
                {
                    // we'll generate one line of the zoomed image, then duplicate it (zoom) times vertcally
                    //  holds the data for the scanline
                    byte[] scanln = new byte[outScanlnBytes];
                    // keeps tracks of which pixel to write to in the output scanline
                    int pixelPtr = 0;

                    // loop through all the bytes in the line...
                    for (int loop_W = 0; loop_W < srcStride; loop_W++)
                    {
                        // get the pixel value from the source to enlarge
                        byte thispixel = srcData[(srcLoopH * srcStride) + loop_W];
                        // duplicate (zoom) number of bytes to the output scanline
                        for (int pixelDupe = 0; pixelDupe < Zoom; pixelDupe++)
                            scanln[pixelPtr + pixelDupe] = thispixel;
                        pixelPtr += Zoom;
                    }
                    // copy the full scanline to the output array (zoom) number of times
                    for (int j = 0; j < Zoom; j++)
                        Buffer.BlockCopy(scanln, 0, outData, (outRowBytes * srcLoopH) + (outScanlnBytes * j), scanln.Length);
                }

                // creating a new bitmap with our modified data requires an intptr, so we'll need to get a pointer to the array...
                fixed (byte* outStart = outData)
                {
                    // ... and convert that pointer to an intptr
                    _out = new Bitmap(outW, outH, outScanlnBytes, Image.PixelFormat, new IntPtr(outStart));
                }
                // release the original image
                Image.UnlockBits(srcBmpData);
            }
            //copy over the palette from the original
            _out.Palette = Image.Palette;
            return _out;
        }
    }
}
