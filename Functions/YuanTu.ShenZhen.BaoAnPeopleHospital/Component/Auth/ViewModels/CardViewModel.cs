using System;
using System.Text;
using System.Threading;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.ShenZhenArea.Models;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Component.Auth.ViewModels
{
    public class CardViewModel : Default.Component.Auth.ViewModels.CardViewModel
    {
        private static readonly byte[] KeyA = {0xff, 0xff, 0xff, 0xff, 0xff, 0xff};

        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
            : base(rfCardReaders, magCardReaders)
        {
        }

        protected override void StartRF()
        {
            string cardNo = null;
            string secrityNo = null;
            try
            {
                var ret = _rfCardReader.Connect();
                if (!ret.IsSuccess)
                {
                    ReportService.读卡器离线(null, ErrorSolution.读卡器离线);
                    ShowAlert(false, "友好提示", $"读卡器打开失败({ret.ResultCode})", debugInfo: ret.Message);
                    return;
                }
                if (!_rfCardReader.Initialize().IsSuccess)
                {
                    ShowAlert(false, "友好提示", $"读卡器初始化失败({ret.ResultCode})", debugInfo: ret.Message);
                    return;
                }
                _working = true;
                while (_working)
                {
                    var pos = _rfCardReader.GetCardPosition();
                    if (pos.IsSuccess && pos.Value == CardPos.停卡位)
                    {
                        //读取2扇区2块数据(卡号)
                        var rest = _rfCardReader.ReadBlock(2, 2, true, KeyA);
                        if (rest.IsSuccess)
                        {
                            var bCardNo = new byte[16];
                            Array.Copy(rest.Value, bCardNo, 16);
                            cardNo =bCardNo.ByteToString();
                            if ((!string.IsNullOrEmpty(cardNo)) && cardNo.Length >12)
                                cardNo = cardNo.Substring(0, 12);
                            Logger.Main.Info($"[读取卡号成功][cardNo]{cardNo}");
                        }
                        //读取3扇区0块数据(验证码)
                        rest = _rfCardReader.ReadBlock(3, 0, true, KeyA);
                        if (rest.IsSuccess)
                        {
                            var bSecrityNo = new byte[16];
                            Array.Copy(rest.Value, bSecrityNo, 16);
                            secrityNo = bSecrityNo.ByteToString().Replace("0", "");
                            Logger.Main.Info($"[读取验证码成功][secrityNo]{secrityNo}");
                        }
                        break;
                    }
                    Thread.Sleep(300);
                }
            }
            finally
            {
                _rfCardReader.UnInitialize();
                if (_working)
                    OnGetInfo(cardNo, secrityNo);
            }
        }


        protected override void OnGetInfo(string cardNo, string extendInfo = null)
        {
            if (cardNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "病人信息查询", "卡片信息为空，请确认插卡方向是否正确和卡片是否有效");
                StartRead();
                return;
            }
            DoCommand(ctx =>
            {
                PatientModel.Req病人信息查询 = new req病人信息查询
                {
                    Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                    cardNo = cardNo,
                    cardType = ((int) CardModel.CardType).ToString(),
                    secrityNo = extendInfo
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
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                        StartRead();
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(PatientModel.当前病人信息.idNo) && string.IsNullOrEmpty(PatientModel.当前病人信息.guardianNo))
                    {
                        ShowAlert(false, "病人信息查询", PatientModel.当前病人信息.name + "未登记身份证号或监护人身份证号进行实名认证");
                        Navigate(A.Home);
                        return; 
                    }

                    CardModel.CardNo = cardNo;
                    var cm = (CardModel as ShenZhenCardModel);
                    cm.RealCardType = CardType.就诊卡;
                    Next();
                }
                else
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
                    StartRead();
                }
            });
        }
        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试卡号");
            if (!ret.IsSuccess)
                return;
            var list = ret.Value.Replace("\r\n", "\n").Split('\n');
            if (list.Length < 2)
                return;
                
            OnGetInfo(list[0], list[1]);
        }
    }
}