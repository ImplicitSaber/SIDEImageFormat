using BigGustave;

namespace SIDEImageFormat
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Simple Image Data Encoding Format Tool");
            if (args.Length == 0)
            {
                Console.WriteLine("error: no parameters found");
                Console.WriteLine("try using syntax: SIDEImageFormat <from-png/to-png/view> <filename>");
                return;
            }
            string path = "";
            for (uint i = 1; i < args.Length; i++) path += args[i];
            if (args[0] == "from-png")
            {
                try
                {
                    if (File.Exists(path))
                    {
                        string nPath = path + ".side";
                        if (!File.Exists(nPath))
                        {
                            Png p = Png.Open(path);
                            uint[] raw = new uint[p.Width * p.Height];
                            for(int y = 0; y < p.Height; y++)
                            {
                                for(int x = 0; x < p.Width; x++)
                                {
                                    Pixel px = p.GetPixel(x, y);
                                    raw[y * p.Width + x] = ((uint) px.A << 24) | ((uint) px.R << 16) | ((uint) px.G << 8) | px.B;
                                }
                            }
                            byte[] data = SIDEEncoder.ToSIDE((uint) p.Width, (uint) p.Height, raw);
                            FileStream s = File.Open(nPath, FileMode.Create);
                            s.Write(data, 0, data.Length);
                            s.Close();
                            Console.WriteLine("successfully converted file");
                        }
                        else Console.WriteLine("error: output file already exists");
                    }
                    else Console.WriteLine("error: no file exists at path");
                } catch(Exception)
                {
                    Console.WriteLine("error: conversion failed due to exception");
                }
            } else if (args[0] == "to-png")
            {
                try
                {
                    if (File.Exists(path))
                    {
                        string nPath = path + ".png";
                        if (!File.Exists(nPath))
                        {
                            byte[] data = File.ReadAllBytes(path);
                            uint[] raw = SIDEDecoder.FromSIDE(data, out uint width, out uint height, out string? error);
                            if (error == null)
                            {
                                PngBuilder p = PngBuilder.Create((int) width, (int) height, true);
                                for (int y = 0; y < height; y++)
                                {
                                    for (int x = 0; x < width; x++)
                                    {
                                        uint color = raw[y * width + x];
                                        Pixel px = new Pixel((byte) ((color >> 16) & 0xFF), (byte) ((color >> 8) & 0xFF), (byte) (color & 0xFF), (byte) ((color >> 24) & 0xFF), false);
                                        p.SetPixel(px, x, y);
                                    }
                                }
                                FileStream stream = File.Open(nPath, FileMode.Create);
                                p.Save(stream);
                                stream.Close();
                                Console.WriteLine("successfully converted file");
                            }
                            else Console.WriteLine("error: " + error);
                        }
                        else Console.WriteLine("error: output file already exists");
                    }
                    else Console.WriteLine("error: no file exists at path");
                }
                catch (Exception)
                {
                    Console.WriteLine("error: conversion failed due to exception");
                }
            } else if (args[0] == "view")
            {
                byte[] data = File.ReadAllBytes(path);
                uint[] raw = SIDEDecoder.FromSIDE(data, out uint width, out uint height, out string? error);
                if(error == null)
                {
                    Console.WriteLine("Running image viewer...");
                    using(ImageViewerWindow window = new ImageViewerWindow((int) width, (int) height, raw))
                    {
                        window.Run();
                    }
                } else Console.WriteLine("error: " + error);
            } else
            {
                Console.WriteLine("error: unknown action type '" + args[0] + "'");
                Console.WriteLine("try using syntax: SIDEImageFormat <from-png/to-png/view> <filename>");
            }
        }
    }
}
