using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.Core.Models;
using YuanTu.Core.Navigating;

namespace YuanTu.Core.Infrastructure
{
    internal class IocInitializer
    {
        private readonly AssemblyInfo[] _assemblies;
        private readonly IUnityContainer _container;
        private readonly ScopeManager _scopeManager;
        private readonly IStartup[] _startups;
        private string[] _strategy;

        public IocInitializer(IUnityContainer container, ScopeManager scopeManager, AssemblyInfo[] assemblies,
            IStartup[] startups)
        {
            _container = container;
            _scopeManager = scopeManager;
            _assemblies = assemblies;
            _startups = startups;
        }

        /// <summary>
        /// IOC完整注册
        /// </summary>
        public void Do()
        {
            //确定ViewModel查找策略
            var strategies = _startups.First().GetStrategy();
            if (strategies.ContainsKey(FrameworkConst.DeviceType))
            {
                _strategy = strategies[FrameworkConst.DeviceType];
            }
            else
            {
                _strategy = new[] { DeviceType.Default };
                Logger.Main.Info($"[系统初始化] 未找到设备型号【{FrameworkConst.DeviceType}】对应的ViewModel查找策略 使用默认");
            }
            FrameworkConst.Strategies = _strategy;
            //过滤
            foreach (var assemblyInfo in _assemblies)
                assemblyInfo.Types = assemblyInfo.Types
                    .Where(p => typeof(IDependency).IsAssignableFrom(p))
                    .ToArray();

            //初始化视图切换引擎
            InitNavigationEngine();

            //注册IShell
            InitGlobal(typeof(IShell));

            //注册IShellViewModel
            InitGlobal(typeof(IShellViewModel));

            //注册ITopBottomModel
            InitGlobal(typeof(ITopBottomModel));

            //注册ViewModels
            InitViewModels();

            var depends = _assemblies
                .SelectMany(a => a.Types)
                .ToArray();

            //IModel
            InitModels(depends);

            //初始化IService，注意IService将会被初始化为全局单例
            InitServices(depends);

            //注册Loading弹窗
            InitLoading();
        }

        #region[IOC初始化]

        private void InitNavigationEngine()
        {
            var engine = new NavigationEngine();
            _container.RegisterInstance(typeof(NavigationEngine), engine, new ContainerControlledLifetimeManager());
            foreach (var startup in _startups)
                if (startup.RegisterTypes(engine.Children))
                {
                    Logger.Main.Info($"[系统初始化] 调用[{startup.GetType().FullName}]进行RegisterTypes初始化");
                    return;
                }
            throw new Exception("未找到合适的启动器");
        }

        /// <summary>
        /// 注册<paramref name="type"/>的最优先实现的全局单例
        /// </summary>
        /// <param name="type"></param>
        private void InitGlobal(Type type)
        {
            var typeName = type.Name;
            var candidates = _assemblies
                .Select(a => a.Types
                    .Where(t => t.IsSolidTypeOf(type))
                    .Select(TypeInfo.Get)
                    .ToArray()
                )
                .Where(a => a.Any())
                .Reverse()
                .ToArray();
            if (!candidates.Any())
                throw new Exception($"{typeName}-查找失败");
            var winner = Decide(typeName, candidates);
            _container.RegisterType(type, winner.Type, new ContainerControlledLifetimeManager());
            Logger.Main.Info($"[系统初始化] 注册{typeName}:[{winner.Type.FullName}]");
        }

        private void InitLoading()
        {
            var mainVm = ServiceLocator.Current.GetInstance<IShellViewModel>();
            var loading = new LoadingProcesser(mainVm);
            _container.RegisterInstance(typeof(LoadingProcesser), loading, new ContainerControlledLifetimeManager());
        }

        private void InitViewModels()
        {
            var views = GetViews();

            var viewModels = GetViewModels();

            var dic = new Dictionary<Type, Type>();

            foreach (var view in views)
            {
                var viewModel = Decide(view, viewModels);                
                _container.RegisterType(typeof(ViewModelBase), viewModel.Type, viewModel.Type.FullName, new NormalLifetimeManager(_scopeManager));
                dic[view.Type] = viewModel.Type;
            }

            ServiceLocator.Current.GetInstance<NavigationEngine>().LookUpDictionary = dic;

            Logger.Main.Info($"[系统初始化] 注册ViewModel，共计:{dic.Values.Count}个");
        }

        /// <summary>
        /// 按Assembly顺序 按Streategy策略 取最优先实现
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        private TypeInfo Decide(string typeName, TypeInfo[][] types)
        {
            foreach (var candidate in types)
            {
                foreach (var s in _strategy)
                {
                    var winners = candidate.Where(t => t.Device == s).ToArray();
                    if (!winners.Any())
                        continue;
                    if (winners.Length > 1)
                        throw new Exception($"找到多于一个同等优先级的[{typeName}]的实现");
                    return winners.First();
                }
            }
            throw new Exception($"未找到[{typeName}]的实现");
        }

