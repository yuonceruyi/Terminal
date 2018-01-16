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
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.ShenZhenArea.CardReader;
using YuanTu.ShenZhenArea.Models;
using YuanTu.ShenZhenArea.Services;

namespace YuanTu.ShenZhen.BaoAnCenterHospital.Component.Auth.ViewModels
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
            if (ConfigBaoAnCenterHospital.OpenSheBaoCardPasswordEnter)
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
                string cardNo = GRXX.DNH;
                if (cardNo.IsNullOrWhiteSpace())
                {
                    ShowAlert(false, "病人信息查询", "社保卡卡号为空，请确认插卡方向是否正确和卡片是否有效");
                    Navigate(A.Home);
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
                }
                else
                {
                    DoCommand(ctx =>
                    {
                        PatientModel.Req病人信息查询 = new req病人信息查询
                        {
                            Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                            cardNo = cardNo,
                            cardType = ((int)CardModel.CardType).ToString(),
                            secrityNo = ""
                        };
                        PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
                        //如果是这样则重新查一次
                        //{"success":false,"msg":"没有有效账户,不能充值.","code":-3,"data":null}       
                        if ((!(PatientModel.Res病人信息查询.success)) && PatientModel.Res病人信息查询.code == -3 && PatientModel.Res病人信息查询.msg.Trim().Contains("没有有效账户"))
                        {
                            PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
                        }

                        if (PatientModel.Res病人信息查询.success)
                        {
                            if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                            {
                                ShowAlert(false, "病人信息查询", "未查到该卡在我院有档案信息，请返回主页选择“自助建档”新建档案！");
                                Navigate(A.Home);
                                return;
                            }
                            else
                            {
                                CardModel.CardNo = cardNo;
                                CardModel.CardType = CardType.社保卡;
                                var cm = (CardModel as ShenZhenCardModel);
                                cm.RealCardType = CardType.社保卡;
                                Navigate(A.CK.Info);
                            }
                        }
                        else   //查询失败
                        {
                            ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
                            Navigate(A.Home);
                        }
                    });
                }
            }
            else
            {
                ShowAlert(false, "病人信息查询", "未找到该社保卡对应的信息。", debugInfo: result.Message);
                Preview();
                return;
            }
        }
    }
}
