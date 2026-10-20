using System.Drawing;

namespace MosaicMeCli.Test.MosaicAndric;

public class BitmapConvert
{
    public static Color[,] ToBidimensionalArray(Bitmap Source)
    {
        int width = ((Image) Source).Width;
        int height = ((Image) Source).Height;
        Color[,] bidimensionalArray = new Color[width, height];
        for (int index1 = 0; index1 < width; ++index1)
        {
            for (int index2 = 0; index2 < height; ++index2)
                bidimensionalArray[index1, index2] = Source.GetPixel(index1, index2);
        }
        return bidimensionalArray;
    }
}