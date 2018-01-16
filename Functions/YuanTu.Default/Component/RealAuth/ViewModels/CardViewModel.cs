using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
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

namespace YuanTu.Default.Component.RealAuth.ViewModels
{
    public class CardViewModel : ViewModelBase
    {
        protected IMagCardReader _magCardReader;
        protected IRFCardReader _rfCardReader;

        protected bool _working;

        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
        {
            _rfCardReader = rfCardReaders?.FirstOrDefault(p => p.DeviceId == "Act_A6_RF");
            _magCardReader = magCardReaders?.FirstOrDefault(p => p.DeviceId == "ACT_A6_Mag&IC");
        }

        public override string Title => "插入就诊卡";

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        #region Bindings

        private string _hint = "请按提示插卡";
        public string Hint
        {
            get
            { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }
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

        private string _tipContent = "插卡时卡面朝上";
        public string TipContent
        {
            get { return _tipContent; }
            set
            {
                _tipContent = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public override void OnSet()
        {
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
            Task.Run(() => StartRF());
        }

        protected virtual void StopRead()
        {
            StopRF();
        }

        protected virtual void StartRF()
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
                    OnGetInfo(track);
            }
        }

        protected virtual void StopRF()
        {
            //这里会导致重复退卡，封掉这个处理，但是会少一个A6_SetCardIn的操作，测试貌似没有问题
            //_rfCardReader.UnInitialize();
            _rfCardReader?.DisConnect();
        }

        protected virtual void StartMag()
        {
            string track = null;
            try
            {
                var ret = _magCardReader.Connect();
                if (!ret.IsSuccess)
                {
                    ReportService.读卡器离线(null, ErrorSolution.读卡器离线);
                    ShowAlert(false, "友好提示", $"读卡器打开失败({ret.ResultCode})", debugInfo: ret.Message);
                    return;
                }
                if (!_magCardReader.Initialize().IsSuccess)
                {
                    ShowAlert(false, "友好提示", $"读卡器初始化失败({ret.ResultCode})", debugInfo: ret.Message);
                    return;
                }
                _working = true;
                while (_working)
                {
                    var pos = _magCardReader.GetCardPosition();
                    if (pos.IsSuccess && pos.Value == CardPos.停卡位)
                    {
                        var rest = _magCardReader.ReadTrackInfos(TrackRoad.Trace2, ReadType.ASCII);
                        if (rest.IsSuccess)
                        {
                            track = rest.Value[TrackRoad.Trace2];
                            Logger.Main.Info($"[读取卡号成功][cardNo]{track}");
                        }
                        break;
                    }
                    Thread.Sleep(300);
                }
            }
            finally
            {
                _magCardReader.UnInitialize();
                if (_working)
                {
                    OnGetInfo(track);
                }
            }
        }

        protected virtual void StopMag()
        {
            _magCardReader?.DisConnect();
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            _working = false;

            StopRead();
            return base.OnLeaving(navigationContext);
        }

        protected virtual void OnGetInfo(string cardNo, string extendInfo = null)
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

                    //todo 临时卡校验
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

        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试卡号");
            if (ret.IsSuccess)
                OnGetInfo(ret.Value);
        }
    }
}