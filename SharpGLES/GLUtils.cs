using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SharpGLES
{
	public static class GLUtils
	{
		public static void TexImage2D(int target, int level, Image image, int border)
		{
			PixelFormat format = PixelFormat.Format32bppArgb;

			Bitmap bitmap = new Bitmap(image.Width, image.Height, format);

			Graphics graphics = Graphics.FromImage(bitmap);

			graphics.DrawImage(image, 0, 0, image.Width, image.Height);

			BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, format);

			int size = data.Stride * data.Height;

			byte[] buffer = new byte[size];

			Marshal.Copy(data.Scan0, buffer, 0, size);

			bitmap.UnlockBits(data);

			bitmap.Dispose();

			for (int i = 0; i < buffer.Length; i += 4)
			{
				byte b = buffer[i];
				//byte g = buffer[i + 1];
				byte r = buffer[i + 2];
				//byte a = buffer[i + 3];

				buffer[i] = r;
				//buffer[i + 1] = g;
				buffer[i + 2] = b;
				//buffer[i + 3] = a;
			}

			GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

			IntPtr address = handle.AddrOfPinnedObject();

			GLES20.TexImage2D(target, level, GLES20.GL_RGBA, image.Width, image.Height, border, GLES20.GL_RGBA, GLES20.GL_UNSIGNED_BYTE, address);

			handle.Free();
		}
	}
}
