namespace YuanTu.AutoUpdater
{
    public class RemoteFile
    {
        #region The private fields

        #endregion The private fields

        #region The public property

        public string Path { get; set; } = "";

        public string Url { get; set; } = "";

        public string LastVer { get; set; } = "";

        public long Size { get; set; }

        public bool NeedRestart { get; set; }

        #endregion The public property
    }
}