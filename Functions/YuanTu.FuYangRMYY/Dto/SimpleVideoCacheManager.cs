using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.FuYangRMYY
{
    public static class SimpleVideoCacheManager
    {

        public static string[] MergeVideoList()
        {
            var videoPath = Default.Clinic.Startup.VideoPath;
            var http=new HttpClient();
            var resp = http.GetStringAsync(videoPath).Result;
            var data = resp.ToJsonObject<Result<VideoItem[]>>();
            var dir = Path.Combine(FrameworkConst.RootDirectory, "Cache");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);

            }
            Logger.Main.Info($"计入:{data.ToJsonString()}");
            if (data)
            {
                var kv=new Dictionary<string,string>();
                //删除不播放的文件
                var files = Directory.GetFiles(dir);
                var newfile = data.Value.Select(p => Path.GetFileName(p.Url));
                foreach (var file in files)
                {
                    if (!newfile.Contains(Path.GetFileName(file)))
                    {
                        File.Delete(file);
                    }
                }
                //循环出需要下载的文件，并构建本地地址映射
                foreach (var item in data.Value)
                {
                    var fileName = Path.GetFileName(item.Url) ?? "tmp";
                    var fullPath = Path.Combine(dir, fileName);
                    if (File.Exists(fullPath))
                    {
                        continue;
                    }
                    kv[item.Url] = fullPath;
                }
                Logger.Main.Info($"下载计划:{kv.ToJsonString()}");
                if (kv.Any())
                {
                    Task.Run(() =>
                    {
                        foreach (var video in kv)
                        {
                            Logger.Main.Info($"下载文件:{video.ToJsonString()}");
                            var fullPathtmp = video.Value + ".tmp";
                            new WebClient().DownloadFile(video.Key, fullPathtmp);
                            Thread.Sleep(300);
                            File.Delete(video.Value);
                            File.Move(fullPathtmp, video.Value);
                        }
                    });
                }
               
                return Directory.GetFiles(dir).Concat(kv.Values).Distinct().ToArray();
            }
            return Directory.GetFiles(dir).ToArray();

        }
    }
}
