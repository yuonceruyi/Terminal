using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.EventModels;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using System.Collections.Generic;
using System.Text;

namespace YuanTu.Core.FrameworkBase
{
    public abstract class ViewModelBase : BindableBase, INavigationAware, IConfirmNavigationRequest, IDependency
    {
        private static int _cmdSeed;
        private static int _cmdId;
        private bool _hasinit;
        public FrameworkElement View { get; set; }

        /// <summary>
        ///     是否禁用返回主页按钮(默认为否)
        /// </summary>
        public virtual bool DisableHomeButton
        {
            get { return _disableHomeButton; }
            set
            {
                _disableHomeButton = value;
                RaiseSystemInfoChangedEvent();
            }
        }

        /// <summary>
        ///     是否禁用返回上一页按钮(默认为否)
        /// </summary>
        public virtual bool DisablePreviewButton
        {
            get { return _disablePreviewButton; }
            set
            {
                _disablePreviewButton = value;
                RaiseSystemInfoChangedEvent();
            }
        }

        /// <summary>
        ///     发布系统级别变更信息（返回、主页按钮状态等）
        /// </summary>
        private void RaiseSystemInfoChangedEvent()
        {
            var eventAggregator = GetInstance<IEventAggregator>();
            eventAggregator.GetEvent<SystemInfoEvent>().Publish(new SystemInfoEvent
            {
                DisablePreviewButton = DisablePreviewButton,
                DisableHomeButton = DisableHomeButton
            });
        }

        protected static TService GetInstance<TService>(string name = "")
        {
            if (string.IsNullOrWhiteSpace(name))
                return ServiceLocator.Current.GetInstance<TService>();
            return ServiceLocator.Current.GetInstance<TService>(name);
        }

        [Dependency]
        public IResourceEngine ResourceEngine { get; set; }

        [Dependency]
        public NavigationEngine NavigationEngine { get; set; }

        public void PlaySound(string name)
        {
            GetInstance<IAudioPlayer>().StartPlayAsync(ResourceEngine.GetResourceFullPath(name));
        }

        public void Wait(bool busy)
        {
            var sm = GetInstance<IShellViewModel>();
            if (busy)
            {
                if (!sm.Busy.IsBusy)
                {
                    sm.Busy.IsBusy = true;
                    sm.Busy.BusyContent = "正在数据处理，请稍候...";
                }
            }
            else
            {
                sm.Busy.IsBusy = false;
            }
        }

        public void DoCommand(Action<LoadingProcesser> act, bool mutex = true)
        {
            DoCommand(lp =>
            {
                act(lp);
                return true;
            }, mutex, $"{act.Target}.{act.Method.Name}");
        }

