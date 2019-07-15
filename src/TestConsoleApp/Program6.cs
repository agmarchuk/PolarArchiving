using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace TestConsoleApp
{
    partial class Program
    {
        static void Main6(string[] args)
        {
            args = new string[] { @"C:\home\syp\syp_cassettes\SypCassete\originals\0001", @"C:\home\data\photos", "800" };
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch(); sw.Start();
            DirectoryInfo di = new DirectoryInfo(args[0]);
            string dir2 = args[1]; char d2 = dir2[dir2.Length - 1];
            if (d2 != '/' && d2 != '\\') dir2 = dir2 + "/";
            int pixels = Int32.Parse(args[2]);
            foreach (var fi in di.GetFiles())
            {
                string file = fi.FullName;
                int lastpoint = file.LastIndexOf('.');
                if (lastpoint == -1) continue;
                string ext = file.Substring(lastpoint + 1).ToLower();
                if (ext != "jpg" && ext != "png" && ext != "tiff" && ext != "gif") continue;
                int lastslash = file.LastIndexOf('/');
                int lastbackslash = file.LastIndexOf('\\');
                int begname = System.Math.Max(lastslash, lastbackslash) + 1;
                string name = file.Substring(begname, lastpoint - begname);

                Console.Write($"Loading {name}.{ext} ...");
                using (FileStream pngStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                using (var image = new Bitmap(pngStream))
                {
                    int width, height;
                    int w0 = image.Width, h0 = image.Height;
                    if (w0 <= pixels && h0 <= pixels)
                    {
                        // Если меньше, чем нужно, то может нужна специальная обработка???
                    }
                    if (w0 > h0) { width = pixels; height = pixels * h0 / w0; }
                    else { width = pixels * w0 / h0; height = pixels; }
                    var resized = new Bitmap(width, height);
                    using (var graphics = Graphics.FromImage(resized))
                    {
                        graphics.CompositingQuality = CompositingQuality.HighSpeed;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.CompositingMode = CompositingMode.SourceCopy;
                        graphics.DrawImage(image, 0, 0, width, height);
                        resized.Save(dir2 + name +".jpg", ImageFormat.Jpeg);
                        //resized.Save($"resized-{file}", ImageFormat.Png);
                        Console.WriteLine(" Saving resized");
                    }
                }
            }
            sw.Stop();
            Console.WriteLine($"duration {sw.ElapsedMilliseconds} ms.");
        }
    }
}