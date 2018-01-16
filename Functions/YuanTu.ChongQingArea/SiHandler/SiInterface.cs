using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.ChongQingArea.SiHandler
{
    public class SiInterface
    {
        private const string DllPathSiInterface = @"SiInterface.dll";
        private static long _count;
        public static bool IsBebug=false;

        private static bool _inited;

        [DllImport(DllPathSiInterface, EntryPoint = "INIT", CharSet = CharSet.Ansi)]
        protected static extern int Si_Init();

        private static int Init()
        {
            if (FrameworkConst.FakeServer || IsBebug)
                return 0;
            return Si_Init();
        }

        [DllImport(DllPathSiInterface, EntryPoint = "BUSINESS_HANDLE", CharSet = CharSet.Ansi)]
        protected static extern int Si_Business_Handle(string inputData, StringBuilder outputData);

        private static int Business_Handle(string inputData, StringBuilder outputData)
        {
            if (FrameworkConst.FakeServer || IsBebug)
            {
                outputData.Append(Load(inputData));
                return 0;
            }
            return Si_Business_Handle(inputData, outputData);
        }

        private static string Load(string inputData)
        {
            var code = inputData.Split('|')[0];
            return File.ReadAllText($"FakeServer/{code}.txt");
        }

        private static Result<TRes> Handle<TRes, TReq>(TReq req, Func<string, TRes> func)
            where TReq : Req
            where TRes : Res
        {
            if (InnerConsts.MockSocialSecurity)
            {
                var fakerdir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FakeServer");
                string res;
                var file = Path.Combine(fakerdir, FrameworkConst.HospitalId, $"{req.交易类别}.txt");
                if (!File.Exists(file))
                {
                    file = Path.Combine(fakerdir, $"{req.交易类别}.txt");
                    if (!File.Exists(file))
                        return Result<TRes>.Fail($"模拟参数{file}未找到");
                }
                using (var sr = new StreamReader(file))
                {
                    res = sr.ReadToEnd();
                }


                Thread.Sleep(500);
                var ret = func(res);
                return Result<TRes>.Success(ret);
            }
            try
            {

                if (!_inited)
                {
                    Logger.Net.Info($"[医保][Init]");
                    var isw = Stopwatch.StartNew();
                    var initRet = Init();
                    isw.Stop();
                    Logger.Net.Info($"[医保][Init][{isw.ElapsedMilliseconds}ms][{initRet}]");
                    if (initRet < 0)
                        return Result<TRes>.Fail($"[Init]:{initRet}");
                    _inited = true;
                }
            }
            catch (Exception exception)
            {
                Logger.Net.Warn($"[医保][Init][Exception]{exception}");
                return Result<TRes>.Fail(exception.Message, exception);
            }
            var i = Interlocked.Increment(ref _count);
            string name = $"{req.交易类别代码}-{req.交易类别}";
            var prefix = $"[医保][{i}][{name}]";
            try
            {
                var query = req.ToQuery();

                Logger.Net.Info($"{prefix}[Send] {query}");
                //#if DEBUG
                Logger.Net.Debug($"{prefix}[Send@Json] {req.ToJsonString()}");
                //#endif
                var resBuilder = new StringBuilder(1024);
                var sw = Stopwatch.StartNew();
                var ret = Business_Handle(query, resBuilder);
                sw.Stop();
                var resString = resBuilder.ToString();

                Logger.Net.Info($"{prefix}[{sw.ElapsedMilliseconds}ms][Recv][{ret}] {resString}");

                if (ret != 0)
                    return Result<TRes>.Fail(resString);

                var res = func(resString);
                //#if DEBUG
                Logger.Net.Debug($"{prefix}[{sw.ElapsedMilliseconds}ms][Recv@Json][{ret}] {res.ToJsonString()}");
                //#endif
                if (res.执行代码 != "1")
                    return Result<TRes>.Fail(res.错误信息);
                return Result<TRes>.Success(res);
            }
            catch (Exception exception)
            {
                Logger.Net.Warn($"{prefix}[Exception]{exception}");
                return Result<TRes>.Fail(exception.Message, exception);
            }
        }

        public static Result<Res获取人员基本信息> 获取人员基本信息(Req获取人员基本信息 req)
        {
            return Handle(req, s => Res获取人员基本信息.Parse(s, req.险种类别));
        }

        public static Result<Res就诊登记> 就诊登记(Req就诊登记 req)
        {
            return Handle(req, Res就诊登记.Parse);
        }

        public static Result<Res更新就诊信息> 更新就诊信息(Req更新就诊信息 req)
        {
            return Handle(req, Res更新就诊信息.Parse);
        }

        public static Result<Res添加处方明细> 添加处方明细(Req添加处方明细 req)
        {
            return Handle(req, Res添加处方明细.Parse);
        }

        public static Result<Res结算> 结算(Req结算 req)
        {
            return Handle(req, Res结算.Parse);
        }

        public static Result<Res预结算> 预结算(Req预结算 req)
        {
            return Handle(req, Res预结算.Parse);
        }

        public static Result<Res获取医保特殊病审批信息> 获取医保特殊病审批信息(Req获取医保特殊病审批信息 req)
        {
            return Handle(req, Res获取医保特殊病审批信息.Parse);
        }

        public static Result<Res获取人员账户基础信息> 获取人员账户基础信息(Req获取人员账户基础信息 req)
        {
            return Handle(req, s => Res获取人员账户基础信息.Parse(s, req.险种类别));
        }

        public static Result<Res获取新老卡卡号> 获取新老卡卡号(Req获取新老卡卡号 req)
        {
            return Handle(req, Res获取新老卡卡号.Parse);
        }

        public static Result<Res读卡交易> 读卡交易(Req读卡交易 req)
        {
            return Handle(req, Res读卡交易.Parse);
        }

        public static Result<Res冲正交易> 冲正交易(Req冲正交易 req)
        {
            return Handle(req, Res冲正交易.Parse);
        }
    }
}