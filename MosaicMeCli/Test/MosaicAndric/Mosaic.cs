using System.Collections;

namespace MosaicMeCli.Test.MosaicAndric;

public class Mosaic
{
    protected Random R;
    protected ArrayList m_DataBase;
    public int MinTileDistance = 3;
    public bool AvoidUsingNearEqualTiles = true;
    protected NeighboroudHandling Adiacence;

    public ArrayList DataBase => this.m_DataBase;

    public Mosaic(ArrayList p_DataBase)
    {
        this.m_DataBase = p_DataBase;
        this.R = new Random();
        this.InitializeDB();
    }

    public virtual ImageLink MatchBlock(ImageBlockInfo pBlock) => this.MatchBlock(pBlock, this.m_DataBase);

    protected virtual double CalculateDistance(ImageBlockInfo b1, ImageLink b2, double MinDistance) => b1.Distance(b2.m_info, MinDistance);

    public ImageLink MatchBlock(ImageBlockInfo pBlock, ImageLink[] p_DataBase)
    {
        double MinDistance = double.MaxValue;
        ImageLink imageLink = (ImageLink) null;
        for (int index = 0; index < p_DataBase.Length; ++index)
        {
            ImageLink b2 = p_DataBase[index];
            double distance = this.CalculateDistance(pBlock, b2, MinDistance);
            if (distance < MinDistance)
            {
                MinDistance = distance;
                imageLink = b2;
            }
        }
        return imageLink;
    }

    public ImageLink MatchBlock(ImageBlockInfo pBlock, ArrayList p_DataBase)
    {
        double MinDistance = double.MaxValue;
        ImageLink imageLink = (ImageLink) null;
        for (int index = 0; index < p_DataBase.Count; ++index)
        {
            ImageLink b2 = (ImageLink) p_DataBase[index];
            if (b2.m_info != null && b2.usable)
            {
                double distance = this.CalculateDistance(pBlock, b2, MinDistance);
                if (distance < MinDistance)
                {
                    MinDistance = distance;
                    imageLink = b2;
                }
            }
        }
        if (PMWorkClass.WorkStep != null)
            PMWorkClass.WorkStep();
        ++PMWorkClass.WorkCurrent;
        return imageLink;
    }

    public void InitializeDB()
    {
        for (int index = 0; index < this.m_DataBase.Count; ++index)
        {
            ((ImageLink) this.m_DataBase[index]).usable = true;
            ((ImageLink) this.m_DataBase[index]).MemoryId = index;
        }
    }

    public virtual ArrayList MatchBlockList(ArrayList pBlockList)
    {
        if (this.AvoidUsingNearEqualTiles)
            this.Adiacence = new NeighboroudHandling(pBlockList);
        this.InitializeDB();
        ArrayList arrayList = new ArrayList();
        for (int index = 0; index < pBlockList.Count; ++index)
        {
            ImageBlockInfo pBlock = (ImageBlockInfo) pBlockList[index];
            if (this.AvoidUsingNearEqualTiles)
                this.Adiacence.TagAdicacentBlocks(this.m_DataBase, pBlock.x, pBlock.y, (double) (this.MinTileDistance + (int) (2.0 * this.R.NextDouble())));
            ImageLink imageLink = this.MatchBlock((ImageBlockInfo) pBlockList[index]);
            if (imageLink == null && this.AvoidUsingNearEqualTiles)
            {
                double probability = 0.1;
                do
                {
                    this.Adiacence.UntagAdicacentBlocks(this.m_DataBase, probability);
                    imageLink = this.MatchBlock((ImageBlockInfo) pBlockList[index]);
                    probability *= 2.0;
                }
                while (imageLink == null);
            }
            arrayList.Add((object) imageLink);
            if (this.AvoidUsingNearEqualTiles)
            {
                this.Adiacence.UntagAdicacentBlocks(this.m_DataBase, 1.0);
                this.Adiacence.neighboroud[pBlock.x, pBlock.y] = imageLink.MemoryId;
            }
        }
        return arrayList;
    }
}