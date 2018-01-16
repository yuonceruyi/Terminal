using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Devices.CardReader;
using System;
using System.Text;
using YuanTu.Core.Extension;
using YuanTu.Consts.Gateway;

namespace YuanTu.HuNanHangTianHospital.Component.Auth.ViewModels
{
    public class CardViewModel : YuanTu.Default.Component.Auth.ViewModels.CardViewModel
    {

        private static readonly byte[] _keyA = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders) : base(rfCardReaders, magCardReaders)
        {

        }

        protected override void StartRead()
        {
            Task.Run(() => StartRF());
        }

        protected override void StartRF()
        {
            string cardNo = null;
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
                        var rest = _rfCardReader.ReadBlock(2, 0, true, _keyA);
                        if (rest.IsSuccess)
                        {
                            byte[] bCardNo = new byte[16];
                            Array.Copy(rest.Value, bCardNo, 16);
                            cardNo = Encoding.ASCII.GetString(bCardNo,0,10);
                            if (cardNo.Contains("\0"))
                            {
                                cardNo = cardNo.Substring(0, cardNo.IndexOf("\0"));
                            }
                            Logger.Main.Info($"[读取卡号成功][cardNo]{cardNo}");
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
                {
                    OnGetInfo(cardNo);
                }
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
                    cardNo = cardNo,
                    cardType = ((int)CardModel.CardType).ToString()
                };
                PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
                if (PatientModel.Res病人信息查询.success)
                {
                    if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                    {
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                        StartRead();
                        return;
                    }
                    PatientModel.Res病人信息查询.data.First().guardianNo = null;
                    CardModel.CardNo = cardNo;
                    Next();
                }
                else
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
                    StartRead();
                }
            });
        }
    }
}
