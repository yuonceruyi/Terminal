using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.XiaoShanZYY.CitizenCard
{
    public class MisposHelper
    {
        private const string Prefix = "杭州市民卡/健康卡";
        private static long _count;

        public static Result<string> Query<TReq>(TReq req)
            where TReq : IReqBase
        {
            var i = Interlocked.Increment(ref _count);
            var type = req.transCode;
            var amount = req.amount;
            var prefix = $"[{Prefix}] [{req.serviceName}] [{i}]";
            try
            {
                if (Startup.SMK)
                    return Result<string>.Success(FakerServer(type));

                var reqStr = req.Serilize();
                Logger.Net.Info($"{prefix} 发送内容:{req.ToJsonString()}");
                var watch = Stopwatch.StartNew();
                var sb = new StringBuilder(1024);
                int ret;
                using (var unloadMispos = new UnloadMispos())
                {
                    ret = unloadMispos.MisPos_Handle(type, sb, amount, reqStr);
                }
                watch.Stop();
                var time = watch.ElapsedMilliseconds;
                Logger.Net.Info($"{prefix} 耗时:{time}毫秒 接收内容:{sb}");
                
                if (sb.ToString().IsNullOrWhiteSpace())
                    return Result<string>.Fail("返回内容为空");

                var result = ret < 0 
                    ? Result<string>.Fail(ErrorCodeParse.ErrorParse(ret)) 
                    : Result<string>.Success(sb.ToString());
                result.ResultCode = ret;
                return result;
            }
            catch (Exception ex)
            {
                Logger.Net.Error($"{prefix}  错误内容:{ex.Message} {ex.StackTrace}");
                return Result<string>.Fail(ex.Message, ex);
            }
        }

        public static Result<TRes> Query<TRes, TReq>(TReq req)
            where TReq : IReqBase
            where TRes : IResBase, new()
        {
            var result = Query(req);
            if (!result.IsSuccess)
                return result.Convert().Convert<TRes>();
            var res = new TRes();
            res.Parse(result.Value);
            return Result<TRes>.Success(res);
        }

        public static string FakerServer(int type)
        {
            string dest = string.Empty;
            switch (type)
            {
                case 1000:
                    dest = string.Empty;
                    break;
                case 1010:
                    dest = string.Empty;
                    break;
                case 6215:
                    dest = "00";
                    break;
                case 6225:
                    dest = "00";
                    break;
                case 57005:
                    dest = "00#125800000001#00139008#0001#7005#          #0004#  0514#131913#150514062777#000001";
                    break;
                case 81025:
                    dest = "00#000000047390# A22896947#1#0000000000";
                    // dest= "00#000000003872# 950477452#1#朱蒋杰              #330682199211082830  #18664944394#浙江省上虞市沥海镇东海村朱邵俞家４２号                      #0000000000";
                    //dest = "14";
                    //ret = 12;
                    break;

                //00#000000000000# B10646606#1#0000000000;
                //00#000000003872# 950477452#1#朱蒋杰              #330682199211082830  #18664944394#浙江省上虞市沥海镇东海村朱邵俞家４２号                      #0000000000;

                case 1001:
                    dest = "55386880142976012800003538";
                    break;
                case 1003:
                    dest =
                        "339900D15600000500654DE6EEBA20BD|3|W20798392|IND0L4253735      |RAJNI AGGARWAL                |2|";
                    break;
                case 1004:
                    //dest = "330109D1560000050063CDB24944F5F3|3|B22451259|332501197808090023|白冬丽                        |2|||||330100831518|";
                    // dest = "|9|50476090|||||||||";
                    //dest = "330109D156000005004829043B88A39E|1|333333333|330109201110261915|测试                        |1|||||330100837199|";
                    //ret = 5;
                    // dest = "339900D1560000050031152817B06199|3|A02796279|330103198602024382| 公务员11 | 2 | 0 | 0 | DLLVER1.2.7 | LIBVER14 | 330100837225"; //省医保
                    dest = "330621D1560000050150461078A455F0|3|B07541184|432922197306307429|庾韩英|2|0|0|DLLVER1.2.7|LIBVER14|3301008018F0";
                    break;
                case 7020:
                    //dest = "00#00#宋开冰|330102198401010010|13757100010|900003538|1429760128|0|2|4|0000050200|9949800";
                    dest = "00#00";
                    break;
                case 7010:
                    dest = "00#330160400375#00018336#0000#7010#4047245300#0530#112734#150530261567#000001#000056#000000010000#00000000#   A22896947#000000057390#000000200000";
                    break;
                case 9901:
                    dest = "3128002027";
                    break;
                case 9904:
                    dest = "0#0#0016#123456";
                    //dest = "2#";
                    break;
                case 91025:
                    //dest = "00#125800000001#00139009#0000#1325#3520909293#2015#062517#090809490632#000001#017701";
                    dest = "55";
                    break;
                case 91325:
                    dest = "00###########";
                    break;

            }
            return dest;
        }
    }
}