using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.NingXiaHospital.HisService.Request;

namespace YuanTu.NingXiaHospital.Component.Auth.ViewModels
{
    public class IDCardViewModel : Default.Component.Auth.ViewModels.IDCardViewModel
    {
        public IDCardViewModel(IIdCardReader[] idCardReaders) : base(idCardReaders)
        {
            _idCardReader = CurrentStrategyType() != DeviceType.Clinic ? idCardReaders.FirstOrDefault(p => p.DeviceId == "Xzx_XZX") : idCardReaders.FirstOrDefault(p => p.DeviceId == "HuaDa_HUADA");
        }

        protected override void OnGetInfo(string idCardNo)
        {
            DoCommand(ctx =>
            {
                if (ChoiceModel.Business == Business.缴费)
                {
                    if (CardModel.CardType == CardType.银行卡)
                    {
                        var type = "1";
                        InitData(type);
                        Navigate(A.CK.Info);
                    }
                    else
                    {
                        ctx.ChangeText("开始查询签约记录");
                        Logger.Main.Info("开始查询签约记录 入参构造结束");
                        var resreqPatientSignInfoQuery = HisService.DllHandler.QueryPatientSignInfo("1", IdCardModel.IdCardNo);
                        if (resreqPatientSignInfoQuery)
                        {
                            if (resreqPatientSignInfoQuery.Value.redata == null)
                            {
                                InitData("1");
                            }
                            else
                            {
                                InitData("0", resreqPatientSignInfoQuery.Value.redata.head.patient_id);
                            }
                            Navigate(A.CK.Info);
                            return;
                        }
                        ShowAlert(false, "信息查询", $"签约信息查询失败：{resreqPatientSignInfoQuery.Message}");
                    }
                }
                else
                {
                    CardModel.CardType = CardType.身份证;
                    PatientModel.Req病人信息查询 = new req病人信息查询
                    {
                        Timeout = new TimeSpan(0, 1, 0),
                        cardNo = idCardNo,
                        cardType = ((int)CardModel.CardType).ToString(),
                        patientName = IdCardModel.Name
                    };
                    PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

                    if (!PatientModel.Res病人信息查询.success || PatientModel.Res病人信息查询.data == null ||
                        PatientModel.Res病人信息查询.data.Count == 0)
                        InitData("2");
                    Next();
                }
            });
        }

        private void InitData(string patientType,string patientId=null)
        {
            Logger.Net.Info("InitData||" + patientType);
            PatientModel.Res病人信息查询 = new res病人信息查询();
            PatientModel.Res病人信息查询.data = new List<病人信息>();
            PatientModel.Res病人信息查询.data.Add(new 病人信息
            {
                patientId =string.IsNullOrEmpty(patientId)? IdCardModel.IdCardNo: patientId,
                name = IdCardModel.Name,
                sex = IdCardModel.Sex == Sex.男 ? "男" : "女",
                birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                idNo = IdCardModel.IdCardNo,
                phone = null,
                patientType = patientType //"0:已经签约" "1:未签约" "2:未查到病人信息"
            });
        }

        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试身份证号和姓名");
            if (!ret.IsSuccess)
                return;
            var list = ret.Value.Replace("\r\n", "\n").Split('\n');
            if (list.Length < 2)
                return;
            IdCardModel.IdCardNo = list[0];
            IdCardModel.Name = list[1];
            IdCardModel.Sex = Sex.男;
            IdCardModel.Address = "浙江杭州西湖";
            IdCardModel.Nation = "汉";
            IdCardModel.GrantDept = "远图";
            IdCardModel.ExpireDate = DateTimeCore.Now;
            IdCardModel.EffectiveDate = DateTimeCore.Now.AddYears(10);
            Logger.Main.Debug($"[读取身份证信息成功]{IdCardModel.Name} {IdCardModel.Sex} {IdCardModel.IdCardNo}");
            OnGetInfo(IdCardModel.IdCardNo);
        }

        protected override void FillIdCardModel(IdCardDetail detail)
        {
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

    }
}