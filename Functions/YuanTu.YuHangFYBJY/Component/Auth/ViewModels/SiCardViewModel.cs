using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Devices.CardReader;
using YuanTu.YuHangArea.CitizenCard;
using YuanTu.YuHangFYBJY.Component.Auth.Models;
using DataHandlerEx = YuanTu.Consts.Gateway.DataHandlerEx;

namespace YuanTu.YuHangFYBJY.Component.Auth.ViewModels
{
    public class SiCardViewModel : Default.Component.Auth.ViewModels.SiCardViewModel
    {
        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders)
            : base(icCardReaders, rfCpuCardReaders)
        {
        }

        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("读卡器_明华");

        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            CardUri = ResourceEngine.GetImageResourceUri("卡_社保卡");
        }

        public override void Confirm()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在读卡，请稍候...");

                var cm = CardModel as CardModel;
                string cardNo = null;
                CardModel.ExternalCardInfo = null;
                if (CardModel.CardType == CardType.社保卡)
                {
                    try
                    {
                        var req = new Req读接触卡号
                        {
                            amount = 0,
                        };
                        var result = YuHangArea.CitizenCard.DataHandlerEx.Query(req);
                        if (!result.IsSuccess)
                        {
                            ShowAlert(false, "温馨提示", result.Message);
                            return;
                        }
                        cm.RealCardType = CardType.社保卡;
                        cm.Res读接触卡号 = Res读接触卡号.Deserilize(result.Value.dest);
                        cardNo = cm.Res读接触卡号.卡号;
                        CardModel.CardType = CardType.社保卡;
                    }
                    catch (Exception ex)
                    {
                        Logger.Device.Error($"[市民卡Mispos]读卡异常:{ex.Message} {ex.StackTrace}");
                        ShowAlert(false, "读卡失败", "读卡时发生异常", debugInfo: ex.Message);
                    }

                }
                else
                {

                    //todo 读市民卡 健康卡
                    var req = new Req读非接触卡号
                    {
                        transCode = 1001,
                        amount = 0,
                    };
                    var result = YuHangArea.CitizenCard.DataHandlerEx.Query(req);
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", result.Message);
                        return;
                    }

                    cm.Res读非接触卡号 = Res读非接触卡号.Deserilize(result.Value.dest);
                    var incardNo = cm.Res读非接触卡号.卡号;
                    cardNo = $"8019{incardNo.SafeSubstring(incardNo.Length - 8, 8)}";
                    cm.RealCardType = CardType.居民健康卡;
                    CardModel.CardType = CardType.社保卡;
                }



                lp.ChangeText("正在查询病人信息，请稍候...");
                PatientModel.Req病人信息查询 = new req病人信息查询
                {
                    Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                    cardNo = cardNo,
                    cardType = ((int)CardModel.CardType).ToString()
                };
                PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

                if (PatientModel.Res病人信息查询.success)
                {
                    if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                    {
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                        Preview();
                        return;
                    }
                    CardModel.CardNo = PatientModel.当前病人信息.cardNo;//卡号强制从平台返回
                    Next();
                }
                else
                {
                    //if (cm.RealCardType == CardType.社保卡)
                    //{
                    //    ShowConfirm("信息补全", "您的身份信息不完整，需要补全身份信息", bl =>
                    //    {
                    //        if (!bl)
                    //        {
                    //            Navigate(A.Home);
                    //            return;
                    //        }
                    //        CardModel.ExternalCardInfo = "社保_信息补全";
                    //        Navigate(A.CK.IDCard);

                    //    }, extend: ConfirmExModel.Build("立即补全信息", "去人工窗口补全", false));
                    //}
                    //else
                    //{
                    //    ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
                    //    Preview();
                    //}
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
                    Preview();
                }



            });
        }
    }
}
