using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;
using YuanTu.Devices.PrinterCheck;

namespace YuanTu.QDQLYY.Component.ViewModels
{
    public class ChoiceViewModel : YuanTu.QDKouQiangYY.Component.ViewModels.ChoiceViewModel
    {

        protected override void Do(ChoiceButtonInfo param)
        {
            Result result;
            var choiceModel = GetInstance<IChoiceModel>();

            choiceModel.Business = param.ButtonBusiness;
            choiceModel.HasAuthFlow = true; //默认有插卡流程
            var engine = NavigationEngine;

            if (FrameworkConst.DeviceType != "YT-740")
            {
                result = GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
            }
            bool FindedBusiness = false;
            switch (param.ButtonBusiness)
            {
                case Business.科室信息:
                    FindedBusiness = true;
                    choiceModel.HasAuthFlow = false;
                    engine.JumpAfterFlow(null,
                      CreateJump,
                      new FormContext(AInner.DeptInfo_Context, AInner.KSXX.DeptList), param.Name);
                    break;
                case Business.出院清单打印:
                    FindedBusiness = true;
                    //choiceModel.HasAuthFlow = false;
                    choiceModel.AuthContext = A.ZhuYuan_Context;
                    NavigationEngine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.Choice), CreateJump,
                        new FormContext(AInner.CYQD_Context, AInner.CYQD.FeeList), param.Name);
                    break;
                default:
                    break;
            }
            if (!FindedBusiness)
            {
                base.Do(param);
            }

        }
        protected override void OnInRecharge(ChoiceButtonInfo param)
        {
            var choiceModel = GetInstance<IChoiceModel>();
            //choiceModel.HasAuthFlow = false;
            choiceModel.AuthContext = A.ZhuYuan_Context;
            NavigationEngine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.Choice), this.IpRechargeJump,
                new FormContext(A.IpRecharge_Context, A.ZYCZ.RechargeWay), param.Name);
        }
    }
}
