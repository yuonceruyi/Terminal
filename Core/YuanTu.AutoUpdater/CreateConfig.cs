using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace YuanTu.AutoUpdater
{
    public class CreateFileConfig
    {
        #region The public property
        public string ServerUrl { get; set; } = string.Empty;

        public CreateFileList CreateFileList { get; set; } = new CreateFileList();

        #endregion The public property

        #region The public method

        public static CreateFileConfig LoadConfig(string file)
        {
            if (!File.Exists(file))
            {
                CreateConfig(file);
            }
            var xs = new XmlSerializer(typeof (CreateFileConfig));
            var sr = new StreamReader(file);
            var createFileConfig = xs.Deserialize(sr) as CreateFileConfig;
            sr.Close();

            return createFileConfig;
        }

        public void SaveConfig(string file)
        {
            var xs = new XmlSerializer(typeof (CreateFileConfig));
            var sw = new StreamWriter(file);
            xs.Serialize(sw, this);
            sw.Close();
        }

        private static void CreateConfig(string file)
        {
            var createFileConfig = new CreateFileConfig
            {
                ServerUrl = "http://192.168.0.151:8082/UpdateVer2/"
            };

            List<string> pathList = new List<string>
            {
                "YuanTu.AutoUpdater.dll",
                "Terminal.exe",
                "YuanTu.Default.dll",
                "YuanTu.Devices.dll",
                "YuanTu.Core.dll",
                "YuanTu.Consts.dll",
                "YuanTu.QDKouQiangYY.dll",
                "YuanTu.WeiHaiArea.dll",
            };
            foreach (var fileName in pathList)
            {
                var f = new FileInfo(Path.Combine(AutoUpdater.BaseDir, fileName));
                createFileConfig.CreateFileList.Add(new CreateFile
                {
                    FileName = fileName,
                });
            }
            createFileConfig.SaveConfig(file);
        }

        #endregion The public method
    }
}