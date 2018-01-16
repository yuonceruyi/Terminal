using System;
using YuanTu.ChongQingArea.SiHandler;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Devices.CardReader;

namespace YuanTu.ChongQingArea.Component.Auth.ViewModels
{
    public class InSiCardViewModel : SiCardViewModel
    {
        public InSiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders, IRFCardDispenser[] rfCardDispenser) : base(icCardReaders, rfCpuCardReaders,rfCardDispenser)
        {
        }

        protected override void Query(Res获取人员基本信息 siInfo)
        {
            var req = new req住院患者信息查询
            {
                Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                cardNo = siInfo.身份证号,
                cardType = ((int)CardType.身份证).ToString(),
                patientName = siInfo.姓名
            };
            var res = DataHandlerEx.住院患者信息查询(req);
            PatientModel.Res住院患者信息查询 = res;
            SiModel.NeedCreate = false;
            if (!res.success)
            {
                //var code = PatientModel.Res病人信息查询.code;
                //if (code == -2 || code == -4 || code == -100)
                //{
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
                    return;
                //}
                //SiModel.NeedCreate = true;
                //IdCardModel.Name = siInfo.姓名;
                //IdCardModel.Sex = siInfo.性别.SafeToSex();
                //IdCardModel.IdCardNo = siInfo.身份证号;
                //IdCardModel.Address = siInfo.住址;
                //IdCardModel.Birthday = Convert.ToDateTime(siInfo.身份证号.Substring(6, 8).SafeConvertToDate("yyyyMMdd", "yyyy-MM-dd"));
                //IdCardModel.Nation = siInfo.民族;
                //Next();
                //return;
            }
            if (res.data == null)
            {
                ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                return;
            }
            Next();
        }
    }
}