        public Task<T> DoCommand<T>(Func<LoadingProcesser, T> act, bool mutex = true, string actName = null)
        {
            if (actName == null)
                actName = $"{act.Target}.{act.Method.Name}";
            var id = Interlocked.Increment(ref _cmdId);
            var prefix = $"[DoCommand][{id}][{actName}]";
            StopTimer();
            var sm = GetInstance<IShellViewModel>();
            if (mutex && sm.Busy.IsBusy)
            {
                Logger.Main.Info($"{prefix}Busy 被跳过");
                return Task.FromResult(default(T));
            }
            Logger.Main.Info($"{prefix}开始执行");
            var watch = Stopwatch.StartNew();
            sm.Busy.IsBusy = true;
            sm.Busy.BusyContent = "正在数据处理，请稍候...";
            var trace =new StackTrace(16).ToString(); /*Environment.StackTrace;*/
            return Task.Run(() =>
            {
                Interlocked.Increment(ref _cmdSeed);
                var process = GetInstance<LoadingProcesser>();
                return act.Invoke(process);
            }).ContinueWith(ctx =>
            {
                Interlocked.Decrement(ref _cmdSeed);
                if (_cmdSeed <= 0)
                    sm.Busy.IsBusy = false;

                watch.Stop();
                var total = watch.ElapsedMilliseconds;
                Logger.Main.Info($"{prefix}运行时长:{total}ms");
                StartTimer();
                if (!ctx.IsFaulted)
                    return ctx.Result;
                var msg = PrintAggregateException(ctx.Exception);
                Logger.Main.Error($"{prefix}发生异常:{msg}\r\n完整调用堆栈:{trace}");
                //throw new Exception("异步操作中存在未处理异常");
                ShowAlert(false, "异步操作异常", "异步操作过程中发生未知异常，请联系工作人员处理！");

                var defaultRep = default(T);
                try
                {
                    if (defaultRep is Result)
                    {
                        var tmp = ((Result) (object) defaultRep);
                        tmp.ResultCode = -100;
                        tmp.Message = "业务执行过程中发生未知异常";
                        return (T) (object) tmp;
                    }
                    else if (typeof(T).Name == "Result`1")
                    {
                        var subType = typeof(T).GetGenericArguments()[0];
                        var result = typeof(Result<>).MakeGenericType(subType);
                        var obj = Activator.CreateInstance(result);
                        result.GetProperty("ResultCode").SetValue(obj, -100);
                        result.GetProperty("Message").SetValue(obj, "业务执行过程中发生未知异常");
                        return (T) obj;
                    }
                }
                catch (Exception e)
                {
                    Logger.Main.Error($"{prefix}构建Result发生异常:{e.Message}\r\n完整调用堆栈:{e.StackTrace}");
                }

                return defaultRep;
            });
        }

        public static string PrintAggregateException(AggregateException ex)
        {
            var sb = new StringBuilder();
            foreach (var inner in ex.InnerExceptions)
            {
                sb.AppendLine($"{inner.Message}\n{inner.StackTrace}");
                var n = 1;
                var pointer = inner.InnerException;
                while (pointer != null)
                {
                    sb.AppendLine($"{"".PadLeft(n, '\t')}{pointer.Message}\n{"".PadLeft(n, '\t')}{pointer.StackTrace}");
                    pointer = pointer.InnerException;
                    n++;
                }
            }
            return sb.ToString();
        }
        #region NavigationBar

        public void ChangeNavigationContent(string address, string context, string content)
        {
            var model = GetInstance<INavigationModel>();
            var item = model.LocateNavigationItem(new FormContext(context, address));
            if (item != null)
                item.Content = content;
        }

        public void ChangeNavigationContent(string content)
        {
            var engine = NavigationEngine;
            ChangeNavigationContent(engine.State, engine.Context, content);
        }

        #endregion NavigationBar

        #region Show

