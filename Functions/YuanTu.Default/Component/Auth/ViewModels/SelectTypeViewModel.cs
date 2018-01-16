using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Create;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Component.Auth.ViewModels
{
    public class SelectTypeViewModel : ViewModelBase
    {
        public override string Title => "选择发卡对象";

        [Dependency]
        public ICreateModel CreateModel { get; set; }

        public class CreateTypeModel
        {
            public string Name { get; set; }
            public int Order { get; set; }
            public Uri ImageSource { get; set; }

            public bool Visable { get; set; }
            public bool Disabled { get; set; }
            public Color Color { get; set; }
            public CreateType Value { get; set; }
        }

        public override void OnSet()
        {
            var buttonCmd = new DelegateCommand<Info>(OnButtonClick);
            var resource =ResourceEngine;
            var config = GetInstance<IConfigurationManager>();
            var choices = new List<CreateTypeModel>();
            foreach (CreateType type in Enum.GetValues(typeof(CreateType)))
            {
                var spex = $"CreateType:{type}";
                var v = config.GetValue($"{spex}:Visabled");
                if (v != "1")
                    continue;
                var color = config.GetValue($"{spex}:Color").Split(',');
                choices.Add(new CreateTypeModel
                {
                    Name = config.GetValue($"{spex}:Name") ?? "未定义",
                    Order = config.GetValueInt($"{spex}:Order"),
                    ImageSource = resource.GetImageResourceUri(config.GetValue($"{spex}:ImageName")),
                    Color = Color.FromRgb(byte.Parse(color[0]), byte.Parse(color[1]), byte.Parse(color[2])),
                    Value = type,
                });
            }
            var list = choices.OrderBy(p => p.Order).Select(p => new InfoIcon
            {
                Title = p.Name,
                ConfirmCommand = buttonCmd,
                IconUri = p.ImageSource,
                Tag = p,
                Color = p.Color
            });

            Data = new ObservableCollection<InfoIcon>(list);

         
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            PlaySound(SoundMapping.请选择办卡类型);
        }
        protected virtual void OnButtonClick(Info obj)
        {
            var choice = obj.Tag as CreateTypeModel;
            CreateModel.CreateType = choice.Value;
            Navigate(A.CK.Choice);
        }

        #region Binding

        private ObservableCollection<InfoIcon> _data;

        public ObservableCollection<InfoIcon> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding
    }
}
