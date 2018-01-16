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
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Default.Component.Tools.Models;

namespace YuanTu.YuHangZYY.Component.ViewModels
{
    public class ConfirmViewModel:YuanTu.Default.Component.ViewModels.ConfirmViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            //建档不能快捷充值
            if (ChoiceModel.Business==Business.建档)
            {
                CanQuickRecharge = false;
            }
            var pmodel = PaymentModel as Models.PaymentModel;
            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();
            var listOut = PayMethodDto.GetInfoPays(config, resource, "PayOut", new DelegateCommand<Info>(Confirm),
                dto =>
                {
                    if (dto.PayMethod==PayMethod.智慧医疗)
                    {
                        if (dto.IsEnabled)//配置上是启用状态的
                        {
                            if (CardModel.CardType!=CardType.社保卡)
                            {
                                dto.Visabled = false;
                               // dto.Hint = "余额不足";
                            }
                            else if (pmodel.CitizenBlance < pmodel.Self)//智慧医疗金额不足
                            {
                                dto.IsEnabled = false;
                                dto.Hint = "余额不足";
                            }
                           
                        }
                    }
                    if (dto.PayMethod == PayMethod.预缴金)
                    {
                        if (dto.IsEnabled&& ChoiceModel.Business == Business.建档)
                        {
                            dto.IsEnabled = false;
                        }
                    }
                });
            //var btns = new ObservableCollection<InfoIcon>(listOut);
            var i = listOut.FirstOrDefault(p => (PayMethod)p.Tag == PayMethod.预缴金);
            if (i!=null
                && navigationContext.Parameters["FromContext"].ToString()==A.ChongZhi_Context
                && navigationContext.Parameters["From"].ToString()==A.CZ.Print

                )//哥从充值回来滴
            {
                PayOut=new ObservableCollection<InfoIcon>();//清空支付方式好不好？
                StartPayWithOC(i);
            }
            else
            {
                PayOut = new ObservableCollection<InfoIcon>(listOut);
            }
        }


        private void StartPayWithOC(Info button)
        {
            
            var payMethod = PayMethod.预缴金;
            PaymentModel.PayMethod = payMethod;

            ExtraPaymentModel.CurrentBusiness = ChoiceModel.Business;
            ExtraPaymentModel.TotalMoney = PaymentModel.Self;
            ExtraPaymentModel.CurrentPayMethod = payMethod;
            ExtraPaymentModel.Complete = false;
            //准备门诊支付所需病人信息

            ExtraPaymentModel.PatientInfo = new PatientInfo
            {
                Name = PatientModel.当前病人信息.name,
                PatientId = PatientModel.当前病人信息.patientId,
                IdNo = PatientModel.当前病人信息.idNo,
                GuardianNo = PatientModel.当前病人信息.guardianNo,
                CardNo = CardModel.CardNo,
                Remain = decimal.Parse(PatientModel.当前病人信息.accBalance),
                CardType = CardModel.CardType,
            };

            HandlePaymethod(button, payMethod);
        }

        protected override void HandlePaymethod(Info i, PayMethod payMethod)
        {
            if (payMethod== PayMethod.智慧医疗)
            {
                PaymentModel.ConfirmAction?.BeginInvoke(cp =>
                {
                    var rest = PaymentModel.ConfirmAction?.EndInvoke(cp);
                    if (rest?.IsSuccess ?? false)
                        ChangeNavigationContent(i.Title);
                }, null);
            }
            else
            {
            base.HandlePaymethod(i, payMethod);

            }

        }
    }
}
