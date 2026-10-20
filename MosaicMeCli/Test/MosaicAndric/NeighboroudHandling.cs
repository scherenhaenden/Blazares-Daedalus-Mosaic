using System.Collections;
using System.Drawing;

namespace MosaicMeCli.Test.MosaicAndric;

[Serializable]
public class ImageLink : MyObject
{
    [NonSerialized]
    public static EventHandler WorkStep;
    public ImageBlockInfo m_info;
    public Size ImageSize;
    public float ImageAspectRatio;
    [NonSerialized]
    protected Bitmap m_Bitmap;
    [NonSerialized]
    public bool usable = true;
    [NonSerialized]
    public int MemoryId;

    public virtual string GetImageInfo() => "";

    public ImageLink()
    {
        this.ImageAspectRatio = 1f;
        this.usable = true;
    }

    public virtual Bitmap GetLinkedBitmap() => this.m_Bitmap;

    public virtual Bitmap GetLinkedBitmap(int lx, int ly) => this.m_Bitmap;

    public virtual void SetLinkedBitmap(Bitmap p_Bitmap) => this.m_Bitmap = p_Bitmap;
}

public class NeighboroudHandling
{
    public int[,] neighboroud;
    private int[] tagged;
    private Random R;

    public NeighboroudHandling(ArrayList sourceBlocks)
    {
        Size blockMatrixSize = BlockTransform.GetBlockMatrixSize(sourceBlocks);
        this.neighboroud = new int[blockMatrixSize.Width, blockMatrixSize.Height];
        for (int index1 = 0; index1 < blockMatrixSize.Width; ++index1)
        {
            for (int index2 = 0; index2 < blockMatrixSize.Height; ++index2)
                this.neighboroud[index1, index2] = -1;
        }
        this.R = new Random();
    }

    public void TagAdicacentBlocks(ArrayList pImageLink, int x, int y, double distance)
    {
        this.tagged = this.GetAdiacentBlocks(x, y, distance);
        for (int index = 0; index < this.tagged.Length; ++index)
        {
            if (this.tagged[index] != -1)
                ((ImageLink) pImageLink[this.tagged[index]]).usable = false;
        }
    }

    public void UntagAdicacentBlocks(ArrayList pImageLink, double probability)
    {
        for (int index = 0; index < this.tagged.Length; ++index)
        {
            if (this.tagged[index] != -1)
            {
                bool flag = false;
                if (this.R.NextDouble() <= probability)
                    flag = true;
                ((ImageLink) pImageLink[this.tagged[index]]).usable = flag;
            }
        }
    }

    protected int[] GetAdiacentBlocks(int x, int y, double distance)
    {
        int num1 = (int) Math.Max(0.0, (double) x - distance);
        int num2 = (int) Math.Max(0.0, (double) y - distance);
        int num3 = (int) Math.Min((double) (this.neighboroud.GetLength(0) - 1), (double) x + distance);
        int num4 = (int) Math.Min((double) (this.neighboroud.GetLength(1) - 1), (double) y + distance);
        int[] adiacentBlocks = new int[(num3 - num1 + 1) * (num4 - num2 + 1)];
        int index1 = 0;
        double num5 = distance * distance;
        for (int index2 = num1; index2 <= num3; ++index2)
        {
            for (int index3 = num2; index3 <= num4; ++index3)
            {
                if ((double) ((x - index2) * (x - index2) + (y - index3) * (y - index3)) <= num5)
                {
                    adiacentBlocks[index1] = this.neighboroud[index2, index3];
                    ++index1;
                }
            }
        }
        for (int index4 = index1 + 1; index4 < adiacentBlocks.Length; ++index4)
            adiacentBlocks[index4] = -1;
        return adiacentBlocks;
    }
}