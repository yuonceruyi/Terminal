using System;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Core.FrameworkBase;
using System.Collections.Generic;
using YuanTu.Core.Log;
using System.Diagnostics;
using YuanTu.Consts.Models;
using System.Threading;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Component.InfoQuery.ViewModels
{
    public class DateTimeViewModel : Default.Component.InfoQuery.ViewModels.DateTimeViewModel
    {
        private static Lis4Print.TCPrint Lis4 = new Lis4Print.TCPrint();

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        [Dependency]
        public IInDailyDetailModel InDailyDetailModel { get; set; }


        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
        }
        protected override void Confirm()
        {
            if (DateTimeStart > DateTimeEnd)
            {
                ShowAlert(false, "友好提示", "开始时间不能晚于结束时间！");
                return;
            }
            if (DateTimeStart.AddMonths(6) < DateTimeEnd)
            {
                ShowAlert(false, "友好提示", "查询时间区间不能超过6个月！");
                return;
            }

            switch (ChoiceModel.Business)
            {
                case Business.未定义:
                    break;
                case Business.建档:
                    break;
                case Business.挂号:
                    break;
                case Business.预约:
                    break;
                case Business.取号:
                    break;
                case Business.缴费:
                    break;
                case Business.充值:
                    break;
                case Business.查询:
                    break;
                case Business.住院押金:
                    break;
                case Business.出院结算:
                    break;
                case Business.健康服务:
                    break;
                case Business.体测查询:
                    break;
                case Business.外院卡注册:
                    break;
                case Business.补打:
                    break;
                case Business.实名认证:
                    break;
                case Business.生物信息录入:
                    break;
                case Business.药品查询:
                    break;
                case Business.项目查询:
                    break;
                case Business.已缴费明细:
                    break;
                case Business.检验结果:
                    QueryChoiceModel.InfoQueryType = InfoQueryTypeEnum.检验结果;
                    break;
                case Business.影像结果:
                    break;
                case Business.住院一日清单:
                    QueryChoiceModel.InfoQueryType = InfoQueryTypeEnum.住院一日清单;
                    break;
                case Business.住院押金查询:
                    break;
                case Business.住院床位查询:
                    break;
                case Business.执业资格查询:
                    break;
                case Business.交易记录查询:
                    break;
                case Business.门诊排班查询:
                    break;
                case Business.材料费查询:
                    break;
                case Business.收银:
                    break;
                case Business.签到:
                    break;
                default:
                    break;
            }

            switch (QueryChoiceModel.InfoQueryType)
            {
                case InfoQueryTypeEnum.已缴费明细:
                    PayCostQuery();
                    break;
                case InfoQueryTypeEnum.药品查询:
                    break;
                case InfoQueryTypeEnum.项目查询:
                    break;
                case InfoQueryTypeEnum.检验结果:
                    DiagReportQuery();
                    break;
                case InfoQueryTypeEnum.交易记录查询:
                    RechargeRecorQuery();
                    break;
                case InfoQueryTypeEnum.住院押金查询:
                    InPrePayRecordQuery();
                    break;
                case InfoQueryTypeEnum.影像结果:
                    PacsReportQuery();
                    break;
                case InfoQueryTypeEnum.住院一日清单:  //住院一日清单
                    InHospitalFeeQuery();
                    break;
                default:
                    ShowAlert(false, "友好提示", "业务未实现");
                    break;
            }
        }


        protected override void PayCostQuery()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询已缴费信息，请稍候...");
                PayCostRecordModel.Req获取已结算记录 = new req获取已结算记录
                {
                    patientId = PatientModel.Res病人信息查询?.data[PatientModel.PatientInfoIndex].patientId,
                    cardNo = CardModel.CardNo,
                    cardType = ((int)CardModel.CardType).ToString(),
                    beginDate = DateTimeStart.ToString("yyyy-MM-dd"),
                    startDate = DateTimeStart.ToString("yyyy-MM-dd"),
                    endDate = DateTimeEnd.ToString("yyyy-MM-dd")
                };
                PayCostRecordModel.Res获取已结算记录 = DataHandlerEx.获取已结算记录(PayCostRecordModel.Req获取已结算记录);
                if (PayCostRecordModel.Res获取已结算记录?.success ?? false)
                {
                    if (PayCostRecordModel.Res获取已结算记录?.data?.Count > 0)
                    {
                        #region 将处方日期加上
                        //List<已缴费概要信息> temp = new List<已缴费概要信息>();
                        //foreach (var item in PayCostRecordModel.Res获取已结算记录.data)
                        //{
                        //    if (item.deptName.Contains("-"))
                        //    {
                        //        item.deptName = item.deptName.Split('-')[1] + "[" + Convert.ToDateTime(item.tradeTime).ToString("yyyy-MM-dd") + "]";
                        //    }
                        //    temp.Add(item);
                        //}
                        //PayCostRecordModel.Res获取已结算记录.data = temp;
                        #endregion

                        ChangeNavigationContent($"{DateTimeStart.ToString("yyyy-MM-dd")}至\n{DateTimeEnd.ToString("yyyy-MM-dd")}");
                        Next();
                        return;
                    }
                    ShowAlert(false, "已缴费信息查询", "没有获得已缴费信息(列表为空)");
                    return;
                }
                ShowAlert(false, "已缴费信息查询", "没有获得已缴费信息", debugInfo: PayCostRecordModel.Res获取已结算记录?.msg);
            });
        }

        protected virtual void InHospitalFeeQuery()
        {
            InDailyDetailModel.StartDate = DateTimeStart;
            InDailyDetailModel.EndDate = DateTimeEnd;

            DoCommand(lp =>
            {
                lp.ChangeText("正在查询住院费用，请稍候...");
                var req = new req住院患者费用明细查询
                {
                    patientId = PatientModel.住院患者信息.patientHosId,
                    cardNo = PatientModel.住院患者信息.cardNo,
                    startDate = InDailyDetailModel.StartDate.ToString("yyyy-MM-dd"),
                    endDate = InDailyDetailModel.EndDate.ToString("yyyy-MM-dd")
                };

                InDailyDetailModel.Res住院患者费用明细查询 = DataHandlerEx.住院患者费用明细查询(req);
                if (InDailyDetailModel.Res住院患者费用明细查询?.success ?? false)
                    if (InDailyDetailModel.Res住院患者费用明细查询?.data?.Count > 0)
                    {
                        ChangeNavigationContent($"{DateTimeStart.ToString("yyyy-MM-dd")}至\n{InDailyDetailModel.EndDate.ToString("yyyy-MM-dd")}");
                        Navigate(InnerA.ZYFYCX.DailyDetail);
                        return;
                    }
                ShowAlert(false, "住院患者费用明细查询", "没有获得住院患者费用明细");
            });
        }


        /// <summary>
        ///     检验结果查询
        /// </summary>
        protected override void DiagReportQuery()
        {
            try
            {
                DoCommand(lp =>
                {
                    lp.ChangeText("正在查询检验报告信息，请稍候...");
                    //try
                    //{
                    //    var mre = new ManualResetEvent(false);
                    //    var printThread = new Thread(() =>
                    //    {
                    //        try
                    //        {
                    //            PrintLis4();
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            Logger.Main.Info($"[DiagReportQuery][error][{ex.Message}]");
                    //        }
                    //        finally
                    //        {
                    //            mre.Set();
                    //        }
                    //    });
                    //    printThread.SetApartmentState(ApartmentState.STA);
                    //    printThread.Start();
                    //    printThread.Join();
                    //    mre.WaitOne();
                    //    Navigate(A.Home);
                    //    return;
                    //}
                    //catch (Exception ex)
                    //{
                    //    Logger.Main.Info($"[DiagReportQuery][error][{ex.Message}]");
                    //}
                    //finally
                    //{
                    //}

                    List<检验基本信息> list基本信息 = new List<检验基本信息>();
                    var patientId = PatientModel.Res病人信息查询?.data[PatientModel.PatientInfoIndex].patientId;
#if DEBUG
                    patientId = "184664";
#endif
                    Logger.Net.Info($"[LIS][getReportByPatientID][Req][{patientId} {DateTimeStart} {DateTimeEnd}]");
                    if (string.IsNullOrEmpty(patientId))
                    {
                        ShowAlert(false, "检验报告查询", "病人登记号为空，无法查询", 20);
                        //Navigate(A.Home);
                    }
                    var watch = Stopwatch.StartNew();
                    while (patientId.StartsWith("0"))
                    {
                        patientId = patientId.Substring(1, patientId.Length - 1);
                    }
                    var dataSet = Lis4.GetReportByPatientID(patientId, DateTimeStart, DateTimeEnd);
                    var res = dataSet.GetXml();
                    watch.Stop();
                    var time = watch.ElapsedMilliseconds;
                    Logger.Net.Info($"[LIS][getReportByPatientID][Res][{time}ms]{res}");
                    //{
                    //    "Table1":{
                    //        "ErrorID":"0",
                    //        "ErrorString":"成功"
                    //    },
                    //    "Table2":[
                    //        {
                    //            "SampleNo":"7623431",
                    //            "SampleState":"500",
                    //            "PatientName":"蒋园春",
                    //            "PatientID":"953931",
                    //            "PatientID1":"5702023_4",
                    //            "SampleBarCodeNo":"17010602448",
                    //            "Column1":"0",
                    //            "TestDate":"2017-01-06T00:00:00+08:00",
                    //            "PatientID2":"430406199002090064",
                    //            "PatientID4":"5707317",
                    //            "PatientTypeID":"1"
                    //        },
                    //        {
                    //            "SampleNo":"7623614",
                    //            "SampleState":"800",
                    //            "PrintTimes":"1",
                    //            "PatientName":"蒋园春",
                    //            "PatientID":"953931",
                    //            "PatientID1":"5702023_3",
                    //            "SampleBarCodeNo":"17010602447",
                    //            "Column1":"1",
                    //            "SendTime":"2017-01-06T11:18:05+08:00",
                    //            "TestDate":"2017-01-06T00:00:00+08:00",
                    //            "PatientID2":"430406199002090064",
                    //            "PatientID4":"5707317",
                    //            "PatientTypeID":"1"
                    //        }
                    //    ]
                    //}
                    //标本信息为空
                    string errorStr = "";
                    if (dataSet.Tables[0].Rows[0]["ErrorID"].ToString() != "0")
                    {
                        errorStr = dataSet.Tables[0].Rows[0]["ErrorString"].ToString();
                        Logger.Main.Error("获取LIS数据出错，错误信息：" + errorStr);
                        if (errorStr.Contains("标本信息为空"))
                        {
                            ShowAlert(false, "检验报告信息查询", "没有获得检验报告信息(列表为空)");
                        }
                        else
                        {
                            ShowAlert(false, "检验报告信息查询", "获取LIS数据出错，错误信息：" + errorStr);
                        }
                        return;
                    }
                    if (dataSet.Tables[1].Rows.Count == 0)
                    {
                        ShowAlert(false, "检验报告信息查询", "没有获得检验报告信息(列表为空)");
                        return;
                    }

                    for (var index = 0; index < dataSet.Tables[1].Rows.Count; index++)
                    {
                        list基本信息.Add(new 检验基本信息()
                        {
                            reportId = dataSet.Tables[1].Rows[index]["SampleNo"].ToString(),
                            printTimes = dataSet.Tables[1].Rows[index]["PrintTimes"].ToString() ?? "0",
                            patientId = dataSet.Tables[1].Rows[index]["PatientID"].ToString(),
                            patientName = dataSet.Tables[1].Rows[index]["PatientName"].ToString(),
                            barCode = dataSet.Tables[1].Rows[index]["SampleBarCodeNo"].ToString(),//条码
                            sendTime = dataSet.Tables[1].Rows[index]["SendTime"].ToString(),
                            date = dataSet.Tables[1].Rows[index]["TestDate"].ToString(),
                            checkPart = dataSet.Tables[1].Rows[index]["SampleState"].ToString(),
                            cardNo = dataSet.Tables[1].Rows[index]["PatientID1"].ToString(),
                            inhospId = dataSet.Tables[1].Rows[index]["PatientID4"].ToString(),
                            examType = dataSet.Tables[1].Rows[index]["PatientTypeID"].ToString(),
                            bedNo = dataSet.Tables[1].Rows[index]["PatientID2"].ToString(),
                            remark = dataSet.Tables[1].Rows[index]["Column1"].ToString()
                        });
                    }
                    int unfinished = 0;  //未完成
                    int finished = 0;//已完成
                    int unprinted = 0;//已完成  未打印
                    int printed = 0;//已完成  已打印
                    List<string> unprintSampleNo = new List<string>();  //未打印的编号
                    foreach (检验基本信息 item in list基本信息)
                    {
                        if (item.checkPart == "800")  //已经出来了报告
                        {
                            finished++;
#if DEBUG
                            item.printTimes = "0";
#endif
                            if ((!string.IsNullOrEmpty(item.printTimes)) && Convert.ToInt32(item.printTimes) > 0)
                            {
                                printed++;
                            }
                            else
                            {
                                unprinted++;
                                unprintSampleNo.Add(item.reportId);
                            }
                        }
                        else
                        {
                            unfinished++;
                        }
                    }

                    string showMessage = string.Format("卡号：{0}\n姓名：{1}\n报告总数：{2}\n已出报告数：{3}\n未出报告数：{4}\n已打印报告数：{5}\n已出未打印报告数：{6}\n", CardModel.CardNo, PatientModel.Res病人信息查询?.data[PatientModel.PatientInfoIndex].name, list基本信息.Count, finished, unfinished, printed, unprinted);

                    //ShowAlert(true, "友好提示", showMessage);
                    if (unprinted == 0)
                    {
                        ShowAlert(true, "您没有未打印的报告", showMessage);
                        Navigate(A.Home);
                        return;
                    }

                    ShowConfirm("打印检验报告", showMessage, cb =>
                    {
                        if (!cb) return;
                        DoCommand(tt =>
                        {
                            var da = new Process
                            {
                                StartInfo =
                                {
                                    FileName = "YuanTu.ShenZhen.BaoAnPeopleHospital.CallLIS.exe",
                                    UseShellExecute = false,
                                    RedirectStandardInput = true,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true,
                                    CreateNoWindow = true,
                                    Arguments ="SampleNo="+ string.Join("-",unprintSampleNo)
                                }

                            };
                            da.Start();
                            var outputStr = da.StandardOutput.ReadToEnd().Trim();
                            ShowAlert(true, "打印成功!", $"已成功发送{unprinted}份报告到打印机\n请耐心等待打印结束并拿走报告，如有疑问请联系工作人员");
                            Navigate(A.Home);
                        });
                    });
                });
            }
            catch (Exception ex)
            {
                Logger.Main.Info($"[DiagReportQuery][error][{ex.Message}]");
            }
        }

        /// <summary>
        ///     影像结果查询
        /// </summary>
        protected override void PacsReportQuery()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询影像报告信息，请稍候...");
                PacsReportModel.Req影像诊断结果查询 = new req影像诊断结果查询
                {
                    cardNo = CardModel.CardNo,
                    cardType = ((int)CardModel.CardType).ToString(),
                    platformId = PatientModel.当前病人信息.platformId,
                    startDate = DateTimeStart.ToString("yyyy-MM-dd"),
                    endDate = DateTimeEnd.ToString("yyyy-MM-dd"),
                    type = "1" //查询类型 N1、门诊号 2、住院号3、全部
                };
                PacsReportModel.Res影像诊断结果查询 = DataHandlerEx.影像诊断结果查询(PacsReportModel.Req影像诊断结果查询);
                if (PacsReportModel.Res影像诊断结果查询?.success ?? false)
                {
                    if (PacsReportModel.Res影像诊断结果查询?.data?.Count > 0)
                    {
                        ChangeNavigationContent(
                            $"{DateTimeStart.ToString("yyyy-MM-dd")}至\n{DateTimeEnd.ToString("yyyy-MM-dd")}");
                        Next();
                        return;
                    }
                    ShowAlert(false, "影像报告信息查询", "没有获得影像报告信息(列表为空)");
                    return;
                }
                ShowAlert(false, "影像报告信息查询", "没有获得影像报告信息");
            });
        }

        /// <summary>
        ///预交金明细查询
        /// </summary>
        protected override void RechargeRecorQuery()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询交易记录信息，请稍候...");
                RechargeRecordModel.Req查询预缴金充值记录 = new req查询预缴金充值记录
                {
                    cardNo = CardModel.CardNo,
                    cardType = ((int)CardModel.CardType).ToString(),
                    patientId = PatientModel.当前病人信息.patientId,
                    patientName = PatientModel.当前病人信息.name,
                    startDate = DateTimeStart.ToString("yyyy-MM-dd"),
                    endDate = DateTimeEnd.ToString("yyyy-MM-dd")
                };
                RechargeRecordModel.Res查询预缴金充值记录 = DataHandlerEx.查询预缴金充值记录(RechargeRecordModel.Req查询预缴金充值记录);
                if (RechargeRecordModel.Res查询预缴金充值记录?.success ?? false)
                {
                    if (RechargeRecordModel.Res查询预缴金充值记录?.data?.Count > 0)
                    {
                        #region 把时间和日期分开

                        #endregion
                        ChangeNavigationContent($"{DateTimeStart.ToString("yyyy-MM-dd")}至\n{DateTimeEnd.ToString("yyyy-MM-dd")}");
                        Next();
                        return;
                    }
                    ShowAlert(false, "交易记录查询", "没有获得交易记录(列表为空)");
                    return;
                }
                ShowAlert(false, "交易记录查询", "没有获得交易记录");
            });
        }


        /// <summary>
        ///     住院押金查询
        /// </summary>
        protected override void InPrePayRecordQuery()
        {
            ShowAlert(false, "温馨提示", "业务未实现");
        }

        [Obsolete]
        private void PrintLis4()
        {
            List<检验基本信息> list基本信息 = new List<检验基本信息>();
            var patientId = PatientModel.Res病人信息查询?.data[PatientModel.PatientInfoIndex].patientId;
#if DEBUG
            patientId = "184664";
#endif
            Logger.Net.Info($"[LIS][getReportByPatientID][Req][{patientId} {DateTimeStart} {DateTimeEnd}]");
            if (string.IsNullOrEmpty(patientId))
            {
                ShowAlert(false, "检验报告查询", "病人登记号为空，无法查询", 20);
                //Navigate(A.Home);
            }
            var watch = Stopwatch.StartNew();
            while (patientId.StartsWith("0"))
            {
                patientId = patientId.Substring(1, patientId.Length - 1);
            }
            var dataSet = Lis4.GetReportByPatientID(patientId, DateTimeStart, DateTimeEnd);
            var res = dataSet.GetXml();
            watch.Stop();
            var time = watch.ElapsedMilliseconds;
            Logger.Net.Info($"[LIS][getReportByPatientID][Res][{time}ms]{res}");
            //{
            //    "Table1":{
            //        "ErrorID":"0",
            //        "ErrorString":"成功"
            //    },
            //    "Table2":[
            //        {
            //            "SampleNo":"7623431",
            //            "SampleState":"500",
            //            "PatientName":"蒋园春",
            //            "PatientID":"953931",
            //            "PatientID1":"5702023_4",
            //            "SampleBarCodeNo":"17010602448",
            //            "Column1":"0",
            //            "TestDate":"2017-01-06T00:00:00+08:00",
            //            "PatientID2":"430406199002090064",
            //            "PatientID4":"5707317",
            //            "PatientTypeID":"1"
            //        },
            //        {
            //            "SampleNo":"7623614",
            //            "SampleState":"800",
            //            "PrintTimes":"1",
            //            "PatientName":"蒋园春",
            //            "PatientID":"953931",
            //            "PatientID1":"5702023_3",
            //            "SampleBarCodeNo":"17010602447",
            //            "Column1":"1",
            //            "SendTime":"2017-01-06T11:18:05+08:00",
            //            "TestDate":"2017-01-06T00:00:00+08:00",
            //            "PatientID2":"430406199002090064",
            //            "PatientID4":"5707317",
            //            "PatientTypeID":"1"
            //        }
            //    ]
            //}
            //标本信息为空
            string errorStr = "";
            if (dataSet.Tables[0].Rows[0]["ErrorID"].ToString() != "0")
            {
                errorStr = dataSet.Tables[0].Rows[0]["ErrorString"].ToString();
                Logger.Main.Error("获取LIS数据出错，错误信息：" + errorStr);
                if (errorStr.Contains("标本信息为空"))
                {
                    ShowAlert(false, "检验报告信息查询", "没有获得检验报告信息(列表为空)");
                }
                else
                {
                    ShowAlert(false, "检验报告信息查询", "获取LIS数据出错，错误信息：" + errorStr);
                }
                return;
            }
            if (dataSet.Tables[1].Rows.Count == 0)
            {
                ShowAlert(false, "检验报告信息查询", "没有获得检验报告信息(列表为空)");
                return;
            }

            for (var index = 0; index < dataSet.Tables[1].Rows.Count; index++)
            {
                list基本信息.Add(new 检验基本信息()
                {
                    reportId = dataSet.Tables[1].Rows[index]["SampleNo"].ToString(),
                    printTimes = dataSet.Tables[1].Rows[index]["PrintTimes"].ToString() ?? "0",
                    patientId = dataSet.Tables[1].Rows[index]["PatientID"].ToString(),
                    patientName = dataSet.Tables[1].Rows[index]["PatientName"].ToString(),
                    barCode = dataSet.Tables[1].Rows[index]["SampleBarCodeNo"].ToString(),//条码
                    sendTime = dataSet.Tables[1].Rows[index]["SendTime"].ToString(),
                    date = dataSet.Tables[1].Rows[index]["TestDate"].ToString(),
                    checkPart = dataSet.Tables[1].Rows[index]["SampleState"].ToString(),
                    cardNo = dataSet.Tables[1].Rows[index]["PatientID1"].ToString(),
                    inhospId = dataSet.Tables[1].Rows[index]["PatientID4"].ToString(),
                    examType = dataSet.Tables[1].Rows[index]["PatientTypeID"].ToString(),
                    bedNo = dataSet.Tables[1].Rows[index]["PatientID2"].ToString(),
                    remark = dataSet.Tables[1].Rows[index]["Column1"].ToString()
                });
            }
            int unfinished = 0;  //未完成
            int finished = 0;//已完成
            int unprinted = 0;//已完成  未打印
            int printed = 0;//已完成  已打印
            List<string> unprintSampleNo = new List<string>();  //未打印的编号
            foreach (检验基本信息 item in list基本信息)
            {
                if (item.checkPart == "800")  //已经出来了报告
                {
                    finished++;
#if DEBUG
                    item.printTimes = "0";
#endif
                    if ((!string.IsNullOrEmpty(item.printTimes)) && Convert.ToInt32(item.printTimes) > 0)
                    {
                        printed++;
                    }
                    else
                    {
                        unprinted++;
                        unprintSampleNo.Add(item.reportId);
                    }
                }
                else
                {
                    unfinished++;
                }
            }

            string showMessage = string.Format("卡号：{0}\n姓名：{1}\n报告总数：{2}\n已出报告数：{3}\n未出报告数：{4}\n已打印报告数：{5}\n已出未打印报告数：{6}\n", CardModel.CardNo, PatientModel.Res病人信息查询?.data[PatientModel.PatientInfoIndex].name, list基本信息.Count, finished, unfinished, printed, unprinted);

            //ShowAlert(true, "友好提示", showMessage);
            if (unprinted == 0)
            {
                ShowAlert(true, "您没有未打印的报告", showMessage);
                //Navigate(A.Home);
                return;
            }
            int errNumber = 0;
            foreach (var item in unprintSampleNo)
            {
                int sampleNo = Convert.ToInt32(item);
                Logger.Net.Info($"[LIS][printReportBySampleNo][Req][{sampleNo}]");
                bool suc = Lis4.PrintReportBySampleNo(sampleNo, out errorStr);
                Logger.Net.Info($"[LIS][printReportBySampleNo][Res][{suc} {errorStr}]");
                if (!suc)   //打印失败
                {
                    errNumber++;
                    Logger.Main.Error("打印检查报告失败，条码号：" + item + "；错误信息：" + errorStr);
                }
            }
            ShowAlert(true, "已成功打印", "已发送至打印机...\n共成功打印" + (unprinted - errNumber) + "份\n" + (errNumber > 0 ? ("失败" + errNumber + "份\n打印失败的可重新打印") : ""), 10);
            //Navigate(A.Home);
            return;
        }
    }
}