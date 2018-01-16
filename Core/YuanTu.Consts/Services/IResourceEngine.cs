using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Consts.Services
{
    public interface IResourceEngine : IService
    {
        /// <summary>
        ///     引擎的初始路径
        /// </summary>
        string[] LocalResourceRoots { get; }

        /// <summary>
        ///     获取资源完整路径
        /// </summary>
        /// <param name="name">资源名称</param>
        /// <returns></returns>
        string GetResourceFullPath(string name);

        /// <summary>
        ///   根据正则匹配所有路径
        /// </summary>
        /// <param name="namePattern"></param>
        /// <param name="localOnly"></param>
        /// <param name="judgeFunc"></param>
        /// <returns></returns>
        List<string> GetResourceFullPathWithRegex(string namePattern, bool localOnly = true,
            Func<string, bool> judgeFunc = null);
    }

    public static class ResourceEngineExtension
    {
        public static Uri GetImageResourceUri(this IResourceEngine engine, string name)
        {
            var file = engine.GetResourceFullPath(name);
            return string.IsNullOrWhiteSpace(file) ? null : new Uri(file, UriKind.Absolute);
        }

        public static BitmapImage GetImageResource(this IResourceEngine engine, string name)
        {
            var file = engine.GetResourceFullPath(name);
            return string.IsNullOrWhiteSpace(file) ? null : new BitmapImage(new Uri(file, UriKind.Absolute));
        }
    }
}