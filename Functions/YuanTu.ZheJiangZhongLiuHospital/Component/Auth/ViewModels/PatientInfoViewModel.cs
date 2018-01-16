using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Models;
using YuanTu.Devices.CardReader;
using YuanTu.ZheJiangZhongLiuHospital.Component.QueueSignIn;
using YuanTu.ZheJiangZhongLiuHospital.Minghua;
using Prism.Regions;
using YuanTu.Consts.Gateway;
using YuanTu.ZheJiangZhongLiuHospital.ICBC;
using YuanTu.Core.Log;
using Newtonsoft.Json;
using YuanTu.ZheJiangZhongLiuHospital.NativeService;

namespace YuanTu.ZheJiangZhongLiuHospital.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : YuanTu.Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser) : base(rfCardDispenser)
        {
        }

        [Dependency]
        public IQueueSignInService QueueSignInService { get; set; }

        [Dependency]
        public IQueueSiginInModel QueueSiginInModel { get; set; }

        [Dependency]
        public IYiBaoCardContent YiBaoCardContent { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            TopBottom.InfoItems = null;

            var a = PatientModel.Res病人信息查询?.success ?? false;
            Logger.Main.Info($"数据3：{a}");
            if (!(PatientModel.Res病人信息查询?.success ?? false) && CardModel.CardType == CardType.社保卡)
            {
                Logger.Main.Info("数据4" + JsonConvert.SerializeObject(YiBaoCardContent));
                Name = YiBaoCardContent.Name;
                Sex = YiBaoCardContent.Sex.ToString() == "1" ? "男" : "女";
                Birth = YiBaoCardContent.Birthday;
                Phone = null;
                IdNo = YiBaoCardContent.PId;
                IsAuth = false;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;
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
            if (ChoiceModel.Business == Business.签到)
            {
                DoCommand(lp =>
                {
                    lp.ChangeText("正在查询挂号信息,请稍候...");
                    var retLst = QueueSignInService.GetRegisterList(CardModel.CardNo, CardModel.CardType);
                    if (retLst.IsSuccess)
                    {
                        QueueSiginInModel.RegisterInfos = retLst.Value.data;
                        if (QueueSiginInModel.RegisterInfos.Any())
                        {
                            if (QueueSiginInModel.RegisterInfos.Length == 1)
                            {
                                QueueSiginInModel.SelectRegisterInfo = QueueSiginInModel.RegisterInfos[0];
                                var ret = QueueSignInService.QueueSignIn(QueueSiginInModel.SignType,
                                    QueueSiginInModel.SelectRegisterInfo);
                                if (ret.IsSuccess)
                                {
                                    ShowAlert(true, "签到", "签到成功");
                                }
                                else
                                {
                                    ShowAlert(false, "签到失败", ret.Message);
                                }
                            }
                            //else
                            //{
                            //    Manager.Switch(InnerA.GuoHaoSignIn_Context);
                            //}
                        }
                        else
                        {
                            ShowAlert(false, "签到挂号信息查询", $"未查询到信息");
                        }
                    }
                    else
                    {
                        ShowAlert(false, "签到挂号信息查询", $"查询失败:{retLst.Message}");
                    }
                });
                return;
            }


            if (!(PatientModel.Res病人信息查询?.success ?? false) && CardModel.CardType == CardType.社保卡)
            {
                DoCommand(lp =>
                {
                    lp.ChangeText("正在自动为您医院建档，请稍后");

                    Logger.Net.Info("开始HIS建档读卡");
                    var res = HisReadCardService.ReadCard();
                    if (!res)
                    {
                        ShowAlert(false, "个人信息", $"个人自动建档失败：{res.Message}");
                        return;
                    }

                    var reqCreateAndGetCard = new req病人建档发卡
                    {
                        operId = FrameworkConst.OperatorId,
                        cardNo = res.Value,
                        cardType = "2",
                        name = YiBaoCardContent.Name,
                        sex = YiBaoCardContent.Sex,
                        birthday = YiBaoCardContent.Birthday,
                        idNo = YiBaoCardContent.PId,
                        idType = "1",
                        nation = YiBaoCardContent.Nation,
                        address = YiBaoCardContent.Birthplace,
                        phone = Phone,
                        pwd = "123456",
                        tradeMode = "CA",
                        setupType = "1",
                        extend = res.Value
                    };
                    var resCreateAndGetCard = DataHandlerEx.病人建档发卡(reqCreateAndGetCard);
                    if (resCreateAndGetCard.success && resCreateAndGetCard.data != null)
                    {
                        lp.ChangeText("正在自动为您工行开户，请稍后");

                        var reqCreateAccount = new Req开户
                        {
                            Chanel = "1",
                            IDKind = "00",
                            PID = YiBaoCardContent.PId,
                            Pname = YiBaoCardContent.Name,
                            Phone = Phone,
                            Psex = YiBaoCardContent.Sex,
                            PAddress = YiBaoCardContent.Birthplace??"社保卡地址读取失败",
                            Pbirthday = YiBaoCardContent.Birthday,
                            Policemark = YiBaoCardContent.Fkjg,
                            Cardkind = "2",
                            Cardid = resCreateAndGetCard.data.patientCard,
                            SignBankCardFlag = "0",
                            BankCardNo = "",
                            OperId = FrameworkConst.OperatorId,
                            DeviceInfo = FrameworkConst.OperatorId,
                            TradeSerial = DateTimeCore.Now.ToString("yyyyMMddHHmmssffff"),
                            Rsv1 = "",
                            Rsv2 = ""
                        };
                        var resCreateAccount = PConnection.Handle<Res开户>(reqCreateAccount);
                        if (resCreateAccount.IsSuccess)
                        {
                            lp.ChangeText("正在同步更新个人信息，请稍候...");
                            PatientModel.Req病人基本信息修改 = new req病人基本信息修改
                            {
                                patientId = resCreateAndGetCard.data.patientid,
                                platformId = null,
                                cardNo = CardModel.CardNo,
                                cardType = ((int)CardModel.CardType).ToString(),
                                phone = NewPhone,
                                birthday = YiBaoCardContent.Birthday,
                                guardianNo = null,
                                idNo = YiBaoCardContent.PId,
                                name = YiBaoCardContent.Name,
                                sex = YiBaoCardContent.Sex,
                                address = YiBaoCardContent.Birthplace,
                                operId = FrameworkConst.OperatorId,
                               extend = resCreateAccount.Value.AccountNo
                            };
                            PatientModel.Res病人基本信息修改 = DataHandlerEx.病人基本信息修改(PatientModel.Req病人基本信息修改);
                            if (PatientModel.Res病人基本信息修改?.success ?? false)
                            {
                                ShowUpdatePhone = false;

                                PatientModel.Req病人信息查询 = new req病人信息查询
                                {
                                    cardNo = CardModel.CardNo,
                                    cardType = ((int)CardModel.CardType).ToString()
                                };
                                PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
                                if (PatientModel.Res病人信息查询.success)
                                {
                                    if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                                    {
                                        ShowAlert(false, "个人信息", $"个人信息查询失败：{PatientModel.Res病人信息查询.msg}");
                                    }
                                    ShowAlert(true, "个人信息", "个人信息更新成功");
                                    Next();
                                }
                                else
                                {
                                    ShowAlert(false, "个人信息", $"个人信息查询失败：{PatientModel.Res病人信息查询.msg}");
                                }
                            }
                            else
                            {
                                ShowAlert(false, "温馨提示", $"个人信息更新失败:{ PatientModel.Res病人基本信息修改.msg}");
                            }
                        }
                        else
                        {
                            ShowAlert(false, "温馨提示", $"自动建档工行开户失败:{resCreateAccount.Message},请到窗口处理");
                        }
                    }
                    else
                    {
                        ShowAlert(false, "温馨提示", $"自动建档失败:{resCreateAndGetCard.msg}");
                    }
                });
                return;
            }

            var patientInfo = PatientModel.当前病人信息;
            if (ChoiceModel.Business != Business.缴费 && ChoiceModel.Business != Business.签到)
            {
                ChangeNavigationContent($"{patientInfo.name}\r\n余额{patientInfo.accBalance.In元()}");


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
                    Title = "余额",
                    Value = patientInfo.accBalance.In元(),
                    Icon = resource.GetImageResourceUri("余额图标_YellowBlue")
                }
            });
            }
            Next();
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

            if (!(PatientModel.Res病人信息查询?.success ?? false) && CardModel.CardType == CardType.社保卡)
            {
                Phone = NewPhone;
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
                    ShowAlert(false, "个人信息", "个人信息更新失败");
                }
            });
        }
    }
}
