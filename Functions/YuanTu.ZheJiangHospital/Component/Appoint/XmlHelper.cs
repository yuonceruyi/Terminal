using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace YuanTu.ZheJiangHospital.Component.Appoint
{
    internal class XmlHelper
    {
        private static readonly Encoding Encoding;
        private static readonly XmlWriterSettings Settings;
        private static readonly XmlSerializerNamespaces Ns;

        static XmlHelper()
        {
            Encoding = new UTF8Encoding(false);
            Settings = new XmlWriterSettings
            {
                Encoding = Encoding,
                Indent = false,
                OmitXmlDeclaration = true
            };
            Ns = new XmlSerializerNamespaces();
            Ns.Add("", "");
        }

        public static string Serialize(object o)
        {
            var xs = new XmlSerializer(o.GetType());
            using (var stream = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(stream, Settings))
                {
                    xs.Serialize(writer, o, Ns);
                }
                return Encoding.GetString(stream.ToArray());
            }
        }

        public static T Deserialize<T>(string text)
            where T : class
        {
            var res = new XmlSerializer(typeof(T))
                .Deserialize(new StringReader(text)) as T;
            return res;
        }
    }
}