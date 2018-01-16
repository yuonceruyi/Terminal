using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace YuanTu.YuHangArea.CYHIS.DLL
{
    public static class Utility
    {
        private static readonly Encoding encoding = new UTF8Encoding(false);
        private static readonly XmlWriterSettings settings;
        private static readonly XmlSerializerNamespaces ns;

        static Utility()
        {
            settings = new XmlWriterSettings
            {
                Encoding = encoding,
                Indent = false,
                OmitXmlDeclaration = false
            };
            ns = new XmlSerializerNamespaces();
            ns.Add("", "");
        }

        public static string Serilize(object reqItem)
        {
            var serializer = new XmlSerializer(reqItem.GetType());
            string text;
            using (var ms = new MemoryStream())
            {
                using (var textWriter = new StreamWriter(ms, encoding))
                {
                    using (var xmlWriter = XmlWriter.Create(textWriter, settings))
                    {
                        serializer.Serialize(xmlWriter, reqItem, ns);
                    }
                    text = encoding.GetString(ms.GetBuffer()).Trim('\0');
                }
            }
            return text;
        }
    }
}