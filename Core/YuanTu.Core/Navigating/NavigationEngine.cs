using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.EventModels;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Infrastructure;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;

namespace YuanTu.Core.Navigating
{
    public class FormContext : IEquatable<FormContext>
    {
        public FormContext(string context, string address)
        {
            Context = context;
            Address = address;
        }

        public string Address { get; }

        public string Context { get; }

        public bool Equals(FormContext other)
        {
            if (other == null)
                return false;
            return Context == other.Context && Address == other.Address;
        }

        public override bool Equals(object obj)
        {
            var other = obj as FormContext;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return $"{Context}-{Address}".GetHashCode();
        }

        public override string ToString()
        {
            return $"{Context}-{Address}";
        }
    }

    public class FormContextEx : FormContext
    {
        public FormContextEx(string context, string address, string title, string icon) : base(context, address)
        {
            Title = title;
            Icon = icon;
        }

        public string Title { get; }
        public string Icon { get; }
    }

    public class NavigationEngine
    {
        public NavigationEngine()
        {
            Children = new ViewCollection(this);
        }

        public ViewCollection Children { get; }

        public Dictionary<Type, Type> LookUpDictionary { get; set; }

        #region [流程引导]

        public Dictionary<string, List<FormContextEx>> ContextDictionary { get; } =
            new Dictionary<string, List<FormContextEx>>();

        #endregion

        #region[View和ViewModel]

        public Type GetViewModelTypeByView(Type view)
        {
            Type viewModel;
            if (LookUpDictionary.TryGetValue(view, out viewModel))
                return viewModel;
            if (Debugger.IsAttached)
                Debugger.Break();
            MessageBox.Show("VM-Lost:" + view.FullName);
            throw new Exception();
        }

        #endregion

        #region Navigate

        public Dictionary<FormContext, string> NextDictionary { get; } = new Dictionary<FormContext, string>();

        private Dictionary<FormContext, FormContext> PrevDictionary { get; } =
            new Dictionary<FormContext, FormContext>();

        public string HomeAddress { get; set; }

        public string Context { get; set; }

        private string _state;

        /// <summary>
        ///     当前页面地址 使用该方式导航不会产生返回记录
        /// </summary>
        public string State
        {
            get { return _state; }
            set { InternalNavigate(Context, value); }
        }

        protected long Count;

        public long LastSwitchTime { get; protected set; }

        /// <summary>
        ///     执行导航
        /// </summary>
        /// <param name="context"></param>
        /// <param name="address"></param>
        protected bool InternalNavigate(string context, string address)
        {
#warning 各位注意哈
            /*
             * 此处判断屏蔽 By 叶飞
             * 原因：1.首页退卡按钮，会重复调用返回首次
             * 原因：2.以后一个页面内可多次
             * 尝试注释掉
             */
            //if (context == Context && address == State)
            //    return false;
            var result = false;
            var shell = ServiceLocator.Current.GetInstance<IShell>() as FrameworkElement;
            shell.Dispatcher.Invoke(DispatcherPriority.ContextIdle, (Action)(() =>
            {
                var stopwatch = Stopwatch.StartNew();
                Count++;
                var srcAddress = _state;
                var srcContext = Context;

                var manager = ServiceLocator.Current.GetInstance<IRegionManager>();
                var fullName = Children.GetType(context, address).FullName;
                var view = manager.Regions[RegionNames.正文].GetView(fullName);
                if (view == null)
                {
                    var newView = Children?.Resolve(context, address);
                    manager.Regions[RegionNames.正文].Add(newView, fullName);
                }

                _state = address;
                Context = context;
                manager.RequestNavigate(RegionNames.正文, new Uri(fullName, UriKind.Relative), cbk =>
                {
                    stopwatch.Stop();
                    LastSwitchTime = stopwatch.ElapsedMilliseconds;

                    Logger.Main.Info(
                        $"[系统跳转] [{Count}] 内部导航完成 从[{srcAddress}] 到 [{address}] 耗时:{stopwatch.ElapsedMilliseconds} 导航结果:{((cbk.Result ?? false) ? "成功跳转" : $"跳转取消：{cbk.Error}")}");

                    if (!(cbk.Result ?? false))
                    {
                        _state = srcAddress;
                        Context = srcContext;
                        result = false;
                        return;
                    }
                    if (address == HomeAddress) //确定已经回到首页试，清栈
                    {
                        DestinationStack.Clear(); //清空堆栈
                        PrevDictionary.Clear(); //清除前翻页
                        var vs = manager.Regions[RegionNames.正文].Views;
                        foreach (var v in vs)
                        {
                            if (v.GetType().FullName == fullName)
                                continue;
                            manager.Regions[RegionNames.正文].Remove(v);
                        }
                        SystemStartup.ScopeManager.InitializeLifeScope(); //初始化生命周期
                    }

                    //发布页面跳转事件
                    ServiceLocator.Current.GetInstance<IEventAggregator>()
                        .GetEvent<ViewChangeEvent>()
                        .Publish(new ViewChangeEvent
                        {
                            FromContext = srcContext,
                            From = srcAddress,
                            ToContext = context,
                            To = address
                        });
                    result = true;
                },
                    new NavigationParameters($"FromContext={srcContext}&From={srcAddress}&ToContext={context}&To={address}"));
            }));
            return result;
        }

