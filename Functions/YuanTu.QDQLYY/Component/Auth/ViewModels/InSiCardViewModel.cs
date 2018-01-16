using Prism.Regions;
using System;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.Devices.CardReader;
using YuanTu.QDArea.QingDaoSiPay;

namespace YuanTu.QDQLYY.Component.Auth.ViewModels
{
    public class InSiCardViewModel : YuanTu.QDKouQiangYY.Component.Auth.ViewModels.SiCardViewModel
    {
        public InSiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {

        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            myAction = new Action<LoadingProcesser>(Command);
        }
        public void Command(LoadingProcesser ctx)
        {
            PatientModel.Req住院患者信息查询 = new req住院患者信息查询
            {
                cardType = ((int)CardModel.CardType).ToString(),
                cardNo = idNo,
                patientName = name
            };
            var res = DataHandlerEx.住院患者信息查询(PatientModel.Req住院患者信息查询);
            if (res?.success ?? false)
            {
                if (res?.data == null)
                {
                    ShowAlert(false, "住院患者信息查询", "查询患者信息失败");
                    StartRead();
                    return;
                }
                ChangeNavigationContent($"{idNo}");
                PatientModel.Res住院患者信息查询 = res;
                Next();
            }
            else
            {
                ShowAlert(false, "住院患者信息查询", "查询患者信息失败" + res?.msg);
                StartRead();
            }
        }
        public override void StartReadA6HuaDa()
        {
            if (!CheckSiCard())
            {
                ShowAlert(false, "医保读卡", "未检测到社保卡片，请确认朝向是否正确，重新插入社保卡", extend: new AlertExModel()
                {
                    HideCallback = tp =>
                    {
                        if (tp == AlertHideType.ButtonClick)
                        {
                            if (NavigationEngine.State == A.ZY.HICard) //确定还在当前页面
                            {
                                _rfCpuCardReader.MoveCard(CardPos.不持卡位);
                                StartRead();
                            }
                        }
                    }
                });
                return;
            }
            name = string.Empty;
            var sign = Function.ReadCard(ref idNo, ref name);
            if (sign != 0)
            {
                Logger.Device.Info("医保卡 读卡失败");
                ShowAlert(false, "医保读卡", "医保读卡失败" + Function.ErrMsg, extend: new AlertExModel()
                {
                    HideCallback = tp =>
                    {
                        if (tp == AlertHideType.ButtonClick)
                        {
                            if (NavigationEngine.State == A.ZY.HICard) //确定还在当前页面
                            {
                                _rfCpuCardReader.MoveCard(CardPos.不持卡位);
                                StartRead();
                            }
                        }
                    }
                });
                return;
            }
            DoCommand(myAction);
        }

    }
}