using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.ShenZhenArea.CardReader;
using YuanTu.ShenZhenArea.Enums;
using YuanTu.ShenZhenArea.Models;
using YuanTu.ShenZhenArea.Services;

namespace YuanTu.ShenZhen.BaoAnShiYanHospital.Component.Auth.ViewModels
{
    public class SiCardPasswordViewModel : ViewModelBase
    {

        [Dependency]
        public IYBService YBServices { get; set; }

        [Dependency]
        public IYBModel YBModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }


        [Dependency]
        public IIdCardModel IdCardModel { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        private string _siPassword;
        public string SiPassword
        {
            get { return _siPassword; }
            set
            {
                _siPassword = value;
                OnPropertyChanged();
            }
        }
        public ICommand ConfirmCommand { get; set; }


        public SiCardPasswordViewModel()
        {
            ConfirmCommand = new DelegateCommand(Do);
        }

        public override string Title => "请输入社保卡密码，没密码直接按确定键";

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            if (ConfigBaoAnShiYanHospital.OpenSheBaoCardPasswordEnter)
                ShowAlert(true, "温馨提示", "没有设置密码，请点击确认");
            else
                ConfirmCommand.Execute("");
        }


        public virtual void Do()
        {
            if (string.IsNullOrEmpty(SiPassword))
                YBModel.医保密码 = "";
            else
                YBModel.医保密码 = SiPassword.Trim();
            var result = YBServices.医保个人基本信息查询();
            if (result.IsSuccess)
            {
                var GRXX = YBModel.医保个人基本信息;
                if (GRXX.DNH.IsNullOrWhiteSpace())
                {
                    ShowAlert(false, "病人信息查询", "社保卡卡号为空，请确认插卡方向是否正确和卡片是否有效");
                    Preview();
                    return;
                }
                if (ChoiceModel.Business == Business.建档)
                {
                    int xb = Convert.ToInt32(GRXX.XB);
                    CardModel.CardNo = GRXX.DNH;
                    IdCardModel.Name = GRXX.XM;
                    IdCardModel.Sex = xb == 1 ? Sex.男 : (xb == 2 ? Sex.女 : Sex.未知);
                    IdCardModel.IdCardNo = GRXX.SFZH;
                    IdCardModel.Birthday = Convert.ToDateTime(GRXX.SFZH.Substring(6, 4) + "-" + GRXX.SFZH.Substring(10, 2) + "-" + GRXX.SFZH.Substring(12, 2));
                    Logger.Main.Info($"[读取社保卡信息成功]{IdCardModel.Name} {IdCardModel.Sex} {IdCardModel.IdCardNo}");
                    DoCommand(ctx =>
                    {
                        Navigate(A.CK.Info);
                    });
                    return;
                }
                else
                {

                    if (GRXX.XM != PatientModel.当前病人信息.name)
                    {
                        ShowAlert(false, "病人信息校验失败", "社保卡对应的姓名与就诊卡姓名不一致\n请到服务中心更改就诊信息");
                        return;
                    }

                    if (!string.IsNullOrEmpty(PatientModel.当前病人信息.idNo))
                    {
                        if (GRXX.SFZH != PatientModel.当前病人信息.idNo)
                        {
                            ShowAlert(false, "病人信息校验失败", "社保卡对应的身份证号码与就诊卡身份证号码不一致\n请到服务中心更改就诊信息");
                            //Preview();
                            return;
                        }
                    }
                    #region 判断HIS档案的病人类型与社保局获取到的是否一致

                    Cblx hisPatientType = Cblx.不参加;
                    switch (PatientModel.当前病人信息.patientType.Trim())
                    {
                        case "一档":
                            hisPatientType = Cblx.基本医疗保险一档;
                            break;
                        case "二档":
                            hisPatientType = Cblx.基本医疗保险二档;
                            break;
                        case "三档":
                            hisPatientType = Cblx.基本医疗保险三档;
                            break;
                        default:
                            break;
                    }
                    if(hisPatientType!= YBModel.参保类型)
                    {
                        ShowAlert(true, "温馨提示", $"目前您在我院档案的参保类型“{hisPatientType}”与您在社保局的参保类型“{YBModel.参保类型}”不一致，请抽空去人工窗口修改我院档案的参保类型");
                    }

                    #endregion

                    if (YBModel.参保类型 != Cblx.基本医疗保险一档)
                    {
                        ShowAlert(false, "温馨提示", $"目前自助机仅支持基本医疗保险一档与自费缴费。\n您的社保参保类型为：{YBModel.参保类型}\n请到人工窗口进行结算！");
                        //YBModel.Res医保个人基本信息 = null;
                        //NavigationEngine.Next(new FormContext(A.ChaKa_Context, A.CK.Info));
                        return;
                    }
                    
                    NavigationEngine.Next(new FormContext(A.ChaKa_Context, A.CK.Info));
                    return;
                }
            }
            else
            {
                ShowAlert(false, "病人信息查询", "未找到该社保卡对应的信息。", debugInfo: result.Message);
                //Preview();
                return;
            }
        }
    }
}
