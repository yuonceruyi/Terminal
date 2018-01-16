using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Core.AutoUpdate;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Gateway;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Core.Reporter.Kestrel;
using YuanTu.Core.Systems;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using YuanTu.Consts.Services;

namespace YuanTu.Core.Infrastructure
{
    public static class SystemStartup
    {
        public static IConfigurationRoot Configuration { get; private set; }

        public static ScopeManager ScopeManager { get; } = new ScopeManager();

        /// <summary>
        ///     该方法主要工作:
        ///     1.基于“约定优先”读取配置文件
        ///     2.初始化基于HospitalId的所有项目
        /// </summary>
        public static void Initialize(IUnityContainer container, string assemblyName)
        {
            try
            {
                if (assemblyName.IsNullOrWhiteSpace())
                {
                    var cfg = LoadConfig(null);
                    assemblyName = cfg["Assembly"];
                }
                DataHandlerEx.Handler = new DataHandler();
                YuanTu.Consts.UserCenter.DataHandlerEx.Handler = new YuanTu.Core.UserCenter.DataHandler();
                YuanTu.Consts.Triage.DataHandlerEx.Handler = new YuanTu.Core.Triage.DataHandler();
                FrameworkConst.HospitalAssembly = assemblyName;
                FrameworkConst.RootDirectory = Environment.CurrentDirectory;
                var assembly = Assembly.GetEntryAssembly()?.GetName();
                Logger.Main.Info($"[系统初始化] 加载功能插件：【{assemblyName}】主项目版本号:{assembly?.Version}");

                var stopwatch = Stopwatch.StartNew();

                var assemblies = GetSortedAssembly(assemblyName);
                var startups = GetStartUps(assemblies);

                stopwatch.Stop();
                Logger.Main.Info($"[系统初始化] 加载功能插件共消耗:{stopwatch.ElapsedMilliseconds}毫秒");
                stopwatch.Restart();
                Configuration = LoadConfig(startups);
                stopwatch.Stop();
                Logger.Main.Info($"[系统初始化] 初始化配置文件共消耗:{stopwatch.ElapsedMilliseconds}毫秒");

                stopwatch.Restart();
                var initializer = new IocInitializer(container, ScopeManager, assemblies, startups);
                initializer.Do();
                stopwatch.Stop();
                Logger.Main.Info($"[系统初始化] Ioc初始化共消耗:{stopwatch.ElapsedMilliseconds}毫秒");
                stopwatch.Restart();
                ModuleInitialize(startups);
                stopwatch.Stop();
                Logger.Main.Info($"[系统初始化] 模块内部初始化共消耗:{stopwatch.ElapsedMilliseconds}毫秒");

                // 单元测试环境以下代码略过
                if (FrameworkConst.UnitTest)
                    return;

                stopwatch.Restart();
                LoadResourceDictionaries(startups.First());
                stopwatch.Stop();
                Logger.Main.Info($"[系统初始化] 加载资源字典共消耗:{stopwatch.ElapsedMilliseconds}毫秒");

                ReportService.SignInAsync();
                AutoUpdateMonitor();
                LocalHttpServer();
                GlobalTimer();
            }
            catch (Exception ex)
            {
                ReportService.软件初始化异常(ex.Message, ErrorSolution.软件初始化异常);
                throw;
            }
        }

        private static IStartup[] GetStartUps(AssemblyInfo[] assemblies)
        {
            return assemblies
                .SelectMany(a => a.Types
                    .Where(p => p.IsSolidTypeOf(typeof(IStartup)))
                    .Select(p => (IStartup)Activator.CreateInstance(p))
                )
                .Reverse()
                .ToArray();
        }

        public static bool IsSolidTypeOf(this Type type, Type baseType)
        {
            return baseType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface;
        }

        #region [时间触发器]

        static void GlobalTimer()
        {
            var eventAggrator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            Task.Run(async () =>
            {
                var pubSubEvent = eventAggrator.GetEvent<PubSubEvent<DateTime>>();
                while (true)
                {
                    await Task.Delay(1000);
                    pubSubEvent.Publish(DateTimeCore.Now);
                }
            });
        }

        #endregion

        #region[本地Http服务]

        private static void LocalHttpServer()
        {
            var server = new Server();
            Task.Run(() => { server.Start(); });
            var current = Application.Current;
            if (current != null)
                current.Exit += (s, a) => { server.Dispose(); };
        }

        #endregion

        #region[自动更新监控打开]

        private static void AutoUpdateMonitor()
        {
            var host = ServiceLocator.Current.GetInstance<IAutoUpdateHost>();
            host.Start();
        }

        #endregion

        #region[加载资源字典]

