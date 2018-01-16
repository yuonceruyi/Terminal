using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.YanTaiYDYY.Component.InfoQuery.Service
{
    public static class ScheduleFile
    {
        public static List<string> ScheduleFileList =
            new List<string>();

        public static string ScheduleFileListPath;
    }

    public class ScheduleService
    {
        /// <summary>  
        /// 获取ftp服务器上指定文件夹的文件列表 
        /// </summary>  
        /// <param name="serverIp"></param>  
        /// <param name="userid"></param>  
        /// <param name="passWord"></param>  
        /// <param name="path"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>  
        public static List<string> GetFTPList(string serverIp, string userid, string passWord, string path, string fileType)
        {
            List<string> dic = new List<string>
            {
                "mzpb1.jpg","mzpb2.jpg","mzpb3.jpg"
            };
            ScheduleFile.ScheduleFileListPath = "ftp://client: @202.202.203.17/门诊部";
            return dic;

            #region  测试屏蔽

            //            List<string> dic = new List<string>();
            //            if (path == null)
            //                path = "";
            //            FtpWebRequest reqFtp;
            //            try
            //            {
            //                reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + serverIp + "/" + path));
            //                reqFtp.KeepAlive = false;
            //                reqFtp.UseBinary = true;   //指定ftp数据传输类型为 二进制  
            //                reqFtp.Credentials = new NetworkCredential(userid, passWord);     //设置于ftp通讯的凭据  
            //                reqFtp.Method = WebRequestMethods.Ftp.ListDirectoryDetails;      //指定操作方式  
            //                WebResponse response = reqFtp.GetResponse();  //获取一个FTP响应  
            //
            //                ScheduleFile.ScheduleFileListPath = reqFtp.RequestUri.OriginalString;
            //                var reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("GB2312"));   //读取响应流  
            //                var line = reader.ReadLine();
            //                while (line != null)
            //                {
            //                    if (line != "." && line != "..")
            //                    {
            //                        int end = line.LastIndexOf(' ');
            //                        string filename = line.Substring(end + 1);
            //                        if (filename.Contains(".") && filename.Contains(fileType))
            //                        {
            //                            dic.Add(filename.Trim());
            //                        }
            //                    }
            //                    line = reader.ReadLine();
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                return new List<string>();
            //            }
            //
            //            return dic;

            #endregion
        }
    }
}
