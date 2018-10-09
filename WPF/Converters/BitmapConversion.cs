using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

namespace WPF
{
    public static class BitmapConversion
    {
        public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        public static BitmapSource BitmapToBitmapSource(Bitmap source)
        {
            if (source == null)
            {
                return null;
            }
            return Imaging.CreateBitmapSourceFromHBitmap(
                source.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        public static Bitmap BitmapSourceToBitmap(BitmapSource source)
        {
            if (source == null)
            {
                return null;
            }
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            Bitmap result = new Bitmap(width, height);
            BitmapData bits = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int size = width * height * 4;
            byte[] argb = new byte[size];
            source.CopyPixels(argb, bits.Stride, 0);
            Marshal.Copy(argb, 0, bits.Scan0, size);
            return result;
        }
    }
}
