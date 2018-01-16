using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Mvvm;
using Prism.Unity;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.UserControls.Transitions;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Infrastructure;
using YuanTu.Core.Navigating;

namespace Terminal
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override void ConfigureServiceLocator()
        {
            base.ConfigureServiceLocator();
            SystemStartup.Initialize(Container, "");
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();
            /*ViewModel的查找规则是(基于命名规则)：
             1.View中均需要设定自动查找ViewModel
             2.根据设定的医院程序集自动查找对应的ViewModel（规则是：相同程序路径）
             3.全局限定名称获取Type，如果该type不存在，则使用基础类中的对应ViewModel
             */
            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(viewType =>
                ServiceLocator.Current.GetInstance<NavigationEngine>()
                    .GetViewModelTypeByView(viewType)
            );
            ViewModelLocationProvider.SetDefaultViewModelFactory((obj, ty) =>
            {
                var objModel = ServiceLocator.Current.GetInstance<ViewModelBase>(ty.FullName);
                objModel.View = obj as ViewsBase;
                return objModel;
            });
        }

        protected override DependencyObject CreateShell()
        {
            return (DependencyObject)ServiceLocator.Current.GetInstance<IShell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            Application.Current.MainWindow = (Window)Shell;
            var shellVm = Container.Resolve<IShellViewModel>();
            (shellVm as ViewModelBase).View = (Window)Shell;
            Application.Current.MainWindow.DataContext = shellVm;
            shellVm.OnViewInit();
            Application.Current.MainWindow.Show();
        }

        public override void Run(bool runWithDefaultConfiguration)
        {
            //处理非UI线程异常
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            try
            {
                base.Run(runWithDefaultConfiguration);
            }
            catch (Exception ex)
            {
                var str = GetExceptionMsg(ex, ex.ToString());
                YuanTu.Core.Log.Logger.Main.Error("[系统错误0x000000001]" + str);
                MessageBox.Show(str, "系统错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(-1);
            }
        }

        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            RegionAdapterMappings regionAdapterMappings = ServiceLocator.Current.GetInstance<RegionAdapterMappings>();
            if (regionAdapterMappings != null)
            {
                regionAdapterMappings.RegisterMapping(typeof(Selector), ServiceLocator.Current.GetInstance<SelectorRegionAdapter>());
                regionAdapterMappings.RegisterMapping(typeof(ItemsControl), ServiceLocator.Current.GetInstance<ItemsControlRegionAdapter>());
                regionAdapterMappings.RegisterMapping(typeof(ContentControl), ServiceLocator.Current.GetInstance<ContentControlRegionAdapter>());
                regionAdapterMappings.RegisterMapping(typeof(TransitionPresenter), ServiceLocator.Current.GetInstance<TransitionPresenterRegionAdapter>());
            }

            return regionAdapterMappings;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var str = GetExceptionMsg(e.ExceptionObject as Exception, e.ToString());
            YuanTu.Core.Log.Logger.Main.Error("[系统错误0x000000002]" + str);
            MessageBox.Show(str, "系统错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(-1);
        }

        private static string GetExceptionMsg(Exception ex, string backStr)
        {
            var sb = new StringBuilder();
            sb.AppendLine("****************************异常文本****************************");
            sb.AppendLine("【出现时间】：" + DateTimeCore.Now);
            if (ex != null)
            {
                sb.AppendLine("【异常类型】：" + ex.GetType().Name);
                sb.AppendLine("【异常信息】：" + ex.Message);
                if (ex is ReflectionTypeLoadException)
                {
                    var le = ex as ReflectionTypeLoadException;
                    foreach (var exception in le.LoaderExceptions)
                    {
                        sb.AppendLine("【加载异常】：" + exception.Message);
                    }
                }
                else
                {
                    sb.AppendLine("【堆栈调用】：" + ex.StackTrace);
                    sb.AppendLine("【其他错误】：" + ex.Source);
                }
                var innexcpetion = ex.InnerException;
                var innIndex = 0;
                while (innexcpetion != null)
                {
                    sb.AppendLine($"【内部异常{innIndex++}】：" + innexcpetion.Source);
                    innexcpetion = innexcpetion.InnerException;
                }
            }
            else
            {
                sb.AppendLine("【未处理异常】：" + backStr);
            }
            sb.AppendLine("***************************************************************");
            return sb.ToString();
        }

       
    }

    public class TransitionPresenterRegionAdapter : Prism.Regions.RegionAdapterBase<TransitionPresenter>
    {
        public TransitionPresenterRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory) : base(regionBehaviorFactory)
        {
        }

        protected override void Adapt(IRegion region, TransitionPresenter regionTarget)
        {
            if (regionTarget == null)
                throw new ArgumentNullException(nameof(regionTarget));

            bool contentIsSet = regionTarget.Content != null;
            contentIsSet = contentIsSet || (BindingOperations.GetBinding(regionTarget, ContentControl.ContentProperty) != null);

            if (contentIsSet)
                throw new InvalidOperationException("ContentControlHasContentException");

            region.ActiveViews.CollectionChanged += (s, a) =>
            {
                regionTarget.Content = region.ActiveViews.FirstOrDefault();
            };

            region.Views.CollectionChanged += (s, a) =>
            {
                if (a.Action == NotifyCollectionChangedAction.Add && !region.ActiveViews.Any())
                {
                    region.Activate(a.NewItems[0]);
                }
            };
        }

        protected override IRegion CreateRegion()
        {
            return new SingleActiveRegion();
        }
    }
}
