using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Core.Gateway;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.YiWuArea.Insurance.Models;
using YuanTu.YiWuArea.Insurance.Models.Base;
using YuanTu.YiWuFuBao.Component.ViewModels.Models;
using YuanTu.YiWuFuBao.Models;

namespace YuanTu.YiWuFuBao.Component.ViewModels
{
    public class ConfirmViewModel: YuanTu.Default.Component.ViewModels.ConfirmViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            var choice = GetInstance<IChoiceModel>();
            ChangeNavigationContent("");

            LeftList = PaymentModel.LeftList;
            RightList = PaymentModel.RightList;
            MidList = PaymentModel.MidList;

            NoPay = PaymentModel.NoPay;
            ViewTitle = NoPay ? $"请点击确定完成{ChoiceModel.Business}" : "请点击下方卡片选择支付方式";
            Hint = $"{ChoiceModel.Business}信息";

            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();

            var prefix = "PayIn:充值";
            QuickRechargeContent = config.GetValue($"{prefix}:Name") ?? "未定义";
            ImgTxtBtnIconUri = resource.GetImageResourceUri(config.GetValue($"{prefix}:ImageName"));
            CanQuickRecharge = config.GetValue($"{prefix}:Visabled") == "1" && !NoPay;

            var listOut = PayMethodDto.GetInfoPays(config, resource, "PayOut", new DelegateCommand<Info>(Confirm),
                dto =>
                {
                    if (dto.PayMethod == PayMethod.智慧医疗)
                    {
                        dto.IsEnabled = false;

                        if (CardModel.CardType == CardType.社保卡 && choice.Business != Business.建档) //社保卡多一个智慧医疗功能
                        {
                            var insurPatientInfo = (CardModel as CardModel)?.参保人员信息;
                            if (insurPatientInfo != null && insurPatientInfo.IsSuccess)
                            {
                                if (insurPatientInfo.身份验证结果[9] == '0')//诊间结算是否开通 0开通 1未开通
                                {
                                    dto.IsEnabled = true;

                                }
                            }
                            //
                        }
                    }
                    if (dto.PayMethod == PayMethod.支付宝)
                    {
                        dto.Hint = "随机奖励金";
                        dto.Template = "InfoItemPayBubble";
                    }

                });
            PayOut = new ObservableCollection<InfoIcon>(listOut);

            PlaySound();


           
            //var newlst = PayOut.ToList();
            //var insuranceBtn = newlst.FirstOrDefault(p => ((PayMethod)p.Tag) == PayMethod.智慧医疗);
            //if (insuranceBtn != null&&!NoPay)
            //{
            //    insuranceBtn.DisableText = "未开通";
            //    insuranceBtn.IsEnabled = false;
            //    if (CardModel.CardType == CardType.社保卡 && choice.Business != Business.建档) //社保卡多一个智慧医疗功能
            //    {
            //        var insurPatientInfo = (CardModel as CardModel)?.参保人员信息;
            //        if (insurPatientInfo != null && insurPatientInfo.IsSuccess)
            //        {
            //            if (insurPatientInfo.身份验证结果[9] == '0')//诊间结算是否开通 0开通 1未开通
            //            {
            //                insuranceBtn.IsEnabled = true;
                           
