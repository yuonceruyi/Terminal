using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Devices.CardReader;
using YuanTu.ISO8583;
using YuanTu.ISO8583.Interfaces;
using YuanTu.ISO8583.IO;
using YuanTu.NingXiaHospital.CardReader.BankCard;
using YuanTu.NingXiaHospital.HisService;
using YuanTu.NingXiaHospital.HisService.Request;

namespace YuanTu.NingXiaHospital.Component.Auth.ViewModels
{
    public class CardViewModel : YuanTu.Default.Component.Auth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders, IIcCardReader[] icCardReaders) : base(rfCardReaders,
            magCardReaders)
        {
            _isClinic = CurrentStrategyType() == DeviceType.Clinic;
            _icCardReader = _isClinic ? icCardReaders.FirstOrDefault(p => p.DeviceId == "HuaDa_IC"): icCardReaders.FirstOrDefault(p => p.DeviceId == "ACT_A6_Mag&IC");
            _magCardReader = _isClinic ? magCardReaders?.FirstOrDefault(p => p.DeviceId == "HuaDa_Mag") : magCardReaders?.FirstOrDefault(p => p.DeviceId == "ACT_A6_Mag&IC");
        }

        private IIcCardReader _icCardReader;

        private bool _isClinic;

        private bool _yT550Enable;

        public bool YT550Enable
        {
            get => _yT550Enable;
            set
            {
                _yT550Enable = value;
                OnPropertyChanged();
            }
        }

        private bool _clinicEnable;

        public bool ClinicEnable
        {
            get => _clinicEnable;
            set
            {
                _clinicEnable = value;
                OnPropertyChanged();
            }
        }

        private Uri _gifUrl;

        public Uri GifUrl
        {
            get { return _gifUrl; }
            set { _gifUrl = value; OnPropertyChanged(); }
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

        public ICommand ConfirmCommand=>new DelegateCommand(()=>
        {
            WrapperReader.IcCardReader = _icCardReader;
            WrapperReader.MagCardReader = _magCardReader;
            var result1=WrapperReader.IcRead();
            if (result1)
            {
                OnGetInfo(result1.Value);
            }
            else
            {
                var result2= WrapperReader.MagRead();
                if (result2)
                {
                    OnGetInfo(result2.Value);
                }
                else
                {
                    ShowAlert(false,"温馨提示","读卡失败请重新插卡，再点击确定按钮");
                }
            }
        });

        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("插卡口");
            CardUri = ResourceEngine.GetImageResourceUri("卡_银行卡");
            GifUrl = ResourceEngine.GetImageResourceUri("刷卡动画");
            YT550Enable = !_isClinic;
            ClinicEnable = !YT550Enable;
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            if (!_isClinic)
            {
                StartRead();
            }
        }

        protected override void StartRead()
        {
            Task.Run(() =>
            {
                StartMag();
            });
        }

        protected override void StartMag()
        {
            string no = null;
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
                    if (pos.IsSuccess && (pos.Value == CardPos.停卡位|| pos.Value == CardPos.IC位))
                    {
                        var rest = _magCardReader.ReadTrackInfos(TrackRoad.Trace2, ReadType.ASCII);
                        if (rest.IsSuccess)
                        {
                            no = rest.Value[TrackRoad.Trace2];
                            Logger.Main.Info($"[读取卡号成功][cardNo]{no}");
                        }
                        else
                        {
                            WrapperReader.IcCardReader = _icCardReader;
                            var res = WrapperReader.IcRead();
                            if (res)
                            {
                                no = res.Value;
                                Logger.Main.Info($"[读取卡号成功][cardNo]{no}");
                            }
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
                    OnGetInfo(no);
                }
            }
        }

        protected override void StopRead()
        {
            StopMag();
        }

        protected override void OnGetInfo(string cardNo, string extendInfo = null)
        {
            if (cardNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "病人信息查询", "卡片信息为空，请确认插卡方向是否正确和卡片是否有效", extend: new AlertExModel
                {
                    HideCallback =
                        p =>
                        {
                            if (p == AlertHideType.ButtonClick)
                                StartRead();
                        }
                });
                return;
            }
            if (CardModel.CardType == CardType.银行卡)
                CardModel.CardNo = cardNo.Split('=')[0];
            Logger.Main.Info("开始查询签约记录");
            DoCommand(ctx =>
            {
                Logger.Main.Info("开始查询签约记录 入参构造结束");
                var resQueryPatientSignInfo = DllHandler.QueryPatientSignInfo("1", CardModel.CardNo);
                if (resQueryPatientSignInfo)
                {
                    if (resQueryPatientSignInfo.Value.redata == null)
                    {
                        Logger.Main.Info("开始查询签约记录 开始跳转");
                        BeginInvoke(DispatcherPriority.ContextIdle, () =>
                        {
                            ShowConfirm($"您的银行卡未签约", $"通过身份证信息签约", cp =>
                            {
                                if (cp)
                                {
                                    Logger.Net.Info("[宁夏]去签约跳转");
                                    Navigate(A.CK.IDCard);
                                }
                            }, 30, ConfirmExModel.Build("去签约", "暂不", true));
                        });
                    }
                    else
                    {
                        PatientModel.Res病人信息查询 = new res病人信息查询();
                        PatientModel.Res病人信息查询.data = new List<病人信息>();
                        PatientModel.Res病人信息查询.data.Add(new 病人信息
                        {
                            patientId = resQueryPatientSignInfo.Value.redata.head.patient_id,
                            name = resQueryPatientSignInfo.Value.redata.head.xm,
                            sex = resQueryPatientSignInfo.Value.redata.head.xb == "0" ? "男" : "女",
                            birthday = resQueryPatientSignInfo.Value.redata.head.sfzh.Substring(6, 8).Insert(4, "-")
                                .Insert(7, "-"),
                            idNo = resQueryPatientSignInfo.Value.redata.head.sfzh,
                            phone = resQueryPatientSignInfo.Value.redata.head.sjhm,
                            patientType = "0" //"0:已经签约" "1:未签约"
                        });
                        Next();
                    }
                    return;
                }
                ShowAlert(false, "信息查询", $"签约信息查询失败：{resQueryPatientSignInfo.Message}");
            });
        }
    }
}