using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Systems.Ini;
using YuanTu.Devices.UnionPay;
using YuanTu.YiWuFuBao.Device;

namespace YuanTu.YiWuFuBao.Services
{
    public class XuZiUnionService : IMisposUnionService
    {
        public string ServiceName => "旭子";
        private static readonly IniFile PosIniFile = new IniFile(Path.Combine(FrameworkConst.RootDirectory, "Pos.ini"));
        private IniString LoginDate = new IniString(PosIniFile, "XuZi", "LoginDate");

        private bool _hasEject = false;
        public bool IsConnected { get; set; }
        public bool IsBusy { get; set; }

        public Result Initialize(Business businessType, string misposdllPath, BanCardMediaType bankMediaTyp)
        {
            try
            {
                IsConnected = false;
                _hasEject = false;

                var xz = XzKeyBoard.Initialize();
                if (xz)
                {
                    var nowdate = DateTimeCore.Now.ToString("yyyyMMdd");
                    if (nowdate != LoginDate.Value)
                    {
                        var ret = XzKeyBoard.DoLogon();
                        if (!ret.IsSuccess)
                        {
                            Logger.POS.Error($"[{ServiceName}银联]签到失败");
                            return Result.Fail("初始化签到失败");
                        }
                        LoginDate.Value = nowdate;
                        Logger.POS.Info($"[{ServiceName}银联]签到成功，签到日期：{nowdate}");
                    }
                    IsConnected = true;
                    Logger.POS.Info($"[{ServiceName}银联]初始化成功");
                    return Result.Success();
                }
                Logger.POS.Error($"[{ServiceName}银联]初始化失败");
            }
            catch (Exception ex)
            {

                Logger.POS.Error($"[{ServiceName}银联]初始化失败,{ex.Message} {ex.StackTrace}");
            }
           
            return Result.Fail("初始化失败");
        }

        public Result SetReq(TransType transType, decimal totalMoneySencods)
        {
            throw new NotImplementedException("SetReq 未实现");
        }

        public Result<string> ReadCard(BanCardMediaType bankMediaTyp)
        {
            IsBusy = false;
            var ret = XzKeyBoard.SetEnterCard(true);
            if (!ret)
            {
                Logger.POS.Error($"[{ServiceName}银联]设置允许进卡失败");
                IsBusy = false;
                return Result<string>.Fail("设置允许进卡失败");
            }
            while (IsConnected)
            {
                IsBusy = true;
                try
                {
                    if (!XzKeyBoard.HaveCard())
                    {
                        IsBusy = false;
                        Thread.Sleep(500);
                        continue;
                    }
                    Thread.Sleep(300);
                    Logger.POS.Info($"[{ServiceName}银联]检测到卡，准备移动");
                    if (!XzKeyBoard.MoveCard())  //移动到读卡位置
                    {
                        IsBusy = false;
                        return Result<string>.Fail("读卡器无法识别您的银行卡，请换一张试试！");
                    }
                   
                    Logger.POS.Info($"[{ServiceName}银联]读卡");

                    int n = 0;
                    if (!XzKeyBoard.Read(out n))
                    {
                        Logger.POS.Error($"[{ServiceName}银联]读卡失败，返回值:{n}");
                        IsBusy = false;
                        return Result<string>.Fail("读卡失败，请重试！");
                    }
                    IsBusy = false;
                    Logger.POS.Info($"[{ServiceName}银联]读卡成功！");
                    return Result<string>.Success("");
                }
                catch (Exception ex)
                {
                    Logger.POS.Error($"[{ServiceName}]读卡时发生异常，详情:{ex.Message} 堆栈:{ex.StackTrace}");
                    IsBusy = false;
                    return Result<string>.Fail("读卡异常，请重试！",ex);
                }
               
            }
            IsBusy = false;
            return Result<string>.Fail("取消操作");
        }

