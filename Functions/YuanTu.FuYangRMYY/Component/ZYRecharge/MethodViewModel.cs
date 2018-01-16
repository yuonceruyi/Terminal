using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Recharge;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Default.Component.ViewModels;
using SerialPrinter = YuanTu.Devices.Printer;

namespace YuanTu.FuYangRMYY.Component.ZYRecharge.ViewModels
{
    public class MethodViewModel : YuanTu.Default.Component.ZYRecharge.ViewModels.MethodViewModel
    {
        
        protected override void FillRechargeRequest(req住院预缴金充值 req)
        {
            req.patientHosNo = PatientModel.住院患者信息.patientHosId;
            req.patientHosId= PatientModel.住院患者信息.patientHosId;
            req.cardNo = PatientModel.住院患者信息.cardNo;
            req.patientId = PatientModel.住院患者信息.accountNo;
            req.accountNo = PatientModel.住院患者信息.accountNo;
            base.FillRechargeRequest(req);
        }

    }
}