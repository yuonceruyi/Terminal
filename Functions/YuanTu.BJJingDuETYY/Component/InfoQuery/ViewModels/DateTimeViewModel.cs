using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;

namespace YuanTu.BJJingDuETYY.Component.InfoQuery.ViewModels
{
    public class DateTimeViewModel : Default.Component.InfoQuery.ViewModels.DateTimeViewModel
    {
        protected override void InPrePayRecordQuery()
        {
            if (DateTimeStart > DateTimeEnd)
            {
                ShowAlert(false, "住院押金查询", "开始时间不能晚于结束时间！");
                return;
            }

            DoCommand(lp =>
            {
                lp.ChangeText("正在查询住院押金信息，请稍候...");
                InPrePayRecordModel.Req住院预缴金充值记录查询 = new req住院预缴金充值记录查询
                {
                    patientId = PatientModel.住院患者信息.patientHosId,
                    startDate = DateTimeStart.ToString("yyyy-MM-dd"),
                    endDate = DateTimeEnd.ToString("yyyy-MM-dd"),
                };
                InPrePayRecordModel.Res住院预缴金充值记录查询 = DataHandlerEx.住院预缴金充值记录查询(InPrePayRecordModel.Req住院预缴金充值记录查询);
                if (InPrePayRecordModel.Res住院预缴金充值记录查询?.success ?? false)
                {
                    if (InPrePayRecordModel.Res住院预缴金充值记录查询?.data?.Count > 0)
                    {
                        ChangeNavigationContent($"{DateTimeStart.ToString("yyyy-MM-dd")}至\n{DateTimeEnd.ToString("yyyy-MM-dd")}");
                        Next();
                        return;
                    }
                    ShowAlert(false, "住院押金查询", "没有获得住院押金信息(列表为空)");
                    return;
                }
                ShowAlert(false, "住院押金查询", "没有获得住院押金信息");

            });
        }
    }
}
