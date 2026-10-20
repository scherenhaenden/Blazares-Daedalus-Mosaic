using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
//using System.Runtime.Serialization.Formatters.Soap;

namespace MosaicMeCli.Test.MosaicAndric;

public class MyObject
{
    public bool WriteToStream(Stream stream)
    {
        try
        {
            new BinaryFormatter().Serialize(stream, (object) this);
        }
        catch (Exception ex)
        {
            return false;
        }
        return true;
    }

    public static MyObject ReadFromStream(Stream stream)
    {
        MyObject myObject = (MyObject) null;
        try
        {
            IFormatter formatter = (IFormatter) new BinaryFormatter();
            try
            {
                myObject = (MyObject) formatter.Deserialize(stream);
            }
            catch
            {
            }
        }
        catch (Exception ex)
        {
        }
        return myObject;
    }

    public bool WriteToXML(string fileName)
    {
        try
        {
            IFormatter formatter = (IFormatter) new SoapFormatter();
            Stream serializationStream = (Stream) new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(serializationStream, (object) this);
            serializationStream.Close();
        }
        catch (Exception ex)
        {
            return false;
        }
        return true;
    }

    public static MyObject ReadFromXML(string fileName)
    {
        MyObject myObject = (MyObject) null;
        try
        {
            Stream serializationStream = (Stream) new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            IFormatter formatter = (IFormatter) new SoapFormatter();
            try
            {
                myObject = (MyObject) formatter.Deserialize(serializationStream);
            }
            catch
            {
            }
            serializationStream.Close();
        }
        catch (Exception ex)
        {
        }
        return myObject;
    }
}