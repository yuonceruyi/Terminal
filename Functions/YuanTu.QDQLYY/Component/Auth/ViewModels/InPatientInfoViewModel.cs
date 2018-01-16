using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using YuanTu.Core.FrameworkBase;
using Prism.Regions;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.Extension;
using YuanTu.Consts.Models;
using YuanTu.Consts;
using YuanTu.QDQLYY.Current.Models;
using YuanTu.Consts.Gateway;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts.Services;

namespace YuanTu.QDQLYY.Component.Auth.ViewModels
{
    public class InPatientInfoViewModel : YuanTu.Default.Component.Auth.ViewModels.InPatientInfoViewModel
    {

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }
        [Dependency]
        public ICardModel CardModel { get; set; }
        public static int maxPrintTimes = 1;
        public override void Confirm()
        {

            if (ChoiceModel.Business == Consts.Enums.Business.住院押金)
            {

                if (PatientModel.住院患者信息.status != "入院")
                {
                    ShowAlert(false, "住院押金", "您不是入院状态，无法进行充值。", 10);
                    Navigate(A.Home);
                }
                else
                {
                    ChangeNavigationContent($"{Name}");
                    Next();
                }
            }
            else if(ChoiceModel.Business== Consts.Enums.Business.出院清单打印)
            {
                ChangeNavigationContent($"{Name}");
                if (GetInAllDetailData())
                {
                    Next();
                }
                else
                {
                    Navigate(A.Home);
                }
            }
            else
            {
                ChangeNavigationContent($"{Name}");
                Next();
            }
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            AccBalance = null;
        }
        private bool GetInAllDetailData()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            maxPrintTimes = config.GetValueInt("maxPrintTimes", 1);
            bool printflag = true;
            var inAllDetailModel = GetInstance<IInAllDetailModel>();
            switch (CardModel.CardType)
            {
                case Consts.Enums.CardType.住院号:
                    inAllDetailModel.Req出院结算明细次数 = new req出院结算明细次数
                    {
                        patientHosId = PatientModel.住院患者信息.patientHosId,
                    };
                    break;
                case Consts.Enums.CardType.社保卡:
                case Consts.Enums.CardType.身份证:
                    inAllDetailModel.Req出院结算明细次数 = new req出院结算明细次数
                    {
                        cardType = PatientModel.Req住院患者信息查询.cardType,
                        cardNo = PatientModel.Req住院患者信息查询.cardNo,
                    };
                    break;
                default:
                    inAllDetailModel.Req出院结算明细次数 = new req出院结算明细次数
                    {
                        patientHosId = PatientModel.住院患者信息.patientHosId,
                        cardType = PatientModel.Req住院患者信息查询.cardType,
                        cardNo = PatientModel.Req住院患者信息查询.cardNo,
                    };
                    break;
            }

            inAllDetailModel.Res出院结算明细次数 = DataHandlerEx.出院结算明细次数(inAllDetailModel.Req出院结算明细次数);
            if ((inAllDetailModel.Res出院结算明细次数?.success ?? false) && (inAllDetailModel.Res出院结算明细次数.data.Count > 0))
            {
                var dl = inAllDetailModel.Res出院结算明细次数.data;
                dl.Sort((x, y) => { return DateTime.Parse(x.billDate).CompareTo(DateTime.Parse(y.billDate)); });
                var 出院结算次数 = dl[dl.Count - 1];
                //多次结算
                if (dl.Count > 1)
                {
                    for (int i = 0; i < dl.Count - 1; i++)
                    {
                        var d = dl[i];
                        if (d.patientHosId == 出院结算次数.patientHosId)
                        {
                            ShowAlert(false, "住院患者信息查询", "最后一次出院有多次结算信息，请到窗口进行打印", 10);
                            printflag = false;
                        }
                    }
                }
                //中途结算
                if (出院结算次数.isMidwayBalance == "1")
                {
                    ShowAlert(false, "住院患者信息查询", "您的最后一次结算是中途结算，请到窗口进行打印", 10);
                    printflag = false;
                }
                if (int.Parse(出院结算次数.printTimes) >= maxPrintTimes)
                {
                    ShowAlert(false, "住院患者信息查询", "您的出院结算记录已经打印过，不能重复打印", 10);
                }
                else
                {
                    if (出院结算次数.isBalanceRecord != "0")
                    {
                        string msg = "您尚有未结算的费用，请确认当前明细信息是否是您需要的\r\n";
                        msg += $"在院日期:{出院结算次数.createDate}-{出院结算次数.outDate}";
                        ShowAlert(false, "住院患者信息查询", msg, 30);
                    }
                    inAllDetailModel.CanPrint = printflag;
                }
                inAllDetailModel.Req出院结算明细查询 = new req出院结算明细查询
                {
                    patientHosId = PatientModel.住院患者信息.patientHosId,
                    receiptNo = 出院结算次数.receiptNo,
                };
                inAllDetailModel.Res出院结算明细查询 = DataHandlerEx.出院结算明细查询(inAllDetailModel.Req出院结算明细查询);
                if (inAllDetailModel.Res出院结算明细查询?.success ?? false)
                {
                    if (inAllDetailModel.Res出院结算明细查询.data[0].billItem.Count > 0)
                    {
                        //Next();
                        return true;
                    }
                    else
                    {
                        ShowAlert(false, "住院患者信息查询", "您的出院结算记录为空，不需要打印", 5);
                    }
                }
                else
                {
                    ShowAlert(false, "住院患者信息查询", "查询出院结算记录错误" + inAllDetailModel.Res出院结算明细次数.msg, 10);
                    return false;
                }
            }
            else if (!(inAllDetailModel.Res出院结算明细次数?.success ?? false))
            {
                ShowAlert(false, "住院患者信息查询", "查询出院结算记录错误" + inAllDetailModel.Res出院结算明细次数.msg, 10);
                return false;
            }
            else if (inAllDetailModel.Res出院结算明细次数.data.Count == 0)
            {
                ShowAlert(false, "住院患者信息查询", "您还没有进行结算，无法打印。", 10);
                return false;
            }
            return true;
        }
    }
}
