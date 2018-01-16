using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Devices.CardReader;
using YuanTu.ZheJiangZhongLiuHospital.ICBC;

namespace YuanTu.ZheJiangZhongLiuHospital.Component.Auth.ViewModels
{
    public class CardViewModel : YuanTu.Default.Component.Auth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
            : base(rfCardReaders, magCardReaders)
        {
            _magCardReader = magCardReaders?.FirstOrDefault(p => p.DeviceId == "HuaDa_Mag");
        }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        private Uri _gifUrl;

        public Uri GifUrl
        {
            get { return _gifUrl; }
            set { _gifUrl = value; OnPropertyChanged(); }
        }

        public override void OnSet()
        {
            base.OnSet();
            GifUrl = ResourceEngine.GetImageResourceUri("刷卡动画");
        }
        public ICommand MediaEndedCommand
        {
            get
            {
                return new DelegateCommand<object>((sender) =>
                {
                    MediaElement media = (MediaElement)sender;
                    media.LoadedBehavior = MediaState.Manual;
                    media.Position = TimeSpan.FromMilliseconds(1);
                    media.Play();
                });
            }

        }


        protected override void StartRead()
        {
            Task.Run(() => StartMag());
        }

        protected override void StopRead()
        {
            StopMag();
        }

        protected override void StartMag()
        {
            Task.Run(() =>
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
                    Logger.Device.Info($"[华大连接成功]");
                    if (!_magCardReader.Initialize().IsSuccess)
                    {
                        ShowAlert(false, "友好提示", $"读卡器初始化失败({ret.ResultCode})", debugInfo: ret.Message);
                        return;
                    }
                    Logger.Device.Info($"[华大序列化成功]");
                    _working = true;
                    while (_working)
                    {
                        Logger.Device.Info($"[华大开始读取卡号]");
                        var rest = _magCardReader.ReadTrackInfos(2);
                        if (!rest.IsSuccess)
                        {
                            Logger.Device.Info($"[华大开始读取卡号失败 false]");
                            continue;
                        }
                        if (rest.Value.IsNullOrWhiteSpace())
                        {
                            Logger.Device.Info($"[华大开始读取卡号失败 空值]");
                            ShowAlert(false, "友好提示", $"读卡失败，请刷正确的就诊卡");
                            continue;
                        }
                        track = rest.Value.Substring(1, rest.Value.IndexOf("?") - 1); 
                        Logger.Device.Info($"[读取卡号成功][cardNo]{track}");
                        break;
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
            });
        }

        protected override void OnGetInfo(string cardNo, string extendInfo = null)
        {
            if (cardNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "病人信息查询", "卡片信息为空，请确认插卡方向是否正确和卡片是否有效", extend: new Consts.Models.AlertExModel
                {
                    HideCallback =
                        p =>
                        {
                            if (p == Consts.Models.AlertHideType.ButtonClick)
                                StartRead();
                        }
                });
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
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)", extend: new Consts.Models.AlertExModel
                        {
                            HideCallback =
                                p =>
                                {
                                    if (p == Consts.Models.AlertHideType.ButtonClick)
                                        StartRead();
                                }
                        });
                        return;
                    }
                    if (ChoiceModel.Business == Business.缴费|| ChoiceModel.Business==Business.签到)
                    {
                        CardModel.CardNo = cardNo;
                        Next();
                        return;
                    }
                    var req = new Req查询虚拟账户余额()
                    {
                        Chanel = "1",
                        AccountNo = PatientModel.当前病人信息.accountNo,
                        AccountId = $"{PatientModel.当前病人信息.name}^{PatientModel.当前病人信息.idNo}",

                        OperId = FrameworkConst.OperatorId,
                        DeviceInfo = FrameworkConst.OperatorId,
                        TradeSerial =DateTimeCore.Now.ToString("yyyyMMddHHmmssffff"),

                        Rsv1 = "",
                        Rsv2 = "",
                    };
                    var qResult = PConnection.Handle<Res查询虚拟账户余额>(req);
                    if (qResult.IsSuccess)
                    {
                        var res = qResult.Value;
                        PatientModel.当前病人信息.accBalance = res.Remain;
                        CardModel.CardNo = cardNo;
                        Next();
                        return;
                    }
                    ShowAlert(false, "病人信息查询", "余额查询失败", debugInfo: qResult.Message, extend: new Consts.Models.AlertExModel
                    {
                        HideCallback =
                            p =>
                            {
                                if (p == Consts.Models.AlertHideType.ButtonClick)
                                    StartRead();
                            }
                    });
                }
                else
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg, extend: new Consts.Models.AlertExModel
                    {
                        HideCallback =
                            p =>
                            {
                                if (p == Consts.Models.AlertHideType.ButtonClick)
                                    StartRead();
                            }
                    });

                }
            });
        }
    }
}
