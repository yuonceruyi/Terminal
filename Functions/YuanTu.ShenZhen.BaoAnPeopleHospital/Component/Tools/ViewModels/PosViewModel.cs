using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Gateway;
using YuanTu.Core.Log;
using YuanTu.Devices.CardReader;
using YuanTu.Devices.MKeyBoard;
using YuanTu.ISO8583.CPUCard;
using YuanTu.ISO8583.Interfaces;
using YuanTu.ISO8583.IO;
using YuanTu.ISO8583.Util;
using YuanTu.ISO8583.深圳;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Component.Tools.ViewModels
{
    public class PosViewModel : Default.Component.Tools.ViewModels.PosViewModel
    {
        protected Input _cardResource;
        protected Guid _seed;
        protected IMKeyboard ZtMKeyboard;
        protected IMagCardReader magCardReader;
        protected IIcCardReader icCardReader;

        public PosViewModel(IMKeyboard[] imKeyboards, IMagCardReader[] magCardReaders, IIcCardReader[] icCardReaders)
        {
            PosConfig = ConfigBaoAnPeopleHospital.PosConfig;

            ZtMKeyboard = imKeyboards?.FirstOrDefault(p => p.DeviceId == "ZTKeyboard");
            magCardReader = magCardReader ?? magCardReaders.FirstOrDefault(d => d.DeviceId == "ACT_A6_Mag&IC");
            icCardReader = icCardReader ?? icCardReaders.FirstOrDefault(d => d.DeviceId == "ACT_A6_Mag&IC");
        }

        public IManager Manager { get; set; }
        public IConfig PosConfig { get; set; }

        protected override void StartPosFlow()
        {
            Tips = "初始化设备...";
            DoCommand(p =>
            {
                p.ChangeText("正在初始化银联网络，请稍候...");
                var success = InitializePos();
                return success;
            }).ContinueWith(ctx =>
            {
                if (!(ctx.Result && SureGetCard()))
                {
                    TryPreview();
                    return false;
                }
                return true;
            }).ContinueWith(p =>
            {
                if (FrameworkConst.VirtualThridPay)
                    return;
                if (p.Result)
                {
                    SurePassword();
                }
                else
                {
                    _mustClose = true;
                    CloseDevices("异常操作，结束");
                }
            });
        }

        protected override bool InitializePos()
        {
            if (FrameworkConst.VirtualThridPay)
                return true;
            var sw = new Stopwatch();
            sw.Start();
            CPUDecoder.TagNames = ISO8583.Manager.LoadTagsDevice();
            Manager = new Loader().Initialize(ConfigBaoAnPeopleHospital.PosConfig);
            Manager.POS.CalcMacFunc = CalcMac; //该计算资源应由金属键盘操作
            if (Manager.MagCardReader == null)
                Manager.MagCardReader = magCardReader;
            if (Manager.IcCardReader == null)
                Manager.IcCardReader = icCardReader;
            var rest = Manager.Initialize();
            sw.Stop();
            Logger.POS.Debug($"LoadTagsDevice&Initialize:{sw.ElapsedMilliseconds}");
            if (!rest.IsSuccess)
            {
                ShowAlert(false, "温馨提示", $"银联初始化异常:{rest.Message}");
                return false;
            }
            if (!PosConfig.IsLogon)
            {
                Logger.POS.Info($"Config.IsLogon:{PosConfig.IsLogon} 未签到,开始签到");
                var section61 = Manager.DoLogon();
                if (!section61.IsSuccess)
                {
                    ShowAlert(false, "温馨提示", $"银联签到失败:{section61.Message}");
                    return false;
                }
                var result = ZtMKeyboard.Connect();

                if (!result.IsSuccess)
                {
                    ShowAlert(false, "温馨提示", $"金属键盘初始化失败:{result.Message}");
                    return false;
                }
                var pinkey = section61.Value.Take(16).ToArray().Bytes2Hex();
                var pinchk = section61.Value.Skip(16).Take(4).ToArray().Bytes2Hex();
                var mackey = section61.Value.Skip(20).Take(8).ToArray().Bytes2Hex();
                var macchk = section61.Value.Skip(36).Take(4).ToArray().Bytes2Hex();
                Logger.POS.Debug($"银联签到成功：pinkey：{pinkey} pinchk：{pinchk} mackey：{mackey} macchk：{macchk}");
                var re = ZtMKeyboard.LoadWorkKey(pinkey, pinchk, mackey, macchk);
                if (!re.IsSuccess)
                {
                    ShowAlert(false, "温馨提示", $"金属键盘密钥装载失败:{re.Message}");
                    return false;
                }
                PosConfig.IsLogon = true;
            }
            else
            {
                Logger.POS.Info($"Config.IsLogon:{PosConfig.IsLogon} 已签到");
                var result = ZtMKeyboard.Connect();

                if (!result.IsSuccess)
                {
                    ShowAlert(false, "温馨提示", $"金属键盘初始化失败:{result.Message}");
                    return false;
                }
            }

            return rest.IsSuccess;
        }

        protected override bool SureGetCard()
        {
            PlaySound(SoundMapping.银行卡支付);
            Tips = "请插入银行卡...";
            if (FrameworkConst.VirtualThridPay)
            {
                _cardResource = new Input();
                return true;
            }
            if (_mustClose)
                return false;
            if (CurrentStrategyType() == DeviceType.Clinic) //银联非接
                return true;

            var result = Manager.IcCardReader.Connect();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "温馨提示", result.Message);
                return false;
            }

            Manager.MagCardReader.Initialize();
            _seed = Guid.NewGuid();
            var local = _seed;
            while (local == _seed)
            {
                var res = Manager.MagCardReader.GetCardPosition();
                var pos = result.IsSuccess ? res.Value : CardPos.未知;
                if (!res.IsSuccess || pos != CardPos.停卡位 && pos != CardPos.IC位)
                {
                    Thread.Sleep(200);
                    continue;
                }
                Thread.Sleep(200);   //立马去读的话会出现读取失败的情况
                _cardResource = new Input { Now = DateTimeCore.Now, Amount = (int)ExtraPaymentModel.TotalMoney };
                var rest = Manager.ReadCard(_cardResource);
                if (!rest.IsSuccess)
                {
                    CloseDevices(res.Message);
                    ShowAlert(false, "温馨提示", result.Message);
                    return false;
                }
                return true;
            }
            CloseDevices("该操作已经取消");

            return false;
        }

        protected override bool SurePassword()
        {
            Tips = "请输入密码...";
            ShowKeyboardAnimation = true;
            ShowInputPassWord = true;

            PlaySound(SoundMapping.输入银行卡密码);
            if (FrameworkConst.VirtualThridPay)
                return true;
            ZtMKeyboard.SetKeyAction(KeyboardOrder);
            Task.Factory.StartNew(() =>
                {
                    _cardResource.PIN = ZtMKeyboard.BeforeAddPin().Value.Hex2Bytes();
                    //_cardResource.PIN = ZtMKeyboard.BeforeAddPin(_cardResource.BankNo).Value.Hex2Bytes();
                });
            return true;
        }

        protected override void StartPay()
        {
            DoCommand(p =>
            {
                if (FrameworkConst.VirtualThridPay)
                {
                    var pret = new Output
                    {
                        Ret = "00",
                        Message = "交易成功",
                        BankNo = "622319******7113",
                        Amount = (int)ExtraPaymentModel.TotalMoney,
                        TransSeq = 111132,
                        TransTime = DateTimeCore.Now,
                        CenterSeq = "094117503517",
                        MerchantID = "302053280620002",
                        TerminalID = "00020026"
                    };
                    return Result<Output>.Success(pret);
                }
                return Manager.DoSale(_cardResource);
            }).ContinueWith(ret =>
            {
                if (ret.Result.IsSuccess)
                {
                    ExtraPaymentModel.PaymentResult = OutPut2TransRes(ret.Result.Value);
                    var tsk = ExtraPaymentModel.FinishFunc?.Invoke();
                    if (tsk != null)
                    {
                        tsk.ContinueWith(payRet =>
                        {
                            if (!payRet?.Result.IsSuccess ?? false)
                            {
                                if (FrameworkConst.VirtualThridPay)
                                {
                                    ShowAlert(false, "扣费失败", "交易失败，请重试！" + payRet?.Result.Message);
                                    return;
                                }
                                var code = payRet?.Result.ResultCode ?? 0;
                                if (DataHandler.UnKnowErrorCode.Contains(code))
                                {
                                    var errorMsg = $"银联消费成功，网关返回未知结果{code}，打印凭条结束交易！\n请执凭条到人工咨询此交易结果！";
                                    PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"),
                                        GatewayUnknowErrorPrintables(errorMsg));
                                    ShowAlert(false, "业务处理异常", errorMsg);
                                    CloseDevices(errorMsg);
                                    Navigate(A.Home);
                                }
                                else
                                {
                                    if (CurrentStrategyType() == DeviceType.Clinic) //银联非接
                                    {
                                        //todo 诊间屏处理
                                    }
                                    var refundRet = Manager.DoReverse();
                                    if (!refundRet.IsSuccess)
                                    {
                                        PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"),
                                            RefundFailPrintables(payRet?.Result.Message, refundRet.Message));
                                        ShowAlert(false, "扣费失败", "银联冲正失败，请持凭条与医院工作人员联系！\n请尝试其他支付方式！");
                                    }
                                    else
                                    {
                                        ShowAlert(false, "扣费失败", "交易失败！\n" + payRet?.Result.Message);
                                    }
                                    TryPreview();
                                }
                            }
                            CloseDevices("消费结束");
                        });
                    }
                    else
                    {
                        PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"),
                            BusinessFailPrintables());
                        ShowAlert(false, "扣费失败", "交易失败，请重试！");
                        CloseDevices("系统异常，交易操作没有任何返回！");
                        TryPreview();
                    }
                }
                else
                {
                    ShowAlert(false, "扣费失败", $"{ret.Result.Message}");
                    CloseDevices("扣费失败");
                    TryPreview();
                }
            });
        }

        protected override void CloseDevices(string reason)
        {
            if (FrameworkConst.VirtualThridPay)
                return;
            MisposUnionService.DisConnect(reason);
            if (ExtraPaymentModel.Complete || _mustClose)
            {
                if (_hasExit)
                    return;
                _hasExit = true;
                try
                {
                    Manager.MagCardReader.UnInitialize();

                    Manager.MagCardReader.DisConnect();
                }
                catch (Exception ex)
                {
                }
            }
        }

        protected virtual TransResDto OutPut2TransRes<T>(T res)
        {
            var outPut = res as Output;
            return new TransResDto
            {
                RespCode = outPut?.Ret,
                RespInfo = outPut?.Message,
                CardNo = outPut?.BankNo,
                Amount = outPut?.Amount.ToString(),
                Trace = outPut?.TransSeq.ToString(),
                //Batch = outPut.BatchNo.ToString(),
                Batch = PosConfig?.BatchNo.ToString(),
                TransDate = outPut?.TransTime.ToString("MMdd"),
                TransTime = outPut?.TransTime.ToString("HHmmss"),
                Ref = outPut?.CenterSeq,
                Auth = outPut?.AID,
                MId = outPut?.MerchantID,
                TId = outPut?.TerminalID
            };
        }

        public virtual void KeyboardOrder(KeyText ktext)
        {
            var action = (Action<string>)(p => { BankPassword = p; });
            switch (ktext.KeyOrder)
            {
                case KeyEnum.Number:
                    action("".PadRight(ktext.KeyLength, '*'));
                    break;

                case KeyEnum.Confirm:
                    StartPay();
                    break;

                case KeyEnum.Clear:
                    action("");
                    break;

                case KeyEnum.Cancel:
                    CloseDevices("取消操作");
                    Preview();
                    break;

                case KeyEnum.BackSpace:
                    action("".PadRight(ktext.KeyLength, '*'));
                    break;

                case KeyEnum.Timeout:
                    CloseDevices("键盘操作超时");
                    Preview();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private byte[] CalcMac(byte[] array)
        {
            var len = array.Length - 13 - 8;
            var tmpData = new byte[len];
            Array.Copy(array, 13, tmpData, 0, len);

            var text = tmpData.Bytes2Hex();
            var mac = ZtMKeyboard.CalcMac(text, KMode.PEA_DES, MacMode.银联算法);

            var desArray = Encoding.ASCII.GetBytes(mac.Value.Substring(0, 8));
            return desArray;
        }
    }
}