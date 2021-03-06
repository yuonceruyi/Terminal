using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace YuanTu.AutoUpdater
{
    public class AutoUpdater : IAutoUpdater
    {
        #region The constructor of AutoUpdater

        public AutoUpdater()
        {
            config = Config.LoadConfig(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConstFile.FILENAME));
        }

        #endregion The constructor of AutoUpdater

        #region The public event

        //public event Action OnShow;

        #endregion The public event

        #region The private fields

        private readonly Config config;
        private bool bDownload;
        private bool bNeedRestart;
        private List<DownloadFileInfo> downloadFileListTemp;
        public static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;
        public static string ServerUrl;
        public static List<string> PathList=new List<string>();

        public static Config Config
        {
            get { return Config.LoadConfig(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConstFile.FILENAME)); }
        }

        #endregion The private fields

        #region The public method

        public bool Check()
        {
            if (!config.Enabled)
                return false;
            var listRemotFile = ServerConfig.ParseXml(Path.Combine(config.ServerUrl, ConstFile.REMOTEFILENAME));

            foreach (var file in config.UpdateFileList)
            {
                if (!listRemotFile.ContainsKey(file.Path))
                    continue;
                var rf = listRemotFile[file.Path];
                var v1 = new Version(rf.LastVer);
                var v2 = new Version(file.LastVer);
                if (v1 > v2)
                    return true;
                listRemotFile.Remove(file.Path);
            }
            if (listRemotFile.Values.Count > 0)
                return true;
            return false;
        }

        public void Update()
        {
            if (!config.Enabled)
                return;

            var listRemotFile = ServerConfig.ParseXml(Path.Combine(config.ServerUrl, ConstFile.REMOTEFILENAME));

            var downloadList = new List<DownloadFileInfo>();

            //旧文件更新
            foreach (var file in config.UpdateFileList)
            {
                if (!listRemotFile.ContainsKey(file.Path))
                    continue;
                var rf = listRemotFile[file.Path];
                var v1 = new Version(rf.LastVer);
                var v2 = new Version(file.LastVer);
                if (v1 > v2)
                {
                    downloadList.Add(new DownloadFileInfo(rf.Url, file.Path, rf.LastVer, rf.Size));
                    file.LastVer = rf.LastVer;
                    file.Size = rf.Size;

                    if (rf.NeedRestart)
                        bNeedRestart = true;

                    bDownload = true;
                }

                listRemotFile.Remove(file.Path);
            }

            //新文件下载
            foreach (var file in listRemotFile.Values)
            {
                config.UpdateFileList.Add(new LocalFile(file.Path, file.LastVer, file.Size));
                downloadList.Add(new DownloadFileInfo(file.Url, file.Path, file.LastVer, file.Size));

                if (file.NeedRestart)
                    bNeedRestart = true;

                bDownload = true;
            }

            downloadFileListTemp = downloadList;

            if (bDownload)
                StartDownload(downloadList);
        }

        public void RollBack()
        {
            foreach (var file in downloadFileListTemp)
            {
                var tempUrlPath = CommonUnitity.GetFolderUrl(file);
                var oldPath = string.Empty;
                try
                {
                    if (!string.IsNullOrEmpty(tempUrlPath))
                    {
                        oldPath = Path.Combine(CommonUnitity.SystemBinUrl + tempUrlPath.Substring(1), file.FileName);
                    }
                    else
                    {
                        oldPath = Path.Combine(CommonUnitity.SystemBinUrl, file.FileName);
                    }

                    if (oldPath.EndsWith("_"))
                        oldPath = oldPath.Substring(0, oldPath.Length - 1);

                    MoveFolderToOld(oldPath + ".old", oldPath);
                }
                catch (Exception ex)
                {
                    //log the error message,you can use the application's log code
                    CommonUnitity.log(ex.Message + "\n" + ex.StackTrace);
                }
            }
        }

        #endregion The public method

        #region The private method

        private string newfilepath = string.Empty;

        private void MoveFolderToOld(string oldPath, string newPath)
        {
            if (File.Exists(oldPath) && File.Exists(newPath))
            {
                File.Copy(oldPath, newPath, true);
            }
        }

        private void StartDownload(List<DownloadFileInfo> downloadList)
        {
            var dp = new DownloadProgress(downloadList);
            if (dp.ShowDialog() == DialogResult.OK)
            {
                //
                if (DialogResult.Cancel == dp.ShowDialog())
                {
                    return;
                }
                //Update successfully
                config.SaveConfig(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConstFile.FILENAME));

                if (bNeedRestart)
                {
                    //Delete the temp folder
                    Directory.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConstFile.TEMPFOLDERNAME), true);

                    //MessageBox.Show(ConstFile.APPLYTHEUPDATE, ConstFile.MESSAGETITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CommonUnitity.RestartApplication();
                }
            }
        }

        private Dictionary<string, RemoteFile> ParseRemoteXml(string xml)
        {
            var serverConfig = ServerConfig.LoadConfig(xml);
            var dic = new Dictionary<string, RemoteFile>();
            foreach (var file in serverConfig.FileList)
            {
                file.Url = serverConfig.ServerUrl + file.Url;
                dic.Add(file.Path, file);
            }
            return dic;
        }

        #endregion The private method
    }
}