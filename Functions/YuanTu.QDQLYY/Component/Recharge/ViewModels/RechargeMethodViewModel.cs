using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.DB;
using YuanTu.Core.Log;

namespace YuanTu.QDQLYY.Component.Recharge.ViewModels
{
    public class RechargeMethodViewModel :YuanTu.QDKouQiangYY.Component.Recharge.ViewModels.RechargeMethodViewModel
    {
        protected override void OnCAClick()
        {

            if (ConfigurationManager.GetValueInt("StopRecharge:Enabled") == 1)
            {
                DateTime dtime;
                var time = ConfigurationManager.GetValue("StopRecharge:Time");
                if (DateTime.TryParseExact(time, "HH:mm", null, System.Globalization.DateTimeStyles.None, out dtime))
                {
                    if (DateTimeCore.Now >
                        new DateTime(DateTimeCore.Now.Year, DateTimeCore.Now.Month, DateTimeCore.Now.Day, dtime.Hour,
                            dtime.Minute, 0))
                    {
                        ShowAlert(false, "现金充值", $"{time}后停止现金充值\r\n请使用其他支付方式，如需现金充值，请到人工窗口");
                        return;
                    }
                }
            }

            Navigate(A.Third.Cash);
        }
    }
}
