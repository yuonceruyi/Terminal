using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Devices.CardReader;
using YuanTu.HuNanHangTianHospital.Common;
using static YuanTu.Devices.CardReader.ACTF3CardDispenser;
using static YuanTu.Devices.UnSafeMethods;

namespace YuanTu.HuNanHangTianHospital.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        [Dependency]
        public IPaymentModel PaymentModel { get; set; }

        private static readonly byte[] _keyA = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser) : base(null)
        {
            _rfCardDispenser = rfCardDispenser?.FirstOrDefault(p => p.DeviceId == "Act_F3_RF");

            ConfirmCommand = new DelegateCommand(Confirm);
            UpdateCommand = new DelegateCommand(() =>
            {
                IsAuth = !Phone.IsNullOrWhiteSpace();
                ShowUpdatePhone = true;
            });
            UpdateCancelCommand = new DelegateCommand(() => { ShowUpdatePhone = false; });
            UpdateConfirmCommand = new DelegateCommand(UpdateConfirm);
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            if (ChoiceModel.Business == Business.建档 && CardModel.CardType == CardType.身份证)
            {
                if (IdCardModel != null)
                {
                    Logger.Main.Info($"IdCardModel:{IdCardModel?.Name},{IdCardModel?.Sex},{IdCardModel?.Birthday:yyyy-MM-dd},{IdCardModel?.IdCardNo.Mask(14, 3)}");
                }
                Name = IdCardModel.Name;
                Sex = IdCardModel.Sex.ToString();
                Birth = IdCardModel.Birthday.ToString("yyyy-MM-dd");
                Phone = null;
                IdNo = IdCardModel.IdCardNo.Mask(14, 3);
                IsAuth = false;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;
            }
            else if (ChoiceModel.Business == Business.建档 && CardModel.CardType == CardType.社保卡)
            {
            }
            else
            {
                IsAuth = true;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;
                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                Name = patientInfo.name;
                Sex = patientInfo.sex;
                Birth = patientInfo.birthday.SafeToSplit(' ', 1)[0];
                Phone = patientInfo.phone.Mask(3, 4);
                IdNo = patientInfo.idNo.Mask(14, 3);
                GuardIdNo = patientInfo.guardianNo.Mask(14, 3);
            }
        }

        public override void Confirm()
        {
            if (Phone.IsNullOrWhiteSpace())
            {
                ShowUpdatePhone = true;
                return;
            }
            if (ChoiceModel.Business == Business.建档)
            {
                Task.Run(() =>
                {
                    if (!DoCommand(lp =>
                    {
                        lp.ChangeText("正在准备就诊卡，请稍候...");
                        if (!GetNewCardNo())
                            return Result.Fail("获取新卡失败");
                        return Result.Success();
                    }).Result.IsSuccess)
                        return;

                    if (IdCardModel != null)
                    {
                        Logger.Main.Info($"IdCardModel2:{IdCardModel?.Name},{IdCardModel?.Sex},{IdCardModel?.Birthday:yyyy-MM-dd},{IdCardModel?.IdCardNo.Mask(14, 3)}");
                    }
                    PaymentModel.Self = 200;
                    PaymentModel.Insurance = 0;
                    PaymentModel.Total = 200;
                    PaymentModel.NoPay = true;
                    PaymentModel.PayMethod = PayMethod.银联;
                    PaymentModel.ConfirmAction = CreatePatientInfo;
                    PaymentModel.MidList = new List<PayInfoItem>()
                            {
                                new PayInfoItem("办卡费用：",PaymentModel.Self.In元(),true),
                            };
                    Next();
                    return;
                });

            }
            else
            {
                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                ChangeNavigationContent($"{patientInfo.name}\r\n余额{ patientInfo.accBalance.In元()}");
                Next();
            }
        }

        protected virtual Result CreatePatientInfo()
        {
            Logger.Main.Info("进入 创建病人 接口");
            return DoCommand(lp =>
            {
                Logger.Main.Info($"CardModel.CardType{CardModel?.CardType.ToString()}");
                if (IdCardModel != null)
                {
                    Logger.Main.Info($"IdCardModel3:{IdCardModel?.Name},{IdCardModel?.Sex},{IdCardModel?.Birthday:yyyy-MM-dd},{IdCardModel?.IdCardNo.Mask(14, 3)}");
                }
                CreateModel.Req病人建档发卡 = new req病人建档发卡
                {
                    operId = FrameworkConst.OperatorId,
                    cardNo = CardModel.CardNo,
                    cardType = "1",
                    name = IdCardModel.Name,
                    sex = IdCardModel.Sex.ToString(),
                    birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                    idNo = IdCardModel.IdCardNo,
                    idType = "1",
                    nation = IdCardModel.Nation,
                    address = IdCardModel.Address,
                    phone = Phone,
#pragma warning disable 612
                    guardianName = null,
                    school = null,
#pragma warning restore 612
                    pwd = "6666",
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    cash = PaymentModel.Self.ToString(),
                    accountNo = null,
                    patientType = "自费",
                    setupType = ((int)CreateModel.CreateType).ToString(),
                    extend = CardModel.ExternalCardInfo,
                };
                FillRequest(CreateModel.Req病人建档发卡);

                lp.ChangeText("正在建档，请稍候...");
                CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);
                if (CreateModel.Res病人建档发卡?.success ?? false)
                {

                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "建档成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分建档",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = CreatePrintables(),
                        TipImage = "提示_凭条和发卡"
                    });
                    ChangeNavigationContent($"{IdCardModel.Name}\r\n卡号{ CardModel.CardNo}");
                    Logger.Main.Info("从前端弹出卡片开始");
                    _rfCardDispenser.MoveCardOut();
                    Logger.Main.Info("卡友退卡开始");
                    KY.MoveOutCard();
                    Navigate(A.JD.Print);
                    return Result.Success();
                }
                _rfCardDispenser.MoveCard(CardPostition.MM_CAPTURE_TO_BOX);
                ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: CreateModel.Res病人建档发卡?.msg);
                return Result.Fail(CreateModel.Res病人建档发卡?.code ?? -100, CreateModel.Res病人建档发卡?.msg);
            }).Result;
        }

        protected override bool GetNewCardNo()
        {
            try
            {
                Logger.Main.Info($"[建档发卡]进入到获取卡号逻辑");
                if (!_rfCardDispenser.Connect().IsSuccess)
                {
                    ReportService.发卡器离线(null, ErrorSolution.发卡器离线);
                    ShowAlert(false, "建档发卡", "发卡机连接失败");
                    return false;
                }
                Logger.Main.Info($"[建档发卡]发卡器判断是否含有卡");
                if (!_rfCardDispenser.EnterCard().IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机获取序列号失败");
                    return false;
                }
                if (!_rfCardDispenser.MfVerifyPassword(0, true, _keyA).IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡器新卡密码验证失败");
                    return false;
                }
                byte[] data1;//序列号
                if (!_rfCardDispenser.MfReadSector(0, 0, out data1).IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡器新卡 序列号读取失败");
                    return false;
                }
                if (!_rfCardDispenser.MfVerifyPassword(2, true, _keyA).IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡器新卡 密码验证失败");
                    return false;
                }
                byte[] data2;//卡号
                if (!_rfCardDispenser.MfReadSector(2, 0, out data2).IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡器新卡 卡号读取失败");
                    return false;
                }

                byte[] bCardNo = new byte[16];
                byte[] bSerialNo = new byte[4];
                Array.Copy(data2, bCardNo, 16);
                Array.Copy(data1, bSerialNo, 4);
                Array.Reverse(bSerialNo);
                var serialNo = ByteToHexStr(bSerialNo);
                var cardNo = Encoding.Default.GetString(bCardNo, 0, 10);
                if (cardNo.Contains("\0"))
                {
                    cardNo = cardNo.TrimEnd('\0');
                }
                if (string.IsNullOrEmpty(cardNo) || string.IsNullOrEmpty(serialNo))
                {
                    ShowAlert(false, "建档发卡", "获取卡数据失败");
                    return false;
                }
                Logger.Main.Info($"[建档发卡]发卡器获取序列号完毕:{serialNo}");
                Logger.Main.Info($"[建档发卡]发卡器获取卡号完毕:{ cardNo}");
                CardModel.CardNo = cardNo;
                CardModel.ExternalCardInfo = serialNo;
                return true;
            }
            catch (Exception ex)
            {
                ShowAlert(false, "建档发卡", "发卡机读卡失败");
                Logger.Main.Error($"[建档发卡]{ex.Message + ex.StackTrace}");
                return false;
            }
        }

        protected virtual void FillRequest(req病人建档发卡 req)
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

            //todo Update
            DoCommand(lp =>
            {
                lp.ChangeText("正在更新个人信息，请稍候...");
                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                PatientModel.Req病人基本信息修改 = new req病人基本信息修改
                {
                    patientId = patientInfo.patientId,
                    platformId = patientInfo.platformId,
                    cardNo = CardModel.CardNo,
                    cardType = ((int)CardModel.CardType).ToString(),
                    phone = NewPhone,
                    birthday = patientInfo.birthday,
                    guardianNo = patientInfo.guardianNo,
                    idNo = patientInfo.idNo,
                    name = patientInfo.name,
                    sex = patientInfo.sex,
                    address = patientInfo.address,
                    operId = FrameworkConst.OperatorId,
                    patientType = "自费",
                };
                PatientModel.Res病人基本信息修改 = DataHandlerEx.病人基本信息修改(PatientModel.Req病人基本信息修改);
                if (PatientModel.Res病人基本信息修改?.success ?? false)
                {
                    ShowUpdatePhone = false;
                    PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex].phone = NewPhone;
                    Phone = NewPhone.Mask(3, 4);
                    ShowAlert(false, "个人信息", "个人信息更新成功");
                }
                else
                {
                    ShowAlert(false, "个人信息", "个人信息更新失败");
                }
            });
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            ShowMask(false);
            return true;
        }

        private static string ByteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }
    }
}