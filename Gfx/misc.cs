using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dumplib.Gfx
{
    public unsafe static class Misc
    {
        /// <summary> 
        /// A simple zoom resize of a bitmap
        /// </summary> 
        /// <param name="Image">The image to resize</param> 
        /// <param name="Zoom">The size multiplier</param> 
        /// <returns>The resized image</returns> 
        public static Bitmap ZoomImage(Bitmap Image, int Zoom)
        {
            //validation
            if (Image.PixelFormat != PixelFormat.Format8bppIndexed) throw new NotSupportedException("Only supports 8bpp indexed images");
            if (Zoom < 1) throw new ArgumentOutOfRangeException("Zoom must be a positive value");
            if (Zoom == 1) return Image;

            //get the BitmapData from the source image
            BitmapData srcBmpData = Image.LockBits(new Rectangle(Point.Empty, Image.Size), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            // lets cache some values to makes things a tiny bit faster
            // stride is the length of the line in bytes (width * bytes per pixel)
            int srcStride = srcBmpData.Stride,
            //total size in bytes of the source image
                srcBytes = srcStride * Image.Height,
            
            // final image dimensions
                outWidth = Image.Width * Zoom,
                outHeight = Image.Height * Zoom,
            // final image stride is just the stride * zoom
                outStride = srcStride * Zoom,
            // final image size in bytes
                outBytes=outStride * outHeight,
            // a 'row' is the height of a resized, single line of pixels
            // i.e. zoom factor of 4, one pixel is now 4 pixels high
            // the amount of bytes in a row, then, is the size of one final image Stride times the zoom factor
                outRowBytes = outStride * Zoom;

            
            
            // holds the actual data for both images
            byte[] srcData =  new byte[srcBytes],
                outData = new byte[outBytes];
            
            // copy the bitmap data source image to its data array
            Marshal.Copy(srcBmpData.Scan0, srcData, 0, srcBytes);
            // and unlock the source image
            Image.UnlockBits(srcBmpData);
            // now that we have the bitmap data stored in a managed array, we will read through each pixel
            // and write out the resized data to the output array
            
            //value caching for the loop
            int pixelPtr, srcH;
            byte[] thisstride;

            // outer loop is the height of the image
            for (int srcLoopH = 0; srcLoopH < Image.Height; srcLoopH++)
            {
                // we'll generate one line of the zoomed image, then duplicate it zoom times vertcally
                //  holds the data for the scanline
                thisstride = new byte[outStride];
                // keeps tracks of which pixel to write to in the output scanline
                pixelPtr = 0;
                srcH = (srcLoopH * srcStride);

                // loop through all the data in the line...
                for (int srcLoopW = 0; srcLoopW < srcStride; srcLoopW++)
                {
                    // get the pixel value from the source to enlarge
                    byte thispixel = srcData[srcH + srcLoopW];
                    // duplicate (zoom) number of bytes to the output scanline
                    for (int pixelDupe = 0; pixelDupe < Zoom; pixelDupe++)
                        thisstride[pixelPtr + pixelDupe] = thispixel;
                    pixelPtr += Zoom;
                }
                // copy the full scanline to the output array (zoom) number of times
                for (int j = 0; j < Zoom; j++)
                    Buffer.BlockCopy(thisstride, 0, outData, (outRowBytes * srcLoopH) + (outStride * j), thisstride.Length);
            }
            Bitmap _out = new Bitmap(outWidth, outHeight, PixelFormat.Format8bppIndexed);
            var writetoout = _out.LockBits(new Rectangle(Point.Empty, _out.Size), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            // safely copy our new bitmap data as a byte array into the final image
            Marshal.Copy(outData, 0, writetoout.Scan0, outData.Length);
            _out.UnlockBits(writetoout);

            // creating a new bitmap with our modified data requires an intptr, so we'll need to get a pointer to the array...



            //IntPtr outstart = System.Runtime.InteropServices.Marshal.AllocHGlobal(outData.Length);
            //System.Runtime.InteropServices.Marshal.Copy(outData, 0, outstart, outData.Length);
            /*fixed (byte* outStart = outData)
            {
                // ... and convert that pointer to an intptr
                    
                _out = new Bitmap(outW, outH, outStride, Image.PixelFormat, (IntPtr)outStart);
                //BitmapData test = new BitmapData();
                    
            }*/
            // release the original image
            //System.Runtime.InteropServices.Marshal.FreeHGlobal(outstart);
            

            //copy over the palette from the original
            _out.Palette = Image.Palette;
            return _out;
        }
    }
}
