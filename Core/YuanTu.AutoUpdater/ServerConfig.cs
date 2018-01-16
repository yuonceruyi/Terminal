using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace YuanTu.AutoUpdater
{
    public class ServerConfig
    {
        public List<RemoteFile> FileList;
        public string ServerUrl;

        public static ServerConfig LoadConfig(string file)
        {
            ServerConfig config;
            using (var ms = new MemoryStream())
            {
                var doc = new XmlDocument();
                doc.Load(file);
                doc.Save(ms);

                ms.Seek(0, SeekOrigin.Begin);

                var xs = new XmlSerializer(typeof (ServerConfig));
                using (var sr = new StreamReader(ms))
                    config = xs.Deserialize(sr) as ServerConfig;
            }
            return config;
        }

        public void SaveConfig(string file)
        {
            var xs = new XmlSerializer(typeof (ServerConfig));
            var sw = new StreamWriter(file);
            xs.Serialize(sw, this);
            sw.Close();
        }

        public static Dictionary<string, RemoteFile> ParseXml(string xml)
        {
            var config = LoadConfig(xml);
            var configLocal = Config.LoadConfig(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConstFile.FILENAME));
            var dic = new Dictionary<string, RemoteFile>();
            foreach (var file in config.FileList)
            {
                file.Url = Path.Combine(configLocal.ServerUrl, file.Url);
                dic.Add(file.Path, file);
            }

            return dic;
        }
    }
}