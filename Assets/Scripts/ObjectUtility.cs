using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class ObjectUtility
{
    // Start is called before the first frame update
    public static T DeepClone<T>(this T o)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, o);
            stream.Position = 0;
            return (T)formatter.Deserialize(stream);
        }
    }
}
