using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Devices.CardReader;
using YuanTu.NingXiaHospital.CardReader;
using YuanTu.NingXiaHospital.CardReader.DkT10;

namespace YuanTu.NingXiaHospital.Component.Auth.ViewModels
{
    public class SiCardViewModel : Default.Component.Auth.ViewModels.SiCardViewModel
    {
        protected bool _working;

        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders,
            rfCpuCardReaders)
        {
            _rfCpuCardReader = rfCpuCardReaders?.FirstOrDefault(p => p.DeviceId == "DKA6_IC");
        }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        private bool _confirmEnable;
        public bool ConfirmEnable
        {
            get => _confirmEnable;
            set
            {
                _confirmEnable = value;
                OnPropertyChanged();
            }
        }

        private string _info;
        public string Info
        {
            get => _info;
            set
            {
                _info = value;
                OnPropertyChanged();
            }
        }


        public override void OnEntered(NavigationContext navigationContext)
        {
            if (CurrentStrategyType() != DeviceType.Clinic)
            {
                Info = "读卡完成后会自动进行跳转";
                ConfirmEnable = false;
                StartCheckCard();
            }
            else
            {
                Info = "请插卡完成后点击确定按钮";
                ConfirmEnable = true;
            }
        }

        public override void Confirm()
        {
            if (CurrentStrategyType() == DeviceType.Clinic)
            {
                Read();
            }
        }

        protected void StartCheckCard()
        {
            if (FrameworkConst.DoubleClick)
            {
                IdCardModel.IdCardNo = "123123";
                IdCardModel.Name = "312312";
                OnGetInfo();
                return;
            }
            _working = true;
            Logger.Device.Info("StartCheckCard");
            Task.Run(() =>
            {
                var ret = _rfCpuCardReader.Connect();
                if (!ret.IsSuccess)
                {
                    ReportService.读卡器离线(null, ErrorSolution.读卡器离线);
                    ShowAlert(false, "友好提示", $"读卡器打开失败({ret.ResultCode})", debugInfo: ret.Message);
                    return;
                }
                _rfCpuCardReader.MoveCard(CardPos.不持卡位); //退卡
                Logger.Device.Info("StartCheckCard1");
                if (!_rfCpuCardReader.Initialize().IsSuccess)
                {
                    ShowAlert(false, "友好提示", $"读卡器初始化失败({ret.ResultCode})", debugInfo: ret.Message);
                    return;
                }
                _working = true;
                while (_working)
                {
                    Logger.Device.Info("StartCheckCard2");
                    _rfCpuCardReader.MoveCard(CardPos.IC位);
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
            _working = false;
            DoCommand(lp =>
            {
                lp.ChangeText("正在读取社保卡内信息，请稍后...");
                try
                {
                    Dkt10 reader = new Dkt10();
                    var result = reader.Read();
                    if (result)
                    {
                        lp.ChangeText("正在查询病人信息");
                        IdCardModel = result.Value;
                        CardModel.CardNo = result.Value.IdCardNo;
                        Logger.Device.Info($" {CardModel.CardNo}|------|{IdCardModel.ToJsonString()}");
                        OnGetInfo();
                        return;
                    }
                    ShowAlert(false, "社保读卡", $"读卡异常，请重新插卡 ");
                    _rfCpuCardReader.UnInitialize();
                    StartCheckCard();
                }
                catch (Exception ex)
                {
                    Logger.Main.Error($"[社保读卡]读卡过程中发生异常，{ex.Message}");
                    _rfCpuCardReader.UnInitialize();
                    StartCheckCard();
                }
                finally
                {
                }
            });
        }

        protected virtual void OnGetInfo()
        {
            PatientModel.Req病人信息查询 = new req病人信息查询
            {
                Timeout = new TimeSpan(0, 1, 0),
                cardNo = IdCardModel.IdCardNo,
                cardType = ((int)CardModel.CardType).ToString(),
                patientName = IdCardModel.Name
            };
            PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
            CardModel.CardType = CardType.社保卡;
            if (!PatientModel.Res病人信息查询.success || PatientModel.Res病人信息查询.data == null ||
                PatientModel.Res病人信息查询.data.Count == 0)
            {
                if (ChoiceModel.Business == Business.缴费)
                {
                    ShowAlert(false, "病人信息查询", "社保卡病人信息查询失败");
                    return;
                }
                InitData();
            }
            Next();
        }

        private void InitData()
        {
            PatientModel.Res病人信息查询 = new res病人信息查询
            {
                data = new List<病人信息>
                {
                    new 病人信息
                    {
                        name = IdCardModel.Name,
                        sex = IdCardModel.Sex == Sex.男 ? "男" : "女",
                        birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                        idNo = IdCardModel.IdCardNo,
                        phone = null,
                        patientType = "0"
                    }
                }
            };
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            if (ChoiceModel.Business == Business.住院押金)
            {
                _rfCpuCardReader.UnInitialize();
            }
            _working = false;
            return base.OnLeaving(navigationContext);
        }
    }
}