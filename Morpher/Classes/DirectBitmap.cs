using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace Morpher
{
      public class DirectBitmap : IDisposable
        {
            public Bitmap Bitmap { get; private set; }
            public BitmapSource BitmapSource
            {
                get { return ImageUtility.BitmapToSource(Bitmap); }
                private set { BitmapSource = value; }
            }
            public int[] Bits { get; private set; }
            public bool Disposed { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }

            protected GCHandle BitsHandle { get; private set; }

            public DirectBitmap(int width, int height)
            {
                Width = width;
                Height = height;
                Bits = new int[width * height];
                BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
                Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
            }

            public DirectBitmap(Bitmap bitmap)
            {
                Width = bitmap.Width;
                Height = bitmap.Height;
                Bits = new int[Width * Height];
                BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
                Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppArgb, BitsHandle.AddrOfPinnedObject());
                Rectangle rect = new Rectangle(0, 0, Width, Height);
                Console.WriteLine(bitmap.PixelFormat);
                BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            unsafe
                {
                    int* pos = (int*)data.Scan0;
                    for (int i = 0; i < data.Height; i++)
                    {
                        for (int j = 0; j < data.Width; j++)
                        {
                            Color color = Color.FromArgb(*pos++);
                            SetPixel(j, i, color);
                        }
                    }
                }

                bitmap.UnlockBits(data);
            }

            public void SetPixel(int x, int y, Color color)
            {
                int index = x + (y * Width);
                int col = color.ToArgb();

                Bits[index] = col;
            }

            public Color GetPixel(int x, int y)
            {
                int index = x + (y * Width);
                int col = Bits[index];
                Color result = Color.FromArgb(col);

                return result;
            }

            public void Dispose()
            {
                if (Disposed) return;
                Disposed = true;
                Bitmap.Dispose();
                BitsHandle.Free();
            }
        }
 }
