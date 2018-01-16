using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Tablet.Component.Cashier.Models;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;

namespace YuanTu.Default.Tablet.Component.Cashier.ViewModels
{
    internal class CardViewModel : ViewModelBase
    {
        protected IMagCardReader _magCardReader;
        protected IRFCardReader _rfCardReader;

        protected bool _working;

        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
        {
            _rfCardReader = rfCardReaders?.FirstOrDefault(p => p.DeviceId == "HuaDa_RF");
            _magCardReader = magCardReaders?.FirstOrDefault(p => p.DeviceId == "ACT_A6_Mag&IC");
        }

        public override string Title => "放上就诊卡";

        [Dependency]
        public ICashierModel Cashier { get; set; }

        public override void OnSet()
        {
            CardUri = ResourceEngine.GetImageResourceUri("卡_诊疗卡");
            BackUri = ResourceEngine.GetImageResourceUri("身份证扫描处");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            StartRead();
            LeftList = new List<PayInfoItem>
            {
                new PayInfoItem("应付金额：", Cashier.Amount.In元(), true),
                new PayInfoItem("优惠金额：", 0m.In元()),
                new PayInfoItem("支付方式：", "区域诊疗卡")
            };
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            _working = false;

            StopRead();
            return true;
        }

        protected virtual async void OnGetInfo(string cardNo)
        {
            if (cardNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "信息查询", "卡片信息为空，请确认插卡方向是否正确和卡片是否有效");
                StartRead();
                return;
            }
            await Cashier.GotCardFunc(cardNo, "2");
        }

        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试卡号");
            if (ret.IsSuccess)
                OnGetInfo(ret.Value);
        }

        #region Bindings

        private string _hint = "请按提示放卡";

        public string Hint
        {
            get => _hint;
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        private Uri _cardUri;

        public Uri CardUri
        {
            get => _cardUri;
            set
            {
                _cardUri = value;
                OnPropertyChanged();
            }
        }

        private Uri _backUri;

        public Uri BackUri
        {
            get => _backUri;
            set
            {
                _backUri = value;
                OnPropertyChanged();
            }
        }

        private string _tipContent = "将卡置于读卡器上方";

        public string TipContent
        {
            get => _tipContent;
            set
            {
                _tipContent = value;
                OnPropertyChanged();
            }
        }

        private List<PayInfoItem> _leftList;

        public List<PayInfoItem> LeftList
        {
            get => _leftList;
            set
            {
                _leftList = value;
                OnPropertyChanged();
            }
        }

        #endregion Bindings

        #region Read

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
                    OnGetInfo(track);
            }
        }

        protected virtual void StopMag()
        {
            _magCardReader?.DisConnect();
        }

        #endregion Read
    }
}