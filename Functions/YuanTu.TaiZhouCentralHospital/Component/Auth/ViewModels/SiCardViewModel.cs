using Microsoft.Practices.Unity;
using Prism.Regions;
using System;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Create;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Devices.CardReader;
using YuanTu.TaiZhouCentralHospital.HealthInsurance;
using YuanTu.TaiZhouCentralHospital.HealthInsurance.Model;
using YuanTu.TaiZhouCentralHospital.HealthInsurance.Request;
using YuanTu.TaiZhouCentralHospital.HealthInsurance.Service;

namespace YuanTu.TaiZhouCentralHospital.Component.Auth.ViewModels
{
    public class SiCardViewModel : Default.Component.Auth.ViewModels.SiCardViewModel
    {
        private bool _working;
        private bool _showConfirmButton;
        private string _readSiCardTips;
        private const long NoCreateCode = 3;

        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {
        }

        [Dependency]
        public ISiService SiService { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        [Dependency]
        public ISiModel SiModel { get; set; }

        [Dependency]
        public ICreateModel CreateModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            if (FrameworkConst.Strategies[0] != DeviceType.Clinic)
            {
                CardUri = ResourceEngine.GetImageResourceUri(CardModel.CardType == CardType.社保卡 ? "动画素材_社保卡" : "卡_健康卡");
                ReadSiCardTips = "读卡完成后会自动进行跳转";
                ShowConfirmButton = false;
                StartRead();
            }
            else
            {
                ReadSiCardTips = "请在读卡器黄灯亮起后点击确定读卡";
                ShowConfirmButton = true;
            }

            base.OnEntered(navigationContext);
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            if (FrameworkConst.Strategies[0] != DeviceType.Clinic)
            {
                _working = false;
                StopRead();
            }
            return base.OnLeaving(navigationContext);
        }

        protected virtual void StartRead()
        {
            Task.Run(() => StartA6Z9());
        }

        protected virtual void StopRead()
        {
            StopA6Z9();
        }

        protected virtual void StopA6Z9()
        {
            _rfCpuCardReader?.DisConnect();
        }

        protected virtual void StartA6Z9()
        {
            try
            {
                var ret = _rfCpuCardReader.Connect();
                if (!ret.IsSuccess)
                {
                    ReportService.读卡器离线(null, ErrorSolution.读卡器离线);
                    ShowAlert(false, "友好提示", $"读卡器打开失败({ret.ResultCode})", debugInfo: ret.Message);
                    return;
                }
                if (!_rfCpuCardReader.Initialize().IsSuccess)
                {
                    ShowAlert(false, "友好提示", $"读卡器初始化失败({ret.ResultCode})", debugInfo: ret.Message);
                    return;
                }
                _working = true;
                while (_working)
                {
                    var pos = _rfCpuCardReader.GetCardPosition();
                    Logger.Device.Debug($"{pos.Value}");
                    if (pos.IsSuccess && (pos.Value == CardPos.停卡位 || pos.Value == CardPos.IC位))
                    {
                        Logger.Device.Debug($"有卡，开始读卡信息");
                        //有卡，开始读卡信息
                        _working = false;
                        _rfCpuCardReader.MoveCard(CardModel.CardType == CardType.社保卡 ? CardPos.IC位 : CardPos.后端持卡位); //移动到IC卡位
                        Thread.Sleep(200); //等卡停稳
                        Confirm();
                        break;
                    }
                    Thread.Sleep(300);
                }
            }
            catch (Exception ex)
            {
                ShowAlert(false, "温馨提示", $"读卡失败:{ex.Message}");
            }
        }

        public override void Confirm()
        {
            DoCommand(lp =>
            {
                if (CardModel.CardType == CardType.社保卡)
                {
                    lp.ChangeText("正在查询医保系统个人信息,请稍候...");
                    var result = GetSiPatInfo();
                    if (!result.IsSuccess)
                        return false;
                    CardModel.CardNo = SiModel.医保个人基本信息?.公民身份号;
                }
                else
                {
                    lp.ChangeText("正在读取健康卡个人信息,请稍候...");
                    var result = HealthCard.ReadCitizenCard();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", result.Message);
                        return false;
                    }
                    SiModel.健康卡信息 = result.Value;
                    CardModel.CardNo = result.Value.市民卡健康卡卡号;
                }

                if (ChoiceModel.Business == Business.建档)
                {
                    Next();
                    return true;
                }

                lp.ChangeText("正在查询医院系统个人信息,请稍候...");
                var res = GetHisPatInfo();
                if (!res.IsSuccess)
                    return false;
                Next();
                return true;
            }).ContinueWith(ret =>
            {
                if (FrameworkConst.Strategies[0] != DeviceType.Clinic)
                {
                    if (!ret.Result)
                        _rfCpuCardReader.UnInitialize();//退卡
                }
            });
        }

        protected virtual Result GetSiPatInfo()
        {
            var reqSiPatientInfo = new Req获取参保人信息
            {
                读卡方式 = "10",
                终端机编号 = "0"
            };
            var res = SiService.GetSiPatientInfo(reqSiPatientInfo);
            if (!res.IsSuccess)
            {
                ShowAlert(false, "获取医保信息", $"{res.Message}");
                return Result.Fail(res.Message);
            }
            SiModel.SiPatientInfo = SiModel.RetMessage;
            return Result.Success();
        }

        protected virtual Result GetHisPatInfo()
        {
            PatientModel.Req病人信息查询 = new req病人信息查询
            {
                cardType = ((int)CardModel.CardType).ToString(),
                cardNo = CardModel.CardNo
            };
            PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

            if (PatientModel.Res病人信息查询.success)
            {
                if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                    return Result.Fail("未查询到病人的信息(列表为空)");
                }
                return Result.Success();
            }
            if (PatientModel.Res病人信息查询.code == NoCreateCode)
            {
                //未建档特殊处理
                SiModel.NeedCreate = true;
                CreateModel.CreateType = CreateType.成人;
                return Result.Success();
            }
            ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
            return Result.Fail("未查询到病人的信息");
        }

        public bool ShowConfirmButton
        {
            get { return _showConfirmButton; }
            set
            {
                _showConfirmButton = value;
                OnPropertyChanged();
            }
        }

        public string ReadSiCardTips
        {
            get { return _readSiCardTips; }
            set
            {
                _readSiCardTips = value;
                OnPropertyChanged();
            }
        }
    }
}