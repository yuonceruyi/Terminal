using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.FuYangRMYY.HisNative;
using YuanTu.FuYangRMYY.HisNative.Models;
using YuanTu.FuYangRMYY.Managers;
using YuanTu.FuYangRMYY.Services;

namespace YuanTu.FuYangRMYY.Component.TakeNum
{
    public class TakeNumViewModel:Default.Component.TakeNum.ViewModels.TakeNumViewModel
    {
        protected override Queue<IPrintable> TakeNumPrintables()
        {
            var queue = PrintManager.NewQueue("取号单");
            var patientInfo = PatientModel.当前病人信息;
            var takeNum = TakeNumModel.Res预约取号.data;
            var record = RecordModel.所选记录;
            var sb = new StringBuilder();
            sb.Append($"状态：取号成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"登记号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：预约取号\n");
            sb.Append($"科室名称：{record.deptName}\n");
            sb.Append($"就诊医生：{record.doctName}\n");
            sb.Append($"诊疗费：{record.treatFee.In元()}\n");
            sb.Append($"挂号金额：{record.regAmount.In元()}\n");
            sb.Append($"就诊场次：{record.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{takeNum?.address}\n");
            if (record.medAmPm.SafeToAmPmEnum() == AmPmSession.上午)
            {
                sb.Append($"挂号序号：{takeNum?.appoNo}\n");
            }
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"机器编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
        private void PasswordNotify()
        {
            var sm = GetInstance<IShellViewModel>();
            if (!sm.Busy.IsBusy)
            {
                sm.Busy.IsBusy = true;
            }
            sm.Busy.BusyContent = "请输入6位社保卡密码，并按“确认”键";
            PlaySound("请输入社保卡密码并按确认键结束");
        }
        private Result<社保挂号结算> PayWithSi(LoadingProcesser lp)
        {
            lp.ChangeText("正在获取取号参数信息，请稍后...");
            SiOperatorManager.StartMonitor(PasswordNotify);
            var insurancepara = HisService.GetRegisterInsuranceParams(PatientModel.当前病人信息, RecordModel.所选记录.orderNo);
            if (!insurancepara.IsSuccess)
            {
                return Result<社保挂号结算>.Fail($"从HIS系统中获取社保参数失败!\r\n{insurancepara.Message}");
            }
            lp.ChangeText("正在社保交易，请稍后...");
            var resp = HisInsuranceService.InsuOPReg(0, "1", RecordModel.所选记录.orderNo, "1",
                "26", insurancepara.Value.ExtStr);
            var takeNum = TakeNumModel as Models.TakeNumModel;
            takeNum.社保挂号结算 = resp.Value;
            takeNum.ExpString = insurancepara.Value.ExtStr;
            SiOperatorManager.StopMonitor();
            return resp;
        }
    }
}
