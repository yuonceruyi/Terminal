using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Core.Extension;
using YuanTu.Devices.CardReader;
using YuanTu.Consts.Gateway;
using YuanTu.Consts;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Consts.Models.Print;
using YuanTu.QDArea.Card;
using YuanTu.QDArea;
using YuanTu.Core.Reporter;
using YuanTu.Core.Log;
using System.Windows.Input;
using Prism.Commands;
using YuanTu.QDHD2ZY.Component.Auth.Models;
using YuanTu.QDArea.QingDaoSiPay;

namespace YuanTu.QDHD2ZY.Component.Auth.ViewModels
{
    class PatientInfoViewModel : YuanTu.QDKouQiangYY.Component.Auth.ViewModels.PatientInfoViewModel
    {
        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser) : base(rfCardDispenser)
        {

        }
        public override void Confirm()
        {
            if (Name.IsNullOrWhiteSpace())
            {
                ModifyNameCommand.Execute(null);
                return;
            }
            if (Phone.IsNullOrWhiteSpace())
            {
                ShowUpdatePhone = true;
                return;
            }
            DoCommand(lp =>
            {
                if (ChoiceModel.Business == Business.建档 ||
                    CardModel.CardType == CardType.社保卡 &&
                    CardModel.ExternalCardInfo == "建档")
                {
                    switch (CreateModel.CreateType)
                    {
                        case CreateType.成人:
                            CreatePatient(lp);
                            break;
                        case CreateType.儿童:
                            Navigate(A.CK.InfoEx);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    return;
                }
                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                var innerModel = (CardModel as CardModel);
                string showMsg;
                if (CardModel.CardType == CardType.社保卡&&
                    !innerModel.PersonNo.IsNullOrWhiteSpace())
                {
                    decimal siRemain=0;
                    var sign = Function.GetRemain(innerModel.PersonNo, ref siRemain);
                    if (sign)
                    {
                        showMsg = $"{patientInfo.name}\r\n余额{patientInfo.accBalance.InRMB()} 医保{siRemain}";
                    }
                    else
                    {
                        showMsg = $"{patientInfo.name}\r\n余额{patientInfo.accBalance.In元()}";
                    }
                }
                else
                {
                    showMsg = $"{patientInfo.name}\r\n余额{patientInfo.accBalance.In元()}";
                }
                ChangeNavigationContent(showMsg)
                ;
                Next();
            });
        }
    }
}
