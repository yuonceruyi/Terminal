using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.Core.Configuration
{
    public class CentralConfigurationProvider : ConfigurationProvider
    {
        private readonly CentralClient _client;

        public CentralConfigurationProvider(string ip, string mac, string url, Func<Dictionary<string, string>> getLocalConfigFunc = null)
        {
            _client = new CentralClient(ip, mac, url)
            {
                UpdateLocalConfigAction = dic =>
                {
                    Data = dic;
                    OnReload();
                },
                GetLocalConfigFunc = getLocalConfigFunc
            };
        }

        public override void Load()
        {
            var sw = Stopwatch.StartNew();
            var task = Task.Run(async () =>
            {
                try
                {
                    return await _client.Load();
                }
                catch (Exception e)
                {
                    Logger.Main.Info($"{CentralClient.GetPrefix()}[Load][{sw.ElapsedMilliseconds}ms]Exception-{e}");
                    return Result<Dictionary<string, string>>.Fail("Load Failed:" + e.Message, e);
                }
            });
            var result = task.Result;
            sw.Stop();
            Logger.Main.Info($"{CentralClient.GetPrefix()}[Load][{sw.ElapsedMilliseconds}ms]{result.IsSuccess}-{result.ResultCode}-{result.Message}");

            if (!result.IsSuccess)
            {
                var cacheResult = GetCache();
                Data = cacheResult.IsSuccess ? cacheResult.Value : new Dictionary<string, string>();
            }
            else
            {
                var data = result.Value;
                PutCache(data);
                Data = data;
            }
            _client.Start();
        }

        private static string FilePath => Path.Combine(FrameworkConst.RootDirectory, "CurrentResource", "Central.Config.json");

        public static void PutCache(Dictionary<string, string> data)
        {
            File.WriteAllText(FilePath, data.ToJsonString());
        }

        public static Result<Dictionary<string, string>> GetCache()
        {
            try
            {
                var json = File.ReadAllText(FilePath);
                var data = json.ToJsonObject<Dictionary<string, string>>();
                return Result<Dictionary<string, string>>.Success(data);
            }
            catch (Exception e)
            {
                return Result<Dictionary<string, string>>.Fail($"GetCache Failed:{e.Message}", e);
            }
        }
    }
}