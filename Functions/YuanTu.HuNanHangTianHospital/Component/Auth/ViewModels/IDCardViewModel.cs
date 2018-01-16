using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Log;
using YuanTu.Devices.CardReader;

namespace YuanTu.HuNanHangTianHospital.Component.Auth.ViewModels
{
    public class IDCardViewModel: YuanTu.Default.Component.Auth.ViewModels.IDCardViewModel
    {
        public IDCardViewModel(IIdCardReader[] idCardReaders) : base(idCardReaders)
        {
            Logger.Main.Info("HuaDa_900");
            _idCardReader = idCardReaders.FirstOrDefault(p => p.DeviceId == "HuaDa_900");
            //_idCardReader = idCardReaders.FirstOrDefault(p => p.DeviceId == "Xzx_XZX");
        }

        protected override void OnGetInfo(string idCardNo)
        {
            DoCommand(ctx =>
            {
                if (ChoiceModel.Business == Business.建档)
                {
                    //Navigate(A.CK.Info);
                    Logger.Main.Info($"进入到 身份证病人信息查询 卡号：{idCardNo}  卡类型：{CardModel.CardType} 名称：{IdCardModel.Name} ");
                    PatientModel.Req病人信息查询 = new req病人信息查询
                    {
                        cardNo = idCardNo,
                        cardType = ((int)CardModel.CardType).ToString(),
                        patientName = IdCardModel.Name
                    };
                    PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

                    if (PatientModel.Res病人信息查询.success)
                    {
                        if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                        {
                            Navigate(A.CK.Info);
                        }
                        else
                        {
                            ShowAlert(false, "友情提示", "该病人已经建档。如需补卡，窗口办理");
                        }
                    }
                    else
                    {
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
                        // StartRead();
                        Navigate(A.Home);
                    }
                }
                else
                {
                    PatientModel.Req病人信息查询 = new req病人信息查询
                    {
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
