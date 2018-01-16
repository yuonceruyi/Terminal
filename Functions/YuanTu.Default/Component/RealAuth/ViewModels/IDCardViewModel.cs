using Microsoft.Practices.Unity;
using Prism.Regions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Create;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;

namespace YuanTu.Default.Component.RealAuth.ViewModels
{
    public class IDCardViewModel : ViewModelBase
    {
        protected IIdCardReader _idCardReader;
        protected bool _working;

        public IDCardViewModel(IIdCardReader[] idCardReaders)
        {
            _idCardReader = idCardReaders.FirstOrDefault(p => p.DeviceId == "Xzx_XZX");
        }

        public override string Title => "身份证扫描";

        private string _hint = "请按提示刷身份证";
        public string Hint
        {
            get { return _hint; }
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

        [Dependency]
        public IIdCardModel IdCardModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public ICreateModel CreateModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("身份证扫描处");
            CardUri = ResourceEngine.GetImageResourceUri("卡_身份证");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            TimeOut = 20;

            PlaySound(SoundMapping.请插入身份证);
            StartRead();
        }

        protected virtual void StartRead()
        {
            Task.Run(() =>
            {
                try
                {
                    if (!_idCardReader.Connect().IsSuccess)
                    {
                        ReportService.身份证读卡器离线(null, ErrorSolution.身份证读卡器离线);
                        ShowAlert(false, "友好提示", "身份证读卡器打开失败");
                        return;
                    }

                    if (!_idCardReader.Initialize().IsSuccess)
                    {
                        ShowAlert(false, "友好提示", "身份证读卡器初始化失败");
                        return;
                    }
                    _working = true;
                    while (_working)
                    {
                        if (!_idCardReader.HasIdCard().IsSuccess)
                        {
                            Thread.Sleep(300);
                            continue;
                        }

                        var result = _idCardReader.GetIdDetail();
                        if (!result.IsSuccess)
                        {
                            ShowAlert(false, "身份证", "读取身份证信息失败,请重新放置身份证");
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            //CardModel.CardNo = result.Value.IdCardNo;
                            IdCardModel.Name = result.Value.Name;
                            IdCardModel.Sex = result.Value.Sex;
                            IdCardModel.IdCardNo = result.Value.IdCardNo;
                            IdCardModel.Address = result.Value.Address;
                            IdCardModel.Birthday = result.Value.Birthday;
                            IdCardModel.Nation = result.Value.Nation;
                            IdCardModel.GrantDept = result.Value.GrantDept;
                            IdCardModel.ExpireDate = result.Value.ExpireDate;
                            IdCardModel.EffectiveDate = result.Value.EffectiveDate;
                            Logger.Main.Info($"[读取身份证信息成功]{IdCardModel.Name} {IdCardModel.Sex} {IdCardModel.IdCardNo}");
                            if (IdCardModel.Name != PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex]?.name)
                            {
                                ShowAlert(false, "身份验证", $"身份证姓名[{IdCardModel.Name}]与就诊卡姓名[{PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex]?.name}]不一致，校验失败");
                                Thread.Sleep(1000);
                                continue;
                            }
                            if (_working)
                                OnGetInfo(IdCardModel.IdCardNo);
                            _working = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Main.Error($"[读取身份证信息失败]{ex.Message + ex.StackTrace}");
                    ShowAlert(false, "身份证", "读取身份证信息失败", debugInfo: ex.Message);
                }
                finally
                {
                    _idCardReader.UnInitialize();
                    _idCardReader.DisConnect();
                }
            });
        }

        protected virtual void OnGetInfo(string idCardNo)
        {
            CardModel.CardType = CardType.就诊卡;

            DoCommand(ctx =>
            {
                ctx.ChangeText("正在进行实名认证，请稍候...");
                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                PatientModel.Req病人基本信息修改 = new req病人基本信息修改
                {
                    patientId = patientInfo.patientId,
                    platformId = patientInfo.platformId,
                    cardNo = CardModel.CardNo,
                    cardType = ((int)CardModel.CardType).ToString(),
                    birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                    guardianNo = patientInfo.guardianNo,
                    idNo = IdCardModel.IdCardNo,
                    name = IdCardModel.Name,
                    sex = IdCardModel.Sex.ToString(),
                    address = IdCardModel.Address,
                    operId = FrameworkConst.OperatorId
                };
                PatientModel.Res病人基本信息修改 = DataHandlerEx.病人基本信息修改(PatientModel.Req病人基本信息修改);
                if (PatientModel.Res病人基本信息修改?.success ?? false)
                {
                    PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex].idNo = IdCardModel.IdCardNo;
                    ShowAlert(true, "实名认证", "临时卡实名认证成功");
                }
                else
                {
                    ShowAlert(false, "实名认证", "临时卡实名认证失败:" + PatientModel.Res病人基本信息修改?.msg);
                }
                Navigate(A.Home);
            });
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            _working = false;
            _idCardReader.UnInitialize();
            _idCardReader.DisConnect();
            return base.OnLeaving(navigationContext);
        }

        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试身份证号和姓名");
            if (!ret.IsSuccess)
                return;
            var list = ret.Value.Replace("\r\n", "\n").Split('\n');
            if (list.Length < 2)
                return;
            //CardModel.CardNo = list[0];
            IdCardModel.Name = list[1];

            IdCardModel.Sex = Convert.ToInt32(CardModel.CardNo[16]) % 2 == 0 ? Sex.女 : Sex.男;
            IdCardModel.IdCardNo = CardModel.CardNo;
            IdCardModel.Address = "浙江杭州西湖";
            IdCardModel.Birthday = DateTime.Parse(string.Format("{0}-{1}-{2}",
                CardModel.CardNo.Substring(6, 4), CardModel.CardNo.Substring(10, 2), CardModel.CardNo.Substring(12, 2)));
            IdCardModel.Nation = "汉";
            IdCardModel.GrantDept = "远图";
            IdCardModel.ExpireDate = DateTimeCore.Now;
            IdCardModel.EffectiveDate = DateTimeCore.Now.AddYears(10);
            Logger.Main.Debug($"[读取身份证信息成功]{IdCardModel.Name} {IdCardModel.Sex} {IdCardModel.IdCardNo}");
            OnGetInfo(CardModel.CardNo);
            base.DoubleClick();
        }
    }
}