            //            }
            //        }
            //        //
            //    }
            //}
            //var alipay = newlst.FirstOrDefault(p => ((PayMethod)p.Tag) == PayMethod.支付宝) as InfoPay;
            //if (alipay != null && !NoPay)
            //{
            //    alipay.HintText = "随机奖励金";
            //    alipay.TemplateKey = "InfoItemPayBubble";
            //}
            //PayOut = new ObservableCollection<InfoIcon>(newlst);
        }

        protected override void Confirm(Info i)
        {
            var td = DataHandlerEx.查询网关状态(new req查询网关状态() {});
           
            var paymethod = (PayMethod) i.Tag;
            if (td.data.platform=="true")
            {
                //临时屏蔽
                if (paymethod == PayMethod.微信支付 || paymethod == PayMethod.支付宝)
                {
                    ShowAlert(false, "暂不支持", "由于平台网络故障，暂不支持该支付方式，请选择“银联”支付！");
                    return;
                }
            }
           
            SecondConfirm(p =>
            {
                if (p)
                {
                    PaymentModel.PayMethod = paymethod;

                    ExtraPaymentModel.CurrentBusiness = ChoiceModel.Business;
                    ExtraPaymentModel.TotalMoney = PaymentModel.Self;
                    ExtraPaymentModel.CurrentPayMethod = PaymentModel.PayMethod;
                    ExtraPaymentModel.Complete = false;
                    var zhuyuanBusiness = new[] {Business.住院押金, Business.出院结算};
                    //准备门诊支付所需病人信息
                    var choice = GetInstance<IChoiceModel>();
                    if (choice.Business == Business.建档)
                    {
                        var idcardModel = GetInstance<IdCardModel>();
                        ExtraPaymentModel.PatientInfo = new PatientInfo
                        {
                            Name = idcardModel.Name, PatientId = "", IdNo = idcardModel.IdCardNo, GuardianNo = "", CardNo = "",CardType = CardType.NoCard, Remain = 0m
                        };
                    }
                    else if (zhuyuanBusiness.Contains(choice.Business))
                    {
                        var zhuyunP = PatientModel.住院患者信息;
                        ExtraPaymentModel.PatientInfo = new PatientInfo
                        {
                            Name = zhuyunP.name,
                            PatientId = zhuyunP.patientHosId,
                            IdNo = zhuyunP.idNo,
                            GuardianNo = "",
                            CardNo = CardModel.CardNo,
                            CardType = CardModel.CardType,
                            Remain = decimal.Parse(zhuyunP.accBalance)
                        };
                    }
                    else
                    {
                        ExtraPaymentModel.PatientInfo = new PatientInfo
                        {
                            Name = PatientModel.当前病人信息.name,
                            PatientId = PatientModel.当前病人信息.patientId,
                            IdNo = PatientModel.当前病人信息.idNo,
                            GuardianNo = PatientModel.当前病人信息.guardianNo,
                            CardNo = CardModel.CardNo,
                            CardType = CardModel.CardType,
                            Remain = decimal.Parse(PatientModel.当前病人信息.accBalance)
                        };
                    }

                    switch (PaymentModel.PayMethod)
                    {
                        case PayMethod.预缴金:
                            var patientModel = PatientModel.当前病人信息;
                            var cardModel = GetInstance<ICardModel>();
                            if (decimal.Parse(patientModel.accBalance) < PaymentModel.Total)
                            {
                                ShowAlert(false, "余额不足", "您的余额不足以支付该次诊疗费用，请充值");
                                return;
                            }
                            var currentPayment = (PaymentModel as Models.PaymentModel);
                            currentPayment.Req预缴金消费 = new req预缴金消费()
                            {
                                cardNo = cardModel.CardNo, cardType = ((int) cardModel.CardType).ToString(), patientName = patientModel.name, cash = currentPayment.Total.ToString("0"),
                            };
                            currentPayment.Res预缴金消费 = DataHandlerEx.预缴金消费(currentPayment.Req预缴金消费);
                            if (currentPayment.Res预缴金消费.success)
                            {
                                PaymentModel.ConfirmAction?.BeginInvoke(cp =>
                                {
                                    var rest = PaymentModel.ConfirmAction?.EndInvoke(cp);
                                    if (rest?.IsSuccess ?? false)
                                        ChangeNavigationContent(i.Title);
                                    else
                                    {
                                        currentPayment.Req预缴金消费冲正 = new req预缴金消费冲正()
                                        {
                                            sFlowId = currentPayment.Res预缴金消费.data.sFlowId, cash = currentPayment.Req预缴金消费.cash, orderId = currentPayment.Res预缴金消费.data.orderId,
                                        };
                                        currentPayment.Res预缴金消费冲正 = DataHandlerEx.预缴金消费冲正(currentPayment.Req预缴金消费冲正);
                                        if (!currentPayment.Res预缴金消费冲正.success)
                                        {
                                            //todo: 考虑冲正失败打印单边账凭条
                                        }
                                        // ShowAlert(false,"消费失败",rest?.Message);
                                    }
                                }, null);
                            }
                            else
                            {
                                ShowAlert(false, "交易失败", currentPayment.Res预缴金消费.msg);
                            }

                            break;

                        case PayMethod.银联:
                            ExtraPaymentModel.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                            GoToUnion();
                            break;

                        case PayMethod.支付宝:
                            ExtraPaymentModel.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                            Navigate(A.Third.ScanQrCode);
                            break;

                        case PayMethod.微信支付:
                            ExtraPaymentModel.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                            Navigate(A.Third.ScanQrCode);
                            break;

                        case PayMethod.苹果支付:
                            ShowAlert(false, "支付确认", "业务未实现");
                            break;
                        case PayMethod.智慧医疗:
                            PaymentModel.ConfirmAction?.BeginInvoke(cbk =>
                            {
                                
                            },null);
                            break;
                    }
                }
            });
        }

        protected override void SecondConfirm(Action<bool> act)
        {
            act.Invoke(true);
            //if (ChoiceModel.Business == Business.建档)
            //{
            //    act.Invoke(true);
            //    return;
            //}
            //base.SecondConfirm(act);
        }
    }
}
