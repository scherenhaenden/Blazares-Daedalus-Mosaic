using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;

namespace MosaicMeCli.Test.MosaicAndric;

public class RenderMatching
{
    private ArrayList m_ImageLinkDataBase;
    private ArrayList m_MatchingList;
    private ArrayList m_SourceBlockList;
    private Size m_SourceImageSize;
    private AlgorithmParameters m_params;
    public ArrayList InfoList;

    public RenderMatching(
        Size p_SourceImageSize,
        ArrayList p_ImageLinkDataBase,
        ArrayList p_MatchingList,
        ArrayList p_SourceBlockList,
        AlgorithmParameters p_params)
    {
        this.m_ImageLinkDataBase = p_ImageLinkDataBase;
        this.m_SourceImageSize = p_SourceImageSize;
        this.m_MatchingList = p_MatchingList;
        this.m_SourceBlockList = p_SourceBlockList;
        this.m_params = p_params;
    }

    public string GetStatistics()
    {
        int[] numArray = new int[this.m_ImageLinkDataBase.Count];
        for (int index = 0; index < this.m_MatchingList.Count; ++index)
        {
            ImageLink matching = (ImageLink) this.m_MatchingList[index];
            ++numArray[matching.MemoryId];
        }
        int num1 = 0;
        int num2 = 0;
        for (int index = 0; index < this.m_ImageLinkDataBase.Count; ++index)
        {
            if (numArray[index] > num2)
                num2 = numArray[index];
            if (numArray[index] > 0)
                ++num1;
        }
        return "Number of used images=" + (object) num1 + Environment.NewLine + "An image has been used " + (object) num2 + " times" + Environment.NewLine;
    }

    public Bitmap DoRender()
    {
        Bitmap bitmap = new Bitmap(this.m_params.OutputSize.Width, this.m_params.OutputSize.Height, (PixelFormat) 137224);
        this.InfoList = new ArrayList();
        Size size = new Size((int) ((double) this.m_params.OutputSize.Width / (double) this.m_SourceImageSize.Width * (double) this.m_params.SourceBlockSize.Width), (int) ((double) this.m_params.OutputSize.Height / (double) this.m_SourceImageSize.Height * (double) this.m_params.SourceBlockSize.Height));
        Graphics gr = Graphics.FromImage((Image) bitmap);
        RenderMatching.SetGraphicsProperties(gr);
        int index1 = 0;
        int y = 0;
        for (int index2 = 0; index2 < this.m_SourceImageSize.Height; index2 += this.m_params.SourceBlockSize.Height)
        {
            int x = 0;
            for (int index3 = 0; index3 < this.m_SourceImageSize.Width; index3 += this.m_params.SourceBlockSize.Width)
            {
                int num1 = size.Width;
                int num2 = size.Height;
                if (x + size.Width >= ((Image) bitmap).Width)
                    num1 = ((Image) bitmap).Width - x;
                if (y + size.Height >= ((Image) bitmap).Height)
                    num2 = ((Image) bitmap).Height - y;
                ImageLink matching = (ImageLink) this.m_MatchingList[index1];
                Bitmap linkedBitmap = matching.GetLinkedBitmap(num1, num2);
                this.InfoList.Add((object) new AreaInfo(new Rectangle(x, y, num1, num2), matching.GetImageInfo()));
                ImageBlockInfoEx sourceBlock = (ImageBlockInfoEx) this.m_SourceBlockList[index1];
                this.BitmapFill(linkedBitmap, gr, x, y, num1, num2, sourceBlock);
                ++index1;
                x += size.Width;
                if (PMWorkClass.WorkStep != null)
                    PMWorkClass.WorkStep();
                ++PMWorkClass.WorkCurrent;
            }
            GC.Collect();
            y += size.Height;
        }
        return bitmap;
    }

    public static void SetGraphicsProperties(Graphics gr)
    {
        gr.PixelOffsetMode = (PixelOffsetMode) 2;
        gr.InterpolationMode = (InterpolationMode) 4;
        gr.SmoothingMode = (SmoothingMode) 2;
        gr.CompositingQuality = (CompositingQuality) 2;
        gr.CompositingMode = (CompositingMode) 1;
    }

    private void BitmapFill(
        Bitmap MatchedBlock,
        Graphics gr,
        int x,
        int y,
        int lx,
        int ly,
        ImageBlockInfoEx Target)
    {
        int index1 = Target.Frequencies.Count - 1;
        int num1 = 0;
        int num2 = 0;
        if (index1 >= 0)
        {
            num1 = ((Array) Target.Frequencies[index1]).GetLength(0);
            num2 = ((Array) Target.Frequencies[index1]).GetLength(1);
        }
        Bitmap bitmap = new Bitmap(((Image) MatchedBlock).Width, ((Image) MatchedBlock).Height, (PixelFormat) 137224);
        for (int index2 = 0; index2 < ((Image) bitmap).Width; ++index2)
        {
            double num3 = (double) num1 * (double) index2 / (double) ((Image) bitmap).Width;
            int index3 = (int) num3;
            for (int index4 = 0; index4 < ((Image) bitmap).Height; ++index4)
            {
                double num4 = (double) num2 * (double) index4 / (double) ((Image) bitmap).Height;
                int index5 = (int) num4;
                Color Target1;
                if (index1 >= 0)
                {
                    Color TX0Y0 = ((Color[,]) Target.Frequencies[index1])[index3, index5];
                    Color TX1Y0 = TX0Y0;
                    Color TX0Y1 = TX0Y0;
                    Color TX1Y1 = TX0Y0;
                    if (index3 < num1 - 1)
                        TX1Y0 = ((Color[,]) Target.Frequencies[index1])[index3 + 1, index5];
                    if (index5 < num2 - 1)
                        TX0Y1 = ((Color[,]) Target.Frequencies[index1])[index3, index5 + 1];
                    if (index3 < num1 - 1 && index5 < num2 - 1)
                        TX1Y1 = ((Color[,]) Target.Frequencies[index1])[index3 + 1, index5 + 1];
                    Target1 = ColorTransform.ConvexCombination(TX0Y0, TX1Y0, TX0Y1, TX1Y1, num3 - (double) index3, num4 - (double) index5);
                }
                else
                    Target1 = Target.Mean;
                bitmap.SetPixel(index2, index4, ColorTransform.ConvexCombination(MatchedBlock.GetPixel(index2, index4), Target1, this.m_params.ColorCorrection));
            }
        }
        gr.SetClip(new Rectangle(x, y, ((Image) bitmap).Width, ((Image) bitmap).Height));
        gr.DrawImageUnscaled((Image) bitmap, x, y);
    }
}