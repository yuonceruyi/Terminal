using Microsoft.Practices.Unity;
using System;
using System.Threading;
using YuanTu.Consts;
using YuanTu.Devices.CardReader;
using YuanTu.XiaoShanArea.CitizenCard;
using YuanTu.XiaoShanHealthStation.Component.Auth.Models;
using YuanTu.XiaoShanArea.Consts.Enums;
using YuanTu.XiaoShanArea.Consts.Extensions;
using YuanTu.XiaoShanArea.CYHIS.DLL;

namespace YuanTu.XiaoShanHealthStation.Component.Auth.ViewModels
{
    public class SiCardViewModel : Default.Component.Auth.ViewModels.SiCardViewModel
    {
        [Dependency]
        public IChaKaModel ChaKaModel { get; set; }

        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {
        }

        public override void Confirm()
        {
            DoCommand(lp =>
            {

                lp.ChangeText("正在读卡，请稍候...");
                try
                {
                    //todo 读市民卡 健康卡
                    var req = new Req读接触非接卡号
                    {
                        transCode = 1004,
                        amount = 0,
                    };
                    var result = DataHandlerEx.Query(req);
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", result.Message);
                        return false;
                    }
                    ChaKaModel.Res读接触非接卡号 = Res读接触非接卡号.Deserilize(result.Value.dest);
                    ChaKaModel.IsCitizenCard = ChaKaModel.Res读接触非接卡号.卡类型 != "9";
                    ChaKaModel.CardType = ChaKaModel.Res读接触非接卡号.卡类型;
                    if (ChaKaModel.IsCitizenCard && ChaKaModel.Res读接触非接卡号.卡号.Length == 8)
                    {
                        ShowAlert(false, "温馨提示", "请检查您的卡位置是否正确\n\n请按提示正确插卡\n\n芯片朝下 向前插入");
                        return false;
                    }
                    if (!ChaKaModel.IsCitizenCard)
                    {
                        ChaKaModel.CardNo = ("801" + ChaKaModel.Res读接触非接卡号.卡类型 + ChaKaModel.Res读接触非接卡号.卡号).Trim(' ');
                        ChaKaModel.PatientType = PatientTypeParse.CY_PatintTypePare("健康卡");
                    }
                    else
                    {
                        ChaKaModel.CardNo = ChaKaModel.Res读接触非接卡号.卡号.Trim(' ');
                        ChaKaModel.PatientType =
                            PatientTypeParse.CY_PatintTypePare(
                                PatientTypeParse.SMK_PatintTypePare(ChaKaModel.Res读接触非接卡号.卡识别码.Substring(0, 6)));
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    if (ex.Message == ErrorCodeParse.ErrorParse(5))
                    {
                        ShowAlert(false, "温馨提示", "请先将卡插入读卡器之后\n再点击确定按钮读卡");
                    }
                    else
                    {
                        ShowAlert(false, "温馨提示", "读卡异常:" + ex.Message);
                    }
                    return false;
                }


            }).ContinueWith(ctx =>
            {
                if(ctx.Result)
                    ValidateCard();
            });

        }

        protected virtual void ValidateCard()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询信息，请稍候...");
                if (ChaKaModel.IsCitizenCard)
                {
                    //TODO 查询市民卡
                    var req = new Req可扣查询_SMK
                    {
                        transCode = 81025,
                        amount = 0,
                        卡号 = ChaKaModel.Res读接触非接卡号?.卡号,
                        卡类型 = CardType.市民卡.ToString()
                    };
                    var result = DataHandlerEx.Query(req);
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", result.Message);
                        return;
                    }
                    ChaKaModel.Res可扣查询Smk = Res可扣查询_SMK.Deserilize(result.Value.dest);
                    ChaKaModel.Remain = decimal.Parse(ChaKaModel.Res可扣查询Smk?.返回金额 ?? "0");
                    GetAccountStatus(result.Value.ret, ChaKaModel.Res可扣查询Smk?.应答码);
                }
                else
                {
                    //TODO 查询健康卡
                    var req = new Req可扣查询_JKK
                    {
                        transCode = 81025,
                        amount = 0,
                        卡号 = ChaKaModel.CardNo.FillPadChar(12),
                        卡类型 = CardType.健康卡.ToString()
                    };
                    var result = DataHandlerEx.Query(req);
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", result.Message);
                        return;
                    }
                    ChaKaModel.Res可扣查询Jkk = Res可扣查询_JKK.Deserilize(result.Value.dest);
                    ChaKaModel.Remain = decimal.Parse(ChaKaModel.Res可扣查询Jkk?.返回金额 ?? "0");
                    GetAccountStatus(result.Value.ret, ChaKaModel.Res可扣查询Jkk?.应答码);
                }
                //TODO HIS信息查询
                var req人员信息 = new Req查询建档
                {
                    卡号 = ChaKaModel.CardNo.Trim(),
                    病人类别 = ChaKaModel.PatientType,
                    操作工号 = FrameworkConst.OperatorId,

                };
                查询建档_OUT 查询建档Out;
                if (!DataHandler.查询建档(req人员信息, out 查询建档Out))
                {
                    ShowAlert(false, "温馨提示", "查询医院病人信息失败");
                    return;
                }
                ChaKaModel.查询建档Out = 查询建档Out;
                //Thread.Sleep(2000);
                Next();
            });
           
        }

        protected virtual void GetAccountStatus(decimal retValue,string retCode)
        {
            ChaKaModel.HasSmartHealth = retValue==0;
            ChaKaModel.HasAccount = retCode == "00";
        }
    }
}