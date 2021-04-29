using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace FaaMvaToSectorFile
{
    public static class SerializationUtils
    {
        public static T DeserializeObjectFromFile<T>(string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            T result;
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                T t = (T)((object)xmlSerializer.Deserialize(new XmlTextReader(fileStream)
                {
                    Normalization = false
                }));
                bool flag = t is IDeserializationCallback;
                if (flag)
                {
                    (t as IDeserializationCallback).OnDeserialize();
                }
                result = t;
            }
            return result;
        }
    }

    public interface IDeserializationCallback
    {
        void OnDeserialize();
    }
}
