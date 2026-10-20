using System.Collections;
using System.Drawing;

namespace MosaicMeCli.Test.MosaicAndric;



[Serializable]
public class ImageBlockInfoEx : ImageBlockInfo
{
    private ArrayList m_Frequencies;

    public ArrayList Frequencies => this.m_Frequencies;

    public ImageBlockInfoEx()
    {
    }

    public ImageBlockInfoEx(Color p_Mean, ArrayList p_Frequencies)
        : base(p_Mean)
    {
        this.m_Frequencies = p_Frequencies;
    }

    public override double Distance(ImageBlockInfo pOther, double max_error)
    {
        ImageBlockInfoEx imageBlockInfoEx = (ImageBlockInfoEx) pOther;
        int index = Math.Min(this.m_Frequencies.Count, imageBlockInfoEx.Frequencies.Count) - 1;
        return index == -1 ? base.Distance(pOther, max_error) : ImageBlockInfoEx.Distance((Color[,]) this.Frequencies[index], (Color[,]) imageBlockInfoEx.Frequencies[index], max_error);
    }

    public static double Distance(Color[,] c1, Color[,] c2)
    {
        double num1 = 0.0;
        int length = c1.GetLength(0);
        for (int index1 = 0; index1 < length; ++index1)
        {
            for (int index2 = 0; index2 < length; ++index2)
            {
                double num2 = ImageBlockInfo.Distance(c1[index1, index2], c2[index1, index2]);
                num1 += num2;
            }
        }
        return num1 / (double) (length * length);
    }

    public static double Distance(Color[,] c1, Color[,] c2, double max_error)
    {
        double num1 = 0.0;
        int length = c1.GetLength(0);
        int num2 = length * length;
        for (int index1 = 0; index1 < length; ++index1)
        {
            for (int index2 = 0; index2 < length; ++index2)
            {
                double num3 = ImageBlockInfo.Distance(c1[index1, index2], c2[index1, index2]);
                num1 += num3;
            }
            if (num1 > max_error * (double) num2)
                return max_error + 1.0;
        }
        return num1 / (double) (length * length);
    }
}

public class Transform
{
    private Color[,] m_Bitmap;
    private int m_X;
    private int m_Y;

    public Transform(Bitmap p_Bitmap)
    {
        this.m_Bitmap = BitmapConvert.ToBidimensionalArray(p_Bitmap);
        this.m_X = ((Image) p_Bitmap).Width;
        this.m_Y = ((Image) p_Bitmap).Height;
    }

    public Transform(Color[,] p_Bitmap)
    {
        this.m_Bitmap = p_Bitmap;
        this.m_X = p_Bitmap.GetLength(0);
        this.m_Y = p_Bitmap.GetLength(1);
    }

    public ImageBlockInfo Calculate(int Rank)
    {
        ArrayList p_Frequencies = new ArrayList();
        for (int Rank1 = 2; Rank1 <= Rank && this.m_X >= Rank1 && this.m_Y >= Rank1; ++Rank1)
            p_Frequencies.Add((object) this.CalculateMatrix(Rank1));
        return (ImageBlockInfo) new ImageBlockInfoEx(this.MeanColor(), p_Frequencies);
    }

    public ImageBlockInfo Calculate(int Rank, Rectangle rect)
    {
        ArrayList p_Frequencies = new ArrayList();
        for (int Rank1 = 2; Rank1 <= Rank && rect.Width >= Rank1 && rect.Height >= Rank1; ++Rank1)
            p_Frequencies.Add((object) this.CalculateMatrix(Rank1, rect));
        return (ImageBlockInfo) new ImageBlockInfoEx(this.MeanColor(rect), p_Frequencies);
    }

    protected Color MeanColor(Rectangle rect) => this.MeanColor(new Point(rect.X, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height));

    protected Color MeanColor(Point UL, Point DR)
    {
        int num1 = DR.X - UL.X;
        int num2 = DR.Y - UL.Y;
        long num3 = 0;
        long num4 = 0;
        long num5 = 0;
        Color color;
        for (int x = UL.X; x < DR.X; ++x)
        {
            for (int y = UL.Y; y < DR.Y; ++y)
            {
                color = this.m_Bitmap[x, y];
                num3 += (long) color.R;
                num4 += (long) color.G;
                num5 += (long) color.B;
            }
        }
        int num6 = num1 * num2;
        color = Color.FromArgb((int) byte.MaxValue, (int) Math.Round((double) num3 / (double) num6), (int) Math.Round((double) num4 / (double) num6), (int) Math.Round((double) num5 / (double) num6));
        return color;
    }

    protected Color MeanColor() => this.MeanColor(new Point(0, 0), new Point(this.m_X - 1, this.m_Y - 1));

    public Color[,] CalculateMatrix(int Rank)
    {
        this.CalculateMatrix(Rank, new Rectangle(0, 0, this.m_X, this.m_Y));
        return this.CalculateMatrix(Rank, new Rectangle(0, 0, this.m_X, this.m_Y));
    }

    protected Color[,] CalculateMatrix(int Rank, Rectangle rect)
    {
        Color[,] matrix = new Color[Rank, Rank];
        double num1 = (double) rect.Width / (double) Rank;
        double num2 = (double) rect.Height / (double) Rank;
        double x = (double) rect.Location.X;
        for (int index1 = 0; index1 < Rank; ++index1)
        {
            double y = (double) rect.Location.Y;
            for (int index2 = 0; index2 < Rank; ++index2)
            {
                matrix[index1, index2] = this.MeanColor(new Point((int) x, (int) y), new Point((int) (x + num1), (int) (y + num2)));
                y += num2;
            }
            x += num1;
        }
        return matrix;
    }
}