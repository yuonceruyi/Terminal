using Microsoft.Practices.Unity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.Log;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Devices.Common;
using YuanTu.ShengZhouHospital.HisNative.Models;

namespace YuanTu.ShengZhouHospital.Component.ViewModels
{
    public class ConfirmViewModel : YuanTu.Default.Component.ViewModels.ConfirmViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            CanQuickRecharge = false;
            Hint = (int)ChoiceModel.Business == 100 ? "体检缴费信息" : $"{ChoiceModel.Business}信息";
        }
        [Dependency]
        public IIdCardModel IdCardModel { get; set; }

        protected override void Confirm(Info i)
        {

            PaymentModel.PayMethod = (PayMethod)i.Tag;
            ExtraPaymentModel.CurrentBusiness = ChoiceModel.Business;
            ExtraPaymentModel.TotalMoney = PaymentModel.Self;
            ExtraPaymentModel.CurrentPayMethod = PaymentModel.PayMethod;
            ExtraPaymentModel.Complete = false;
            ExtraPaymentModel.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
            //准备门诊支付所需病人信息
            if (ChoiceModel.Business == Business.建档)
            {
                ExtraPaymentModel.PatientInfo = new PatientInfo
                {
                    Name = IdCardModel.Name,
                    PatientId = IdCardModel?.IdCardNo,//嵊州项目 建档的时候 先选择使用身份证号代替PatientId
                    IdNo = IdCardModel.IdCardNo,
                    GuardianNo = null,
                    CardNo = CardModel.CardNo,
                    CardType = CardModel.CardType,
                    Remain = 0
                };
                switch (PaymentModel.PayMethod)
                {
                    case PayMethod.银联:
                        GoToUnion();
                        break;

                    case PayMethod.支付宝:
                        Navigate(A.Third.ScanQrCode);
                        break;

                    case PayMethod.微信支付:
                        Navigate(A.Third.ScanQrCode);
                        break;
                }
            }
            else
            {
                CheckSamePatient().ContinueWith(ret =>
                {
                    if (ret.Result.IsSuccess)
                    {

                        ExtraPaymentModel.PatientInfo = new PatientInfo
                        {
                            Name = PatientModel?.当前病人信息?.name,
                            PatientId = PatientModel?.当前病人信息?.patientId,
                            IdNo = PatientModel?.当前病人信息?.idNo,
                            GuardianNo = PatientModel?.当前病人信息.guardianNo,
                            CardNo = CardModel?.CardNo,
                            CardType = CardModel.CardType,
                            Remain = decimal.Parse(PatientModel?.当前病人信息.accBalance)
                        };
                        switch (PaymentModel.PayMethod)
                        {
                            case PayMethod.银联:
                                GoToUnion();
                                break;

                            case PayMethod.支付宝:
                                Navigate(A.Third.ScanQrCode);
                                break;

                            case PayMethod.微信支付:
                                Navigate(A.Third.ScanQrCode);
                                break;
                        }
                    }
                    else
                    {
                        ShowAlert(false, "病人信息对比失败", ret.Result.Message, 10
                            , extend: new AlertExModel()
                            {
                                HideCallback = ap =>
                                {
                                    Navigate(A.Home);
                                }
                            });
                    }

                });

            }



        }

        protected override void NoPayConfirm()
        {
            CheckSamePatient().ContinueWith(ret =>
            {
                if (ret.Result.IsSuccess)
                {
                    base.NoPayConfirm();
                }
                else
                {
                    ShowAlert(false, "病人信息对比失败", ret.Result.Message, 10
                        , extend: new AlertExModel()
                        {
                            HideCallback = ap =>
                            {
                                Navigate(A.Home);
                            }
                        });
                }
            });

        }


        private Task<Result> CheckSamePatient()
        {
            return DoCommand(lp =>
            {
                if (ChoiceModel.Business == (Business)100)
                {
                    return Result.Success();
                }
                lp.ChangeText("正在确认病人信息");
                var req = new Req门诊读卡() { };
                req.卡类别 = ConstInner.CardTypeMapping[CardModel.CardType].ToString();//医保
                var res = HisHandleEx.执行门诊读卡(req);
                if (res.IsSuccess)
                {
                    if (res.卡号 == CardModel.CardNo)
                    {
                        return Result.Success();
                    }
                    return Result.Fail("当前病人信息已变更，请重新操作！");
                }
                return Result.Fail("读卡失败，请重新操作！");
            });
        }
    }
}
