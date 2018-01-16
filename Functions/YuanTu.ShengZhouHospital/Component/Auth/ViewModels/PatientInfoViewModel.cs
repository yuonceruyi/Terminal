using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Models;
using YuanTu.Core.Navigating;
using YuanTu.Core.Reporter;
using YuanTu.Core.Services.PrintService;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Devices.CardReader;
using YuanTu.ShengZhouHospital.Component.Auth.Models;
using YuanTu.ShengZhouHospital.HisNative.Models;

namespace YuanTu.ShengZhouHospital.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : YuanTu.Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        public PatientInfoViewModel(IMagCardDispenser[] magCardDispenser) : base(null)
        {
            _magCardDispenser = magCardDispenser.FirstOrDefault(p => p.DeviceId == "Act_F6_CPU");
        }
        private static IConfigurationManager cfg = ServiceLocator.Current.GetInstance<IConfigurationManager>();
        protected static readonly BarCode.Code128 BarCode128 = new BarCode.Code128 { Magnify = 1, Height = 80 };
        [Dependency]
        public IPaymentModel PaymentModel { get; set; }
        private string _cardNo;
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            if (ChoiceModel.Business == Business.建档)
            {
                Name = IdCardModel.Name;
                Sex = IdCardModel.Sex.ToString();
                Birth = IdCardModel.Birthday.ToString("yyyy-MM-dd");
                Phone = null;
                IdNo = IdCardModel.IdCardNo.Mask(14, 3);
                IsAuth = false;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;
            }
            else if (CardModel.ExternalCardInfo == "补全身份信息")
            {

                Name = IdCardModel.Name;
                Sex = IdCardModel.Sex.ToString();
                Birth = IdCardModel.Birthday.ToString("yyyy-MM-dd");
                Phone = null;
                IdNo = IdCardModel.IdCardNo.Mask(14, 3);
                IsAuth = false;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;

                var patientModel = PatientModel as PatientInfoModel;
                CardModel.CardNo = patientModel.Res门诊读卡.卡号;
                CardModel.CardType = CardType.就诊卡;

            }
            else
            {
                IsAuth = true;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;
                var patientInfo = PatientModel.当前病人信息;
                Name = patientInfo.name;
                Sex = patientInfo.sex;
                //var year = patientInfo.idNo.SafeSubstring(6, 4);//.Substring(6, 4);
                //var month = patientInfo.idNo.SafeSubstring(10, 2);
                //var date = patientInfo.idNo.SafeSubstring(12, 2);
                Birth = patientInfo.birthday;/* year + "-" + month + "-" + date;*/
                Phone = patientInfo.phone.Mask(3, 4);
                IdNo = patientInfo.idNo.Mask(14, 3);
                GuardIdNo = patientInfo.guardianNo.Mask(14, 3);
            }
        }
        public override void Confirm()
        {
            if (ChoiceModel.Business == Business.建档)
            {
                if (Phone.IsNullOrWhiteSpace())
                {
                    ShowUpdatePhone = true;
                    return;
                }
                DoCommand(lp =>
                {
                    lp.ChangeText("正在准备自费卡，请稍候...");
                    if (!WriteInfo())
                        return Result.Fail("获取新卡失败");
                    return Result.Success();
                }).ContinueWith(ret =>
                {
                    if (ret.Result.IsSuccess)
                    {
                        PaymentModel.Self = 500;
                        PaymentModel.Insurance = 0;
                        PaymentModel.Total = 500;
                        PaymentModel.NoPay = false;
                        PaymentModel.ConfirmAction = CreatePatientInfo;
                        PaymentModel.MidList = new List<PayInfoItem>()
                        {
                            new PayInfoItem("办卡费用：", "5.00元", true),
                        };
                        Next();
                    }
                });

                //Task.Run(() =>
                //{
                //    if (!DoCommand(lp =>
                //    {
                //        lp.ChangeText("正在准备自费卡，请稍候...");
                //        if (!WriteInfo())
                //            return Result.Fail("获取新卡失败");
                //        return Result.Success();
                //    }).Result.IsSuccess)
                //        return;
                //    PaymentModel.Self = 500;
                //    PaymentModel.Insurance = 0;
                //    PaymentModel.Total = 500;
                //    PaymentModel.NoPay = false;
                //    PaymentModel.ConfirmAction = CreatePatientInfo;
                //    PaymentModel.MidList = new List<PayInfoItem>()
                //            {
                //                new PayInfoItem("办卡费用：","5.00元",true),
                //            };
                //    Next();
                //    return;
                //});

            }
            else if (CardModel.ExternalCardInfo == "补全身份信息")
            {
                if (Phone.IsNullOrWhiteSpace())
                {
                    ShowUpdatePhone = true;
                    return;
                }
                DoCommand(lp =>
                {
                    lp.ChangeText("正在提交最新病人信息");
                    var patientInfo = PatientModel.当前病人信息;
                    //var patientModel = PatientModel as PatientInfoModel;
                    var reqModify = new req病人基本信息修改
                    {
                        patientId = patientInfo.patientId,
                        platformId = patientInfo.platformId,
                        cardNo = CardModel.CardNo,
                        cardType = (ConstInner.CardTypeMapping[CardModel.CardType]).ToString(),//到底哪各
                        phone = NewPhone,
                        birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                        guardianNo = patientInfo.guardianNo,
                        idNo = IdCardModel.IdCardNo,
                        name = IdCardModel.Name,
                        sex = patientInfo.sex,
                        address = patientInfo.address,
                        operId = FrameworkConst.OperatorId
                    };
                    var resModify = DataHandlerEx.病人基本信息修改(reqModify);
                    if (!resModify.success)
                    {
                        ShowAlert(false, "温馨提示", $"建档失败:{resModify.msg}");
                        return;
                    }
                    var req = new req病人信息查询
                    {
                        cardNo = CardModel.CardNo,
                        cardType = ConstInner.CardTypeMapping[CardModel.CardType].ToString()
                    };
                    var res = DataHandlerEx.病人信息查询(req);
                    if (res.success)
                    {
                        PatientModel.Req病人信息查询 = req;
                        PatientModel.Res病人信息查询 = res;
                        var shell = GetInstance<IShellViewModel>();
                        shell.Busy.IsBusy = false;
                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "温馨提示", $"病人信息查询失败:{res.msg}");
                    }

                });

            }
            else
            {
                var patientInfo = PatientModel.当前病人信息;
                var resource = ResourceEngine;
                TopBottom.InfoItems = new ObservableCollection<InfoItem>(new[]
                {
                new InfoItem
                {
                    Title = "姓名",
                    Value = patientInfo.name,
                    Icon = resource.GetImageResourceUri("姓名图标_YellowBlue")
                },
                new InfoItem
                {
                    Title = "卡号",
                    Value =CardModel.CardNo /*patientInfo.accBalance.In元()*/,
                    Icon = resource.GetImageResourceUri("余额图标_YellowBlue")
                }
            });
                ChangeNavigationContent($"{patientInfo.name}");
                Next();//GetBillPayInfo();
            };
        }

        public override void UpdateConfirm()
        {
            if (string.IsNullOrWhiteSpace(NewPhone))
            {
                ShowAlert(false, "温馨提示", "请输入手机号");
                return;
            }
            if (!NewPhone.IsHandset())
            {
                ShowAlert(false, "温馨提示", "请输入正确的手机号");
                return;
            }
            if (ChoiceModel.Business == Business.建档)
            {
                Phone = NewPhone;
                CreateModel.Phone = NewPhone;
                ShowUpdatePhone = false;
                return;
            }
            if (CardModel.ExternalCardInfo == "补全身份信息")
            {
                Phone = NewPhone;
                CreateModel.Phone = NewPhone;
                ShowUpdatePhone = false;
                return;
            }
            DoCommand(lp =>
            {
                lp.ChangeText("正在更新个人信息，请稍候...");
                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                PatientModel.Req病人基本信息修改 = new req病人基本信息修改
                {
                    patientId = patientInfo.patientId,
                    platformId = patientInfo.platformId,
                    cardNo = CardModel.CardNo,
                    cardType = (ConstInner.CardTypeMapping[CardModel.CardType]).ToString(),//到底哪各
                    phone = NewPhone,
                    birthday = patientInfo.birthday,
                    guardianNo = patientInfo.guardianNo,
                    idNo = patientInfo.idNo,
                    name = patientInfo.name,
                    sex = patientInfo.sex,
                    address = patientInfo.address,
                    operId = FrameworkConst.OperatorId
                };
                PatientModel.Res病人基本信息修改 = DataHandlerEx.病人基本信息修改(PatientModel.Req病人基本信息修改);
                if (PatientModel.Res病人基本信息修改?.success ?? false)
                {
                    ShowUpdatePhone = false;
                    PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex].phone = NewPhone;
                    Phone = NewPhone.Mask(3, 4);
                    ShowAlert(true, "个人信息", "个人信息更新成功");
                }
                else
                {
                    ShowAlert(false, "个人信息", "个人信息更新失败", debugInfo: PatientModel.Res病人基本信息修改?.msg);
                }
            });
        }

        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }
        protected Result CreatePatientInfo()
        {
            Logger.Device.Info("进入scoket建档接口");
            return DoCommand(lp =>
            {
                var req = new Req建档()
                {
                    卡类别 = "1",
                    姓名 = IdCardModel.Name,
                    性别 = ((int)IdCardModel.Sex).ToString(),
                    居住地址 = IdCardModel.Address,
                    出生日期 = IdCardModel.Birthday.ToString("yyyy-MM-dd HH:mm:ss"),
                    证件号码 = IdCardModel.IdCardNo,
                    手机号码 = Phone,
                    卡号 = _cardNo
                };
                var res = HisHandleEx.执行建档(req);
                if (res.IsSuccess)
                {
                    _magCardDispenser.MoveCardOut();
                    ExtraPaymentModel.Complete = true;
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "建档成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分建档",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = CreatePrintables(),
                        TipImage = "提示_凭条和发卡"
                    });
                    交易记录同步到his系统();
                    Navigate(A.JD.Print);
                    return Result.Success();
                }
                else
                {
                    ExtraPaymentModel.Complete = true;
                    ShowAlert(false, "温馨提示", $"建档失败:{res.Message}");
                    return Result.Fail(res?.RetCode ?? -100, "建档失败");
                }


            }).Result;
        }
        private void 交易记录同步到his系统()
        {
            try
            {
                var req = new req交易记录同步到his系统
                {
                    platformId = FrameworkConst.OperatorId,
                    hisPatientId = CardModel?.CardNo,
                    cardNo = CardModel?.CardNo,
                    idNo = IdCardModel?.IdCardNo,
                    patientName = IdCardModel?.Name,
                    tradeType = "2",
                    cash = PaymentModel?.Total.ToString(),
                    operId = FrameworkConst.OperatorId,
                    bankTime = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    inHos = "1",
                    tradeMode = GetEnumDescription(PaymentModel.PayMethod),
                    remarks = "建档",
                };
                FillRechargeRequest(req);
                var res = DataHandlerEx.交易记录同步到his系统(req);
                if (res.success)
                {
                    Logger.Main.Info($"交易记录同步到his系统成功");
                }
                else
                {
                    Logger.Main.Info($"交易记录同步到his系统失败:{res.msg}");
                }
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"交易记录同步到his系统失败异常:{ex.Message}");
                return;
            }
        }
        public static string GetEnumDescription(PayMethod value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }
        protected virtual void FillRechargeRequest(req交易记录同步到his系统 req)
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
                    req.bankTime = posinfo.TransTime;
                    req.bankDate = posinfo.TransDate;
                    req.posTransNo = posinfo.Trace;
                    req.bankTransNo = posinfo.Ref;
                    req.deviceInfo = posinfo.TId;
                    req.sellerAccountNo = posinfo.MId;
                }
            }
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 || extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
            {
                var thirdpayinfo = extraPaymentModel.PaymentResult as 订单状态;
                if (thirdpayinfo != null)
                {
                    req.payAccountNo = thirdpayinfo.buyerAccount;
                    req.transNo = thirdpayinfo.outPayNo;
                    //req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                }
            }
        }
        protected override Queue<IPrintable> CreatePrintables()
        {
            var queue = PrintManager.NewQueue("自助发卡");
            var sb = new StringBuilder();
            sb.Append($"状态：办卡成功\n");
            sb.Append($"姓名：{IdCardModel.Name}\n");
            sb.Append($"就诊卡号：{CardModel.CardNo}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的个人信息。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", 9, FontStyle.Regular) });
            sb.Clear();
            if (!string.IsNullOrEmpty(CardModel.CardNo))
            {
                var image = BarCode128.GetCodeImage(CardModel.CardNo, BarCode.Code128.Encode.Code128A);
                queue.Enqueue(new PrintItemImage
                {
                    Align = ImageAlign.Center,
                    Image = image,
                    Height = image.Height / 2f,
                    Width = image.Width / 2f
                });
            }
            sb.Append(CardModel.CardNo);
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), StringFormat = PrintConfig.Center, Font = new Font("微软雅黑", 9, System.Drawing.FontStyle.Regular) });
            sb.Clear();
            sb.AppendLine(".");
            sb.AppendLine(".");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", 9, System.Drawing.FontStyle.Regular) });
            return queue;
        }
        protected bool WriteInfo()
        {
            try
            {
                if (!_magCardDispenser.Connect().IsSuccess)
                {
                    ReportService.发卡器离线(null, ErrorSolution.发卡器离线);
                    ShowAlert(false, "建档发卡", "发卡机连接失败");
                    return false;
                }
                Logger.Device.Info($"[建档发卡]连接成功");
                if (!_magCardDispenser.Initialize().IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机初始化失败");
                    return false;
                }
                Logger.Device.Info($"[建档发卡]初始化成功");
                var dispenser = (Special.MagCardDispenserByF6ForZHIFEIKA)_magCardDispenser;
                string msg = string.Empty;
                for (int a = 0; a < 3; a++)
                {
                    var result = dispenser.EnterCard();
                    if (result.IsSuccess)
                    {
                        _cardNo = Encoding.ASCII.GetString(result.Value, 50, 20).Trim('?');
                        CardModel.CardNo = _cardNo;
                        Logger.Device.Info($"[建档发卡]读取卡号{_cardNo}");
                        dispenser.Reset();
                        if (dispenser.VerifyPWD(65535).IsSuccess)
                        {
                            Logger.Main.Error($"[建档发卡]密码验证成功");
                            break;
                        }
                        else
                        {
                            msg = "发卡器验证密码失败";
                        }
                    }
                    else
                    {
                        msg = "发卡器无卡或者移动卡片故障";
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(msg))
                {
                    ShowAlert(false, "建档发卡", msg);
                    return false;
                }

                #region Write
                int i = 300;
                var ret = Write(ref i, 50, "200");
                if (!ret.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机写入失败");
                    return false;
                }
                i = 400;
                ret = Write(ref i, 6, "330683"); //发卡地区
                if (!ret.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机写入失败");
                    return false;
                }
                ret = Write(ref i, 22, "0009"); //发卡机构代码
                if (!ret.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机写入失败");
                    return false;
                }
                ret = Write(ref i, 32, IdCardModel.IdCardNo); //证件号码
                if (!ret.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机写入失败");
                    return false;
                }
                ret = Write(ref i, 2, "01"); //证件类型
                if (!ret.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机写入失败");
                    return false;
                }
                ret = Write(ref i, 8, " "); //籍贯
                if (!ret.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机写入失败");
                    return false;
                }
                ret = Write(ref i, 32, IdCardModel.Name); //姓名
                if (!ret.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机写入失败");
                    return false;
                }
                ret = Write(ref i, 1, IdCardModel.Sex == Consts.Enums.Sex.男 ? "1" : "2"); //性别
                if (!ret.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机写入失败");
                    return false;
                }
                ret = Write(ref i, 2, "50"); //血型
                if (!ret.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机写入失败");
                    return false;
                }
                ret = Write(ref i, 20, ""); //手机号码
                if (!ret.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机写入失败");
                    return false;
                }
                ret = Write(ref i, 128, IdCardModel.Address); //居住地址
                if (!ret.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机写入失败");
                    return false;
                }
                ret = Write(ref i, 8, IdCardModel.Birthday.ToString("yyyyMMdd")); //出生日期
                if (!ret.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机写入失败");
                    return false;
                }
                ret = Write(ref i, 5, IdCardModel.Nation); //民族
                if (!ret.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机写入失败");
                    return false;
                }
                Logger.Device.Info($"[建档发卡]写入成功");
                #endregion
                return true;
            }
            catch (Exception ex)
            {
                ShowAlert(false, "建档发卡", "发卡机写卡失败");
                Logger.Device.Error($"[建档发卡]{ex.Message + ex.StackTrace}");
                return false;
            }
        }

        private static readonly Encoding encoding = Encoding.GetEncoding("GB2312");
        private Result Write(ref int i, int n, string s)
        {
            var bytes = new byte[n];
            var data = encoding.GetBytes(s);
            Array.Copy(data, bytes, data.Length);
            var dispenser = (Special.MagCardDispenserByF6ForZHIFEIKA)_magCardDispenser;
            var ret = dispenser.Write(i, bytes);
            i += n;
            return ret;
        }
        public override bool OnLeaving(NavigationContext navigationContext)
        {
            ShowMask(false);
            _magCardDispenser?.UnInitialize();
            _magCardDispenser?.DisConnect();
            return true;
        }
        private void GetBillPayInfo()
        {
            var billRecordModel = GetInstance<IBillRecordModel>();
            if (ChoiceModel.Business != Business.缴费)
            {
                DoCommand(lp =>
                {
                    lp.ChangeText("正在查询个人业务，请稍候...");

                    billRecordModel.Req获取缴费概要信息 = new req获取缴费概要信息
                    {
                        patientId = PatientModel.当前病人信息.patientId,
                        cardType = ((int)CardModel.CardType).ToString(),
                        cardNo = CardModel.CardNo,
                        billType = ""
                    };
                    billRecordModel.Res获取缴费概要信息 = DataHandlerEx.获取缴费概要信息(billRecordModel.Req获取缴费概要信息);
                    if (billRecordModel.Res获取缴费概要信息?.success ?? false && billRecordModel.Res获取缴费概要信息.data?.Count > 0)
                    {
                        ShowConfirm("缴费业务", "您当前有缴费业务尚未完成，是否跳转到缴费功能", cp =>
                        {
                            if (!cp)
                            {
                                Next();
                                return;
                            }
                            var model = GetInstance<INavigationModel>();
                            model.Items.Clear();
                            NavigationEngine.JumpAfterFlow(null, null, new FormContext(A.JiaoFei_Context, A.JF.BillRecord), "自助缴费");
                            NavigationEngine.DestinationStack.Clear();
                        });
                    }
                    else
                    {
                        if (ChoiceModel.Business != Business.取号)
                        {
                            GetTakeNumInfo();
                        }
                        else
                        {
                            View.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle,
                            (Action)(() =>
                            {
                                Next();
                            }));
                        }
                    }
                });
            }
            else
            {
                Next();
            }
        }
        private void GetTakeNumInfo()
        {

            var AppoRecordModel = GetInstance<IAppoRecordModel>();
            var takeNumModel = GetInstance<ITakeNumModel>();
            AppoRecordModel.Req挂号预约记录查询 = new req挂号预约记录查询
            {
                patientId = PatientModel.当前病人信息?.patientId,
                patientName = PatientModel.当前病人信息?.name,
                startDate = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                endDate = DateTimeCore.Now.AddDays(7).ToString("yyyy-MM-dd"),
                searchType = "1",
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString()
            };
            AppoRecordModel.Res挂号预约记录查询 = DataHandlerEx.挂号预约记录查询(AppoRecordModel.Req挂号预约记录查询);
            if (AppoRecordModel.Res挂号预约记录查询?.success ?? false && AppoRecordModel.Res挂号预约记录查询?.data?.Count > 0)
            {
                if (AppoRecordModel.Res挂号预约记录查询?.data?.Count > 1)
                {
                    ShowConfirm("取号业务", "您当前有取号业务尚未完成，是否跳转到取号功能", cp =>
                    {
                        if (!cp)
                        {
                            Next();
                            return;
                        }
                        var model = GetInstance<INavigationModel>();
                        model.Items.Clear();
                        NavigationEngine.JumpAfterFlow(null, TakeNumJumpLess, new FormContext(A.QuHao_Context, A.QH.Record), "自助缴费");
                        NavigationEngine.DestinationStack.Clear();
                    });
                }
                else
                {
                    AppoRecordModel.所选记录 = AppoRecordModel.Res挂号预约记录查询.data.FirstOrDefault();
                    var record = AppoRecordModel.所选记录;

                    takeNumModel.List = new List<PayInfoItem>
                        {
                            new PayInfoItem("就诊日期：", record.medDate.SafeConvertToDate("yyyy-MM-dd", "yyyy年MM月dd日")),
                            new PayInfoItem("就诊科室：", record.deptName),
                            new PayInfoItem("就诊医生：", record.doctName),
                            new PayInfoItem("就诊时段：", record.medAmPm.SafeToAmPm()),
                            new PayInfoItem("就诊序号：", record.appoNo),
                        };
                    ShowConfirm("取号业务", "您当前有取号业务尚未完成，是否跳转到取号功能", cp =>
                    {
                        if (!cp)
                        {
                            Next();
                            return;
                        }
                        var model = GetInstance<INavigationModel>();
                        model.Items.Clear();
                        NavigationEngine.JumpAfterFlow(null, TakeNumMore, new FormContext(A.QuHao_Context, A.QH.Record), "自助缴费");
                        NavigationEngine.DestinationStack.Clear();
                    });
                }
            }
        }
        protected Task<Result<FormContext>> TakeNumMore()
        {
            return DoCommand(lp =>
            {
                return Result<FormContext>.Success(new FormContext(A.QuHao_Context, A.QH.TakeNum));
            });
        }
        protected Task<Result<FormContext>> TakeNumJumpLess()
        {
            return DoCommand(lp =>
            {
                return Result<FormContext>.Success(default(FormContext));
            });
        }
    }
}
