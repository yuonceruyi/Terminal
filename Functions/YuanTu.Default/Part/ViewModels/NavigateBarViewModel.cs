using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Prism.Commands;
using Prism.Events;
using YuanTu.Consts;
using YuanTu.Consts.EventModels;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Navigating;

namespace YuanTu.Default.Part.ViewModels
{
    public class NavigateBarViewModel : ViewModelBase
    {
        public NavigateBarViewModel()
        {
        }

        public NavigateBarViewModel(IEventAggregator eventAggregator)
        {
            NavClickCommand = new DelegateCommand<NavigationItem>(NavigateClick);
            eventAggregator.GetEvent<ViewChangingEvent>().Subscribe(ViewIsChanging);
        }

        public static int MinimalItemCount { get; set; } = 6;

        public ICommand NavClickCommand { get; set; }

        public override string Title => "导航栏";

        protected string MainContext { get; set; }

        protected virtual void ViewIsChanging(ViewChangingEvent eveEvent)
        {
            var nav = GetInstance<INavigationModel>();
            Items = nav.Items;
            var engine = NavigationEngine;
            if (engine.IsHome(eveEvent.From))
                Items.Clear();
            if (engine.DestinationStack.Any())
            {
                if (!Items.Any())
                {
                    MainContext = nav.MakeNavigationList();
                    OnItemsChanged();
                    UpdateFlowTitle();
                }
                else
                {
                    var top = engine.DestinationStack.Peek();
                    var index = nav.GetIndex(top.DestContext);
                    if (index > 0)
                        Items[index].FormContexts = engine.ContextDictionary[top.SrcContext.Context].ToList();
                }
            }

            var fromContext = new FormContext(eveEvent.FromContext, eveEvent.From);
            var toContext = new FormContext(eveEvent.ToContext, eveEvent.To);

            var fromIndex = nav.GetIndex(fromContext);
            var toIndex = nav.GetIndex(toContext);


            //返回
            if (fromIndex >= 0 && toIndex >= 0 && fromIndex > toIndex)
            {
                OnGoBack(fromIndex, toIndex);
            }

            NavigationItem currentItem;
            if (toIndex < 0)
            {
                if (fromIndex < 0)
                    return;
                currentItem = OnWhereTo(engine, fromContext, toContext, fromIndex);
            }
            else
            {
                currentItem = Items[toIndex];
            }
            currentItem.HasFootprint = true;
            if (currentItem.IsAmbiguous && currentItem.FormContext == null)
                currentItem.FormContext = currentItem.FormContexts.First(c => c.Equals(toContext));
            CurrentItem = currentItem;

        }

        protected virtual void OnGoBack(int fromIndex, int toIndex)
        {
            bool changed = false;
            for (var i = fromIndex; i >= toIndex; i--)
            {
                var item = Items[i];
                if (item.IsDynamic && i != toIndex)
                {
                    Items.RemoveAt(i);
                    changed = true;
                }
                else
                {
                    OnGoBack(item);
                }
            }
            if (changed)
                OnItemsChanged();
        }

        protected virtual void OnGoBack(NavigationItem item)
        {
            item.HasFootprint = false;
            item.Content = null;
            if (item.IsAmbiguous)
                item.FormContext = null;
        }

        protected virtual NavigationItem OnWhereTo(NavigationEngine engine, FormContext fromContext, FormContext toContext, int fromIndex)
        {
            if (toContext.Address == A.Home)
                return new NavigationItem();

            //构造新项
            var fromItem = Items[fromIndex];

            var newContext = engine.ContextDictionary[toContext.Context].Find(f => f != null && f.Equals(toContext));
            if (newContext == null)
            {
                var found = engine.ContextDictionary[string.Empty].Find(f => f.Address == toContext.Address);
                if (found != null)
                    newContext = new FormContextEx(toContext.Context, toContext.Address, found.Title, found.Icon);
            }

            //From是集合
            if (fromItem.FormContexts != null && fromItem.FormContexts.Any(f => f != null && f.Equals(fromContext)))
            {
                fromItem.FormContexts.Add(newContext);
                return fromItem;
            }
            var newItem = new NavigationItem()
            {
                FormContext = newContext,
                Title = newContext?.Title,
                Icon = newContext?.Icon,
                IsDynamic = true,
            };
            Items.Insert(fromIndex + 1, newItem);
            OnItemsChanged();
            return newItem;
        }

        protected virtual void OnItemsChanged()
        {
            var items = Items;

            var index = 1;
            foreach (var item in items.Where(i => !i.IsPlaceHolder))
                item.Index = index++;

            if (items.Count > 0)
                for (var i = items.Count; i < MinimalItemCount; i++)
                    items.Add(new NavigationItem { IsPlaceHolder = true });
        }

        protected virtual void UpdateFlowTitle()
        {
            FlowTitle = NavigationEngine.DestinationStack.FirstOrDefault(d => d.MainTitle != null)?.MainTitle;
        }

        protected virtual void NavigateClick(NavigationItem item)
        {
            //占位 未路过
            if (item.FormContext == null || !item.HasFootprint)
                return;
            var nav = GetInstance<INavigationModel>();
            if (item.FormContext.Context == A.ChaKa_Context || nav.PreventClick)
                return;
            var enging = NavigationEngine;
            var current = enging.Current;
            var fromIndex = nav.GetIndex(current);
            var toIndex = Items.IndexOf(item);
            if (fromIndex == toIndex || fromIndex < 0 || toIndex < 0)
                return;

            DoCommand(p =>
            {
                enging.Switch(item.FormContext.Context, item.FormContext.Address);
            });
        }

        #region DataBinding

        private NavigationItem _currentItem;
        private object _flowTitle;
        private ObservableCollection<NavigationItem> _items;

        public ObservableCollection<NavigationItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     主标题
        /// </summary>
        public object FlowTitle
        {
            get { return _flowTitle; }
            set
            {
                _flowTitle = value;
                OnPropertyChanged();
            }
        }

        public NavigationItem CurrentItem
        {
            get { return _currentItem; }
            set
            {
                _currentItem = value;
                OnPropertyChanged();
            }
        }

        #endregion DataBinding
    }
}