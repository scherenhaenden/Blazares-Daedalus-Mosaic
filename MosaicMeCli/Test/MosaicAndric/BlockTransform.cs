using System.Collections;
using System.Drawing;

namespace MosaicMeCli.Test.MosaicAndric;

public class BlockTransform
{
    private Color[,] m_Bitmap;
    private Size m_BitmapSize;
    private Size m_BlockSize;
    
    public BlockTransform(Bitmap p_Bitmap, Size p_BlockSize)
    {
        this.m_Bitmap = BitmapConvert.ToBidimensionalArray(p_Bitmap);
        this.m_BitmapSize = new Size(((Image) p_Bitmap).Width, ((Image) p_Bitmap).Height);
        this.m_BlockSize = p_BlockSize;
    }

    public BlockTransform(Color[,] p_Bitmap, Size p_BlockSize)
    {
        this.m_Bitmap = p_Bitmap;
        this.m_BitmapSize = new Size(p_Bitmap.GetLength(0), p_Bitmap.GetLength(1));
        this.m_BlockSize = p_BlockSize;
    }

    public static Size GetBlockMatrixSize(ArrayList blocks)
    {
        Size size = new Size(0, 0);
        for (int index = 0; index < blocks.Count; ++index)
        {
            ImageBlockInfo block = (ImageBlockInfo) blocks[index];
            if (block.x > size.Width)
                size.Width = block.x;
            if (block.y > size.Height)
                size.Height = block.y;
        }
        return new Size(size.Width + 1, size.Height + 1);
    }

    public ArrayList MakeBlocks(int Rank)
    {
        Transform transform = new Transform(this.m_Bitmap);
        ArrayList arrayList = new ArrayList();
        for (int y = 0; y < this.m_BitmapSize.Height; y += this.m_BlockSize.Height)
        {
            for (int x = 0; x < this.m_BitmapSize.Width; x += this.m_BlockSize.Width)
            {
                int width = this.m_BlockSize.Width;
                int height = this.m_BlockSize.Height;
                if (x + width >= this.m_BitmapSize.Width)
                    width = this.m_BitmapSize.Width - x;
                if (y + height >= this.m_BitmapSize.Height)
                    height = this.m_BitmapSize.Height - y;
                ImageBlockInfo imageBlockInfo = transform.Calculate(Rank, new Rectangle(x, y, width, height));
                imageBlockInfo.x = x / this.m_BlockSize.Width;
                imageBlockInfo.y = y / this.m_BlockSize.Height;
                imageBlockInfo.index = arrayList.Count;
                arrayList.Add((object) imageBlockInfo);
            }
        }
        return arrayList;
    }
}