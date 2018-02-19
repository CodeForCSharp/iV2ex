using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace MyToolkit.Serialization
{
    public sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }

    public static class XmlSerialization
    {
        public static string Serialize(object obj, Type[] extraTypes = null)
        {
            using (var sw = new Utf8StringWriter())
            {
                var type = obj.GetType();
                var serializer = extraTypes == null ? new XmlSerializer(type) : new XmlSerializer(type, extraTypes);
                serializer.Serialize(sw, obj);
                return sw.ToString();
            }
        }

        public static T Deserialize<T>(string xml, Type[] extraTypes = null)
        {
            using (var sw = new StringReader(xml))
            {
                var serializer = extraTypes == null
                    ? new XmlSerializer(typeof(T))
                    : new XmlSerializer(typeof(T), extraTypes);
                return (T) serializer.Deserialize(sw);
            }
        }

        public static Task<string> SerializeAsync(object obj, Type[] extraTypes = null)
        {
            var source = new TaskCompletionSource<string>();
            Task.Factory.StartNew(() =>
            {
                try
                {
                    source.SetResult(Serialize(obj, extraTypes));
                }
                catch (Exception ex)
                {
                    source.SetException(ex);
                }
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
            return source.Task;
        }

        public static Task<T> DeserializeAsync<T>(string xml, Type[] extraTypes = null)
        {
            var source = new TaskCompletionSource<T>();
            Task.Factory.StartNew(() =>
            {
                try
                {
                    source.SetResult(Deserialize<T>(xml, extraTypes));
                }
                catch (Exception ex)
                {
                    source.SetException(ex);
                }
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
            return source.Task;
        }


        public static string XmlEscape(string unescaped)
        {
            var doc = new XmlDocument();
            var node = doc.CreateElement("root");
            node.InnerText = unescaped;
            return node.InnerText;
        }

        public static string XmlUnescape(string escaped)
        {
            var doc = new XmlDocument();
            var node = doc.CreateElement("root");
            node.InnerText = escaped;
            return node.InnerText;
        }
    }
}