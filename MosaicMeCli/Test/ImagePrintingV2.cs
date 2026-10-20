using System.Text;
using ImageMagick;

namespace MosaicMeCli.Test;

public class ImagePrintingV2
{
    public static CieLab RGBtoLab(int red, int green, int blue)
    {
        var rLinear = red / 255.0;
        var gLinear = green / 255.0;
        var bLinear = blue / 255.0;

        double r = rLinear > 0.04045 ? Math.Pow((rLinear + 0.055) / (1 + 0.055), 2.2) : (rLinear / 12.92);
        double g = gLinear > 0.04045 ? Math.Pow((gLinear + 0.055) / (1 + 0.055), 2.2) : (gLinear / 12.92);
        double b = bLinear > 0.04045 ? Math.Pow((bLinear + 0.055) / (1 + 0.055), 2.2) : (bLinear / 12.92);

        var x = r * 0.4124 + g * 0.3576 + b * 0.1805;
        var y = r * 0.2126 + g * 0.7152 + b * 0.0722;
        var z = r * 0.0193 + g * 0.1192 + b * 0.9505;

        Func<double, double> Fxyz = t => ((t > 0.008856) ? Math.Pow(t, (1.0 / 3.0)) : (7.787 * t + 16.0 / 116.0));

        return new CieLab
        {
            L = 116.0 * Fxyz(y / 1.0) - 16,
            A = 500.0 * (Fxyz(x / 0.9505) - Fxyz(y / 1.0)),
            B = 200.0 * (Fxyz(y / 1.0) - Fxyz(z / 1.0890))
        };
    }
    public class CieLab
    {
        public double L { get; set; }
        public double A { get; set; }
        public double B { get; set; }

        public static double DeltaE(CieLab l1, CieLab l2)
        {
            return Math.Pow(l1.L - l2.L, 2) + Math.Pow(l1.A - l2.A, 2) + Math.Pow(l1.B - l2.B, 2);
        }

        public static CieLab Combine(CieLab l1, CieLab l2, double amount)
        {
            var l = l1.L * amount + l2.L * (1 - amount);
            var a = l1.A * amount + l2.A * (1 - amount);
            var b = l1.B * amount + l2.B * (1 - amount);

            return new CieLab { L = l, A = a, B = b };
        }
    }
    
    class ConsolePixel
    {
        public char Char { get; set; }

        public ConsoleColor Forecolor { get; set; }
        public ConsoleColor Backcolor { get; set; }
        public CieLab Lab { get; set; }
    }

    static List<ConsolePixel> pixels;
    private static void ComputeColors()
    {
        pixels = new List<ConsolePixel>();

        char[] chars = { '█', '▓', '▒', '░' };

        int[] rs = { 0, 0, 0, 0, 128, 128, 128, 192, 128, 0, 0, 0, 255, 255, 255, 255 };
        int[] gs = { 0, 0, 128, 128, 0, 0, 128, 192, 128, 0, 255, 255, 0, 0, 255, 255 };
        int[] bs = { 0, 128, 0, 128, 0, 128, 0, 192, 128, 255, 0, 255, 0, 255, 0, 255 };

        for (int i = 0; i < 16; i++)
        for (int j = i + 1; j < 16; j++)
        {
            var l1 = RGBtoLab(rs[i], gs[i], bs[i]);
            var l2 = RGBtoLab(rs[j], gs[j], bs[j]);

            for (int k = 0; k < 4; k++)
            {
                var l = CieLab.Combine(l1, l2, (4 - k) / 4.0);

                pixels.Add(new ConsolePixel
                {
                    Char = chars[k],
                    Forecolor = (ConsoleColor)i,
                    Backcolor = (ConsoleColor)j,
                    Lab = l
                });
            }
        }
    }
    
    public static void DrawImage(MagickImage source)
    {
        ComputeColors();
        int width = Console.WindowWidth - 1;
        int height = (int)(width * source.Height / 2.0 / source.Width);

        source.Resize((uint)width, (uint)height);

        using (var pixels = source.GetPixels())
        {
            height = (int)source.BoundingBox.Height;
            width = (int)source.BoundingBox.Width;
            for (int j = 0; j < height; j++)
            {
                StringBuilder builder = new StringBuilder();
                var fore = ConsoleColor.White;
                var back = ConsoleColor.Black;
                for (int i = 0;i< width; i++)
                {
                    IPixel<byte> pixel = pixels.GetPixel(i, j);
                    var consolePixel = DrawPixel(pixel);

                    if (consolePixel.Forecolor != fore || consolePixel.Backcolor != back)
                    {
                        Console.ForegroundColor = fore;
                        Console.BackgroundColor = back;
                        Console.Write(builder);

                        builder.Clear();
                    }

                    fore = consolePixel.Forecolor;
                    back = consolePixel.Backcolor;
                    builder.Append(consolePixel.Char);
                }

                Console.ForegroundColor = fore;
                Console.BackgroundColor = back;
                Console.WriteLine(builder);
            }

            Console.ResetColor();
        }
    }
    
    private static ConsolePixel DrawPixel(IPixel<byte> pixel)
    {
        var l = RGBtoLab(pixel.GetChannel(0), pixel.GetChannel(1), pixel.GetChannel(2));

        double diff = double.MaxValue;
        var consolePixel = pixels[0];

        foreach (var item in pixels)
        {
            var delta = CieLab.DeltaE(l, item.Lab);
            if (delta < diff)
            {
                diff = delta;
                consolePixel = item;
            }
        }

        return consolePixel;
    }

    private static ConsolePixel DrawPixel(IMagickColor<byte> color)
    {
        var l = RGBtoLab(color.R, color.G, color.B);

        double diff = double.MaxValue;
        var pixel = pixels[0];

        foreach (var item in pixels)
        {
            var delta = CieLab.DeltaE(l, item.Lab);
            if (delta < diff)
            {
                diff = delta;
                pixel = item;
            }
        }

        return pixel;
    }
}