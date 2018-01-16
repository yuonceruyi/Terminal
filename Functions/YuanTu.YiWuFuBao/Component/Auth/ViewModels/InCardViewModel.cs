using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Navigating;
using YuanTu.Core.Systems;
using YuanTu.Devices.CardReader;
using YuanTu.YiWuArea.Dialog;
using YuanTu.YiWuArea.Insurance;
using YuanTu.YiWuArea.Insurance.Models;
using YuanTu.YiWuArea.Insurance.Models.Base;
using YuanTu.YiWuFuBao.Models;

namespace YuanTu.YiWuFuBao.Component.Auth.ViewModels
{
    public class InCardViewModel: YuanTu.YiWuFuBao.Component.Auth.ViewModels.CardViewModel
    {
        public InCardViewModel(IRFCpuCardReader[] rfCpuCardReader) : base(rfCpuCardReader)
        {
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            ShowSiCardAnimation = Visibility.Visible;
            ShowBarCodeCardAnimation = Visibility.Collapsed;
            HospitalInputFocus = false;
        }

        protected override void ConfirmHospitalCardNo()
        {
            //不进行任何处理
        }

        protected override bool ProcessWithSiData(LoadingProcesser ctx, Res获取参保人员信息 res)
        {
            var ptInfoRest = PatientIcData.Deserialize(res.写卡后IC卡数据);
            if (!ptInfoRest)
            {
                ShowAlert(false, "社保读卡失败", ptInfoRest.Message);
                return false;
            }
            if (ptInfoRest.Value.姓名!=PatientModel.住院患者信息.name)
            {
                ShowAlert(false, "身份信息不匹配",$"社保卡姓名为【{ptInfoRest.Value.姓名}】,与住院登记档案中的姓名【{PatientModel.住院患者信息.name}】不一致，不允许结算！");
                return false;
            }
            Next();
            return true;
        }
    }
}
