using Prism.Regions;
using System;
using System.Linq;
using System.Threading;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;


namespace YuanTu.ShenZhen.BaoAnChineseMedicineHospital.Clinic.Component.Auth.ViewModels
{
    public class CardViewModel : BaoAnChineseMedicineHospital.Component.Auth.ViewModels.CardViewModel
    {
        private static readonly byte[] KeyA = { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
            : base(rfCardReaders, magCardReaders)
        {
            _rfCardReader = rfCardReaders?.FirstOrDefault(p => p.DeviceId == "HuaDa_RF");
        }

        public override string Title => "刷就诊卡";

        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("就诊卡扫描处");
            CardUri = ResourceEngine.GetImageResourceUri("卡_诊疗卡");
            TipContent = "请在磁卡感应区刷卡";
            Hint = "请按提示刷卡";
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            PlaySound(SoundMapping.请刷取您的就诊卡);
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

                        var rest = _rfCardReader.GetCardId();
                        if (rest.IsSuccess)
                        {
                            Logger.Main.Info($"[宝安中医院诊间屏华大读卡序号成功][返回原始数据]{string.Join(" ", rest.Value)}");
                            var trackNo = BitConverter.ToUInt32(rest.Value, 0).ToString();  //读到有卡
                            Logger.Main.Info($"[宝安中医院诊间屏华大读取GetCardId成功][trackNo]{trackNo}");
                        }


                        //读取2扇区2块数据(卡号)
                        rest = _rfCardReader.ReadBlock(2, 2, true, KeyA);
                        if (rest.IsSuccess)
                        {
                            Logger.Main.Info($"[宝安中医院诊间屏华大读卡号成功][返回原始数据]{string.Join(" ", rest.Value)}");
                            var bCardNo = new byte[16];
                            Array.Copy(rest.Value, bCardNo, 16);
                            cardNo = bCardNo.ByteToString();
                            if ((!string.IsNullOrEmpty(cardNo)) && cardNo.Length > 12)
                                cardNo = cardNo.Substring(0, 12);
                            Logger.Main.Info($"[宝安中医院诊间屏华大读取卡号成功][cardNo]{cardNo}");
                        }

                        //读取3扇区0块数据(验证码)
                        rest = _rfCardReader.ReadBlock(3, 0, true, KeyA);
                        if (rest.IsSuccess)
                        {
                            Logger.Main.Info($"[宝安中医院诊间屏华大读验证码成功][返回原始数据]{string.Join(" ", rest.Value)}");
                            var bSecrityNo = new byte[16];
                            Array.Copy(rest.Value, bSecrityNo, 16);
                            secrityNo = bSecrityNo.ByteToString().Replace("0", "");
                            Logger.Main.Info($"[宝安中医院诊间屏华大读取验证码成功][secrityNo]{secrityNo}");
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
    }
}