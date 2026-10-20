using System.Collections;
using System.Drawing;

namespace MosaicMeCli.Test.MosaicAndric;

public class ColorTransform
{
    private static double[,] RGBToXYZMatrix;
    private static float[] XYZReferenceWhite;
    private static Hashtable RGBToCIELabTable = new Hashtable(1153);

    static ColorTransform()
    {
        ColorTransform.RGBToXYZMatrix = new double[3, 3];
        ColorTransform.RGBToXYZMatrix[0, 0] = 0.412453;
        ColorTransform.RGBToXYZMatrix[0, 1] = 0.35758;
        ColorTransform.RGBToXYZMatrix[0, 2] = 0.180423;
        ColorTransform.RGBToXYZMatrix[1, 0] = 0.212671;
        ColorTransform.RGBToXYZMatrix[1, 1] = 0.71516;
        ColorTransform.RGBToXYZMatrix[1, 2] = 0.072169;
        ColorTransform.RGBToXYZMatrix[2, 0] = 0.019334;
        ColorTransform.RGBToXYZMatrix[2, 1] = 0.119193;
        ColorTransform.RGBToXYZMatrix[2, 2] = 0.950227;
        ColorTransform.XYZReferenceWhite = new float[3];
        ColorTransform.XYZReferenceWhite[0] = 1f;
        ColorTransform.XYZReferenceWhite[1] = 1f;
        ColorTransform.XYZReferenceWhite[2] = 1f;
    }

    public static float[] RGBToXYZ(Color s)
    {
        float[] x = new float[3]
        {
            (float) s.R / (float) byte.MaxValue,
            (float) s.G / (float) byte.MaxValue,
            (float) s.B / (float) byte.MaxValue
        };
        return MatrixOp.Mult(ColorTransform.RGBToXYZMatrix, x);
    }

    public static float[] XYZToCIELab(float[] xyz)
    {
        float[] cieLab = new float[3];
        float num1 = ColorTransform.XYZReferenceWhite[0];
        float num2 = ColorTransform.XYZReferenceWhite[1];
        float num3 = ColorTransform.XYZReferenceWhite[2];
        float num4 = xyz[0];
        float num5 = xyz[1];
        float num6 = xyz[2];
        float num7 = (double) num5 / (double) num2 <= 0.008856 ? (float) (903.3 * ((double) num5 / (double) num2)) : (float) (116.0 * Math.Pow((double) num5 / (double) num2, 1.0 / 3.0) - 16.0);
        float num8 = (float) (500.0 * ((double) ColorTransform.CIELabF((double) num4 / (double) num1) - (double) ColorTransform.CIELabF((double) num5 / (double) num2)));
        float num9 = (float) (200.0 * ((double) ColorTransform.CIELabF((double) num5 / (double) num2) - (double) ColorTransform.CIELabF((double) num6 / (double) num3)));
        cieLab[0] = num7;
        cieLab[1] = num8;
        cieLab[2] = num9;
        return cieLab;
    }

    private static float CIELabF(double t) => t > 0.008856 ? (float) Math.Pow(t, 1.0 / 3.0) : (float) (7.787 * t + 4.0 / 29.0);

    public static float[] RGBToCIELab(Color s)
    {
        float[] cieLab1 = (float[]) ColorTransform.RGBToCIELabTable[(object) s];
        if (cieLab1 != null)
            return cieLab1;
        float[] cieLab2 = ColorTransform.XYZToCIELab(ColorTransform.RGBToXYZ(s));
        ColorTransform.RGBToCIELabTable.Add((object) s, (object) cieLab2);
        return cieLab2;
    }

    public static uint RGBToInt(Color s) => (uint) ((int) s.R + (int) s.G * 256 + (int) s.B * 65536);

    public static Color ConvexCombination(Color Source, Color Target, double p) => Color.FromArgb((int) ((double) Source.R + (double) ((int) Target.R - (int) Source.R) * p), (int) ((double) Source.G + (double) ((int) Target.G - (int) Source.G) * p), (int) ((double) Source.B + (double) ((int) Target.B - (int) Source.B) * p));

    public static Color ConvexCombination(
        Color TX0Y0,
        Color TX1Y0,
        Color TX0Y1,
        Color TX1Y1,
        double p0,
        double p1)
    {
        return ColorTransform.ConvexCombination(ColorTransform.ConvexCombination(ColorTransform.ConvexCombination(TX0Y0, TX1Y0, p0), ColorTransform.ConvexCombination(TX0Y0, TX0Y1, p1), 0.5), ColorTransform.ConvexCombination(TX1Y0, TX1Y1, p0 * p1), 0.5);
    }
}