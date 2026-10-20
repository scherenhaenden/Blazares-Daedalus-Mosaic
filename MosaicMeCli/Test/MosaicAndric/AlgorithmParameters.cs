using System.Drawing;

namespace MosaicMeCli.Test.MosaicAndric;

public class AlgorithmParameters
{
    public Size SourceBlockSize;
    public bool UseAssociativeMemory;
    public Size OutputSize;
    public int rank = 8;
    public int MinTileDistance = 3;
    public EDbImageUsage db_images_usage = EDbImageUsage.BlockWithDistance;
    public double ColorCorrection = 0.6;

    public AlgorithmParameters() => this.SourceBlockSize = new Size(16, 16);
}