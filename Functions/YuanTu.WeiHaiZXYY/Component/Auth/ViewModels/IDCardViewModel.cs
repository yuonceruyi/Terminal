using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Log;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;

namespace YuanTu.WeiHaiZXYY.Component.Auth.ViewModels
{
    public class IDCardViewModel : YuanTu.Default.Component.Auth.ViewModels.IDCardViewModel
    {
        public IDCardViewModel(IIdCardReader[] idCardReaders) : base(idCardReaders)
        {
            _idCardReader = idCardReaders.FirstOrDefault(p => p.DeviceId == "MyHuaDa_900");
            //_idCardReader = idCardReaders.FirstOrDefault(p => p.DeviceId == "Xzx_XZX");
        }

        protected override void OnGetInfo(string idCardNo)
        {
            DoCommand(ctx =>
            {
                if (ChoiceModel.Business == Business.建档)
                {
                    if (CreateModel.CreateType == CreateType.成人)
                    {
                        Navigate(A.CK.Info);
                    }
                    else
                    {
                        Navigate(A.CK.InfoEx);
                    }
                }
                else
                {
                    PatientModel.Req病人信息查询 = new req病人信息查询
                    {
                        Timeout = new TimeSpan(0, 1, 0),//读取病人信息设置超时1分钟
                        cardNo = idCardNo,
                        cardType = ((int)CardModel.CardType).ToString(),
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

        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试身份证号和姓名");
            if (!ret.IsSuccess)
                return;
            var list = ret.Value.Replace("\r\n", "\n").Split('\n');
            if (list.Length < 2)
                return;
            CardModel.CardNo = DateTimeCore.Now.ToString("yyyyMMss");
            IdCardModel.Name = list[1];

            IdCardModel.Sex = Sex.男;
            IdCardModel.IdCardNo = DateTimeCore.Now.ToString("yyyyMMss").PadLeft(18,'3');
            IdCardModel.Address = "浙江杭州西湖";
            IdCardModel.Birthday = DateTimeCore.Now;
            IdCardModel.Nation = "汉";
            IdCardModel.GrantDept = "远图";
            IdCardModel.ExpireDate = DateTimeCore.Now;
            IdCardModel.EffectiveDate = DateTimeCore.Now.AddYears(10);
            Logger.Main.Debug($"[读取身份证信息成功]{IdCardModel.Name} {IdCardModel.Sex} {IdCardModel.IdCardNo}");
            OnGetInfo(CardModel.CardNo);
        }
    }
}
