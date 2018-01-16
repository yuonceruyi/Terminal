using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.UserCenter.Register;
using YuanTu.Consts.UserCenter;
using YuanTu.Consts.UserCenter.Entities;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Tablet.Component.Register.ViewModels
{
    public class HospitalsViewModel : ViewModelBase
    {
        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        [Dependency]
        public IRegisterModel RegisterModel { get; set; }

        public override string Title => "请触摸下方卡片选择医院";

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var function = ChoiceModel.Business == Business.挂号 ? "挂号" : "预约";
            var list = RegisterModel.Res医院列表.data.Where(p=>p.funcTagList.Contains(function)).Select(p => new InfoHospital
            {
                Title = p.name,
                CorpTags = p.corpTags,
                Address = p.address,
                Tag = p,
                IsEnabled=p.online==1,
                IconUri = p.corpLogo,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<InfoHospital>(list);

            //todo 添加选择医院语音
            //PlaySound();
        }

        protected virtual void Confirm(Info i)
        {
            RegisterModel.当前选择医院 = i.Tag.As<CorpVO>();

            //todo 打印凭条的医院名称
            FrameworkConst.HospitalName = RegisterModel.当前选择医院.name;

            //TODO 查询科室
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询排班科室，请稍候...");
                var req = new req科室列表
                {
                    corpId = RegisterModel.当前选择医院.corpId.ToString(),
                    regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                    regType = null
                };
                var res = DataHandlerEx.科室列表(req);
                if (res?.success ?? false)
                    if (res?.data?.multiDeptOutParams?.deptOutParams?.Count > 0)
                    {
                        RegisterModel.Res科室列表 = res;
                        ChangeNavigationContent(i.Title);
                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "科室列表查询", "没有获得科室信息(列表为空)");
                    }
                else
                    ShowAlert(false, "科室列表查询", "没有获得科室信息", debugInfo: res?.msg);
            });
        }

        #region Binding

        private ObservableCollection<InfoHospital> _data;

        public ObservableCollection<InfoHospital> Data
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