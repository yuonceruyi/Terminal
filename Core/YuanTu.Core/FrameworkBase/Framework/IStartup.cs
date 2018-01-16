using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using YuanTu.Core.Navigating;

namespace YuanTu.Core.FrameworkBase
{
    /// <summary>
    ///     系统初始化时会调用该接口，注意：当存在多个实现时，顺序按照Order从小到大排序
    /// </summary>
    public interface IStartup
    {
        /// <summary>
        ///     优先级，数值越小优先级越高，内部配置越优先被使用
        /// </summary>
        int Order { get; }

        /// <summary>
        ///     向主程序提供配置信息，优先采用插件内部配置
        /// </summary>
        /// <returns>返回配置文件完整路径(支持xml,json,ini)</returns>
        string[] UseConfigPath();

        /// <summary>
        ///     注册视图引擎
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        bool RegisterTypes(ViewCollection collection);

        /// <summary>
        ///     此处可获取专属的配置信息
        /// </summary>
        /// <param name="root"></param>
        void InitConfig(IConfiguration root);

        /// <summary>
        ///     程序初始化完成后，调用该方法，这里可以初始化插件内部功能
        /// </summary>
        void AfterStartup();

        /// <summary>
        ///     指定设备型号对应的ViewModel查找策略
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string[]> GetStrategy();

        /// <summary>
        ///     指定需要额外加载的资源地址
        /// </summary>
        /// <returns></returns>
        List<Uri> GetResourceDictionaryUris();
    }
}