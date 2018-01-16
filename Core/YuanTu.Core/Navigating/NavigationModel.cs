using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;

namespace YuanTu.Core.Navigating
{
    public interface INavigationModel : IModel
    {
        bool JumpSourceAware { get; set; }
        bool PreventClick { get; set; }
        string MainContext { get; set; }
        ObservableCollection<NavigationItem> Items { get; }

        string MakeNavigationList();

        NavigationItem LocateNavigationItem(FormContext context);

        int GetIndex(FormContext formContext);
    }

    public class NavigationModel : ModelBase, INavigationModel
    {
        [Dependency]
        public NavigationEngine NavigationEngine { get; set; }

        public bool JumpSourceAware { get; set; } = true;

        private bool _preventClick;

        public bool PreventClick
        {
            get { return _preventClick; }
            set
            {
                //仅在主流程有效
                if (NavigationEngine.Context != MainContext)
                    return;
                _preventClick = value;
            }
        }

        public string MainContext { get; set; }

        public ObservableCollection<NavigationItem> Items { get; } = new ObservableCollection<NavigationItem>();

        public string MakeNavigationList()
        {
            var engine = NavigationEngine;
            var main = engine.DestinationStack.First(d => d.MainContext);
            var mainContext = main.DestContext?.Context;
            var priorContext = main.SrcContext?.Context;
            var items=new List<NavigationItem>();
            if (!mainContext.IsNullOrWhiteSpace())
            {
                var mainContexts = engine.ContextDictionary[mainContext];
                items = mainContexts
                    .GroupBy(c => c.Title)
                    .Select(g =>
                    {
                        //多个同名
                        if (g.Count() > 1)
                            return new NavigationItem
                            {
                                IsAmbiguous = true,
                                IsCollection = true,
                                FormContexts = g.ToList(),
                                Title = g.Key
                            };
                        var c = g.First();
                        return new NavigationItem
                        {
                            FormContext = c,
                            Title = c.Title,
                            Icon = c.Icon
                            
                        };
                    })
                    .ToList();
            }


            if (JumpSourceAware&& !priorContext.IsNullOrWhiteSpace())
            {
                var priorContexts = engine.ContextDictionary[priorContext];
                var first = priorContexts.First(c => c.Equals(main.SrcContext));
                items.Insert(0, new NavigationItem
                {
                    IsCollection = true,
                    FormContext = first,
                    FormContexts = priorContexts,
                    Title = first.Title,
                    Icon = first.Icon
                    
                });
            }

            Items.AddRange(items);
            MainContext = mainContext?? priorContext;
            return mainContext;
        }

        public NavigationItem LocateNavigationItem(FormContext context)
        {
            var index = GetIndex(context);
            if (index < 0)
                return null;
            return Items[index];
        }

        public int GetIndex(FormContext formContext)
        {
            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                if (item.IsCollection)
                {
                    foreach (var c in item.FormContexts)
                        if (formContext.Equals(c))
                            return i;
                }
                else if (formContext.Equals(item.FormContext))
                {
                    return i;
                }
                else if (item.FormContexts != null)
                {
                    foreach (var c in item.FormContexts)
                        if (formContext.Equals(c))
                            return i;
                }
            }
            return -1;
        }
    }

    public class NavigationItem : ModelBase
    {
        public bool HasFootprint { get; set; }

        public bool IsAmbiguous { get; set; }

        public bool IsCollection { get; set; }

        public bool IsDynamic { get; set; }

        public bool IsPlaceHolder { get; set; }

        public FormContextEx FormContext { get; set; }

        public List<FormContextEx> FormContexts { get; set; }

        #region DataBinding

        private string _content;
        private int? _index;
        private object _tag;
        private string _title;
        private string _icon;

        public int? Index
        {
            get { return _index; }
            set
            {
                _index = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }

        public object Tag
        {
            get { return _tag; }
            set
            {
                _tag = value;
                OnPropertyChanged();
            }
        }

        public string Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                OnPropertyChanged();
            }
        }
        #endregion DataBinding

        public override string ToString()
        {
            return $"{Title} @{FormContext} {(HasFootprint ? 'F' : ' ')}{(IsCollection?'C':' ')}{(IsAmbiguous?'A':' ')}{(IsDynamic?'D':' ')}{(IsPlaceHolder?'H':' ')} {FormContexts?.Count}";
        }
    }
}