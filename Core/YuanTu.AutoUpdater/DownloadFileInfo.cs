using System.IO;

namespace YuanTu.AutoUpdater
{
    public class DownloadFileInfo
    {
        #region The constructor of DownloadFileInfo

        public DownloadFileInfo(string url, string name, string ver, long size)
        {
            DownloadUrl = url;
            FileFullName = name;
            LastVer = ver;
            Size = size;
        }

        #endregion

        #region The private fields

        #endregion

        #region The public property

        public string DownloadUrl { get; } = string.Empty;

        public string FileFullName { get; } = string.Empty;

        public string FileName
        {
            get { return Path.GetFileName(FileFullName); }
        }

        public string LastVer { get; set; } = string.Empty;

        public long Size { get; }

        #endregion
    }
}