        private static void LoadResourceDictionaries(IStartup startup)
        {
            try
            {
                var dics = Application.Current.Resources.MergedDictionaries;
                dics.Clear();

                //不知道为什么 加载2遍就不会出问题
                dics.Add(new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/YuanTu.Default.Theme;component/default.xaml"),
                });
                dics.Add(new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/YuanTu.Default.Theme;component/default.xaml"),
                });

                var extras = startup.GetResourceDictionaryUris();
                if (extras == null)
                    return;
                foreach (var extra in extras)
                    dics.Add(new ResourceDictionary
                    {
                        Source = extra
                    });
            }
            catch (Exception e)
            {
                //
            }
        }

        #endregion

        #region[插件内部初始化]

        private static void ModuleInitialize(IStartup[] startups)
        {
            foreach (var startup in startups.Reverse()) //反转
                startup.AfterStartup();
        }

        #endregion

        #region[读取配置文件]

        private static readonly string _mainConfig = "Config.xml";

        private static IConfigurationRoot LoadConfig(IStartup[] startups)
        {
            if (!File.Exists(_mainConfig))
            {
                MessageBox.Show($"没有找到主配置文件[{_mainConfig}]，程序终止！");
                Environment.FailFast("自助机启动时没有找到主配置文件，程序终止！");
                //throw new FileNotFoundException("没有找到主配置文件，程序终止！");
            }
            var builder = new ConfigurationBuilder();
            Logger.Main.Info($"[系统初始化] 加载主配置文件：【{_mainConfig}】");
            builder.AddXmlFile(_mainConfig);
            if (startups != null)
            {
                var tmp = startups.Reverse(); //反转
                foreach (var startup in tmp)
                {
                    var doc = startup.UseConfigPath();
                    if (doc == null || !doc.Any())
                        continue;
                    foreach (var eachPath in doc)
                    {
                        Logger.Main.Info($"[系统初始化] 加载功能配置文件：【{eachPath}】");
                        var extension = Path.GetExtension(eachPath)?.ToLower();
                        if (extension.IsNullOrWhiteSpace())
                            continue;
                        switch (extension)
                        {
                            case ".xml":
                            case ".config":
                                builder.AddXmlFile(eachPath, true);
                                break;

                            case ".json":
                                builder.AddJsonFile(eachPath, true);
                                break;

                            case ".ini":
                                builder.AddIniFile(eachPath, true);
                                break;
                        }
                    }
                }
            }
#if DEBUG
            var cmd = Environment.GetCommandLineArgs().Where(p => p.Contains("=")).ToArray();
            builder.AddCommandLine(cmd);
#endif

            var root = builder.Build();

            // 取assemblyName时会多调用一次
            if (startups != null && root["UseCentral"] == "1")
            {
                var ip = root["IP"];
                var mac = root["MAC"];
                var url = root["CentralUrl"];
                Logger.Main.Info($"[系统初始化] 加载功能配置文件：【{url}】");
                builder.AddCentral(
                    string.IsNullOrEmpty(ip) ? NetworkManager.IP : ip,
                    string.IsNullOrEmpty(mac) ? NetworkManager.MAC : mac,
                    url,
                    () => DumpConfigurationRoot(root));
                root = builder.Build();
                RegisterChangeCallback(root, () =>
                {
                    InitializeConfigData(startups, root);
                });
            }

            InitializeConfigData(startups, root);
            return root;
        }
        private static void RegisterChangeCallback(IConfigurationRoot root, Action changeAction)
        {
            var reloadToken = root.GetReloadToken();
            reloadToken.RegisterChangeCallback(s =>
            {
                RegisterChangeCallback(root, changeAction);
                changeAction();
            }, null);
        }

        static Dictionary<string, string> DumpConfigurationRoot(IConfigurationRoot root)
        {
            var dic = new Dictionary<string, string>();
            var children = root.GetChildren();
            if (children != null)
                foreach (var child in children)
                    DumpConfigurationSection(child, dic);
            return dic;
        }
        static void DumpConfigurationSection(IConfigurationSection section, Dictionary<string, string> dic)
        {
            dic[section.Path] = section.Value;
            var children = section.GetChildren();
            if (children == null)
                return;
            foreach (var child in children)
                DumpConfigurationSection(child, dic);
        }

        private static void InitializeConfigData(IStartup[] startups, IConfiguration root)
        {
            FrameworkConst.HospitalId = root["HospitalId"];
            FrameworkConst.OperatorId = root["OperatorId"];
            FrameworkConst.OperatorName = root["OperatorName"];
            FrameworkConst.NotificMessage = root["NotificMessage"] ?? "客服:0571-89916777";
            FrameworkConst.HospitalAreaCode = root["HospitalAreaCode"];
            FrameworkConst.GatewayUrl = root["GatewayUrl"];
            FrameworkConst.DatabasePath = root["DatabasePath"];
            FrameworkConst.DeviceType = root["DeviceType"];
            FrameworkConst.AudioGuideEnabled = root["AudioGuideEnabled"] == "1";
            FrameworkConst.AnimationEnabled = root["AnimationEnabled"] == "1";
            FrameworkConst.DefaultTimeOut = int.Parse(root["TimeOut"] ?? "90");

            FrameworkConst.AutoShutdownEnable = root["AutoShutdown:Enabled"] == "1";
            FrameworkConst.AutoUpdateEnable = root["AutoUpdate"] == "1";
            FrameworkConst.MonitorUrl = root["MonitorUrl"] ?? FrameworkConst.GatewayUrl;
            FrameworkConst.UnionId = root["UnionId"];
            FrameworkConst.UserCenterUrl = root["UserCenterUrl"];
            FrameworkConst.TriageUrl = root["TriageUrl"];
            FrameworkConst.DeviceUrl = root["DeviceUrl"];
            FrameworkConst.FundCustodian = root["FundCustodian"];

            if (FrameworkConst.AutoShutdownEnable)
            {
                var time = root["AutoShutdown:Time"];
                if (DateTime.TryParseExact(time, "HH:mm", null, DateTimeStyles.None, out var autoShutdownTime))
                {
                    // 若当前已过关机时间点 加一天
                    FrameworkConst.AutoShutdownTime = DateTimeCore.Now > autoShutdownTime 
                        ? autoShutdownTime.AddDays(1) 
                        : autoShutdownTime;
                }
            }

#if DEBUG
            //LOCAL=GATEWAY|DOUBLECLICK
            FrameworkConst.LocalArgs = Environment.GetCommandLineArgs().FirstOrDefault(one => one.StartsWith("LOCAL"));
            FrameworkConst.MultiCastIP = root["MultiCastIP"];
            FrameworkConst.MultiCastPort = root["MultiCastPort"];
#endif
        }

        #endregion

        #region[相关引用程序集加载]

        private const string Prefix = "YuanTu.";
        private const string Entry = "Terminal";

        private static AssemblyInfo[] GetSortedAssembly(string assemblyName)
        {
            //强制加载
            Assembly.Load("YuanTu.Default");
            Assembly.Load("YuanTu.Devices");

            //加载依赖
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var loadedAssemblies = new List<Assembly>(assemblies);
            foreach (var assembly in assemblies)
                LoadReferences(loadedAssemblies, assembly);
            if (!string.IsNullOrWhiteSpace(assemblyName))
                LoadReferences(loadedAssemblies, Assembly.Load(assemblyName));

            //构造节点
            var nodes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(FilterAssembly)
                .Select(a => new AssemblyNode
                {
                    Assembly = a
                })
                .ToList();

            //获取各节点依赖
            foreach (var node in nodes)
                node.Depends = node.Assembly.GetReferencedAssemblies()
                    .Where(FilterAssembly)
                    .Select(an => nodes.Find(a => a.Assembly.FullName == an.FullName))
                    .ToList();

            //根据依赖顺序排序 被依赖优先
            var sortedAssemblies = new List<Assembly>();
            while (true)
            {
                var node = nodes.FirstOrDefault(n => !n.Depends.Any());
                if (node == null)
                    break;
                sortedAssemblies.Add(node.Assembly);
                foreach (var n in nodes)
                    n.Depends.Remove(node);
                nodes.Remove(node);
            }
            return sortedAssemblies
                .Select(a => new AssemblyInfo
                {
                    Assembly = a,
                    Types = a.GetTypes()
                })
                .ToArray();
        }

        private static void LoadReferences(List<Assembly> loadedAssemblies, Assembly assembly)
        {
            var toLoad = assembly.GetReferencedAssemblies()
                .Where(FilterAssembly)
                .Where(an => loadedAssemblies.TrueForAll(a => a.FullName != an.FullName));
            var newlyLoaded = toLoad.Select(an => AppDomain.CurrentDomain.Load(an)).ToList();
            loadedAssemblies.AddRange(newlyLoaded);
            foreach (var a in newlyLoaded)
                LoadReferences(loadedAssemblies, a);
        }

        private static bool FilterAssembly(Assembly a)
        {
            return a.FullName.Contains(Prefix) || a.FullName.Contains(Entry);
        }

        private static bool FilterAssembly(AssemblyName a)
        {
            return a.FullName.Contains(Prefix) || a.FullName.Contains(Entry);
        }

        private class AssemblyNode
        {
            public Assembly Assembly { get; set; }

            public List<AssemblyNode> Depends { get; set; }
        }

        #endregion
    }
}