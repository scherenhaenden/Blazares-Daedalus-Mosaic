using System.Drawing;

namespace MosaicMeCli.Test.MosaicAndric;

public class AreaInfo
{
    public Rectangle Area;
    public string Info;

    public AreaInfo(Rectangle p_Area, string p_Info)
    {
        this.Area = p_Area;
        this.Info = p_Info;
    }
}