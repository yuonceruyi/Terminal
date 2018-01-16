using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace YuanTu.Default.Component.Advertisement.ViewModels
{
    public class CacheService
    {
        private readonly Dictionary<string, CacheItem> _cacheItems = new Dictionary<string, CacheItem>();
        private readonly string _defaultUrl;
        private readonly string _root;
        private readonly bool _checkHead;
        private static readonly HttpClient HttpClient = new HttpClient();

        public CacheService(string root, string defaultUrl, bool checkHead = false)
        {
            _root = root;
            _defaultUrl = defaultUrl;
            _checkHead = checkHead;
        }

        public void Request(params string[] urls)
        {
            Request(urls as IEnumerable<string>);
        }

        public void Request(IEnumerable<string> urls)
        {
            foreach (var url in urls)
            {
                if (_cacheItems.TryGetValue(url, out var item))
                {
                    if (!item.Ready && !item.Started)
                        StartGetItem(item);
                    continue;
                }
                var fileName = Path.GetFileName(url);
                var fullPath = Path.Combine(_root, fileName);
                if (!File.Exists(fullPath) || _checkHead)
                {
                    var newItem = new CacheItem
                    {
                        LastAccess = DateTime.Now,
                        Url = url,
                        FileName = fileName
                    };
                    _cacheItems[url] = newItem;
                    StartGetItem(newItem);
                }
                else
                {
                    _cacheItems[url] = new CacheItem
                    {
                        Ready = true,
                        LastAccess = DateTime.Now,
                        Url = url,
                        FileName = fileName
                    };
                }
            }
        }

        private void StartGetItem(CacheItem item)
        {
            Task.Run(() => GetItem(item));
        }

        private async void GetItem(CacheItem item)
        {
            try
            {
                item.Started = true;
                var fullPath = GetFullPath(item);

                if (_checkHead && File.Exists(fullPath))
                {
                    //var res = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, item.Url));
                    //var lengthString = res.Headers.GetValues("ContentLength").FirstOrDefault();
                    //if (!string.IsNullOrEmpty(lengthString))
                    //    if (long.TryParse(lengthString, out var length))
                    //        if (new FileInfo(fullPath).Length == length)
                    //        {
                    //            item.Ready = true;
                    //            return;
                    //        }

                    File.Delete(fullPath);
                }

                var stream = await HttpClient.GetStreamAsync(item.Url);
                var tempPath = Path.GetTempFileName();
                using (var fs = File.OpenWrite(tempPath))
                {
                    stream.CopyTo(fs);
                }

                File.Move(tempPath, fullPath);
                item.Ready = true;
            }
            finally
            {
                item.Started = false;
            }
        }

        string GetFullPath(CacheItem item)
        {
            return Path.Combine(_root, item.FileName);
        }

        public string Get(string url, out bool ready)
        {
            if (!_cacheItems.ContainsKey(url))
                Request(url);
            var item = _cacheItems[url];
            item.LastAccess = DateTime.Now;
            ready = item.Ready;
            return GetFullPath(item);
        }

        private class CacheItem
        {
            public bool Started { get; set; }
            public bool Ready { get; set; }

            public DateTime LastAccess { get; set; }
            public string Url { get; set; }
            public string FileName { get; set; }
        }
    }
}