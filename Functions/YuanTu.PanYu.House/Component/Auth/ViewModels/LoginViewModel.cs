using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.House.HealthManager;
using YuanTu.Devices.CardReader;
using YuanTu.PanYu.House.CardReader;
using YuanTu.PanYu.House.CardReader.HuaDa;
using YuanTu.PanYu.House.PanYuService;

namespace YuanTu.PanYu.House.Component.Auth.ViewModels
{
    public class LoginViewModel : Default.House.Component.Auth.ViewModels.LoginViewModel
    {
        public LoginViewModel(IIdCardReader[] idCardReaders, IMagCardReader[] magCardReaders,IRFCardReader[] rfCardReaders) : base(idCardReaders, magCardReaders, rfCardReaders)
        {
        }

        [Dependency]
        public ICardService CardService { get; set; }

        [Dependency]
        public IMscService MscService { get; set; }

        [Dependency]
        public new IPatientModel PatientModel { get; set; }

        private bool _running;

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
        }
        public override bool OnLeaving(NavigationContext navigationContext)
        {
            _running = false;
            return base.OnLeaving(navigationContext);
        }

        protected override void StartRead()
        {
            Task.Run(() =>
            {
                if (!Common.ReaderOpen())
                {
                    ReportService.读卡器离线(null, ErrorSolution.读卡器离线);
                    ShowAlert(false, "温馨提示", "读卡器离线");
                    return;
                }
                Common.ReaderPosBeep(25);
                _idCardReader.SetHandle(Common.readerHandle);
                _running = true;

                while (_running)
                {
                    if (M1.PICCReaderRequest())
                    {
                        var result = CardService.ReadCardExt2();
                        if (!result.IsSuccess)
                        {
                            ShowAlert(false, "温馨提示", "读卡失败", debugInfo: result.Message);
                            Thread.Sleep(1000);
                        }
                        else
                        {   //TODO 查询个人信息 民生卡
                            MscService.CardNo = result.Value.CardNo;
                            MscService.CardSeq = result.Value.CardSeq;
                            _running = false;
                            CardModel.CardType = CardType.就诊卡;
                            OnGetInfo(result.Value.CardNo);
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
                                Logger.Main.Info($"[读取身份证信息成功]{IdCardModel.Name} {IdCardModel.Sex} {IdCardModel.IdCardNo}");

                                _running = false;
                                CardModel.CardType = CardType.身份证;
                                OnGetInfo(IdCardModel.IdCardNo);
                            }
                        }
                    }

                    Thread.Sleep(200);
                }
            });
        }

       

        public override void OnGetInfo(string cardNo)
        {
            DoCommand(p =>
            {
                if (CardModel.CardType == CardType.就诊卡)
                {
                    p.ChangeText("正在查询卡片信息，请稍后...");
                    //如果是民生卡 查询睿民账户信息
                    var result = MscService.民生卡卡片信息查询();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", "民生卡信息查询失败", debugInfo: result.Message);
                        return;
                    }
                    //睿民有账户信息时 查询健康后台信息
                    var patientInfo = MscService.Res民生卡卡片信息查询?.data;

                    PatientModel.CardNo = patientInfo?.CardNo;
                    PatientModel.Name = patientInfo?.CustName;
                    PatientModel.Gender = patientInfo?.Sex == "1" ? "男" : "女";
                    PatientModel.Birthday = GetBirthday(patientInfo?.BirthDay, patientInfo?.CertNum);
                    PatientModel.Phone = patientInfo?.PhoneNum;
                    PatientModel.IDNo = patientInfo?.CertNum;
                    PatientModel.Nation = patientInfo?.Nation;
                    PatientModel.Address = patientInfo?.Adrr;

                    p.ChangeText("正在查询您的健康档案，请稍后...");

                    var req = new req查询是否已建档
                    {
                        name = PatientModel.Name,
                        idNo = PatientModel.IDNo,
                        cardNo = PatientModel.CardNo,
                        cardType = "1",
                        sex = PatientModel.Gender,
                        age = PatientModel.Birthday.Age().ToString(),
                        birthday = PatientModel.Birthday,
                        nation = PatientModel.Nation,
                        addr = PatientModel.Address,
                        expire = null,
                        photo = null
                    };
                    var res = HealthDataHandlerEx.查询是否已建档(req);
                    if (!(res?.success ?? false))
                    {
                        ShowAlert(false, "温馨提示", "健康档案信息查询失败", debugInfo: res?.msg);
                        return;
                    }
                    HealthModel.Res查询是否已建档 = res;
                    Next();
                }
                else if (CardModel.CardType == CardType.身份证)
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
                        return;
                    }
                    HealthModel.Res查询是否已建档 = res;
                    Next();
                }
            });
        }

        public virtual string GetBirthday(string birthday, string idNo)
        {
            if (!string.IsNullOrEmpty(birthday))
                return birthday.Substring(0, 4) + "-" + birthday.Substring(4, 2) + "-" + birthday.Substring(6, 2);
            if (!string.IsNullOrEmpty(idNo))
            {
                if (idNo.Length == 18)
                    return idNo.Substring(6, 4) + "-" + idNo.Substring(10, 2) + "-" + idNo.Substring(12, 2);
                return "19" + idNo.Substring(6, 2) + "-" + idNo.Substring(8, 2) + "-" + idNo.Substring(10, 2);
            }
            return null;
        }
    }
}