using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Core.Services.LightBar;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.ShengZhouZhongYiHospital.Component.Auth.Models;
using YuanTu.ShengZhouZhongYiHospital.HisNative.Models;

namespace YuanTu.ShengZhouZhongYiHospital.Component.Auth.ViewModels
{
    public class SiCardViewModel : YuanTu.Default.Component.Auth.ViewModels.SiCardViewModel
    {
        protected Action<LoadingProcesser> myAction;
        private Uri _gifUrl;

        public Uri GifUrl
        {
            get { return _gifUrl; }
            set { _gifUrl = value; OnPropertyChanged(); }
        }

        [Dependency]
        public ILightBarService LightBarService { get; set; }


        private bool _enableConfirm;
        public bool EnableConfirm
        {
            get { return _enableConfirm; }
            set { _enableConfirm = value; OnPropertyChanged(); }
        }

        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {
            _rfCpuCardReader = rfCpuCardReaders?.FirstOrDefault(p => p.DeviceId == "CRT310_IC");
            ConfirmHospitalCardNoCommand = new DelegateCommand(ConfirmHospitalCardNo);
        }

        public override void OnSet()
        {
            base.OnSet();
            GifUrl = ResourceEngine.GetImageResourceUri("动画素材_社保卡");
            JiuZhenCard = ResourceEngine.GetImageResourceUri("卡_条码卡");
            if (CurrentStrategyType() == DeviceType.Clinic)
            {
                EnableConfirm = true;
            }
        }

        public override string Title => "插入自费卡或者医保卡或者农保卡";

