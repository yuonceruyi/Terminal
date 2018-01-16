using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.ZheJiangHospital.HIS
{
    internal class DataHandler
    {
        private static long _count;

        private static readonly int[] 省医保Types = { 6291, 6227, 6228, 6229, 6258 };
        private static readonly int[] 市医保Types = { 6148 };
        private static readonly int[] 自费Types =
        {
            6289, 6288, 6286, 6287, 6262, 6238, 6172, 6246, 6224, 6263, 6267, 6279,
            6273, 6272, 6216, 6222, 6221, 6220, 6112, 6149, 6265, 6196, 6254, 6261, 6194, 6260, 6290, 6278, 6270, 6175,
            6176, 6280, 6266, 6151, 6255, 6213, 6230, 6212, 6195, 6171, 6192, 6174, 6245, 6150, 6173, 6170, 6109, 6211,
            6252, 6152, 6153, 6274, 6276, 6275, 6264, 6256, 6247, 6240, 6277, 6215, 6219, 6218, 6217, 6225, 6113, 6110,
            6114
        };

        private static readonly FileMappingHandler FmHandler = new FileMappingHandler();

        public static string ConnectionString { get; set; } =
            "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=ngrok.yuantutech.com)(PORT=15218)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=HRP275)));User Id=ytzzfw;Password=ytzzfw;"
            ;

        public static string ExePath { get; set; }
        public static long Count => _count++;

        private static Result<string> Load(string name, Encoding encoding = null)
        {
            try
            {
                if (encoding == null)
                    encoding = Encoding.UTF8;
                var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FakeServer", "160");
                string path;
                if (name.Contains('.'))
                    path = Path.Combine(dir, name);
                else
                    path = Path.Combine(dir, $"{name}.json");
                if (!File.Exists(path))
                    return Result<string>.Fail($"{name}.json 未找到");
                return Result<string>.Success(File.ReadAllText(path, encoding));
            }
            catch (Exception e)
            {
                return Result<string>.Fail(e.Message, e);
            }
        }

        private static Result<Tuple<string, string>> GetIDs(string idNo)
        {
            if (string.IsNullOrEmpty(idNo))
                return Result<Tuple<string, string>>.Fail("身份证号不存在");

            string idNo18, idNo15;
            if (idNo.Length == 15)
            {
                idNo15 = idNo;
                var result = idNo15.ID15To18();
                if (!result.IsSuccess)
                    return Result<Tuple<string, string>>.Fail(result.Message);
                idNo18 = result.Value;
            }
            else if (idNo.Length == 18)
            {
                idNo18 = idNo;
                var result = idNo.ID18To15();
                if (!result.IsSuccess)
                    return Result<Tuple<string, string>>.Fail(result.Message);

                idNo15 = result.Value;
            }
            else
            {
                return Result<Tuple<string, string>>.Fail("身份证号不正确");
            }
            return Result<Tuple<string, string>>.Success(new Tuple<string, string>(idNo18, idNo15));
        }

        public static Result<List<BINGRENXX>> GetPatient(string patientId)
        {
            var n = $"[本地HIS视图] [GetPatient] [{Count}]";
            Logger.Net.Info($"{n} 发送内容: {patientId}");
            var sw = Stopwatch.StartNew();
            if (FrameworkConst.FakeServer)
            {
                var r = Load("YT_V_BINGRENXX");
                if (!r.IsSuccess)
                {
                    Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: GetIDs Fail: {r.Message}");
                    return Result<List<BINGRENXX>>.Fail(r.Message);
                }
                var l = r.Value.ToJsonObject<List<BINGRENXX>>();
                return
                    Result<List<BINGRENXX>>.Success(
                        l.Where(i => i.PATIENTID.ToString() == patientId).ToList());
            }
            using (var con = new OracleConnection(ConnectionString))
            {
                try
                {
                    con.Open();
                    var items = con.Query<BINGRENXX>(
                        $"SELECT * FROM YT_V_BINGRENXX WHERE 1=1" +
                        $" AND PATIENTID = :{nameof(patientId)}",
                        new { patientId }).ToList();

                    Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: {items.ToJsonString()}");
                    return Result<List<BINGRENXX>>.Success(items);
                }
                catch (Exception exception)
                {
                    Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: Fail: {exception}");
                    return Result<List<BINGRENXX>>.Fail("查询人员信息失败", exception);
                }
            }
        }

        public static Result<List<BINGRENXX>> GetPatient省医保(string medNo, string idNo)
        {
            var idsResult = GetIDs(idNo);
            if (!idsResult.IsSuccess)
                return Result<List<BINGRENXX>>.Fail(idsResult.Message);

            var ids = idsResult.Value;
            var idNo18 = ids.Item1;
            var idNo15 = ids.Item2;

            if (FrameworkConst.FakeServer)
            {
                var r = Load("YT_V_BINGRENXX");
                if (!r.IsSuccess)
                    return Result<List<BINGRENXX>>.Fail(r.Message);
                var l = r.Value.ToJsonObject<List<BINGRENXX>>();
                return
                    Result<List<BINGRENXX>>.Success(
                        l.Where(i => i.MEDNO == medNo && i.IDNO == idNo && 省医保Types.Contains(i.PATIENTTYPE)).ToList());
            }
            using (var con = new OracleConnection(ConnectionString))
            {
                try
                {
                    con.Open();
                    var items = con.Query<BINGRENXX>(
                        $"SELECT * FROM YT_V_BINGRENXX WHERE 1=1" +
                        $" AND MEDNO = :{nameof(medNo)}" +
                        $" AND (IDNO = :{nameof(idNo18)} OR IDNO = :{nameof(idNo15)})" +
                        $" AND PATIENTTYPE IN ({string.Join(",", 省医保Types)})",
                        new { medNo, idNo18, idNo15 }).ToList();

                    return Result<List<BINGRENXX>>.Success(items);
                }
                catch (Exception exception)
                {
                    return Result<List<BINGRENXX>>.Fail("查询人员信息失败", exception);
                }
            }
        }

        public static Result<List<BINGRENXX>> GetPatient市医保(string medNo, string idNo)
        {
            var idsResult = GetIDs(idNo);
            if (!idsResult.IsSuccess)
                return Result<List<BINGRENXX>>.Fail(idsResult.Message);

            var ids = idsResult.Value;
            var idNo18 = ids.Item1;
            var idNo15 = ids.Item2;

            if (FrameworkConst.FakeServer)
            {
                var r = Load("YT_V_BINGRENXX");
                if (!r.IsSuccess)
                    return Result<List<BINGRENXX>>.Fail(r.Message);
                var l = r.Value.ToJsonObject<List<BINGRENXX>>();
                return
                    Result<List<BINGRENXX>>.Success(
                        l.Where(i => i.MEDNO == medNo && i.IDNO == idNo && i.PATIENTTYPE == 6148).ToList());
            }
            using (var con = new OracleConnection(ConnectionString))
            {
                try
                {
                    con.Open();
                    var items = con.Query<BINGRENXX>(
                        $"SELECT * FROM YT_V_BINGRENXX WHERE 1=1" +
                        $" AND MEDNO = :{nameof(medNo)}" +
                        $" AND (IDNO = :{nameof(idNo18)} OR IDNO = :{nameof(idNo15)})" +
                        $" AND PATIENTTYPE IN ({string.Join(",", 市医保Types)})",
                        new { medNo, idNo18, idNo15 }).ToList();

                    return Result<List<BINGRENXX>>.Success(items);
                }
                catch (Exception exception)
                {
                    return Result<List<BINGRENXX>>.Fail("查询人员信息失败", exception);
                }
            }
        }

        public static Result<List<BINGRENXX>> GetPatient自费(string idNo)
        {
            var n = $"[本地HIS视图] [GetPatient自费] [{Count}]";
            Logger.Net.Info($"{n} 发送内容: {idNo}");
            var sw = Stopwatch.StartNew();
            var idsResult = GetIDs(idNo);
            if (!idsResult.IsSuccess)
            {
                Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: GetIDs Fail: {idsResult.Message}");
                return Result<List<BINGRENXX>>.Fail(idsResult.Message);
            }

            var ids = idsResult.Value;
            var idNo18 = ids.Item1;
            var idNo15 = ids.Item2;


            if (FrameworkConst.FakeServer)
            {
                var r = Load("YT_V_BINGRENXX");
                if (!r.IsSuccess)
                    return Result<List<BINGRENXX>>.Fail(r.Message);
                var l = r.Value.ToJsonObject<List<BINGRENXX>>();
                var list = l.Where(i => i.IDNO == idNo).ToList();
                Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: {list.ToJsonString()}");
                return Result<List<BINGRENXX>>.Success(list);
            }
            using (var con = new OracleConnection(ConnectionString))
            {
                try
                {
                    con.Open();
                    var items = con.Query<BINGRENXX>(
                        $"SELECT * FROM YT_V_BINGRENXX WHERE 1=1" +
                        $" AND (IDNO = :{nameof(idNo18)} OR IDNO = :{nameof(idNo15)})" +
                        $" AND PATIENTTYPE IN ({string.Join(",", 自费Types)})" +
                        $" ORDER BY PATIENTID DESC",
                        new { idNo18, idNo15 }).ToList();

                    Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: {items.ToJsonString()}");
                    return Result<List<BINGRENXX>>.Success(items);
                }
                catch (Exception exception)
                {
                    Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: Fail: {exception}");
                    return Result<List<BINGRENXX>>.Fail("查询人员信息失败", exception);
                }
            }
        }

        public static Result<List<KESHI_GUAHAO>> GetDept(int regType)
        {
            var n = $"[本地HIS视图] [GetDept] [{Count}]";
            Logger.Net.Info($"{n} 发送内容: {regType}");
            var sw = Stopwatch.StartNew();
            if (FrameworkConst.FakeServer)
            {
                var r = Load("YT_V_KESHI_GUAHAO");
                if (!r.IsSuccess)
                    return Result<List<KESHI_GUAHAO>>.Fail(r.Message);
                var l = r.Value.ToJsonObject<List<KESHI_GUAHAO>>();
                var list = l.Where(i => i.REGTYPE == regType).ToList();
                Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: {list.ToJsonString()}");
                return Result<List<KESHI_GUAHAO>>.Success(list);
            }
            using (var con = new OracleConnection(ConnectionString))
            {
                try
                {
                    con.Open();
                    var items = con.Query<KESHI_GUAHAO>(
                        $"SELECT * FROM YT_V_KESHI_GUAHAO WHERE REGTYPE = :{nameof(regType)}",
                        new { regType }).ToList();

                    Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: {items.ToJsonString()}");
                    return Result<List<KESHI_GUAHAO>>.Success(items);
                }
                catch (Exception exception)
                {
                    Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: Fail: {exception}");
                    return Result<List<KESHI_GUAHAO>>.Fail("查询科室列表失败", exception);
                }
            }
        }

        public static Result<List<PAIBAN_KESHI>> GetScheduleDept(string deptCode)
        {
            var n = $"[本地HIS视图] [GetScheduleDept] [{Count}]";
            Logger.Net.Info($"{n} 发送内容: {deptCode}");
            var sw = Stopwatch.StartNew();
            if (FrameworkConst.FakeServer)
            {
                var r = Load("YT_V_PAIBAN_KESHI");
                if (!r.IsSuccess)
                    return Result<List<PAIBAN_KESHI>>.Fail(r.Message);
                var l = r.Value.ToJsonObject<List<PAIBAN_KESHI>>();
                var list = l.Where(i => i.DEPTCODE == deptCode).ToList();
                if (DateTimeCore.Now.Hour >= 12)
                    list = list.Where(i => i.MEDAMPM == 2).ToList();
                Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: {list.ToJsonString()}");
                return Result<List<PAIBAN_KESHI>>.Success(list);
            }
            using (var con = new OracleConnection(ConnectionString))
            {
                try
                {
                    con.Open();
                    var items = con.Query<PAIBAN_KESHI>(
                        $"SELECT * FROM YT_V_PAIBAN_KESHI WHERE DEPTCODE = :{nameof(deptCode)}",
                        new { deptCode }).ToList();

                    if (DateTimeCore.Now.Hour >= 12)
                        items = items.Where(i => i.MEDAMPM == 2).ToList();

                    Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: {items.ToJsonString()}");
                    return Result<List<PAIBAN_KESHI>>.Success(items);
                }
                catch (Exception exception)
                {
                    Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: Fail: {exception}");
                    return Result<List<PAIBAN_KESHI>>.Fail("查询科室排班失败", exception);
                }
            }
        }

        public static Result<List<PAIBAN_YISHENG>> GetScheduleDoctor(string deptCode)
        {
            var n = $"[本地HIS视图] [GetScheduleDoctor] [{Count}]";
            Logger.Net.Info($"{n} 发送内容: {deptCode}");
            var sw = Stopwatch.StartNew();
            if (FrameworkConst.FakeServer)
            {
                var r = Load("YT_V_PAIBAN_YISHENG");
                if (!r.IsSuccess)
                    return Result<List<PAIBAN_YISHENG>>.Fail(r.Message);
                var l = r.Value.ToJsonObject<List<PAIBAN_YISHENG>>();
                var list = l.Where(i => i.DEPTCODE == deptCode).ToList();
                if (DateTimeCore.Now.Hour >= 12)
                    list = list.Where(i => i.MEDAMPM == 2).ToList();
                Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: {list.ToJsonString()}");
                return Result<List<PAIBAN_YISHENG>>.Success(list);
            }
            using (var con = new OracleConnection(ConnectionString))
            {
                try
                {
                    con.Open();
                    var items = con.Query<PAIBAN_YISHENG>(
                        $"SELECT * FROM YT_V_PAIBAN_YISHENG WHERE DEPTCODE = :{nameof(deptCode)}",
                        new { deptCode }).ToList();

                    if (DateTimeCore.Now.Hour >= 12)
                        items = items.Where(i => i.MEDAMPM == 2).ToList();

                    Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: {items.ToJsonString()}");
                    return Result<List<PAIBAN_YISHENG>>.Success(items);
                }
                catch (Exception exception)
                {
                    Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: Fail: {exception}");
                    return Result<List<PAIBAN_YISHENG>>.Fail("查询医生排班失败", exception);
                }
            }
        }
        public static Result<List<PAIBAN_YISHENG>> GetScheduleDoctor()
        {
            var n = $"[本地HIS视图] [GetScheduleDoctor] [{Count}]";
            Logger.Net.Info($"{n} 发送内容:");
            var sw = Stopwatch.StartNew();
            if (FrameworkConst.FakeServer)
            {
                var r = Load("YT_V_PAIBAN_YISHENG");
                if (!r.IsSuccess)
                    return Result<List<PAIBAN_YISHENG>>.Fail(r.Message);
                var l = r.Value.ToJsonObject<List<PAIBAN_YISHENG>>();
                var list = l.ToList();
                if (DateTimeCore.Now.Hour >= 12)
                    list = list.Where(i => i.MEDAMPM == 2).ToList();
                Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: {list.ToJsonString()}");
                return Result<List<PAIBAN_YISHENG>>.Success(list);
            }
            using (var con = new OracleConnection(ConnectionString))
            {
                try
                {
                    con.Open();
                    var items = con.Query<PAIBAN_YISHENG>(
                        $"SELECT * FROM YT_V_PAIBAN_YISHENG ").ToList();

                    if (DateTimeCore.Now.Hour >= 12)
                        items = items.Where(i => i.MEDAMPM == 2).ToList();

                    Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: {items.ToJsonString()}");
                    return Result<List<PAIBAN_YISHENG>>.Success(items);
                }
                catch (Exception exception)
                {
                    Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: Fail: {exception}");
                    return Result<List<PAIBAN_YISHENG>>.Fail("查询医生排班失败", exception);
                }
            }
        }

        public static Result<List<YUYUE_JILU>> GetAppointRecord(string idNo)
        {
            var idsResult = GetIDs(idNo);
            if (!idsResult.IsSuccess)
                return Result<List<YUYUE_JILU>>.Fail(idsResult.Message);

            var ids = idsResult.Value;
            var idNo18 = ids.Item1;
            var idNo15 = ids.Item2;

            var n = $"[本地HIS视图] [GetAppointRecord] [{Count}]";
            Logger.Net.Info($"{n} 发送内容: {idNo}");
            var sw = Stopwatch.StartNew();
            if (FrameworkConst.FakeServer)
            {
                var r = Load("YT_V_YUYUE_JILU");
                if (!r.IsSuccess)
                    return Result<List<YUYUE_JILU>>.Fail(r.Message);
                var l = r.Value.ToJsonObject<List<YUYUE_JILU>>();
                var list = l.Where(i => i.IDNO == idNo).ToList();
                Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: {list.ToJsonString()}");
                return Result<List<YUYUE_JILU>>.Success(list);
            }

            using (var con = new OracleConnection(ConnectionString))
            {
                try
                {
                    con.Open();
                    var items = con.Query<YUYUE_JILU>(
                        $"SELECT * FROM YT_V_YUYUE_JILU WHERE 1=1" +
                        $" AND (IDNO = :{nameof(idNo18)} OR IDNO = :{nameof(idNo15)})",
                        new { idNo18, idNo15 }).ToList();
                    Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: {items.ToJsonString()}");

                    return Result<List<YUYUE_JILU>>.Success(items);
                }
                catch (Exception exception)
                {
                    Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: Fail: {exception}");
                    return Result<List<YUYUE_JILU>>.Fail("查询预约记录失败", exception);
                }
            }
        }

        public static Result<List<CFYJ>> GetToPayRecord(long patientId)
        {
            var n = $"[本地HIS视图] [GetToPayRecord] [{Count}]";
            Logger.Net.Info($"{n} 发送内容: {patientId}");
            var sw = Stopwatch.StartNew();
            if (FrameworkConst.FakeServer)
            {
                var r = Load("V_MS_ZT_CFYJ.csv", Encoding.Default);
                if (!r.IsSuccess)
                    return Result<List<CFYJ>>.Fail(r.Message);
                var l = new List<CFYJ>();
                var lines = r.Value.Split('\n').Skip(1);
                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;
                    var columns = line.Split(',').Select(c => c.Trim('\"', '\r')).ToList();
                    l.Add(new CFYJ()
                    {
                        YPFY = columns[0],
                        YPMC = columns[1],
                        CFSB = columns[2],
                        CFTS = columns[3],
                        YPSL = columns[4],
                        YPDJ = Convert.ToDecimal(columns[5]),
                        YFGG = columns[6],
                        YFDW = columns[7],
                        KFRQ = Convert.ToDateTime(columns[8]),
                        HJJE = Convert.ToDecimal(columns[9]),
                        BRID = Convert.ToInt64(columns[10]),
                    });
                }
                var list = l.Where(i => i.BRID == patientId).ToList();
                Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: {list.ToJsonString()}");
                return Result<List<CFYJ>>.Success(list);
            }

            using (var con = new OracleConnection(ConnectionString))
            {
                try
                {
                    con.Open();
                    var items = con.Query<CFYJ>(
                        $"SELECT * FROM hrp275.V_MS_ZT_CFYJ WHERE BRID = :{nameof(patientId)}",
                        new { patientId }).ToList();
                    Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: {items.ToJsonString()}");

                    return Result<List<CFYJ>>.Success(items);
                }
                catch (Exception exception)
                {
                    Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: Fail: {exception}");
                    return Result<List<CFYJ>>.Fail("查询待缴费记录失败", exception);
                }
            }
        }

        public static Result<List<KHJL>> GetAccount(string idNo)
        {
            var idsResult = GetIDs(idNo);
            if (!idsResult.IsSuccess)
                return Result<List<KHJL>>.Fail(idsResult.Message);

            var ids = idsResult.Value;
            var idNo18 = ids.Item1;
            var idNo15 = ids.Item2;

            var n = $"[本地HIS视图] [GetAccount] [{Count}]";
            Logger.Net.Info($"{n} 发送内容: {idNo}");
            var sw = Stopwatch.StartNew();
            if (FrameworkConst.FakeServer)
            {
                var r = Load("YT_YH_KHJL.csv", Encoding.Default);
                if (!r.IsSuccess)
                    return Result<List<KHJL>>.Fail(r.Message);
                var l = new List<KHJL>();
                var lines = r.Value.Split('\n').Skip(1);
                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;
                    var columns = line.Split(',').Select(c => c.Trim('\"', '\r')).ToList();
                    l.Add(new KHJL()
                    {
                        JLXH = columns[0],
                        SFZH = columns[1],
                        XNZH = columns[2],
                        YHLSH = columns[3],
                        YHKBDBZ = columns[4],
                        KHBZ = columns[5],
                        YHKH = columns[6],
                        BRXM = columns[7],
                    });
                }
                var list = l.Where(i => i.SFZH == idNo15 || i.SFZH == idNo18).ToList();
                Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: {list.ToJsonString()}");
                return Result<List<KHJL>>.Success(list);
            }

            using (var con = new OracleConnection(ConnectionString))
            {
                try
                {
                    con.Open();
                    var items = con.Query<KHJL>(
                        $"SELECT * FROM YT_YH_KHJL WHERE 1=1" +
                        $" AND (SFZH = :{nameof(idNo18)} OR SFZH = :{nameof(idNo15)})",
                        new { idNo18, idNo15 }).ToList();
                    Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: {items.ToJsonString()}");

                    return Result<List<KHJL>>.Success(items);
                }
                catch (Exception exception)
                {
                    Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: Fail: {exception}");
                    return Result<List<KHJL>>.Fail("查询银医通账户失败", exception);
                }
            }
        }

        public static T RunExe<T>(Req req)
            where T : Res, new()
        {
            if (!FmHandler.Inited)
                if (0 != FmHandler.Init("SZZT_SHARE_invokeExe", 0x1004))
                    return null;
            var n = $"[本地HIS服务] [{req.业务类型}] [{Count}]";
            var buffer = new byte[0x1004];
            //FmHandler.Write(buffer, 0, 0x1004);

            var reqString = req.ToString();

            Logger.Net.Info($"{n} 发送内容: {reqString}");
            string resString;

            var sw = Stopwatch.StartNew();
            if (FrameworkConst.FakeServer)
            {
                switch (req.业务类型)
                {
                    case "F142":
                        resString = "-1^有未收费的挂号记录,请先结算后再挂号!";
                        resString = "00^银医通账号^就诊卡号";
                        break;
                    case "F144":
                        resString = "-1^有未收费的挂号记录,请先结算后再挂号!";
                        resString = "00^浙江医院^80101709^9070000100^石庆庆^2017/4/5 09:46:33^本次挂号未收费^0^0^自助测试科室^^1^1^1000675910^门诊一楼302";
                        break;
                    case "F199":
                        resString = "-1^有未收费的挂号记录,请先结算后再挂号!";
                        resString = "00^卡号^姓名^1^民族^2000-01-01^330106200001011235^地址";
                        break;
                    case "F201":
                        resString = "-1^有未收费的挂号记录,请先结算后再挂号!";
                        resString = "00^9999.99|888.88|111.11|111.11";
                        break;
                    case "F202":
                        resString = "-1^有未收费的挂号记录,请先结算后再挂号!";
                        resString = "00^浙江医院^9000881636^695888^张敬红^239.20^239.20^0^0(注：绑定银行卡的以银行卡实际余额为准)^2855^1000427706^A02945000^239.20|0.00|0.00|652.06|0.00|0.00|0.00|0.00|0^0^普通门诊诊查费(三级)|甲|1次|10.00|$一次性真空采血器 检验|甲|1支|1.20|$静脉采血|甲|1次|3.00|$血清促黄体生成素测定|甲|1项|32.00|$血清泌乳素测定|甲|1项|32.00|$孕酮测定|甲|1项|32.00|$睾酮测定|甲|1项|25.00|$血清促卵泡刺激素测定|甲|1项|32.00|$雌二醇测定|甲|1项|32.00|$血清人绒毛膜促性腺激素测定|甲|1项|40.00|^张敬红|省级医保|女|27岁|9000881636|317032707381002|检验科||门诊楼一楼|王琦君|β-HCG$张敬红|省级医保|女|27岁|9000881636|317032707381002|检验科||门诊楼一楼|王琦君|生殖激素常规";
                        break;
                    case "F203":
                        Thread.Sleep(5000);
                        resString = "-1^有未收费的挂号记录,请先结算后再挂号!";
                        resString = "00^浙江医院^9000881636^695888^张敬红^239.20^239.20^0^0(注：绑定银行卡的以银行卡实际余额为准)^2855^1000427706^A02945000^239.20|0.00|0.00|652.06|0.00|0.00|0.00|0.00|0^0^普通门诊诊查费(三级)|甲|1次|10.00|$一次性真空采血器 检验|甲|1支|1.20|$静脉采血|甲|1次|3.00|$血清促黄体生成素测定|甲|1项|32.00|$血清泌乳素测定|甲|1项|32.00|$孕酮测定|甲|1项|32.00|$睾酮测定|甲|1项|25.00|$血清促卵泡刺激素测定|甲|1项|32.00|$雌二醇测定|甲|1项|32.00|$血清人绒毛膜促性腺激素测定|甲|1项|40.00|^张敬红|省级医保|女|27岁|9000881636|317032707381002|检验科||门诊楼一楼|王琦君|β-HCG$张敬红|省级医保|女|27岁|9000881636|317032707381002|检验科||门诊楼一楼|王琦君|生殖激素常规";
                        break;
                    default:
                        resString = $"-1^模拟数据未实现:{req.业务类型}";
                        break;
                }
            }
            else
            {
                var p = Process.Start(new ProcessStartInfo()
                {
                    FileName = ExePath,
                    Arguments = reqString,
                    WorkingDirectory = Path.GetDirectoryName(ExePath),
                });
                p.WaitForExit();
                FmHandler.Read(ref buffer, 0, 0x1004);
                resString = Encoding.Default.GetString(buffer, 4, 0x1000).TrimEnd('\0');
            }
            sw.Stop();


            Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 接收内容: {resString}");
            var res = new T();
            res.Parse(resString);
            return res;
        }
    }

    #region Tables

    public class BINGRENXX
    {
        public string MEDNO { get; set; }
        public string CARDNO { get; set; }
        public string IDNO { get; set; }
        public int PATIENTTYPE { get; set; }
        public string PATIENTTYPENAME { get; set; }
        public long PATIENTID { get; set; }
        public string PATIENTNO { get; set; }
        public string NAME { get; set; }
        public string SEX { get; set; }
        public DateTime? BIRTHDAY { get; set; }
        public string ADDRESS { get; set; }
        public string PHONE { get; set; }
    }

    public class KESHI_GUAHAO
    {
        public string DEPTCODE { get; set; }
        public string DEPTNAME { get; set; }
        public int REGTYPE { get; set; }
        public int WEEKDAY { get; set; }
        public int PBBZ { get; set; }
        public int PLXH { get; set; }
        public int ZJMZ { get; set; }
    }

    public class KESHI_YUYUE
    {
        public string DEPTCODE { get; set; }
        public string DEPTNAME { get; set; }
        public int REGTYPE { get; set; }
        public int WEEKDAY { get; set; }
        public int PBBZ { get; set; }
        public int PLXH { get; set; }
    }

    public class PAIBAN
    {
        public int REGTYPE { get; set; }
        public string DEPTCODE { get; set; }
        public string DEPTNAME { get; set; }
        public string DOCTCODE { get; set; }
        public string DOCTNAME { get; set; }
        public string DOCTTECH { get; set; }
        public int MEDAMPM { get; set; }
        public decimal REGFEE { get; set; }
        public decimal TREATFEE { get; set; }
        public decimal REGAMOUNT { get; set; }
        public decimal? RESTNUM { get; set; }
    }

    public class PAIBAN_KESHI : PAIBAN
    {
        public int MEDDATE { get; set; }
        public int WEEKDAY { get; set; }
        public int PBBZ { get; set; }
        public int PLXH { get; set; }
    }

    public class PAIBAN_YISHENG : PAIBAN
    {
        public DateTime? MEDDATE { get; set; }
        public int MZLB { get; set; }
        public int YGRS { get; set; }
    }

    public class YUYUE_JILU
    {
        public long? PATIENTID { get; set; }
        public string CARDNO { get; set; }
        public int STATUS { get; set; }
        public string DEPTCODE { get; set; }
        public string DEPTNAME { get; set; }
        public string DOCTCODE { get; set; }
        public string DOCTNAME { get; set; }
        public int MEDAMPM { get; set; }
        public int REGTYPE { get; set; }
        public string REGTYPENAME { get; set; }
        public decimal REGFEE { get; set; }
        public decimal TREATFEE { get; set; }
        public decimal REGAMOUNT { get; set; }
        public DateTime MEDDATE { get; set; }
        public string MEDTIME { get; set; }
        public string ADDRESS { get; set; }
        public int APPONO { get; set; }
        public string IDNO { get; set; }
    }

    public class CFYJ
    {
        public string YPFY { get; set; }
        public string YPMC { get; set; }
        public string CFSB { get; set; }
        public string CFTS { get; set; }
        public string YPSL { get; set; }
        public decimal YPDJ { get; set; }
        public string YFGG { get; set; }
        public string YFDW { get; set; }
        public DateTime KFRQ { get; set; }
        public decimal HJJE { get; set; }
        public long BRID { get; set; }

    }

    public class KHJL
    {
        public string JLXH { get; set; }
        public string SFZH { get; set; }
        public string XNZH { get; set; }
        public string YHLSH { get; set; }
        public string YHKBDBZ { get; set; }
        public string KHBZ { get; set; }
        public string YHKH { get; set; }
        public string BRXM { get; set; }
    }

    #endregion Tables
}