using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Tools;
using YuanTu.Consts.Models;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Component.Auth.ViewModels
{
    public class InPatientNoViewModel : YuanTu.Default.Component.Auth.ViewModels.InPatientNoViewModel
    {

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        public override string Title => "输入登记号";

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            Hint = "请输入登记号";
        }

        public override void Confirm()
        {
            if (InPatientNo == "")
            {
                ShowAlert(false, "住院患者信息查询", "登记号不能为空");
                return;
            }
            DoCommand(lp =>
            {
                var req = new req住院患者信息查询
                {
                    patientId = InPatientNo
                };
                var res = DataHandlerEx.住院患者信息查询(req);
                if (res?.success ?? false)
                {
                    if (res?.data == null)
                    {
                        ShowAlert(false, "住院患者信息查询", "查询患者信息失败");
                        return;
                    }
                    ChangeNavigationContent($"{InPatientNo}");
                    PatientModel.Res住院患者信息查询 = res;
                    if(ChoiceModel.Business== Consts.Enums.Business.住院押金)
                    {
                        //凡是使用银行卡充值的需要退费必须带上充值银行卡进行原路返回
                        ShowAlert(true, "温馨提示", "凡是使用银行卡充值的需要退费必须带上充值银行卡进行原路返回");
                    }
                    Next();
                }
                else
                {
                    ShowAlert(false, "住院患者信息查询", "查询患者信息失败" + res?.msg);
                }
            });
        }
        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试登记号");
            if (ret.IsSuccess)
            {
                InPatientNo = ret.Value;
                Confirm();
            }
        }
    }
}