        public override void OnEntered(NavigationContext navigationContext)
        {
            if (ChoiceModel.Business == (Business)100)
            {
                CardVisibility = Visibility.Collapsed;
                BarCodeVisibility = Visibility.Visible;
                HospitalInputFocus = true;
            }
            else
            {
                CardVisibility = Visibility.Visible;
                BarCodeVisibility = Visibility.Collapsed;
                PlaySound(SoundMapping.请插入卡);
                if (CurrentStrategyType() != DeviceType.Clinic)
                {
                    StartCheckCard();
                }
            }
            LightBarService?.PowerOn(LightItem.就诊卡社保卡);
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            LightBarService?.PowerOff();
            _working = false;
            return base.OnLeaving(navigationContext);
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
        public override void Confirm()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在读卡，请稍候...");
                var res = RealRead();
                if (!res.IsSuccess)
                {
                    ShowAlert(false, "读卡失败", res.Message);
                }
            });
        }

        private bool _working = false;
        //检测卡
        private void StartCheckCard()
        {
            _working = true;


            Task.Run(() =>
            {
                HospitalInputFocus = false;
                HospitalInputFocus = true;
                var ret = _rfCpuCardReader.Connect();
                if (!ret.IsSuccess)
                {
                    ReportService.读卡器离线(null, ErrorSolution.读卡器离线);
                    ShowAlert(false, "友好提示", $"读卡器打开失败({ret.ResultCode})", debugInfo: ret.Message);
                    return;
                }
                _rfCpuCardReader.MoveCard(CardPos.不持卡位); //退卡
                    if (!_rfCpuCardReader.Initialize().IsSuccess)
                {
                    ShowAlert(false, "友好提示", $"读卡器初始化失败({ret.ResultCode})", debugInfo: ret.Message);
                    return;
                }

                _working = true;
                while (_working)
                {
                    var pos = _rfCpuCardReader.GetCardPosition();
                    if (pos.IsSuccess && (pos.Value == CardPos.停卡位 || pos.Value == CardPos.IC位))
                    {
                        Logger.Main.Info($"[自动读卡]发现卡，开始读卡！");
                        //有卡，读卡
                        _working = false;
                        Read();
                        break;
                    }
                    Thread.Sleep(300);
                }

            });


        }

        private void Read()
        {
            DoCommand(lp =>
            {
                if (CurrentStrategyType() != DeviceType.Clinic)
                {
                    _rfCpuCardReader.MoveCard(CardPos.IC位);
                }
                Thread.Sleep(200);//等卡停稳
                return RealRead();
            }).ContinueWith(ret =>
            {
                if (!ret.Result.IsSuccess)//失败重来
                {
                    ShowAlert(false, "病人信息查询失败", ret.Result.Message, 10, extend: new AlertExModel()
                    {
                        HideCallback = hc =>
                        {
                            if (hc == AlertHideType.ButtonClick)
                            {
                                HospitalInputFocus = true;
                                StartCheckCard();
                            }
                            else
                            {
                                Navigate(A.Home);
                            }
                        }
                    });
                }
            });

        }

        private Result RealRead()
        {
            var pm = PatientModel as PatientInfoModel;
            pm.Req门诊读卡 = new Req门诊读卡() { };
            pm.Req门诊读卡.卡类别 = "2"; //医保
            pm.Res门诊读卡 = HisHandleEx.执行门诊读卡(pm.Req门诊读卡);
            if (pm.Res门诊读卡.IsSuccess)
            {
                CardModel.CardType = CardType.社保卡;
                return 病人信息查询(pm.Res门诊读卡.卡号);
            }
            else if (pm.Res门诊读卡.Message.Contains("您未在本院就诊过"))
            {
                return Result.Fail(pm.Res门诊读卡.Message);
            }
            pm.Req门诊读卡.卡类别 = "1"; //就诊卡
            pm.Res门诊读卡 = HisHandleEx.执行门诊读卡(pm.Req门诊读卡);
            if (pm.Res门诊读卡.IsSuccess)
            {
                CardModel.CardType = CardType.就诊卡;
                return 病人信息查询(pm.Res门诊读卡.卡号);
            }
            return Result.Fail("读卡失败，请确认您的卡片是否正确插入！");
        }

        private Result 病人信息查询(string cardNo)
        {
            PatientModel.Req病人信息查询 = new req病人信息查询
            {
                cardNo = cardNo,
                cardType = ConstInner.CardTypeMapping[CardModel.CardType].ToString()
            };
            PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
            if (PatientModel.Res病人信息查询.success)
            {
                if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                {
                    // ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                    return Result.Fail("未查询到病人的信息(列表为空)");
                }
                //if (string.IsNullOrEmpty(PatientModel.Res病人信息查询.data[0].idNo))
                //{
                //    ShowAlert(false, "病人信息查询", "您的身份信息不完整，请补全您的身份信息！",extend:new AlertExModel()
                //    {
                //        HideCallback = aht =>
                //        {
                //            if (aht==AlertHideType.ButtonClick)
                //            {
                //                CardModel.ExternalCardInfo = "补全身份信息";
                //                Navigate(A.CK.IDCard);
                //            }
                //            else
                //            {
                //                Navigate(A.Home);
                //            }
                //        }
                //    });
                //}
                CardModel.CardNo = cardNo;
                var pm = PatientModel as PatientInfoModel;
                ConstInner.SaveCacheData(pm.Req门诊读卡, pm.Res门诊读卡, pm.Req病人信息查询, pm.Res病人信息查询, CardModel.CardType, CardModel.CardNo);
                Navigate(A.CK.Info);
                return Result.Success();
            }
            //  ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
            return Result.Fail(PatientModel.Res病人信息查询.msg);
        }

        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试卡号");
            if (ret.IsSuccess)
            {
                DoCommand(lp =>
                {
                    lp.ChangeText("正在测试登录");
                    CardModel.CardType = CardType.就诊卡;
                    var cardNo = ret.Value;
                    if (ret.Value.StartsWith("0|"))
                    {
                        var res = new Res门诊读卡();
                        res.Deserialize(ret.Value);
                        var pm = PatientModel as PatientInfoModel;
                        pm.Req门诊读卡 = new Req门诊读卡();
                        pm.Res门诊读卡 = res;
                        cardNo = res.卡号;
                    }
                    病人信息查询(cardNo);
                });

            }
        }


        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }
        private Visibility _cardVisibility;
        private Visibility _barCodeVisibility;
        private bool _hospitalInputFocus;
        private string _hospitalCardNo;
        private Uri _jiuZhenCard;
        public string HospitalCardNo
        {
            get { return _hospitalCardNo; }
            set
            {
                _hospitalCardNo = value;
                OnPropertyChanged();
            }
        }
        public bool HospitalInputFocus
        {
            get { return _hospitalInputFocus; }
            set
            {
                _hospitalInputFocus = value;
                OnPropertyChanged();
            }
        }
        public Uri JiuZhenCard
        {
            get { return _jiuZhenCard; }
            set
            {
                _jiuZhenCard = value;
                OnPropertyChanged();
            }
        }
        public Visibility BarCodeVisibility
        {
            get { return _barCodeVisibility; }
            set
            {
                _barCodeVisibility = value;
                OnPropertyChanged();
            }
        }
        public Visibility CardVisibility
        {
            get { return _cardVisibility; }
            set
            {
                _cardVisibility = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConfirmHospitalCardNoCommand { get; set; }

        private void ConfirmHospitalCardNo()
        {
            if (ChoiceModel.Business != (Business)100)
            {
                return; ;
            }
            CardModel.CardType = CardType.就诊卡;
            CardModel.ExternalCardInfo = "刷条码";
            DoCommand(lp =>
            {
                return 病人信息查询(HospitalCardNo);
            }).ContinueWith(ret =>
            {
                if (!ret.Result.IsSuccess)//失败重来
                {
                    ShowAlert(false, "病人信息查询失败", ret.Result.Message, 10, extend: new AlertExModel()
                    {
                        HideCallback = hc =>
                        {
                            if (hc == AlertHideType.ButtonClick)
                            {
                                return;
                            }
                            Navigate(A.Home);
                        }
                    });
                }
            });
        }
    }
}