        public Result StartKeyboard(Action<KeyText> keyAction)
        {
            XzKeyBoard.GetPin();
            var keycontent = string.Empty;
            while (IsConnected)
            {
                char c;
                XzKeyBoard.Scan(out c);
                Key key = Key.未知;
                switch (c)
                {
                    case '\b': //清空
                        key = Key.清空;
                        keycontent = string.Empty;
                        Logger.POS.Info($"[{ServiceName}银联]按钮按下:[清空]");
                        break;
                    case '\u0011': //取消
                        Logger.POS.Info($"[{ServiceName}银联]按钮按下:[取消]");
                        return Result.Fail("键盘输入取消");
                    case '€': //键盘超时
                        Logger.POS.Info($"[{ServiceName}银联]按钮按下:[超时]");
                        return Result.Fail("键盘输入超时");
                    case '\0': //异常字符
                        break;
                    default: //输入密码
                        key = Key.按键;
                        keycontent += "*";
                        Logger.POS.Info($"[{ServiceName}银联]按钮按下:[{keycontent}]");
                        break;
                }
                var ktext = new KeyText {Key = key, KeyContent = keycontent};
                keyAction?.Invoke(ktext);
                if (keycontent.Length == 6 || key == Key.确认)
                {
                    return Result.Success();
                }
                Thread.Sleep(200);
            }
            return Result.Fail("取消键盘操作");
        }

        private XzKeyBoard.Output _output = null;

        public Result<TransResDto> DoSale(decimal totalMoneySencods)
        {
            string returninfo;
            XzKeyBoard.GetPinBlock(out returninfo);

            int ret = XzKeyBoard.DoSale(Convert.ToInt32(totalMoneySencods), out _output);
            Logger.POS.Info($"[{ServiceName}银联]消费成功，金额:{totalMoneySencods} 返回:{_output}");
            if (ret == 0)
            {
                return Result<TransResDto>.Success(Parse(_output));
            }
            else if (ret == -2) //此处需要打印单边账凭条
            {
                var result = Result<TransResDto>.Fail(-2, "因银联网络故障可能导致您的资金风险");
                result.Value = Parse(_output);
                return result;
            }
            return Result<TransResDto>.Fail(_output?.错误信息);
        }

        public Result<TransResDto> Refund(string reason)
        {
            if (_output == null)
            {
                return Result<TransResDto>.Fail("没有需要撤销的交易");
            }
            var money = decimal.Parse(_output.交易金额)*100;
            XzKeyBoard.Output refundOutput;
            var refund = XzKeyBoard.DoRefund(Convert.ToInt32(money), _output.流水号, _output.系统参考号, out refundOutput);
            Logger.POS.Info($"[{ServiceName}银联]冲正 原因:{reason}\r\n原始报文:{_output}\r\n冲正结果:{refundOutput}");

            if (refund)
            {
                return Result<TransResDto>.Success(Parse(refundOutput));
            }
            return Result<TransResDto>.Fail(refundOutput?.错误信息);
        }

        public Result DisConnect(string reason)
        {
            Logger.POS.Info($"[{ServiceName}银联]断开连接 当前状态:{IsConnected} 原因:{reason}");
            if (IsConnected)
            {
                IsConnected = false;
            }
            return Result.Success();
        }

        public Result UnInitialize(string reason)
        {
            Logger.POS.Info($"[{ServiceName}银联]反初始化 当前状态:{IsConnected} 原因:{reason} 是否已退出:{(_hasEject?"已退卡":"未退卡")}");

            DisConnect(reason);
            if (!_hasEject)
            {
                try
                {
                    var ejectRet = XzKeyBoard.Eject();
                    //var allret = XzKeyBoard.SetEnterCard(false);
                    var ret = XzKeyBoard.SIT_Trans_UnInit();
                    Logger.POS.Info($"[{ServiceName}银联]退卡结果:{(ejectRet ? "成功" : "失败")} 反初始化返回值(0表示成功):{ret}");
                }
                catch (Exception ex)
                {

                    Logger.POS.Info($"[{ServiceName}银联]退卡出现异常，异常原因：{ex.Message} {ex.StackTrace}");
                    return Result.Fail("退卡失败");
                }
              
                _hasEject = true;
            }
            return Result.Success();
        }

        private TransResDto Parse(XzKeyBoard.Output output)
        {
            var tradeTime = DateTimeCore.Now;//DateTime.ParseExact(output?.交易时间, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None);
            var res = new TransResDto()
            {
                //RespCode = ret.ToString(),
                //RespInfo = 
                CardNo = output?.卡号,
                Amount = (decimal.Parse(output?.交易金额 ?? "0")*100).ToString("0"),
                Trace = output?.流水号,
                Batch = output?.批次号,
                TransDate = tradeTime.ToString("yyyyMMdd"),
                TransTime = tradeTime.ToString("HHmmss"),
                Ref = output?.系统参考号,
                Auth = output?.授权号,
                MId = output?.商户代码,
                TId = output?.终端号,
                Receipt = output?.ToString()
            };
            return res;
        }
    }
}
