using System.Collections.Generic;
using System.Collections.ObjectModel;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.BJJingDuETYY.Component.Biometric.ViewModels
{
    internal class ChoiceViewModel : ViewModelBase
    {

        public ChoiceViewModel()
        {
            Command = new DelegateCommand<Info>(Do);
        }

        public override string Title => "生物信息录入选择";
        public DelegateCommand<Info> Command { get; set; }

        private IReadOnlyCollection<InfoCard> _data;
        public IReadOnlyCollection<InfoCard> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            var config = GetInstance<IConfigurationManager>();
            var resource = ResourceEngine;
            var infos = new List<InfoCard>();
            foreach (var keyName in new[] { "面部信息录入", "指纹信息录入" })
            {
                var visable = config.GetValueInt($"Biometric:{keyName}:Visabled") == 1;
                if (!visable)
                    continue;
                infos.Add(new InfoCard
                {
                    Title = config.GetValue($"Biometric:{keyName}:Name") ?? "未定义",
                    No = config.GetValueInt($"Biometric:{keyName}:Order"),
                    IconUri = resource.GetImageResourceUri(config.GetValue($"Biometric:{keyName}:ImageName")),
                    ConfirmCommand = Command
                });
            }
            Data = new ReadOnlyCollection<InfoCard>(infos);
        }

        protected virtual void Do(Info param)
        {
            switch (param.Title)
            {
                case "面部信息录入":
                    Navigate(A.Bio.FaceRec);
                    break;

                case "指纹信息录入":
                    Navigate(A.Bio.FingerPrint);
                    break;
            }
        }
    }
}