        /// <summary>
        /// 根据View 按Assembly顺序 按Streategy策略 取最优先ViewModel实现
        /// </summary>
        /// <param name="view"></param>
        /// <param name="viewModels"></param>
        /// <returns></returns>
        private TypeInfo Decide(TypeInfo view, TypeInfo[][] viewModels)
        {
            var name = view.Name;
            var candidates = viewModels
                .Select(c => c.Where(t => t.Name == name).ToArray())
                .Where(c => c.Any())
                .Reverse()
                .ToArray();
            foreach (var candidate in candidates)
            {
                foreach (var s in _strategy)
                {
                    var winners = candidate.Where(t => t.Device == s).ToArray();
                    if (!winners.Any())
                        continue;
                    if (winners.Length > 1)
                        throw new Exception($"找到多于一个同等优先级的名为[{name}]的ViewModel");
                    return winners.First();
                }
            }
            throw new Exception($"未找到名为[{name}]的ViewModel");
        }

        private void InitModels(Type[] depends)
        {
            /* 注册Model规则：
             * 1.获取所有继承IModel的接口(modelInterfaces)
             * 2.按照优线Assembly原则，获取每个接口的最优实现，注册到Ioc中
             * 3.如果插件中未有实现的，选择namespace路径最长者为当前可用Model
             */
            var modelInterfaces =
                depends.Where(p => typeof(IModel).IsAssignableFrom(p) && p.IsInterface && p != typeof(IModel))
                    .ToArray();
            foreach (var modelInterface in modelInterfaces)
            {
                var sub =
                    depends.Where(p => modelInterface.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract).ToArray();
                var first = sub.FirstOrDefault(p => p.Assembly.GetName().Name == FrameworkConst.HospitalAssembly) ??
                            sub.OrderByDescending(p => p.FullName.Length).FirstOrDefault();
                if (first != null)
                    _container.RegisterType(modelInterface, first, new NormalLifetimeManager(_scopeManager));
            }
            Logger.Main.Info($"[系统初始化] 注册Model，共计:{modelInterfaces.Length}个");
        }

        private void InitServices(Type[] depends)
        {
            var serviceInterfaces =
                depends.Where(p => typeof(IService).IsAssignableFrom(p) && p.IsInterface && p != typeof(IService))
                    .ToArray();
            foreach (var serviceInterface in serviceInterfaces)
            {
                var sub =
                    depends.Where(p => serviceInterface.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
                        .ToArray();
                var first = sub.FirstOrDefault(p => p.Assembly.GetName().Name == FrameworkConst.HospitalAssembly) ??
                            sub.OrderByDescending(p => p.FullName.Length).FirstOrDefault();
                if (first != null)
                    _container.RegisterType(serviceInterface, first, new NormalLifetimeManager(_scopeManager));
            }
            Logger.Main.Info($"[系统初始化] 注册Service，共计:{serviceInterfaces.Length}个");
        }

        #endregion

        #region [ViewViewModel]

        private static readonly string Dot = ".";
        private static readonly string View = "View";
        private static readonly string Views = ".Views.";
        private static readonly string ViewModel = "ViewModel";
        private static readonly string ViewModels = ".ViewModels.";

        private TypeInfo[][] GetViewModels()
        {
            return _assemblies
                .Select(a => a.Types
                    .Where(t => t.IsSolidTypeOf(typeof(ViewModelBase)))
                    .Select(TypeInfo.GetViewModel)
                    .ToArray())
                .Where(a => a.Any())
                .ToArray();
        }

        private TypeInfo[] GetViews()
        {
            var views = _assemblies
                .SelectMany(a => a.Types
                    .Where(t => t.IsSolidTypeOf(typeof(ViewsBase)))
                    .Select(TypeInfo.GetView))
                    .Where(p=>_strategy.Contains(p.Device))
                .ToArray();
            return views;
            //IEnumerable<TypeInfo> winners = new List<TypeInfo>();
            //foreach (var s in _strategy)
            //{
            //    winners = winners.Concat(views.Where(t => t.Device == s)).ToList();
            //}
            //return winners.ToArray();
        }

        private class TypeInfo
        {
            public string Name { get; set; }
            public string Device { get; set; }
            public Type Type { get; set; }

            public override string ToString()
            {
                return $"{Name} @{Device} [{Type.FullName}]";
            }

            public static TypeInfo Get(Type type)
            {
                var fullName = type.FullName;
                var name = fullName.Substring(type.Assembly.GetName().Name.Length);
                for (var i = 0; i < DeviceType.DeviceTypes.Length; i++)
                {
                    var t = DeviceType.DeviceTypeStrings[i];
                    name = name.Replace(t, Dot);
                }

                var device = DeviceType.GetDeviceFromTypeFullName(fullName);

                return new TypeInfo
                {
                    Name = name.Trim('.'),
                    Type = type,
                    Device = device
                };
            }

            public static TypeInfo GetView(Type type)
            {
                var info = Get(type);

                // [ .Views. ] => [ . ]
                var name = info.Name.Replace(Views, Dot);
                // [ -View ] => [ - ]
                if (name.EndsWith(View))
                    name = name.Substring(0, name.Length - View.Length);

                info.Name = name;
                return info;
            }

            public static TypeInfo GetViewModel(Type type)
            {
                var info = Get(type);

                // [ .ViewModels. ] => [ . ]
                var name = info.Name.Replace(ViewModels, Dot);
                // [ -ViewModel ] => [ - ]
                if (name.EndsWith(ViewModel))
                    name = name.Substring(0, name.Length - ViewModel.Length);

                info.Name = name;
                return info;
            }
        }

        #endregion
    }
}