        public void ShowAlert(bool success, string title, string content, int countdown = 0, string debugInfo = null, AlertExModel extend = null)
        {
            if (View == null) return;

            StartTimer();

            Logger.Main.Info($"[消息提示]状态:{(success ? "成功" : "失败")} 标题:{title} 内容:{content} 调试内容:{debugInfo}");
            BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() =>
           {
               if (success)
                   SystemSounds.Hand.Play();
               else
                   SystemSounds.Asterisk.Play();
               var sm = GetInstance<IShellViewModel>();
               var resource = ResourceEngine;
               var extr = debugInfo.IsNullOrWhiteSpace() ? "" : "\r\n错误详情:" + debugInfo;
               if (!sm.Alert.Display)
               {
                   sm.Alert.Display = true;
                   sm.Alert.CountDown = countdown <= 0 ? TimeOut : countdown;
                   sm.Alert.Content = content;
                   sm.Alert.ExtrContent = extr;
               }
               else
               {
                   sm.Alert.Content += "\r\n" + content;
                   sm.Alert.ExtrContent += extr;
               }
               sm.Alert.HideAction = extend?.HideCallback;
               sm.Alert.Image = resource.GetImageResource(success ? "提示_正确" : "提示_感叹号");
               sm.Alert.Title = title;
           }));
        }

        public void ShowMask(bool display, FrameworkElement element = null, double opacity = 0.4,
            Action<Point> pt = null)
        {
            StartTimer();
            var cmd = GetInstance<IShellViewModel>();

            BeginInvoke(DispatcherPriority.ContextIdle,
                (Action)(() =>
               {
                   cmd.Mask.Opacity = opacity;
                   cmd.Mask.ClickAction = pt;

                   cmd.Mask.IsVisiable = display;
                   cmd.Mask.Element = element;
               }));
        }

        public void ShowConfirm(string title, string content, Action<bool> callback,
            int countdown = 0, ConfirmExModel extend = null)
        {
            ShowConfirm(title, content, null, callback, countdown, extend);
        }

        public void ShowConfirm(string title, FrameworkElement element, Action<bool> callback, int countdown = 0,
            ConfirmExModel extend = null)
        {
            ShowConfirm(title, null, element, callback, countdown, extend);
        }

        public void ShowConfirm(string title, string content, FrameworkElement element, Action<bool> callback,
            int countdown = 0, ConfirmExModel extend = null)
        {
            StartTimer();
            if (View == null)
                return;
            Logger.Main.Info($"[消息弹窗]标题:{title} 内容:{content}");
            BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() =>
           {
               var sm = GetInstance<IShellViewModel>();
               if (!sm.Confirm.Display)
               {
                   sm.Confirm.Display = true;
                   sm.Confirm.CountDown = countdown <= 0 ? TimeOut : countdown;
               }
               sm.Confirm.Title = title;
               sm.Confirm.Content = content;
               sm.Confirm.ClickResult = callback;
               sm.Confirm.ConfirmEx = extend ?? ConfirmExModel.Build();
               sm.Confirm.MutiContent = element;
           }));
        }

        #endregion Show

        #region[Invke on UI Thread]

        public void Invoke(DispatcherPriority priority, Action action)
        {
            var shell = GetInstance<IShell>() as Window;
            shell?.Dispatcher.Invoke(priority, action);
        }

        public T Invoke<T>(Func<T> action, DispatcherPriority priority = DispatcherPriority.ContextIdle)
        {
            var shell = GetInstance<IShell>() as Window;
            if (shell != null)
            {
                return shell.Dispatcher.Invoke<T>(action, priority);
            }
            return default(T);
        }
        public void BeginInvoke(DispatcherPriority priority, Action action)
        {
            var shell = GetInstance<IShell>() as Window;
            shell?.Dispatcher.BeginInvoke(priority, action);
        }

        #endregion

        #region Virtual

        public abstract string Title { get; }

        /// <summary>
        ///     仅当在实例化试调用
        /// </summary>
        public virtual void OnSet()
        {
        }

        /// <summary>
        ///     进入当期页面时触发
        /// </summary>
        /// <param name="navigationContext"></param>
        public virtual void OnEntered(NavigationContext navigationContext)
        {
        }

        /// <summary>
        ///     离开当前页面时触发
        /// </summary>
        /// <param name="navigationContext"></param>
        /// <returns>是否允许跳转</returns>
        public virtual bool OnLeaving(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        ///     离开当前页面时触发，可提供异步返回
        /// </summary>
        /// <param name="navigationContext"></param>
        /// <returns></returns>
        public virtual Task<bool> OnLeavingAsync(NavigationContext navigationContext)
        {
            return null;
        }

        /// <summary>
        ///     当处于IsLocal状态下时，双击工作区会引发此事件
        /// </summary>
        public virtual void DoubleClick()
        {
        }

        /// <summary>
        ///     强制指定导航栏是否显示
        /// </summary>
        public bool HideNavigating
        {
            get
            {
                var shellViewModel = GetInstance<IShellViewModel>();
                return !shellViewModel.ShowNavigating;
            }
            set
            {
                var shellViewModel = GetInstance<IShellViewModel>();
                shellViewModel.ShowNavigating = !value;
            }
        }
        public virtual string CurrentStrategyType()
        {
            var stg = GetStrategy();
            if (stg.ContainsKey(FrameworkConst.DeviceType))
                return stg[FrameworkConst.DeviceType].First();
            return "";
        }
        public virtual Dictionary<string, string[]> GetStrategy()
        {
            return DeviceType.FallBackToDefaultStrategy;
        }
        #endregion Virtual

        #region Navigation

        void IConfirmNavigationRequest.ConfirmNavigationRequest(NavigationContext navigationContext,
            Action<bool> continuationCallback)
        {
            //var rest = OnLeaving(navigationContext);
            //continuationCallback?.Invoke(rest);

            var rest = OnLeaving(navigationContext);

            var tskRst = OnLeavingAsync(navigationContext);
            if (rest == false || tskRst == null)
                continuationCallback?.Invoke(rest);
            else
                tskRst.ContinueWith(
                    ascTest =>
                    {
                        if (ascTest.IsFaulted)
                        {
                            var msg = PrintAggregateException(ascTest.Exception);
                            Logger.Main.Error($"[异步离开页面]发生异常:{msg}");
                        }
                    BeginInvoke(DispatcherPriority.ContextIdle,
                            () => { continuationCallback?.Invoke(ascTest.Result); });
                    });
        }

        void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
            var engine = NavigationEngine;
            var shellViewModel = GetInstance<IShellViewModel>();
            shellViewModel.ShowNavigating = !engine.IsHome(engine.State);
            shellViewModel.TimeOutSeconds = TimeOut;
            //ViewChangingEvent
            GetInstance<IEventAggregator>()
                .GetEvent<ViewChangingEvent>()
                .Publish(new ViewChangingEvent
                {
                    FromContext = navigationContext.Parameters["FromContext"]?.ToString(),
                    From = navigationContext.Parameters["From"]?.ToString(),
                    ToContext = navigationContext.Parameters["ToContext"]?.ToString(),
                    To = navigationContext.Parameters["To"]?.ToString()
                });
            RaiseSystemInfoChangedEvent();
            if (!_hasinit)
            {
                if (!engine.IsHome(engine.State))
                    PropertyChanged += StartTimer;
                OnSet();
            }
            _hasinit = true;
            OnEntered(navigationContext);
        }

        bool INavigationAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            PropertyChanged -= StartTimer;
            return true;
        }

        void INavigationAware.OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        protected void Next()
        {
            NavigationEngine.Next();
        }

        protected bool Preview()
        {
            return NavigationEngine.Prev();
        }

        protected bool Switch(string context, string address)
        {
            return NavigationEngine.Switch(context, address);
        }

        protected bool Navigate(string address)
        {
            return NavigationEngine.Navigate(address);
        }

        protected bool StackNavigate(string context, string address)
        {
            return NavigationEngine.StackNavigate(context, address);
        }

        #endregion Navigation

        #region TimeOut

        private int _timeOut = FrameworkConst.DefaultTimeOut;
        private bool _disableHomeButton;
        private bool _disablePreviewButton;

        public int TimeOut
        {
            get { return _timeOut; }
            set
            {
                _timeOut = value;
                var sm = GetInstance<IShellViewModel>();
                sm.TimeOutSeconds = value;
            }
        }

        public void StartTimer()
        {
            var sm = GetInstance<IShellViewModel>();
            TimeOut = sm.TimeOutSeconds;
        }

        private void StartTimer(object sender, PropertyChangedEventArgs e)
        {
#if DEBUG
            Console.WriteLine("触发时间计时的属性:" + e.PropertyName);
#endif
            if (e.PropertyName == "FormCount")
                return;
            StartTimer();
        }

        public void StopTimer()
        {
            var sm = GetInstance<IShellViewModel>();
            sm.TimeOutStop = true;
        }

        #endregion TimeOut
    }
}