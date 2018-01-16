using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.Core.Services.LocalResourceEngine
{
    public class ResourceEngine : IResourceEngine
    {
        public virtual string ServiceName => "默认资源引擎";

        private readonly string _resourceRoot;
        private readonly string _defaultResourceRoot;
        private Dictionary<string, string> _defaultResourceMapping;
        private Dictionary<string, string> _localResourceMapping;

        public ResourceEngine()
        {
            _resourceRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource");
            _defaultResourceRoot = Path.Combine(_resourceRoot, "Default");
        }

        public virtual string[] LocalResourceRoots => new[]
            {
                FrameworkConst.HospitalAssembly, 
                FrameworkConst.HospitalId // 不推荐
                // TODO 加入引用程序集的名称
            }
            .Where(s => !string.IsNullOrEmpty(s)) // 避免与 _resourceRoot 重复
            .Select(s => Path.Combine(_resourceRoot, s))
            .ToArray();

        public virtual string GetResourceFullPath(string name)
        {

            var items = GetNameGroup(name);

            //Logger.Main.Info($"[配置管理]获取资源:{string.Join(",",names)}");

            foreach (var item in items.Distinct())
            {
                if (item == null) continue;
                string resourceFullPath;
                if (_localResourceMapping.TryGetValue(item, out resourceFullPath))
                    return resourceFullPath;

                if (_defaultResourceMapping.TryGetValue(item, out resourceFullPath))
                {
                    _localResourceMapping[item] = resourceFullPath;
                    return resourceFullPath;
                }
            }
            return null;
        }

        public virtual List<string> GetResourceFullPathWithRegex(string namePattern,bool localOnly=true,Func<string,bool>judgeFunc=null)
        {
            var items = GetNameGroup(namePattern);
            var lst=new List<string>();
            judgeFunc= judgeFunc??(p => true);
            foreach (var item in items.Distinct())
            {
                if (item.IsNullOrWhiteSpace()) continue;
                var localRes = _localResourceMapping.Where(p => Regex.IsMatch(p.Key, item)&& judgeFunc(p.Key)).Select(p => p.Value);
                lst.AddRange(localRes);

                if (!localOnly)
                {
                    var defaultRes = _defaultResourceMapping.Where(p => Regex.IsMatch(p.Key, item) && judgeFunc(p.Key)).Select(p => p.Value);
                    lst.AddRange(defaultRes);

                }
            }
            return lst.Distinct().ToList();
        }

        protected List<string> GetNameGroup(string name)
        {
            SureInitialize();
            var items = new List<string> { $"{name}_{FrameworkConst.DeviceType}" };
            if (FrameworkConst.Strategies != null)
                items.AddRange(FrameworkConst.Strategies.Select(p => $"{name}_{p}"));
            items.Add(name);
            return items;
        }

        private bool _initialized;
        /// <summary>
        ///     确保资源已经从指定的文件夹中获取并初始化
        /// </summary>
        protected virtual void SureInitialize()
        {
            if (_initialized)
                return;
            _localResourceMapping = new Dictionary<string, string>();
            foreach (var localResourceRoot in LocalResourceRoots)
            {
                if (!Directory.Exists(localResourceRoot))
                    continue;
                foreach (var file in Directory.GetFiles(localResourceRoot, "*", SearchOption.AllDirectories))
                    _localResourceMapping[GetResourceKey(localResourceRoot, file)] = file;
            }

            _defaultResourceMapping = new Dictionary<string, string>();
            foreach (var file in Directory.GetFiles(_defaultResourceRoot, "*", SearchOption.AllDirectories))
                _defaultResourceMapping[GetResourceKey(_defaultResourceRoot, file)] = file;

            _initialized = true;

            //Logger.Main.Info($"[配置管理]所有资源：{_resourceMapping.ToJsonString()}");
        }

        /// <summary>
        /// 截取资源名称
        ///     ..\_[suffix]\**\[name].[ext]
        /// 转换为
        ///     [name]_[suffix]
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GetResourceKey(string rootPath, string path)
        {
            var suffix = Path.GetDirectoryName(path)
                ?.Substring(rootPath.Length)
                .Split('\\')
                .LastOrDefault(p => p.StartsWith("_"));
            return Path.GetFileNameWithoutExtension(path) + suffix;
        }
    }
}