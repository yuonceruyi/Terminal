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

namespace YuanTu.ShenZhen.PekingUniversityHospital.Component.Auth.ViewModels
{
    public class IDCardViewModel : YuanTu.Default.Component.Auth.ViewModels.IDCardViewModel
    {

        public IDCardViewModel(IIdCardReader[] idCardReaders):base(idCardReaders)
        {
            _idCardReader = idCardReaders.FirstOrDefault(p => p.DeviceId == "Xzx_XZX");
        }

       
        protected override void StartRead()
        {
            _working = true;
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
                            FillIdCardModel(result.Value);
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

        protected override void FillIdCardModel(IdCardDetail detail)
        {
            if (CardModel.ExternalCardInfo != "社保_信息补全")
            {
                CardModel.CardNo = detail.IdCardNo;

            }
            IdCardModel.Name = detail.Name;
            IdCardModel.Sex = detail.Sex;
            IdCardModel.IdCardNo = detail.IdCardNo;
            IdCardModel.Address = detail.Address;
            IdCardModel.Birthday = detail.Birthday;
            IdCardModel.Nation = detail.Nation;
            IdCardModel.GrantDept = detail.GrantDept;
            IdCardModel.ExpireDate = detail.ExpireDate;
            IdCardModel.EffectiveDate = detail.EffectiveDate;
            IdCardModel.PortraitPath = detail.PortraitPath;
            Logger.Main.Info($"[读取身份证信息成功]{IdCardModel.Name} {IdCardModel.Sex} {IdCardModel.IdCardNo}");
        }

        protected override void OnGetInfo(string idCardNo)
        {
            DoCommand(ctx =>
            {
                if (ChoiceModel.Business == Business.建档)
                {
                    Navigate(A.CK.Info);
                }
                else
                {
                    PatientModel.Req病人信息查询 = new req病人信息查询
                    {
                        Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                        cardNo = idCardNo,
                        cardType = ((int) CardModel.CardType).ToString(),
                        patientName = IdCardModel.Name
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
                        //CardModel.CardNo = PatientModel.Res病人信息查询?.data[0]?.idNo;
                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
                        StartRead();
                    }
                }
            });
        }


        //public override void DoubleClick()
        //{
        //    //CardModel.CardNo = "330682199101231225";
        //    //IdCardModel.Name = "潘佳虹";
        //    //IdCardModel.Sex = Sex.男;
        //    //IdCardModel.IdCardNo = CardModel.CardNo;
        //    //IdCardModel.Address = "浙江杭州西湖";
        //    //IdCardModel.Birthday = DateTimeCore.Now;
        //    //IdCardModel.Nation = "汉";
        //    //IdCardModel.GrantDept = "远图";
        //    //IdCardModel.ExpireDate = DateTimeCore.Now;
        //    //IdCardModel.EffectiveDate = DateTimeCore.Now.AddYears(10);

        //    //CardModel.CardNo = "340101196407018016";
        //    var ret = InputTextView.ShowDialogView("输入测试身份证号和姓名");
        //    if (!ret.IsSuccess)
        //        return;
        //    var list = ret.Value.Replace("\r\n", "\n").Split('\n');
        //    if (list.Length < 2)
        //        return;
        //    CardModel.CardNo = list[0];
        //    IdCardModel.Name = list[1];

        //    IdCardModel.Sex = Convert.ToInt32(CardModel.CardNo[16]) % 2 == 0 ? Sex.女 : Sex.男;
        //    IdCardModel.IdCardNo = CardModel.CardNo;
        //    IdCardModel.Address = "浙江杭州西湖";
        //    IdCardModel.Birthday = DateTime.Parse(string.Format("{0}-{1}-{2}",
        //        CardModel.CardNo.Substring(6, 4), CardModel.CardNo.Substring(10, 2), CardModel.CardNo.Substring(12, 2)));
        //    IdCardModel.Nation = "汉";
        //    IdCardModel.GrantDept = "远图";
        //    IdCardModel.ExpireDate = DateTimeCore.Now;
        //    IdCardModel.EffectiveDate = DateTimeCore.Now.AddYears(10);
        //    Logger.Main.Debug($"[读取身份证信息成功]{IdCardModel.Name} {IdCardModel.Sex} {IdCardModel.IdCardNo}");
        //    OnGetInfo(CardModel.CardNo);
        //    base.DoubleClick();
        //}
    }
}