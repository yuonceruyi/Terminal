using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Core.Log;
using YuanTu.QDArea.QingDaoSiPay;
using YuanTu.Devices.CardReader;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Core.Gateway;
using YuanTu.Core.Navigating;
using YuanTu.Core.Extension;

namespace YuanTu.QDQLYY.Component.Tools.ViewModels
{
    class SiPayViewModel : YuanTu.QDKouQiangYY.Component.Tools.ViewModels.SiPayViewModel
    {
        public SiPayViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {
         
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
        }

        protected override void InitDeptCode()
        {
            switch (ChoiceModel.Business)
            {
                case Business.挂号:
                    DeptCode = ScheduleModel.所选排班.deptCode;
                    break;
                default:
                    DeptCode = "0003";
                    break;
            }
        }
    }

}
