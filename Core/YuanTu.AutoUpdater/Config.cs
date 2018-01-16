using System.IO;
using System.Xml.Serialization;

namespace YuanTu.AutoUpdater
{
    public class Config
    {
        #region The private fields

        #endregion The private fields

        #region The public property

        public bool Enabled { get; set; } = true;

        public string ServerUrl { get; set; } = string.Empty;

        public UpdateFileList UpdateFileList { get; set; } = new UpdateFileList();

        #endregion The public property

        #region The public method

        public static Config LoadConfig(string file)
        {
            if (!File.Exists(file))
            {
                CreateConfig(file);
            }
            var xs = new XmlSerializer(typeof (Config));
            var sr = new StreamReader(file);
            var config = xs.Deserialize(sr) as Config;
            sr.Close();

            return config;
        }

        public void SaveConfig(string file)
        {
            var xs = new XmlSerializer(typeof (Config));
            var sw = new StreamWriter(file);
            xs.Serialize(sw, this);
            sw.Close();
        }

        private static void CreateConfig(string file)
        {
            var config = new Config
            {
                Enabled = true,
                ServerUrl = AutoUpdater.ServerUrl
            };
            foreach (var path in AutoUpdater.PathList)
            {
                var f = new FileInfo(Path.Combine(AutoUpdater.BaseDir, path));
                config.UpdateFileList.Add(new LocalFile
                {
                    Path = path,
                    LastVer = "1.0.0.0",
                    Size = f.Length
                });
            }
            config.SaveConfig(file);
        }

        #endregion The public method
    }
}