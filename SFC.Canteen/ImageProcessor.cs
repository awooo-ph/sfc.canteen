﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Devcorner.NIdenticon;
using Devcorner.NIdenticon.BrushGenerators;

namespace SFC.Canteen
{
    static class ImageProcessor
    {
        public const string ACCEPTED_EXTENSIONS = @".BMP.JPG.JPEG.GIF.PNG.BMP.DIB.RLE.JPE.JFIF";

        public static bool IsAccepted(string file)
        {
            if (file == null) return false;
            var ext = System.IO.Path.GetExtension(file)?.ToUpper();
            return File.Exists(file) && (ACCEPTED_EXTENSIONS.Contains(ext));
            
        }

        public static byte[] Generate()
        {
            var rnd = new Random();
            var color = Color.FromArgb(255, rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
            var gen = new IdenticonGenerator()
                .WithBlocks(7, 7)
                .WithSize(256, 256)
                .WithBlockGenerators(IdenticonGenerator.ExtendedBlockGeneratorsConfig)
                .WithBackgroundColor(Color.White)
                .WithBrushGenerator(new StaticColorBrushGenerator(color));
            
            using (var pic = gen.Create("awooo" + DateTime.Now.Ticks))
            {
                using (var stream = new MemoryStream())
                {
                    pic.Save(stream, ImageFormat.Jpeg);
                    return stream.ToArray();
                }
            }
            
        }

        public static byte[] ResizeImage(string file)
        {
            if (!File.Exists(file))
                return null;
            using (var img = System.Drawing.Image.FromFile(file))
            {
                using (var bmp = Resize(img, 777, Color.White))
                {
                    using (var bin = new MemoryStream())
                    {
                        bmp.Save(bin, ImageFormat.Jpeg);
                        return bin.ToArray();
                    }
                }
            }
        }

        public static Image Resize(Image imgPhoto, int size)
        {
            return Resize(imgPhoto, size, Color.White);
        }

        public static Image Resize(Image imgPhoto, int size, Color background)
        {
            var sourceWidth = imgPhoto.Width;
            var sourceHeight = imgPhoto.Height;
            var sourceX = 0;
            var sourceY = 0;
            var destX = 0;
            var destY = 0;
            var nPercent = 0.0f;

            if (sourceWidth > sourceHeight)
                nPercent = (size / (float) sourceWidth);

            else
                nPercent = (size / (float) sourceHeight);



            var destWidth = (int) (sourceWidth * nPercent);
            var destHeight = (int) (sourceHeight * nPercent);

            var bmPhoto = new Bitmap(destWidth, destHeight, PixelFormat.Format32bppArgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            var grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
            grPhoto.Clear(background);
            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }
    }
}
