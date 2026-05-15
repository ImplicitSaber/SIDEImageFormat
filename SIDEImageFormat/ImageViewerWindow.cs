using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace SIDEImageFormat
{
    internal class ImageViewerWindow : GameWindow
    {

        private readonly int width;
        private readonly int height;
        private readonly uint[] rawData;

        private int framebuffer = -1;

        public ImageViewerWindow(int width, int height, uint[] rawData) : 
            base(GameWindowSettings.Default, new NativeWindowSettings() { Size = GetSize(width, height), Title = "Image Viewer" })
        {
            this.width = width;
            this.height = height;
            this.rawData = rawData;
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0, 0, 0, 0);
            byte[] byteData = new byte[rawData.Length * 4];
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    int i = y * width + x;
                    int j = ((height - 1 - y) * width + x) * 4;
                    byteData[j] = (byte)((rawData[i] >> 16) & 0xFF);
                    byteData[j + 1] = (byte)((rawData[i] >> 8) & 0xFF);
                    byteData[j + 2] = (byte)(rawData[i] & 0xFF);
                    byteData[j + 3] = (byte)((rawData[i] >> 24) & 0xFF);
                }
            }
            int textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, byteData);
            GL.GenFramebuffers(1, out int framebuffers);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, framebuffers);
            GL.FramebufferTexture2D(FramebufferTarget.ReadFramebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, textureId, 0);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
            framebuffer = framebuffers;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, framebuffer);
            GL.BlitFramebuffer(0, 0, width, height, 0, 0, Size.X, Size.Y, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
            SwapBuffers();
        }

        private static (int, int) GetSize(int imageWidth, int imageHeight)
        {
            int gcf = (int) Gcf((uint) imageWidth, (uint) imageHeight);
            int aspectX = imageWidth / gcf;
            int aspectY = imageHeight / gcf;
            int factor = 1;
            for(int f = 1; aspectX * f <= 768 && aspectY * f <= 432; f++) factor = f;
            return (aspectX * factor, aspectY * factor);
        }

        private static uint Gcf(uint a, uint b)
        {
            while(a != 0 && b != 0)
            {
                if (a > b) a %= b;
                else b %= a;
            }
            return a | b;
        }
    }
}
