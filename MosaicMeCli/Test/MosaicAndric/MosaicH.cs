using System.Collections;

namespace MosaicMeCli.Test.MosaicAndric;

public class MosaicH : Mosaic
{
    private IMScatterSearch m_scatter;
    public EAction m_Action = EAction.Mosaic;
    public string m_ampl_name;

    public MosaicH(ArrayList p_DataBase)
        : base(p_DataBase)
    {
    }

    public override ArrayList MatchBlockList(ArrayList pBlockList)
    {
        if (pBlockList.Count > this.m_DataBase.Count)
            throw new Exception("The number of blocks to be assigned (" + (object) pBlockList.Count + "), is greater than  size of db (" + (object) this.m_DataBase.Count + ")");
        IMSolution imSolution = (IMSolution) null;
        this.m_scatter = new IMScatterSearch(this, pBlockList);
        switch (this.m_Action)
        {
            case EAction.Mosaic:
                imSolution = (IMSolution) this.m_scatter.Minimize();
                break;
            case EAction.ExportAmpl:
                this.m_scatter.ExportAMPLDatFile(this.m_ampl_name);
                return (ArrayList) null;
            case EAction.ImportAmpl:
                imSolution = this.m_scatter.ImportAMPLSolution(this.m_ampl_name);
                break;
        }
        ArrayList arrayList = new ArrayList();
        for (int index = 0; index < imSolution._x.Length; ++index)
            arrayList.Add(this.m_DataBase[imSolution._x[index]]);
        return arrayList;
    }

    protected override double CalculateDistance(
        ImageBlockInfo b1,
        ImageLink b2,
        double MinDistance)
    {
        return this.m_scatter.K(b2.MemoryId, b1.index);
    }
}