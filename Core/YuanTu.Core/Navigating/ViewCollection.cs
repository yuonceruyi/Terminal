using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Infrastructure;

namespace YuanTu.Core.Navigating
{
    public class ViewCollection
    {
        private readonly NavigationEngine _engine;
        private readonly Dictionary<string, Type> types = new Dictionary<string, Type>();

        public ViewCollection(NavigationEngine manager)
        {
            _engine = manager;
        }

        public string Context { get; set; } = string.Empty;

        /// <summary>
        ///     获取指定地址对应的页面 触发页面构造
        /// </summary>
        /// <param name="context"></param>
        /// <param name="address">地址</param>
        /// <returns></returns>
        //public ViewsBase this[string context,string address] => Resolve(context,address);
        public ViewsBase Resolve(string context, string address)
        {
            var uniqueKey = BuildUniqueKey(context, address);
            return ServiceLocator.Current.GetInstance<ViewsBase>(uniqueKey);
        }

        /// <summary>
        ///     获取指定地址注册的类 不会触发页面构造
        /// </summary>
        /// <param name="context">上下文地址</param>
        /// <param name="address">地址</param>
        /// <returns></returns>
        public Type GetType(string context, string address)
        {
            if (address == null)
                return null;
            var uniqueKey = BuildUniqueKey(context, address);
            return types[uniqueKey];
        }

        /// <summary>
        ///     添加绑定
        /// </summary>
        /// <param name="address">绑定地址</param>
        /// <param name="context"></param>
        /// <param name="title"></param>
        /// <param name="type">Presenter类</param>
        public void Add(string address, string context, string title, Type type)
        {
            if (type != null)
            {
                if (!typeof(ViewsBase).IsAssignableFrom(type))
                    throw new ArgumentException($"{type.FullName}必须继承自 ViewsBase", nameof(type));
                var container = ServiceLocator.Current.GetInstance<IUnityContainer>();
                var uniqueKey = BuildUniqueKey(context, address);

                container.RegisterType(typeof(ViewsBase), type, uniqueKey,
                    new NormalLifetimeManager(SystemStartup.ScopeManager));

                types[uniqueKey] = type;

            }

            var fc = new FormContextEx(context, address, title,null);
            var cd = _engine.ContextDictionary;
            var key = context ?? string.Empty;
            if (cd.ContainsKey(key))
            {
                var list = cd[key];
                var found = false;
                for (var i=0;i< list.Count;i++)
                {
                   if (!list[i].Equals(fc)) continue;
                   found = true;
                   list[i] = fc;
                }

                if(!found)
                    cd[key].Add(fc);
            }
            else
                cd[key] = new List<FormContextEx> { fc };
        }

        /// <summary>
        ///     添加绑定
        /// </summary>
        /// <param name="address">绑定地址</param>
        /// <param name="title">导航标题（为空则不显示）</param>
        /// <param name="type">IView接口</param>
        /// <param name="context">指定Context</param>
        /// <param name="next">默认下一页地址</param>
        public void Add(string address, string title, Type type, string context, string next)
        {
            Add(address, context, title, type);
            var fctx = new FormContext(context, address);
            _engine.NextDictionary[fctx] = next;
        }

        /// <summary>
        ///     在当前Context下添加绑定
        /// </summary>
        /// <param name="address">绑定地址</param>
        /// <param name="title">导航标题（为空则不显示）</param>
        /// <param name="type">IView接口</param>
        /// <param name="next">默认下一页地址</param>
        public void Add(string address, string title, Type type, string next)
        {
            Add(address, title, type, Context, next);
        }

        public void AddWithIcon(string address, string title, Type type, string next,string icon)
        {
            if (type != null)
            {
                if (!typeof(ViewsBase).IsAssignableFrom(type))
                    throw new ArgumentException($"{type.FullName}必须继承自 ViewsBase", nameof(type));
                var container = ServiceLocator.Current.GetInstance<IUnityContainer>();
                var uniqueKey = BuildUniqueKey(Context, address);
                container.RegisterType(typeof(ViewsBase), type, uniqueKey,
                    new NormalLifetimeManager(SystemStartup.ScopeManager));
                types[uniqueKey] = type;
            }

            var fc = new FormContextEx(Context, address, title,icon);
            var cd = _engine.ContextDictionary;
            var key = Context ?? string.Empty;
            if (cd.ContainsKey(key))
                cd[key].Add(fc);
            else
                cd[key] = new List<FormContextEx> { fc };

            var fctx = new FormContext(Context, address);
            _engine.NextDictionary[fctx] = next;
        }
        private string BuildUniqueKey(string context, string address)
        {
            return $"{address}";
        }
    }
}