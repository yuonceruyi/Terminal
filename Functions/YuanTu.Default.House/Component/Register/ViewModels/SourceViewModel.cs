using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Linq;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.House.Component.Register.ViewModels
{
    public class SourceViewModel : ViewModelBase
    {
        public override string Title => "选择预约号源";

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        [Dependency]
        public ISourceModel SourceModel { get; set; }

        [Dependency]
        public IScheduleModel ScheduleModel { get; set; }

        [Dependency]
        public IDeptartmentModel DeptartmentModel { get; set; }

        [Dependency]
        public IPaymentModel PaymentModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = SourceModel.Res号源明细查询.data.Select(p => new InfoTime
            {
                Time = $"{p.medBegtime}-{p.medEndtime}",
                Title = $"序号 {p.appoNo}",
                Tag = p,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<InfoTime>(list);

            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号号源 : SoundMapping.选择预约号源);
        }

        protected virtual void Confirm(Info i)
        {
            SourceModel.所选号源 = i.Tag.As<号源明细>();
            ChangeNavigationContent(i.Title);

            Next();
        }

        #region Binding

        private ObservableCollection<InfoTime> _data;

        public ObservableCollection<InfoTime> Data
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