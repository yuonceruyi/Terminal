using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using YuanTu.Consts;

namespace YuanTu.AutoUpdater
{
    public class CommonUnitity
    {
        public static string SystemBinUrl = AppDomain.CurrentDomain.BaseDirectory;
        public static event Action<string> Log;

        public static void log(string s)
        {
            Log?.Invoke(s);
        }

        public static void RestartApplication()
        {
            try
            {
                FrameworkConst.MutexLock.Dispose();
            }
            catch (Exception ex)
            {
                log(ex.Message);
            }

            Process.Start(Application.ExecutablePath);
            Environment.Exit(0);
        }

        public static string GetFolderUrl(DownloadFileInfo file)
        {
            var folderPathUrl = string.Empty;
            if (file.FileFullName.IndexOf("\\") != -1)
            {
                var ExeGroup = file.FileFullName.Split('\\');
                for (var i = 0; i < ExeGroup.Length - 1; i++)
                {
                    folderPathUrl += "\\" + ExeGroup[i];
                }
                if (!Directory.Exists(SystemBinUrl + ConstFile.TEMPFOLDERNAME + folderPathUrl))
                {
                    Directory.CreateDirectory(SystemBinUrl + ConstFile.TEMPFOLDERNAME + folderPathUrl);
                }
            }
            return folderPathUrl;                       
        }
    }
}