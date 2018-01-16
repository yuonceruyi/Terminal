using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Register;
using YuanTu.Core.FrameworkBase;
using YuanTu.XiaoShanArea.CYHIS.WebService;
using YuanTu.XiaoShanHealthStation.Component.Register.Models;

namespace YuanTu.XiaoShanHealthStation.Component.Register.ViewModels
{
    public class AmPmViewModel : ViewModelBase
    {
        public override string Title { get; } = "选择上午下午";

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }
        [Dependency]
        public IGuaHaoModel GuaHaoModel { get; set; }
        [Dependency]
        public IRegTypesModel RegTypesModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var list = new List<InfoType>
            {
                new InfoType
                {
                    Title = "上午",
                    ConfirmCommand = confirmCommand,
                    IconUri = null,
                    Tag = AmPm.Am,
                    Color = new System.Windows.Media.Color(),
                    Remark = "上午时段的号源"
                },
                new InfoType
                {
                    Title = "下午",
                    ConfirmCommand = confirmCommand,
                    IconUri = null,
                    Tag = AmPm.Pm,
                    Color = new System.Windows.Media.Color(),
                    Remark = "下午时段的号源"
                }
            };
            if (DateTimeCore.Now.Hour >= 12)
                list.RemoveAt(0);
            Data = new ObservableCollection<InfoType>(list);

        }

        protected virtual void Confirm(Info i)
        {
            GuaHaoModel.AmPm = (AmPm) i.Tag;
            ChangeNavigationContent($"{i.Title}");
            DoCommand(p =>
            {
                p.ChangeText("正在查询排班，请稍候...");
                var req = new YIYUANPBXX_IN
                {
                    BASEINFO = Instance.Baseinfo,
                    PAIBANLX = ChoiceModel.Business == Business.预约 ? "1" : "2",
                    PAIBANRQ = GuaHaoModel.RegDate.ToString("yyyy-MM-dd"),
                    GUAHAOBC = ChoiceModel.Business == Business.预约 ? " " : GuaHaoModel.AmPm.ToString(),
                    GUAHAOLB = RegTypesModel.SelectRegType.RegType == RegType.普通门诊 ? "1" : "4",
                    GUAHAOFS = ChoiceModel.Business == Business.预约 ? "2" : "1",
                    KESHIDM = " ",
                    YISHENGDM = "*",
                    HUOQUROW = " "
                };
                YIYUANPBXX_OUT res;
                if (!DataHandler.YIYUANPBXX(req, out res))
                {
                    ShowAlert(false, "温馨提示", "排班信息查询失败");
                    return;
                }

                if (res.PAIBANLB == null || res.PAIBANLB.Count == 0)
                {
                    ShowAlert(false, "温馨提示", "排班信息查询失败（列表为空）");
                    return;
                }

                GuaHaoModel.排班明细 = res.PAIBANLB;
                Next();
            });
           
        }
        

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
    }
}