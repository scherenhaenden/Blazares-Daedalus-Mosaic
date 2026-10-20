namespace MosaicMeCli.Test.MosaicAndric;

public class DoubleKey : IComparable
{
    public double Value;
    public object Info;

    public DoubleKey()
    {
    }

    public DoubleKey(double p_Value, object p_Info)
    {
        this.Value = p_Value;
        this.Info = p_Info;
    }

    public int CompareTo(object obj)
    {
        DoubleKey doubleKey = (DoubleKey) obj;
        if (this.Value < doubleKey.Value)
            return -1;
        return this.Value > doubleKey.Value ? 1 : 0;
    }
}