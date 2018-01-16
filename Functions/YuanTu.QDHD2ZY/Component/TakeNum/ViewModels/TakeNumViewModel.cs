using System;
using System.Windows;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using Prism.Commands;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;
using YuanTu.Default.Component.Tools.Models;
using System.Collections.Generic;
using YuanTu.QDHD2ZY.Component.Auth.Models;
using YuanTu.QDHD2ZY.CurrentService;
using YuanTu.QDArea;
using YuanTu.QDArea.Enums;
using YuanTu.QDArea.Models.TakeNum;

namespace YuanTu.QDHD2ZY.Component.TakeNum.ViewModels
{
    public class TakeNumViewModel : YuanTu.QDKouQiangYY.Component.TakeNum.ViewModels.TakeNumViewModel
    {
        protected override void CancelAction()
        {
            var record = RecordModel.所选记录;
            var textblock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 15, 0, 0)
            };
            textblock.Inlines.Add("\r\n您确定要取消");
            textblock.Inlines.Add(new TextBlock { Text = $"{ record.medDate.SafeConvertToDate("yyyy-MM-dd", "yyyy-MM-dd") } { record.medAmPm.SafeToAmPm() } { record.deptName }", Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0)) });
            textblock.Inlines.Add("\r\n的预约吗？\r\n\r\n\r\n\r\n");
            ShowConfirm("友好提醒", textblock, b =>
            {
                if (!b) return;
                DoCommand(lp =>
                {
                    lp.ChangeText("正在进行取消预约，请稍候...");

                    var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];


                    CancelAppoModel.Req取消预约 = new req取消预约
                    {
                        appoNo = record.appoNo,
                        patientId = patientInfo.patientId,
                        operId = FrameworkConst.OperatorId,
                        regMode = "1",
                        cardNo = CardModel.CardNo,
                        cardType = ((int)CardModel.CardType).ToString(),
#pragma warning disable 612
                        medDate = record.medDate,
                        scheduleId = record.scheduleId,
                        medAmPm = record.medAmPm,
                        orderNo = record.orderNo
#pragma warning restore 612
                    };
                    CancelAppoModel.Res取消预约 = DataHandlerEx.取消预约(CancelAppoModel.Req取消预约);
                    if (CancelAppoModel.Res取消预约?.success ?? false)
                    {
                        ShowAlert(true, "取消预约", "您已取消预约成功");
                        RecordModel.Res挂号预约记录查询.data.Remove(RecordModel.所选记录);
                        Navigate(A.Home);
                    }
                    else
                    {
                        ShowAlert(false, "取消预约", "取消预约失败", debugInfo: CancelAppoModel.Res取消预约?.msg);


                    }

                });

            }, 60, ConfirmExModel.Build("是", "否", true));
        }

        protected override void FillBaseRequest()
        {
            var patientInfo = PatientModel.当前病人信息;
            var record = RecordModel.所选记录;

            TakeNumExtendModel.version = ConfigQD.ScheduleVersion;
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.预缴金 || PaymentModel.NoPay)
            {
                TakeNumExtendModel.payStatus = RecordModel.所选记录.payStatus;
            }
            else
            {
                TakeNumExtendModel.payStatus = ((int)ApptPayStatus.支付成功).ToString();
            }

            TakeNumModel.Req预约取号 = new req预约取号
            {
                patientId = patientInfo.patientId,
                cardType = ((int)CardModel.CardType).ToString(),
                cardNo = CardModel.CardNo,

                appoNo = RecordModel.所选记录.appoNo,
                orderNo = RecordModel.所选记录.orderNo,

                operId = FrameworkConst.OperatorId,
                tradeMode = PaymentModel.NoPay ? PayMethod.预缴金.GetEnumDescription() : PaymentModel.PayMethod.GetEnumDescription(),
                accountNo = patientInfo.patientId,
                cash = PaymentModel.Total.ToString(),
#pragma warning disable 612
                medDate = record.medDate,
                scheduleId = record.scheduleId,
                medAmPm = record.medAmPm,
#pragma warning restore 612
                extend = TakeNumExtendModel.ToJsonString(),
            };
        }

        /// <summary>
        /// 本地化参数追加
        /// </summary>
        /// <param name="req"></param>
        protected virtual void LocalRequest(req预约取号 req)
        {
            var ybinfo = new Models.YbInfoModel();

            var innerModel = (CardModel as CardModel);
            if (CardModel.CardType == CardType.社保卡)
            {
                //门诊大病审批信息
                if (!innerModel.PersonNo.IsNullOrWhiteSpace())
                {
                    ybinfo.IsMZDB = QDArea.QingDaoSiPay.Function.isDiseaseSp(innerModel.PersonNo);
                }

                //获取珠海胶南信息
                if (!innerModel.UnitName.IsNullOrWhiteSpace())
                {
                    if (SpecialDistrictsService.DicSpecialDistricts != null && SpecialDistrictsService.DicSpecialDistricts.ContainsKey(innerModel.UnitName))
                    {
                        ybinfo.LocalTreatType = SpecialDistrictsService.DicSpecialDistricts[innerModel.UnitName];
                    }
                }
                req.ybInfo = ybinfo.ToJsonString();
            }

        }
    }
}
