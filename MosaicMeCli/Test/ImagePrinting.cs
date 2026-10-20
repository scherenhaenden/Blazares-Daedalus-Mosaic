using ImageMagick;
namespace MosaicMeCli.Test;

public class ImagePrinting
{ public static int ToConsoleColor(IMagickColor<byte> c)
    {
        int index = (c.R > 128 || c.G > 128 || c.B > 128) ? 8 : 0;
        index |= (c.R > 64) ? 4 : 0;
        index |= (c.G > 64) ? 2 : 0;
        index |= (c.B > 64) ? 1 : 0;
        return index;
    }

    public static void ConsoleWriteImage(MagickImage src)
    {
        int min = 50;
        decimal pct = Math.Min(decimal.Divide(min, src.Width), decimal.Divide(min, src.Height));
        MagickGeometry size = new MagickGeometry((uint)(src.Width * pct), (uint)(src.Height * pct));
        size.IgnoreAspectRatio = true;
        src.Resize(size);

        using (var pixels = src.GetPixels())
        {
            for (int i = 0; i < src.Height; i++)
            {
                for (int j = 0; j < src.Width; j++)
                {
                    
                   var pixel = pixels.GetPixel(j, i);

                   var color = pixel.ToColor();

                   var consoleColor = ToConsoleColor(color);

                   Console.ForegroundColor = (ConsoleColor)consoleColor;
                    Console.Write("██");
                   
                    
                 
                }
                System.Console.WriteLine();
            }
        }
    }
}