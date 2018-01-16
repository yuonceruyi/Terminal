using System.Xml.Serialization;

namespace YuanTu.AutoUpdater
{
    public class LocalFile
    {
        #region The private fields

        #endregion

        #region The public property

        [XmlAttribute("path")]
        public string Path { get; set; } = "";

        [XmlAttribute("lastver")]
        public string LastVer { get; set; } = "";

        [XmlAttribute("size")]
        public long Size { get; set; }

        #endregion

        #region The constructor of LocalFile

        public LocalFile(string path, string ver, long size)
        {
            Path = path;
            LastVer = ver;
            Size = size;
        }

        public LocalFile()
        {
            Size = 0;
        }

        #endregion
    }
}