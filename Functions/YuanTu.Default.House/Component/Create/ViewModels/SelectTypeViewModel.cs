using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Create;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;


namespace YuanTu.Default.House.Component.Create.ViewModels
{
    public class SelectTypeViewModel:ViewModelBase
    {
       
        public override string Title { get; } = "选择办卡类型";

        public override void OnEntered(NavigationContext navigationContext)
        {
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var config = GetInstance<IConfigurationManager>();
            var bts = new List<CreateTypeDto>();
            var k = Enum.GetValues(typeof(CreateType));
            foreach (CreateType createType in k)
            {
                var spex = $"HouseCreateType:{createType}";
                var v = config.GetValue($"{spex}:Visabled");
                if (v == "1")
                {
                    bts.Add(new CreateTypeDto
                    {
                        CreateType = createType,
                        Name = config.GetValue($"{spex}:Name") ?? "未定义",
                        Order = config.GetValueInt($"{spex}:Order"),
                        Remark = config.GetValue($"{spex}:Remark")
                    });
                }
            }
            var list = bts.OrderBy(p => p.Order).Select(p => new InfoType
            {
                Title = p.Name,
                ConfirmCommand = confirmCommand,
                Tag = p,
                Remark = p.Remark
            });
            Data = new ObservableCollection<InfoType>(list);
         
        }

        protected virtual void Confirm(Info i)
        {
            ChangeNavigationContent(".");
            var choice = i.Tag as CreateTypeDto;
            CreateModel.CreateType = choice.CreateType;
            Navigate(AInner.JD.IdCard);
            //Next();
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            return base.OnLeaving(navigationContext);
        }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }
        [Dependency]
        public ICreateModel CreateModel { get; set; }

        #region Binding

        private ObservableCollection<InfoType> _data;

        public ObservableCollection<InfoType> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding

        public class CreateTypeDto
        {
            public CreateType CreateType { get; set; }
            public string Name { get; set; }

            public int Order { get; set; }
            public string Remark { get; set; }
         }
    }
}
