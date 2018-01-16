using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.House.Component.Auth.Models;
using YuanTu.Default.House.HealthManager;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;

namespace YuanTu.Default.House.Component.Auth.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        protected IIdCardReader _idCardReader;
        protected IMagCardReader _magCardReader;
        protected bool _needReadIdCard;
        protected IRFCardReader _rfCardReader;

        public LoginViewModel(IIdCardReader[] idCardReaders, IMagCardReader[] magCardReaders,
            IRFCardReader[] rfCardReader)
        {
            _idCardReader = idCardReaders.FirstOrDefault(p => p.DeviceId == "HuaDa_HUADA");
            _magCardReader = magCardReaders.FirstOrDefault(p => p.DeviceId == "HuaDa_Mag");
            _rfCardReader = rfCardReader.FirstOrDefault(p => p.DeviceId == "HuaDa_RF");
        }

        [Dependency]
        public IIdCardModel IdCardModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public IHealthModel HealthModel { get; set; }

        public override void OnSet()
        {
            TipImage = ResourceEngine.GetImageResourceUri("登录示例_House");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            HideNavigating = true;
            _needReadIdCard = ChoiceModel.Business != Business.预约;
            Tips = _needReadIdCard ? "请按提示刷贵宾体验卡或者身份证" : "请按提示刷贵宾体验卡";
            TipImage = ResourceEngine.GetImageResourceUri(_needReadIdCard ? "登录示例_House" : "登录示例2_House");
            StartRead();

            PlaySound(_needReadIdCard ? SoundMapping.请将身份证或贵宾体验卡放置于感应区 : SoundMapping.请将贵宾体验卡放置于感应区);
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            _rfCardReader?.DisConnect();
            _working = false;
            return base.OnLeaving(navigationContext);
        }

        protected virtual void StartRead()
        {
            Task.Run(() =>
            {
                try
                {
                    if (!_rfCardReader.Connect().IsSuccess)
                    {
                        ReportService.读卡器离线(null, ErrorSolution.读卡器离线);
                        ShowAlert(false, "温馨提示", "读卡器离线");
                        return;
                    }
                    if (!_rfCardReader.Initialize().IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", "读卡器初始化失败");
                        return;
                    }
                    _idCardReader.SetHandle(_rfCardReader.GetHandle().Value);
                    _working = true;

                    while (_working)
                    {
                        var pos = _rfCardReader.GetCardPosition();
                        if (pos.IsSuccess && pos.Value == CardPos.停卡位)
                        {
                            var rest = _rfCardReader.GetCardId();
                            if (!rest.IsSuccess)
                            {
                                ShowAlert(false, "温馨提示", $"读卡失败:{rest.Message}");
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                var track = BitConverter.ToUInt32(rest.Value, 0).ToString();
                                Logger.Main.Info($"[读取卡号成功][cardNo]{track}");
                                _working = false;
                                CardModel.CardType = CardType.就诊卡;
                                OnGetInfo(track);
                            }
                        }
                        else if (_needReadIdCard)
                        {
                            if (_idCardReader.HasIdCard().IsSuccess)
                            {
                                var res = _idCardReader.GetIdDetail();
                                if (!res.IsSuccess)
                                {
                                    ShowAlert(false, "身份证", "读取身份证信息失败,请重新放置身份证");
                                    Thread.Sleep(1000);
                                }
                                else
                                {
                                    IdCardModel.Name = res.Value.Name;
                                    IdCardModel.Sex = res.Value.Sex;
                                    IdCardModel.IdCardNo = res.Value.IdCardNo;
                                    IdCardModel.Address = res.Value.Address;
                                    IdCardModel.Birthday = res.Value.Birthday;
                                    IdCardModel.Nation = res.Value.Nation;
                                    IdCardModel.GrantDept = res.Value.GrantDept;
                                    IdCardModel.ExpireDate = res.Value.ExpireDate;
                                    IdCardModel.EffectiveDate = res.Value.EffectiveDate;
                                    Logger.Main.Info(
                                        $"[读取身份证信息成功]{IdCardModel.Name} {IdCardModel.Sex} {IdCardModel.IdCardNo}");

                                    _working = false;
                                    CardModel.CardType = CardType.身份证;
                                    OnGetInfo(IdCardModel.IdCardNo);
                                }
                            }
                        }

                        Thread.Sleep(200);
                    }
                }
                catch (Exception ex)
                {
                }
            });
        }

        public virtual void OnGetInfo(string cardNo)
        {
            //查询健康后台用户信息
            DoCommand(p =>
            {
                p.ChangeText("正在查询您的健康档案，请稍后...");
                if (CardModel.CardType == CardType.身份证)
                {
                    var req = new req查询是否已建档
                    {
                        name = IdCardModel.Name,
                        idNo = IdCardModel.IdCardNo,
                        cardNo = null,
                        cardType = null,
                        sex = IdCardModel.Sex.ToString(),
                        age = IdCardModel.Birthday.Age().ToString(),
                        birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                        nation = IdCardModel.Nation,
                        addr = IdCardModel.Address,
                        expire = IdCardModel.ExpireDate.ToString("yyyy-MM-dd"),
                        photo = string.Empty
                    };
                    var res = HealthDataHandlerEx.查询是否已建档(req);
                    if (!(res?.success ?? false))
                    {
                        ShowAlert(false, "温馨提示", "健康档案信息查询失败", debugInfo: res?.msg);
                        Preview();
                        return;
                    }
                    HealthModel.Res查询是否已建档 = res;
                    Next();
                }
                else if (CardModel.CardType == CardType.就诊卡)
                {
                    p.ChangeText("正在查询您的医院档案，请稍后...");
                    PatientModel.Req病人信息查询 = new req病人信息查询
                    {
                        cardNo = cardNo,
                        cardType = ((int) CardModel.CardType).ToString()
                    };
                    PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

                    if (PatientModel.Res病人信息查询.success)
                    {
                        if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                        {
                            ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                            Preview();
                            return;
                        }
                        CardModel.CardNo = cardNo;

                        p.ChangeText("正在查询您的健康档案，请稍后...");
                        var patientInfo = PatientModel.当前病人信息;
                        var req = new req查询是否已建档
                        {
                            name = patientInfo.name,
                            idNo = patientInfo.idNo,
                            cardNo = CardModel.CardNo,
                            cardType = ((int) CardModel.CardType).ToString(),
                            sex = patientInfo.sex,
                            age = patientInfo.birthday.Age().ToString(),
                            birthday = patientInfo.birthday,
                            nation = null,
                            addr = patientInfo.address,
                            expire = null,
                            photo = null
                        };

                        var res = HealthDataHandlerEx.查询是否已建档(req);
                        if (!(res?.success ?? false))
                        {
                            ShowAlert(false, "温馨提示", "健康档案信息查询失败", debugInfo: res?.msg);
                            Preview();
                            return;
                        }
                        HealthModel.Res查询是否已建档 = res;
                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
                        Preview();
                    }
                }
            });
        }

        public override void DoubleClick()
        {
            CardModel.CardType = CardType.就诊卡;
            var ret = InputTextView.ShowDialogView("输入测试卡号");
            if (ret.IsSuccess)
                OnGetInfo(ret.Value);
        }

        #region Binding

        public override string Title => "登录";
        public override bool DisableHomeButton { get; set; } = false;

        private bool _working;

        private string _tips = "请按提示刷民生卡或者身份证";
        private Uri _tipImage;

        public Uri TipImage
        {
            get { return _tipImage; }
            set
            {
                _tipImage = value;
                OnPropertyChanged();
            }
        }

        public string Tips
        {
            get { return _tips; }
            set
            {
                _tips = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding
    }
}