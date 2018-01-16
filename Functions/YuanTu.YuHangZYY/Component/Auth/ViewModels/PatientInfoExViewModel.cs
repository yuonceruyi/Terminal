using System;
using System.Linq;
using YuanTu.Consts;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;

namespace YuanTu.YuHangZYY.Component.Auth.ViewModels
{
    public class PatientInfoExViewModel : Default.Component.Auth.ViewModels.PatientInfoExViewModel
    {
        public PatientInfoExViewModel(IRFCardDispenser[] rfCardDispenser, IMagCardDispenser[] magCardDispenser)
            : base(rfCardDispenser)
        {
            _magCardDispenser = magCardDispenser?.FirstOrDefault(p => p.DeviceId == "Act_F6_Mag");
        }

        protected override bool GetNewCardNo()
        {
            try
            {
                if (FrameworkConst.DoubleClick && FrameworkConst.VirtualHardWare)
                    return View.Dispatcher.Invoke(() =>
                    {
                        var ret = InputTextView.ShowDialogView("输入物理卡号");
                        if (ret.IsSuccess)
                        {
                            CardModel.CardNo = ret.Value;
                            return true;
                        }
                        return false;
                    });

                if (!_magCardDispenser.Connect().IsSuccess)
                {
                    ReportService.发卡器离线(null, ErrorSolution.发卡器离线);
                    ShowAlert(false, "建档发卡", "发卡机连接失败");
                    return false;
                }
                if (!_magCardDispenser.Initialize().IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机初始化失败");
                    return false;
                }
                var result = _magCardDispenser.EnterCard(TrackRoad.Trace2);
                if (!result.IsSuccess)
                {
                    //读卡失败时,回收卡片重新发卡
                    if (!_magCardDispenser.MoveCard(CardPosF6.吞入, "发卡机读卡号失败，故回收卡片").IsSuccess)
                    {
                        ShowAlert(false, "建档发卡", "发卡机读卡号失败后回收卡失败");
                        return false;
                    }
                    GetNewCardNo();
                }
                CardModel.CardNo = result.Value[TrackRoad.Trace2];
                return true;
            }
            catch (Exception ex)
            {
                ShowAlert(false, "建档发卡", "发卡机读卡失败");
                Logger.Main.Error($"[建档发卡]{ex.Message + ex.StackTrace}");
                return false;
            }
        }

        protected override void PrintCard()
        {
            _magCardDispenser?.MoveCardOut();
        }
    }
}