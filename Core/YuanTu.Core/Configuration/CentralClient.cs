using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.Core.Configuration
{
    internal class CentralClient
    {
        private readonly HubConnection _connection;
        private readonly string _ip;
        private readonly string _mac;
        private readonly string _url;
        private readonly IHubProxy _proxy;
        private bool _connecting;
        private bool _refreshing;
        private const int RefreshDelay = 5 * 1000;

        //private const int RefreshDelay = 5 * 60 * 1000;
        private const int ReconnectDelay = 5 * 1000;

        public Func<Dictionary<string, string>> GetLocalConfigFunc { get; set; }
        public Action<Dictionary<string, string>> UpdateLocalConfigAction { get; set; }
        public DateTime LastAlterTime { get; set; }

        private static long _count;
        public static string GetPrefix()
        {
            return $"[配置服务][{_count++}]";
        }

        public CentralClient(string ip, string mac, string url)
        {
            _ip = ip;
            _mac = mac;
            _url = url;
            _connection = new HubConnection(url);
            //_connection.ConnectionSlow += () => Logger.Maintenance.Debug($"{GetPrefix()}[ConnectionSlow]");
            //_connection.Reconnecting += () => Logger.Maintenance.Debug($"{GetPrefix()}[Reconnecting]");
            //_connection.Reconnected += () => Logger.Maintenance.Debug($"{GetPrefix()}[Reconnected]");
            //_connection.Error += exception => Logger.Maintenance.Error($"{GetPrefix()}[Error]{exception.Message}");
            _connection.StateChanged += change =>
            {
                //Logger.Maintenance.Info($"{GetPrefix()}[StateChanged]{change.OldState}=>{change.NewState}");
                if (change.NewState != ConnectionState.Disconnected || _connecting)
                    return;
                var task = Connect();
            };
            _proxy = _connection.CreateHubProxy("DeviceHandlerHub");
            _proxy.On("NotifyConfigChanged", (string i, string m) =>
            {
                Logger.Maintenance.Info($"{GetPrefix()}[NotifyConfigChanged]");
                var task = Refresh();
                task.ContinueWith(taskResult =>
                {
                    if (!taskResult.IsCompleted) return;
                    var result = taskResult.Result;
                    if(result)
                        Logger.Maintenance.Info($"{GetPrefix()}[Refresh]Updated[{result.Value.Count}]");
                });
            });
        }

        public async void Start()
        {
            var task = Task.Run(async () => await Connect());
            _running = true;
            while (_running)
            {
                await Task.Delay(RefreshDelay);
                try
                {
                    if (_connection.State != ConnectionState.Connected || _connecting || _refreshing)
                    {
                        var result = await Refresh2();
                        if (result)
                            Logger.Maintenance.Info($"{GetPrefix()}[Refresh2]Updated[{result.Value.Count}]");
                    }
                    else
                    {
                        var result = await Refresh();
                        if(result)
                            Logger.Maintenance.Info($"{GetPrefix()}[Refresh]Updated[{result.Value.Count}]");
                    }
                }
                catch (Exception e)
                {
                    Logger.Maintenance.Error($"{GetPrefix()}[Start][Exception]{e}");
                }
            }
        }

        private bool _running = false;

        public void Stop()
        {
            _running = false;
        }

        public async Task<Result<Dictionary<string, string>>> Load()
        {
            var timeResult = await GetAlterTime2();
            if (!timeResult.IsSuccess)
            {
                if (timeResult.ResultCode != 404)
                    return timeResult.Convert().Convert<Dictionary<string, string>>();

                //未登记
                var configs = GetLocalConfigFunc?.Invoke() ?? new Dictionary<string, string>();
                var putConfigResult = await PutConfig2(configs);
                if (!putConfigResult.IsSuccess)
                    return putConfigResult.Convert<Dictionary<string, string>>();

                var data = new Dictionary<string, string>();
                return Result<Dictionary<string, string>>.Success(data);
            }
            else
            {
                var getConfigResult = await GetConfig2();
                if (!getConfigResult.IsSuccess)
                    return getConfigResult;

                var data = getConfigResult.Value;
                LastAlterTime = timeResult.Value;
                return Result<Dictionary<string, string>>.Success(data);
            }
        }

        #region WebApi

        private async Task<Result> Post(string path, object param)
        {
            using (var client = new HttpClient())
            {
                var postResult = await client.PostAsync($"{_url}api/{path}", new StringContent(param.ToJsonString(), Encoding.UTF8, "application/json"));
                if (!postResult.IsSuccessStatusCode)
                    return Result.Fail($"{path}:{postResult.ReasonPhrase}");
                var resultString = await postResult.Content.ReadAsStringAsync();
                var result = resultString.ToJsonObject<Result>();
                return result;
            }
        }

        private async Task<Result<T>> Post<T>(string path, object param)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent(param.ToJsonString(), Encoding.UTF8, "application/json");
                var postResult = await client.PostAsync($"{_url}api/{path}", content);
                if (!postResult.IsSuccessStatusCode)
                    return Result<T>.Fail($"{path}:{postResult.ReasonPhrase}");
                var resultString = await postResult.Content.ReadAsStringAsync();
                var result = resultString.ToJsonObject<Result<T>>();
                return result;
            }
        }

        private async Task<Result<DateTime>> GetAlterTime2()
        {
            var sw = Stopwatch.StartNew();
            var result = await Post<DateTime>("GetAlterTime", new { ip = _ip, mac = _mac });
            sw.Stop();
            Logger.Maintenance.Info($"{GetPrefix()}[GetAlterTime][{sw.ElapsedMilliseconds}ms]{result.IsSuccess}-{result.ResultCode}-{result.Message}");
            return result;
        }

        private async Task<Result<Dictionary<string, string>>> GetConfig2()
        {
            var sw = Stopwatch.StartNew();
            var result = await Post<Dictionary<string, string>>("GetConfig", new { ip = _ip, mac = _mac });
            Logger.Maintenance.Info($"{GetPrefix()}[GetConfig][{sw.ElapsedMilliseconds}ms]{result.IsSuccess}-{result.ResultCode}-{result.Message}");
            return result;
        }

        private async Task<Result> PutConfig2(Dictionary<string, string> configs)
        {
            var sw = Stopwatch.StartNew();
            var result = await Post("PutConfig", new { ip = _ip, mac = _mac, configs });
            Logger.Maintenance.Info($"{GetPrefix()}[PutConfig][{sw.ElapsedMilliseconds}ms]{result.IsSuccess}-{result.ResultCode}-{result.Message}");
            return result;
        }

        #endregion WebApi

        #region Hub

        private async Task<Result<DateTime>> GetAlterTime()
        {
            var sw = Stopwatch.StartNew();
            var result = await _proxy.Invoke<Result<DateTime>>("GetAlterTime", _ip, _mac);
            sw.Stop();
            Logger.Maintenance.Info($"{GetPrefix()}[GetAlterTime][{sw.ElapsedMilliseconds}ms]{result.IsSuccess}-{result.ResultCode}-{result.Message}");
            return result;
        }

        private async Task<Result<Dictionary<string, string>>> GetConfig()
        {
            var sw = Stopwatch.StartNew();
            var result = await _proxy.Invoke<Result<Dictionary<string, string>>>("GetConfig", _ip, _mac);
            Logger.Maintenance.Info($"{GetPrefix()}[GetConfig][{sw.ElapsedMilliseconds}ms]{result.IsSuccess}-{result.ResultCode}-{result.Message}");
            return result;
        }

        private async Task<Result> Register()
        {
            var sw = Stopwatch.StartNew();
            var result = await _proxy.Invoke<Result>("Register", _ip, _mac);
            Logger.Maintenance.Info($"{GetPrefix()}[Register][{sw.ElapsedMilliseconds}ms]{result.IsSuccess}-{result.ResultCode}-{result.Message}");
            return result;
        }

        //async Task<Result> PutConfig(Dictionary<string, string> configs)
        //{
        //    var sw = Stopwatch.StartNew();
        //    var result = await _proxy.Invoke<Result>("PutConfig", _ip, _mac, configs);
        //    Logger.Maintenance.Info($"{GetPrefix()}[PutConfig][{sw.ElapsedMilliseconds}ms]{result.IsSuccess}-{result.ResultCode}-{result.Message}");
        //    return result;
        //}

        #endregion Hub

        private async Task<Result<Dictionary<string, string>>> Refresh2(bool invoke = true)
        {
            if (_refreshing)
                return Result<Dictionary<string, string>>.Fail(string.Empty);
            _refreshing = true;
            try
            {
                var timeResult = await GetAlterTime2();
                if (!timeResult.IsSuccess)
                    return timeResult.Convert().Convert<Dictionary<string, string>>();
                if (timeResult.Value == LastAlterTime)
                    return Result<Dictionary<string, string>>.Fail(string.Empty);

                var configResult = await GetConfig2();
                if (!configResult.IsSuccess)
                    return configResult.Convert().Convert<Dictionary<string, string>>();

                var dic = configResult.Value.ToDictionary(c => c.Key, c => c.Value);
                if (invoke)
                    UpdateLocalConfigAction?.Invoke(dic);
                LastAlterTime = timeResult.Value;
                return Result<Dictionary<string, string>>.Success(dic);
            }
            finally
            {
                _refreshing = false;
            }
        }

        private async Task<Result<Dictionary<string, string>>> Refresh(bool invoke = true)
        {
            if (_refreshing)
                return Result<Dictionary<string, string>>.Fail(string.Empty);
            _refreshing = true;
            try
            {
                var timeResult = await GetAlterTime();
                if (!timeResult.IsSuccess)
                    return timeResult.Convert().Convert<Dictionary<string, string>>();
                if (timeResult.Value == LastAlterTime)
                    return Result<Dictionary<string, string>>.Fail(string.Empty);

                var configResult = await GetConfig();
                if (!configResult.IsSuccess)
                    return configResult.Convert().Convert<Dictionary<string, string>>();

                var dic = configResult.Value.ToDictionary(c => c.Key, c => c.Value);
                if (invoke)
                    UpdateLocalConfigAction?.Invoke(dic);
                LastAlterTime = timeResult.Value;
                return Result<Dictionary<string, string>>.Success(dic);
            }
            finally
            {
                _refreshing = false;
            }
        }

        private async Task<Result> Connect(bool retry = true)
        {
            if (_connecting || _connection.State == ConnectionState.Connected)
                return Result.Success();
            _connecting = true;
            try
            {
                while (true)
                {
                    try
                    {
                        await _connection.Start();
                        break;
                    }
                    catch (Exception e)
                    {
                        Logger.Maintenance.Debug($"{GetPrefix()}[Connect][Exception]{e.Message}");
                        if (!retry)
                            throw;
                    }
                    await Task.Delay(ReconnectDelay);
                }
                var regResult = await Register();
                //if (!regResult.IsSuccess && regResult.ResultCode == 404)
                //    return await PutConfig();
                return regResult;
            }
            finally
            {
                _connecting = false;
            }
        }

        //private async Task<Result> PutConfig()
        //{
        //    var configs = GetLocalConfigFunc?.Invoke() ?? new Dictionary<string, string>();

        //    var putResult = await PutConfig(configs);

        //    if (!putResult.IsSuccess)
        //        return putResult;

        //    var regResult = await Register();

        //    return regResult;
        //}
    }
}