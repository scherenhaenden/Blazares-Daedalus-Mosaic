using System.Drawing;

namespace MosaicMeCli.Test.MosaicAndric;

[Serializable]
public class ImageBlockInfo
{
    protected Color m_Mean;
    [NonSerialized]
    protected float[] m_CIELabMean;
    [NonSerialized]
    public static bool UseCIELABMetrics = true;
    [NonSerialized]
    public int x;
    [NonSerialized]
    public int y;
    [NonSerialized]
    public int index;

    public void Init() => this.m_CIELabMean = ColorTransform.RGBToCIELab(this.m_Mean);

    public ImageBlockInfo() => this.Init();

    public ImageBlockInfo(Color p_Mean)
    {
        this.m_Mean = p_Mean;
        this.Init();
    }

    public virtual void OnDeserialization(object sender) => this.m_CIELabMean = ColorTransform.RGBToCIELab(this.m_Mean);

    public virtual double Distance(ImageBlockInfo Other, double max_error) => ImageBlockInfo.UseCIELABMetrics ? MatrixOp.Distance(this.CIELabMean, Other.CIELabMean) : ImageBlockInfo.Distance(this.m_Mean, Other.Mean);

    public static double Distance(Color c1, Color c2)
    {
        if (ImageBlockInfo.UseCIELABMetrics)
            return MatrixOp.Distance(ColorTransform.RGBToCIELab(c1), ColorTransform.RGBToCIELab(c2));
        double num1 = (double) ((int) c1.R - (int) c2.R);
        double num2 = (double) ((int) c1.G - (int) c2.G);
        double num3 = (double) ((int) c1.B - (int) c2.B);
        return num1 * num1 + num2 * num2 + num3 * num3;
    }

    public Color Mean => this.m_Mean;

    public float[] CIELabMean => this.m_CIELabMean;

    public override string ToString() => this.Mean.ToString();
}