using System.Xml.Serialization;

namespace YuanTu.AutoUpdater
{
    public class CreateFile
    {
        #region The private fields

        #endregion

        #region The public property

        [XmlAttribute("fileName")]
        public string FileName { get; set; } = "";

        #endregion

        #region The constructor of LocalFile

        public CreateFile(string fileName)
        {
            FileName = fileName;
        }
        public CreateFile()
        {
        }

        #endregion
    }
}