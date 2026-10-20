namespace MosaicMeCli.Test.MosaicAndric;

public class MatrixOp
{
    public static double[] Mult(double[,] A, double[] x)
    {
        int length1 = A.GetLength(0);
        int length2 = A.GetLength(1);
        double[] numArray = new double[length1];
        for (int index1 = 0; index1 < length1; ++index1)
        {
            double num = 0.0;
            for (int index2 = 0; index2 < length2; ++index2)
                num += A[index1, index2] * x[index2];
            numArray[index1] = num;
        }
        return numArray;
    }

    public static float[] Mult(double[,] A, float[] x)
    {
        int length1 = A.GetLength(0);
        int length2 = A.GetLength(1);
        float[] numArray = new float[length1];
        for (int index1 = 0; index1 < length1; ++index1)
        {
            double num = 0.0;
            for (int index2 = 0; index2 < length2; ++index2)
                num += A[index1, index2] * (double) x[index2];
            numArray[index1] = (float) num;
        }
        return numArray;
    }

    public static double ScalarProduct(double[] x1, double[] x2)
    {
        double num = 0.0;
        for (int index = 0; index < x1.Length; ++index)
            num += x1[index] * x2[index];
        return num;
    }

    public static double Distance(double[] x1, double[] x2)
    {
        double d = 0.0;
        for (int index = 0; index < x1.Length; ++index)
        {
            double num = x1[index] - x2[index];
            d += num * num;
        }
        return Math.Sqrt(d);
    }

    public static double Distance(float[] x1, float[] x2)
    {
        double d = 0.0;
        for (int index = 0; index < x1.Length; ++index)
        {
            double num = (double) x1[index] - (double) x2[index];
            d += num * num;
        }
        return Math.Sqrt(d);
    }
}