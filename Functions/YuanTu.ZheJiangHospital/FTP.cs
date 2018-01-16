using System;
using System.IO;
using System.Net;
using YuanTu.Core.Log;

namespace YuanTu.ZheJiangHospital
{
    public static class FTP
    {
        /// <summary>
        ///     上传文件
        /// </summary>
        /// <param name="fileinfo">需要上传的文件</param>
        /// <param name="targetDir">目标路径</param>
        /// <param name="hostname">ftp地址</param>
        /// <param name="username">ftp用户名</param>
        /// <param name="password">ftp密码</param>
        public static bool UploadFile(FileInfo fileinfo, string targetDir, string hostname, string username,
            string password)
        {
            if (!fileinfo.Exists)
                return false;
            var uri = $"FTP://{hostname}/{targetDir}/{fileinfo.Name}";
            if (targetDir.Trim() == "")
                uri = $"FTP://{hostname}/{fileinfo.Name}";

            var ftp = (FtpWebRequest)WebRequest.Create(uri);
            ftp.Credentials = new NetworkCredential(username, password);
            ftp.KeepAlive = false;
            ftp.Method = WebRequestMethods.Ftp.UploadFile;
            ftp.UseBinary = true;
            ftp.UsePassive = true;
            ftp.ContentLength = fileinfo.Length;
            const int BufferSize = 2048;
            var content = new byte[BufferSize - 1 + 1];
            int dataRead;

            using (var fs = fileinfo.OpenRead())
            {
                try
                {
                    using (var rs = ftp.GetRequestStream())
                    {
                        do
                        {
                            dataRead = fs.Read(content, 0, BufferSize);
                            rs.Write(content, 0, dataRead);
                        } while (!(dataRead < BufferSize));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Main.Error(
                        $"上传失败\n{fileinfo.FullName}\nFTP://{hostname}/{targetDir}/{fileinfo.Name}\n{ex.Message}\n{ex.StackTrace}");
                    return false;
                }
                finally
                {
                    fs.Close();
                }
            }

            Logger.Main.Info($"上传成功\n{fileinfo.FullName}\nFTP://{hostname}/{targetDir}/{fileinfo.Name}");
            return true;
        }
    }
}