        /// <summary>
        ///     导航到指定页 产生返回记录
        /// </summary>
        /// <param name="context"></param>
        public bool Navigate(FormContext context)
        {
            PrevDictionary[context] = Current;
            if (InternalNavigate(context))
                return true;
            PrevDictionary.Remove(context);
            return false;
        }

        /// <summary>
        ///     跳转到指定上下文下一页
        /// </summary>
        /// <param name="context">上下文</param>
        public void Next(FormContext context)
        {
            if (!NextDictionary.ContainsKey(context))
            {
                var nextJs = NextDictionary.ToJsonString();
                Logger.Main.Error($"[系统跳转]当次Next导航出现问题 [当前: Context:{context.Context} Address:{context.Address}] [NextDictionary:{nextJs}] 发生堆栈:{Environment.StackTrace}");
                ReportService.软件系统异常($"系统跳转时出现不可预知异常", "立即联系研发人员处理");
                InternalNavigate(Context, HomeAddress);
                return;
            }
            var newState = NextDictionary[context];
            if (newState == HomeAddress && DestinationStack.Count > 0)
            {
                var top = DestinationStack.Peek();
                if (!top.MainContext)
                {
                    if (!InternalNavigate(top.DestContext))
                        return;
                    if (DestinationStack.Count > 0)
                        DestinationStack.Pop();
                    return;
                }
                //无先导Flow的流程 避免重入
                if (top.SrcContext.Equals(top.DestContext))
                {
                    var homeContext = new FormContext(context.Context, newState);
                    if (!InternalNavigate(homeContext))
                        return;
                    //DestinationStack.Pop();
                    return;
                }
                var controlTask = top.DestJumpControl?.Invoke();
                if (controlTask == null)
                {
                    if (!InternalNavigate(top.DestContext))
                        return;
                    if (DestinationStack.Count > 0)
                        DestinationStack.Pop();
                    return;
                }

                controlTask.ContinueWith(task =>
                {
                    var result = task.Result;
                    if (!result.IsSuccess)
                        return;
                    if (result.Value == null)
                    {
                        if (InternalNavigate(top.DestContext))
                        {
                            if (DestinationStack.Count > 0)
                                DestinationStack.Pop();
                        }
                        return;
                    }

                    var defaultDest = top.DestContext;
                    top.DestContext = result.Value;
                    if (!InternalNavigate(top.DestContext))
                    {
                        top.DestContext = defaultDest;
                        return;
                    }
                    if (DestinationStack.Count > 0)
                        DestinationStack.Pop();
                });
                return;
            }
            var newContext = new FormContext(context.Context, newState);
            PrevDictionary[newContext] = Current;
            if (InternalNavigate(newContext))
                return;
            PrevDictionary.Remove(newContext);
        }

