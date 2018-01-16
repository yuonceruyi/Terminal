using Microsoft.Practices.Unity;
using Prism.Regions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.VirtualHospital.Component.WaiYuan.Models;

namespace YuanTu.VirtualHospital.Component.WaiYuan.ViewModels
{
    public class WaiYuanCardViewModel : ViewModelBase
    {
        public override string Title => "外院卡开通";

        public string Hint => "请插入外院卡";
        protected bool _working;
        private Uri _cardUri;
        public Uri CardUri
        {
            get { return _cardUri; }
            set
            {
                _cardUri = value;
                OnPropertyChanged();
            }
        }

        private Uri _backUri;
        public Uri BackUri
        {
            get { return _backUri; }
            set
            {
                _backUri = value;
                OnPropertyChanged();
            }
        }

        [Dependency]
        public IWaiYuanModel WaiYuanModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        private readonly IRFCardReader _rfCardReader;

        public WaiYuanCardViewModel(IRFCardReader[] rfCardReaders)
        {
            _rfCardReader = rfCardReaders.FirstOrDefault(p => p.DeviceId == "Act_A6_RF");
        }

        public override void OnSet()
        {
            base.OnSet();
            BackUri = ResourceEngine.GetImageResourceUri("插卡口");
            CardUri = ResourceEngine.GetImageResourceUri("卡_诊疗卡");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);

            PlaySound(SoundMapping.请插入就诊卡);
            StartRead();
        }

        protected virtual void StartRead()
        {
            Task.Run(() =>
            {
                string track = null;
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
                                track = BitConverter.ToUInt32(rest.Value, 0).ToString();
                                Logger.Main.Info($"[读取卡号成功][cardNo]{track}");
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
                        OnGetInfo(track);
                    }
                }
            });
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            _working = false;

            //这里会导致重复退卡，封掉这个处理，但是会少一个A6_SetCardIn的操作，测试貌似没有问题
            //_rfCardReader.UnInitialize();
            _rfCardReader?.DisConnect();
            return base.OnLeaving(navigationContext);
        }

        protected virtual void OnGetInfo(string cardNo)
        {
            if (cardNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "病人信息查询", "卡片信息为空，请确认插卡方向是否正确和卡片是否有效");
                StartRead();
                return;
            }
            DoCommand(ctx =>
            {
                var req = new req病人信息查询
                {
                    Timeout = new TimeSpan(0, 1, 0),//读取病人信息设置超时1分钟
                    cardNo = cardNo,
                    cardType = ((int)CardType.就诊卡).ToString(),
                    extend = "1"//先查本院
                };
                var res = DataHandlerEx.病人信息查询(req);
                if (res.success && res.data.Any())//在本院有信息，不需要操作
                {
                    ShowAlert(true, "卡片激活", "您的卡片已经激活!");
                    StartRead();
                    return;
                }

                WaiYuanModel.Req病人信息查询_外院 = req;
                WaiYuanModel.Req病人信息查询_外院.extend = "2";//调整到外院
                var reswy = DataHandlerEx.病人信息查询(req);

                if (reswy.success)
                {
                    if (reswy.data == null || reswy.data.Count == 0)
                    {
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                        StartRead();
                        return;
                    }
                    CardModel.CardNo = cardNo;
                    WaiYuanModel.CardNo = cardNo;
                    WaiYuanModel.病人信息_外院 = reswy.data.First();
                    Next();
                }
                else
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: reswy.msg);
                    StartRead();
                }
            });
        }

        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试卡号");
            if (ret.IsSuccess)
            {
                OnGetInfo(ret.Value);
            }
        }
    }
}