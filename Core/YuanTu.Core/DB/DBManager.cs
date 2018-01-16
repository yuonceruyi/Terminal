using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Core.Log;

namespace YuanTu.Core.DB
{
    public class DBManager
    {
        protected static Dictionary<string, SQLiteConnection> dbMapping;

        private static readonly List<Type>HasAuthTypes=new List<Type>(); 
        private static SQLiteConnection GetConnection(string path)
        {
            var totalPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
            if (dbMapping == null)
            {
                dbMapping = new Dictionary<string, SQLiteConnection>();
            }
            if (!dbMapping.ContainsKey(path))
            {
                var dir = Path.GetDirectoryName(totalPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                dbMapping[path] = new SQLiteConnection(totalPath);
            }

            return dbMapping[path];
        }

        private static void SureTable<T>(string dbpath)
        {
            var arr = new[] {typeof(int)};
            if (arr.Contains(typeof(T)))
            {
                return;
            }
            var tp = typeof(T);

            if (!HasAuthTypes.Contains(tp))
            {
                var conn = GetConnection(dbpath);
                conn.CreateTable<T>();
                HasAuthTypes.Add(tp);

            }
        }
        public static void Insert<T>(string dbpath,T data)
        {
            var conn = GetConnection(dbpath);
            SureTable<T>(dbpath);
            conn.Insert(data);
        }

        public static void Insert<T>(T data)
        {
            Insert(FrameworkConst.DatabasePath,data);
        }

        public static void InsertAll<T>(string dbpath, IEnumerable<T> collection)
        {
            var conn = GetConnection(dbpath);
            SureTable<T>(dbpath);
            conn.InsertAll(collection);
        }

        public static List<T> Query<T>(string query, params object[] args) where T : new()
        {
            return Query<T>(FrameworkConst.DatabasePath, query, args);
        }
        public static List<T> Query<T>(string path,string query, params object[] args) where T : new()
        {
            var conn = GetConnection(path);
            SureTable<T>(path);
            return conn.Query<T>(query, args);
        }
        public static int Excute(string path, string query, params object[] args)
        {
            var conn = GetConnection(path);
            return conn.Execute(query, args);
        }
        public static int Excute( string query, params object[] args)
        {
            return Excute(FrameworkConst.DatabasePath, query, args);
        }
    }

    public abstract class Table
    {
        [PrimaryKey, AutoIncrement, ColumnOrder(0)]
        public int Id { get; set; }

        [Indexed, ColumnOrder(1)]
        public DateTime DateTime { get;set; } = DateTimeCore.Now;

        [Indexed, ColumnOrder(2)]
        public DateTime LocalDateTime { get; set; } = DateTime.Now;
    }

    public class PointTouchTable : Table
    {
        public string ViewName { get; set; }
        public string ButtonContent { get; set; }
        public string CardNo { get; set; }
        public string PatientId { get; set; }
        public double PointX { get; set; }
        public double PointY { get; set; }
    }

    /// <summary>
    /// 充值记录
    /// </summary>
    public class RechargeInfo : Table
    {
        public PayMethod RechargeMethod { get; set; }
        public decimal TotalMoney { get; set; }
        public string PatientId { get; set; }
        public string CardNo { get; set; }
        public CardType CardType { get; set; }
        public bool Success { get; set; }
        public string ErrorMsg { get; set; }
    }
    /// <summary>
    /// 缴住院押金记录
    /// </summary>
    public class ZYRechargeInfo : Table
    {
        public PayMethod RechargeMethod { get; set; }
        public decimal TotalMoney { get; set; }
        public string PatientId { get; set; }
        public bool Success { get; set; }
        public string ErrorMsg { get; set; }
    }
    public class CashInputInfo : Table
    {
        public decimal TotalSeconds { get; set; }
    }
    /// <summary>
    /// 清钞记录
    /// </summary>
    public class CashClearInfo : Table
    {
        public int CashInputInfoId { get; set; }
        public decimal CurrentCount { get; set; }
    }

    /// <summary>
    /// 交易信息，正交易 负交易
    /// </summary>
    public class TradeInfo : Table
    {
        /// <summary>
        /// 交易名称
        /// </summary>
        public string TradeName { get; set; }
        /// <summary>
        /// 病人唯一Id
        /// </summary>
        public string PatientId { get; set; }
        /// <summary>
        /// 病人姓名
        /// </summary>
        public string PatientName { get; set; }
        /// <summary>
        /// 操作卡号
        /// </summary>
        public string CardNo { get; set; }
        /// <summary>
        /// 卡类型
        /// </summary>
        public CardType CardType { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public string IdNo { get; set; }
        /// <summary>
        /// 监护人身份证号
        /// </summary>
        public string GuardianIdNo { get; set; }
        /// <summary>
        /// 交易方式
        /// </summary>
        public PayMethod PayMethod { get; set; }
        /// <summary>
        /// 交易类型
        /// </summary>
        public TradeType TradeType { get; set; }

        /// <summary>
        /// 交易唯一Id
        /// </summary>
        public string TradeId { get; set; }
        /// <summary>
        /// 原始Id（反交易，则是对应正交易的Id)
        /// </summary>
        public string OriginTradeId { get; set; }

        /// <summary>
        /// 交易账号
        /// </summary>
        public string AccountNo { get; set; }

        /// <summary>
        /// 总金额（分）
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// 交易详情
        /// </summary>
        public string TradeDetail { get; set; }

        /// <summary>
        /// 是否已经上传到平台
        /// </summary>
        public bool HaveUpload { get; set; }

    }
}