        /// <summary>
        ///     跳转到指定上下文上一页
        /// </summary>
        /// <param name="context">上下文</param>
        public bool Prev(FormContext context)
        {
            if (!HasPrev(context))
                return false;

            var prev = PrevDictionary[context];
            if (!InternalNavigate(prev))
                return false;
            PrevDictionary.Remove(context);

            if (DestinationStack.Count > 0)
            {
                var top = DestinationStack.Peek();
                // 本次返回记录是StackNavigate构造的
                if (top.DestContext.Equals(prev))
                    DestinationStack.Pop();
            }
            return true;
        }

        /// <summary>
        ///     判断该上下文是否有返回记录
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool HasPrev(FormContext context) => PrevDictionary.ContainsKey(context);

        public virtual bool IsHome(string address)
        {
            return address == HomeAddress;
        }

        /// <summary>
        ///     导航到指定页，不产生
        /// </summary>
        /// <param name="context"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool Switch(string context, string address)
        {
            return InternalNavigate(context, address);
        }

        #region Shortcut

        public FormContext Current => new FormContext(Context, State);

        /// <summary>
        ///     执行导航
        /// </summary>
        /// <param name="target"></param>
        protected bool InternalNavigate(FormContext target)
        {
            return InternalNavigate(target.Context, target.Address);
        }

        /// <summary>
        ///     导航到指定页 产生返回记录
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool Navigate(string address)
        {
            return Navigate(new FormContext(Context, address));
        }

        /// <summary>
        ///     导航到指定页 产生返回记录
        /// </summary>
        /// <param name="context"></param>
        /// <param name="address"></param>
        public bool Navigate(string context, string address)
        {
            return Navigate(new FormContext(context, address));
        }

        /// <summary>
        ///     跳转到当前页下一页
        /// </summary>
        public void Next()
        {
            Next(Current);
        }

        /// <summary>
        ///     跳转到当前页上一页
        /// </summary>
        public bool Prev()
        {
            return Prev(Current);
        }

        #endregion

        #endregion Navigate

        #region[做Jump]

        /// <summary>
        ///     以压栈方式跳转，该方法在点击返回按钮或者Next到首页时会退栈.直接回到首页会清除栈信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="address"></param>
        public bool StackNavigate(string context, string address)
        {
            if (Context == context) //同一个上下文中
                return Navigate(address);
            var nextContext = new FormContext(context, address);
            //当前入栈
            DestinationStack.Push(new JumpDestination
            {
                MainContext = false,
                SrcContext = nextContext,
                DestContext = Current
            });

            if (Navigate(context, address))
                return true;
            DestinationStack.Pop();
            return true;
        }

        public Stack<JumpDestination> DestinationStack { get; } = new Stack<JumpDestination>();

        /// <summary>
        ///     执行流程跳转器
        /// </summary>
        /// <param name="nextContext">首先需要跳转的Context，当它为空时，直接跳转进入destContext</param>
        /// <param name="jumpControl">控制是在<see cref="nextContext" />执行完成后是否进行跳转，Success为跳转，同时可控制新的跳转逻辑</param>
        /// <param name="destContext">二次跳转目的地</param>
        /// <param name="destTitle">导航标题</param>
        /// <returns></returns>
        public bool JumpAfterFlow(FormContext nextContext, Func<Task<Result<FormContext>>> jumpControl,
            FormContext destContext, string destTitle)
        {
            DestinationStack.Push(new JumpDestination
            {
                MainContext = true,
                MainTitle = destTitle,
                SrcContext = nextContext,
                DestContext = destContext,
                DestJumpControl = jumpControl
            });
            if (nextContext == null)
            {
                if (Navigate(destContext))
                    return true;

            }
            else if (Navigate(nextContext))
            {
                return true;
            }
            DestinationStack.Pop();
            return false;
        }

        #endregion


    }

    public class JumpDestination
    {
        public bool MainContext { get; set; }
        public string MainTitle { get; set; }

        public FormContext SrcContext { get; set; }
        public FormContext DestContext { get; set; }

        public Func<Task<Result<FormContext>>> DestJumpControl { get; set; }
    }
}