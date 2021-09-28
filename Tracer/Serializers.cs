using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracerNS
{

    using System.IO;
    using System.Text.Json;
    using System.Xml.Serialization;
    using Newtonsoft.Json;

    public  interface ISerializer
    {
        public void Serialize(System.IO.Stream serializationStream, TraceResult root);
    }

    public class SerializerInJson : ISerializer
    {
        

        public void Serialize(System.IO.Stream serializationStream, TraceResult root)
        {
            //string jsonString = JsonSerializer.Serialize(root, typeof(TraceResult));
            string jsonString = JsonConvert.SerializeObject(root, Formatting.Indented);
            //jsonString = jsonString.Replace("[", "\n[");
            //jsonString = jsonString.Replace("]", "\n]");
            //jsonString = jsonString.Replace("{", "\n{");
            //jsonString = jsonString.Replace("}", "\n}");
            //jsonString = jsonString.Replace(",", "\n,");
            var stream = new StreamWriter(serializationStream);
            stream.AutoFlush = true;
            stream.Write(jsonString);
        }
    }

    public class SerializerInXml : ISerializer
    {
        public void Serialize(System.IO.Stream serializationStream, TraceResult root)
        {
            //XmlSerializer formatter = new XmlSerializer(typeof(TraceResult));
            List<Entry> entries = new List<Entry>(root.threadsInfo.Count);
            foreach (int key in root.threadsInfo.Keys)
            {
                entries.Add(new Entry(key, root.threadsInfo[key]));
            }
            XmlSerializer formatter = new XmlSerializer(typeof(List<Entry>));
            formatter.Serialize(serializationStream, entries);
        }
    }

    public class Entry
    {
        [XmlAttribute]
        public int ThreadId { get; set; }
        [XmlElement(ElementName = "Thread")]
        //[XmlInclude(typeof(ThreadInfo))]
        public ThreadInfo Value { get; set; }
        public Entry()
        {
        }

        public Entry(int key, ThreadInfo value)
        {
            ThreadId = key;
            Value = value;
        }
    }
}
