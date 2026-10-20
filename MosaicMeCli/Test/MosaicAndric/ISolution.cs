namespace MosaicMeCli.Test.MosaicAndric;

public interface ISolution : IComparable
{
    object X { get; set; }

    double Value { get; set; }

    double GetDistance(ISolution s);
}