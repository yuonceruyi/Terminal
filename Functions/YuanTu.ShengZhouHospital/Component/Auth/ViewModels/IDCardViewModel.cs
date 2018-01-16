using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Devices.CardReader;
using YuanTu.ShengZhouHospital.Component.Auth.Models;

namespace YuanTu.ShengZhouHospital.Component.Auth.ViewModels
{
    public class IDCardViewModel : YuanTu.Default.Component.Auth.ViewModels.IDCardViewModel
    {
        public IDCardViewModel(IIdCardReader[] idCardReaders) : base(idCardReaders)
        {
           //_idCardReader = idCardReaders.FirstOrDefault(p => p.DeviceId == "HuaDa_900");
           //_idCardReader = idCardReaders.FirstOrDefault(p => p.DeviceId == "HuaDa900_Id");
            _idCardReader = idCardReaders.FirstOrDefault(p => p.DeviceId == "Xzx_XZX");
        }

        protected override void OnGetInfo(string idCardNo)
        {
            DoCommand(ctx =>
            {
                if (ChoiceModel.Business == Business.建档)
                {
                    Navigate(A.CK.Info);
                }
                else if (CardModel.ExternalCardInfo=="补全身份信息")
                {
                    var pm = PatientModel as PatientInfoModel;
                    if (IdCardModel.Name!= pm?.Res门诊读卡?.姓名)
                    {
                        ShowAlert(false,"身份信息不匹配",$"就诊卡姓名为【{pm?.Res门诊读卡?.姓名}】与您身份证信息不匹配！",extend:new AlertExModel()
                        {
                            HideCallback = atp =>
                            {
                                if (atp==AlertHideType.ButtonClick)
                                {
                                    StartRead();
                                }
                            }
                        });
                        return;
                    }
                    Navigate(A.CK.Info);
                }
                else
                {
                    PatientModel.Req病人信息查询 = new req病人信息查询
                    {
                        Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
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
    }
}
