using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Consts.Gateway;
using YuanTu.Consts;
using System.Linq;
using YuanTu.Consts.Models.Payment;
using System.Collections.Generic;

namespace YuanTu.ShenZhen.BaoAnChineseMedicineHospital.Component.TakeNum.ViewModels
{
    public class QueryViewModel : YuanTu.Default.Component.TakeNum.ViewModels.QueryViewModel
    {
        public override string Title => "输入订单号";

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);

            ShowAlert(true, "温馨提示", "订单号可选择输入\n输入订单号可以更精确的定位到预约记录");
        }


        public override void Do()
        {
            //if (RegNo.IsNullOrWhiteSpace())
            //{
            //    ShowAlert(false, "订单号", "请输入订单号！");
            //    return;
            //}
            if (!string.IsNullOrEmpty(RegNo))
                AppoRecordModel.RegNo = RegNo.Trim();
            else
                AppoRecordModel.RegNo = "";
            QueryAppoInfo();
        }

        public override void QueryAppoInfo()
        {
            DoCommand(lp =>
           {
               var camera = GetInstance<ICameraService>();
               camera.SnapShot("主界面 预约取号");
               var patientModel = GetInstance<IPatientModel>();
               var recordModel = GetInstance<IAppoRecordModel>();
               var cardModel = GetInstance<ICardModel>();
               var takeNumModel = GetInstance<ITakeNumModel>();
               lp.ChangeText("正在查询预约记录，请稍候...");
               recordModel.Req挂号预约记录查询 = new req挂号预约记录查询
               {
                   patientId = patientModel.当前病人信息?.patientId,
                   patientName = patientModel.当前病人信息?.name,
                   startDate = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                   endDate = DateTimeCore.Now.AddDays(7).ToString("yyyy-MM-dd"),
                   searchType = "1",
                   cardNo = cardModel.CardNo,
                   cardType = ((int)cardModel.CardType).ToString(),
                   appoNo = AppoRecordModel.RegNo
               };
               recordModel.Res挂号预约记录查询 = DataHandlerEx.挂号预约记录查询(recordModel.Req挂号预约记录查询);
               if (recordModel.Res挂号预约记录查询?.success ?? false)
               {
                   if (recordModel.Res挂号预约记录查询?.data?.Count > 1)
                   {
                       Navigate(A.QH.Record);
                       return;
                   }
                   if (recordModel.Res挂号预约记录查询?.data?.Count == 1)
                   {
                       recordModel.所选记录 = recordModel.Res挂号预约记录查询.data.FirstOrDefault();
                       var record = recordModel.所选记录;

                       takeNumModel.List = new List<PayInfoItem>
                       {
                            new PayInfoItem("就诊日期：", record.medDate.SafeConvertToDate("yyyy-MM-dd", "yyyy年MM月dd日")),
                            new PayInfoItem("就诊科室：", record.deptName),
                            new PayInfoItem("就诊医生：", record.doctName),
                            new PayInfoItem("就诊时段：", record.medAmPm.SafeToAmPm()),
                            new PayInfoItem("就诊序号：", record.appoNo),
                            new PayInfoItem("挂号金额：", record.regAmount.In元(), true)
                       };
                       Next();
                       return;
                        //return Result<FormContext>.Success(new FormContext(A.QuHao_Context, A.QH.TakeNum));
                    }
                   ShowAlert(false, "预约记录查询", "系统未能获取到该卡有对应的预约记录\n请检查订单号或去人工窗口直接取号");
                   return;
                }
               ShowAlert(false, "预约记录查询", "没有获得预约记录信息", debugInfo: recordModel.Res挂号预约记录查询?.msg);
           });
        }
    }
}