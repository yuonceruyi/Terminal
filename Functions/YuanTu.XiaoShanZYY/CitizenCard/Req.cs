using System;
using System.Collections.Generic;
using System.Text;
using YuanTu.Consts.FrameworkBase;
using YuanTu.XiaoShanZYY.CitizenCard;

namespace YuanTu.XiaoShanZYY.CitizenCard
{
    public class Req初始化 : IReqBase
    {
        public string serviceName { get; set; } = "初始化";
        public int transCode { get; set; } = 1000;
        public decimal amount { get; set; }

        public string Serilize()
        {
            return $"";
        }
    }
    public class Req签到 : IReqBase
    {
        public string serviceName { get; set; } = "签到";
        public int transCode { get; set; } = 6215;
        public decimal amount { get; set; }

        public string Serilize()
        {
            return $"";
        }
    }
    public class Req签退 : IReqBase
    {
        public string serviceName { get; set; } = "签退";
        public int transCode { get; set; } = 6225;
        public decimal amount { get; set; }

        public string Serilize()
        {
            return $"";
        }
    }
    public class Req检验卡是否插入 : IReqBase
    {
        public string serviceName { get; set; } = "检验卡是否插入";
        public int transCode { get; set; } = 1010;
        public decimal amount { get; set; }

        public string Serilize()
        {
            return $"";
        }
    }
    public class Req读非接触卡号 : IReqBase
    {
        public string serviceName { get; set; } = "读非接触卡号";
        public int transCode { get; set; } = 1001;
        public decimal amount { get; set; }

        public string Serilize()
        {
            return $"";
        }
    }
    public class Req读接触卡号 : IReqBase
    {
        public string serviceName { get; set; } = "读接触卡号";
        public int transCode { get; set; } = 1003;
        public decimal amount { get; set; }

        public string Serilize()
        {
            return $"";
        }
    }
    public class Req读接触非接卡号 : IReqBase
    {
        public string serviceName { get; set; } = "读接触非接卡号";
        public int transCode { get; set; } = 1004;
        public decimal amount { get; set; }

        public string Serilize()
        {
            return $"";
        }
    }
    public class Req读证卡卡号 : IReqBase
    {
        public string serviceName { get; set; } = "读证卡卡号";
        public int transCode { get; set; } = 1005;
        public decimal amount { get; set; }

        public string Serilize()
        {
            return $"";
        }
    }
    public class Req可扣查询_JKK : IReqBase
    {
        public string serviceName { get; set; } = "可扣查询_JKK";
        public int transCode { get; set; } = 81025;
        public decimal amount { get; set; }
        public string 卡类型 { get; set; }
        public string 卡号 { get; set; }

        public string Serilize()
        {
            return $"{卡类型}{卡号}";
        }
    }
    public class Req可扣查询_SMK : IReqBase
    {
        public string serviceName { get; set; } = "可扣查询_SMK";
        public int transCode { get; set; } = 81025;
        public decimal amount { get; set; }
        public string 卡类型 { get; set; }
        public string 卡号 { get; set; }

        public string Serilize()
        {
            return $"{卡类型}{卡号}";
        }
    }
    public class Req健康卡信息查询 : IReqBase
    {
        public string serviceName { get; set; } = "健康卡信息查询";
        public int transCode { get; set; } = 7020;
        public decimal amount { get; set; }
        public string 标志位 { get; set; }
        public string 后面信息长度 { get; set; }
        public string 卡号 { get; set; }

        public string Serilize()
        {
            return $"{标志位}{后面信息长度}{卡号}";
        }
    }
    public class Req市民卡账户开通 : IReqBase
    {
        public string serviceName { get; set; } = "市民卡账户开通";
        public int transCode { get; set; } = 57005;
        public decimal amount { get; set; }
        public string 标志位 { get; set; }
        public string 非接卡号 { get; set; }
        public string 手机号 { get; set; }
        public string 操作员号 { get; set; }
        public string 发卡标志 { get; set; }
        public string 姓名 { get; set; }
        public string 身份证号 { get; set; }
        public string 代理人姓名_20 { get; set; }
        public string 代理人身份证号码_18 { get; set; }
        public string 家庭住址_60 { get; set; }

        public string Serilize()
        {
            return $"{标志位}{非接卡号}{手机号}{操作员号}{发卡标志}{姓名}{身份证号}{代理人姓名_20}{代理人身份证号码_18}{家庭住址_60}";
        }
    }
    public class Req智慧医疗开通 : IReqBase
    {
        public string serviceName { get; set; } = "智慧医疗开通";
        public int transCode { get; set; } = 57005;
        public decimal amount { get; set; }
        public string 标志位 { get; set; }
        public string 手机号 { get; set; }
        public string 账户开通 { get; set; }
        public string 网上支付 { get; set; }
        public string 短信提醒 { get; set; }
        public string 智慧医院 { get; set; }
        public string 身份证号 { get; set; }
        public string 交易密码 { get; set; }

        public string Serilize()
        {
            return $"{标志位}{手机号}{账户开通}{网上支付}{短信提醒}{智慧医院}{身份证号}{交易密码}";
        }
    }
    public class Req账户医疗开通 : IReqBase
    {
        public string serviceName { get; set; } = "账户医疗开通";
        public int transCode { get; set; } = 57005;
        public decimal amount { get; set; }
        public string 标志位 { get; set; }
        public string 手机号 { get; set; }
        public string 账户开通 { get; set; }
        public string 网上支付 { get; set; }
        public string 短信提醒 { get; set; }
        public string 智慧医院 { get; set; }
        public string 身份证号 { get; set; }

        public string Serilize()
        {
            return $"{标志位}{手机号}{账户开通}{网上支付}{短信提醒}{智慧医院}{身份证号}";
        }
    }
    public class Req儿童医疗开通 : IReqBase
    {
        public string serviceName { get; set; } = "儿童医疗开通";
        public int transCode { get; set; } = 57005;
        public decimal amount { get; set; }
        public string 标志位 { get; set; }
        public string 手机号 { get; set; }
        public string 身份证号 { get; set; }
        public string 外卡号 { get; set; }
        public string 芯片号 { get; set; }

        public string Serilize()
        {
            return $"{标志位}{手机号}{身份证号}{外卡号}{芯片号}";
        }
    }
    public class Req市民卡账户充值 : IReqBase
    {
        public string serviceName { get; set; } = "市民卡账户充值";
        public int transCode { get; set; } = 7010;
        public decimal amount { get; set; }
        public string 渠道 { get; set; }
        public string 银行卡号 { get; set; }
        public string 银行卡流水 { get; set; }
        public string 卡类型 { get; set; }
        public string 卡号 { get; set; }

        public string Serilize()
        {
            return $"{渠道}{银行卡号}{银行卡流水}{卡类型}{卡号}";
        }
    }
    public class Req消费 : IReqBase
    {
        public string serviceName { get; set; } = "消费";
        public int transCode { get; set; } = 81105;
        public decimal amount { get; set; }
        public string 社保卡芯片号_32 { get; set; }

        public string Serilize()
        {
            return $"{社保卡芯片号_32}";
        }
    }
    public class Req密码回显 : IReqBase
    {
        public string serviceName { get; set; } = "密码回显";
        public int transCode { get; set; } = 9904;
        public decimal amount { get; set; }
        public string 卡号 { get; set; }

        public string Serilize()
        {
            return $"{卡号}";
        }
    }
    public class Req读十进制卡号 : IReqBase
    {
        public string serviceName { get; set; } = "读十进制卡号";
        public int transCode { get; set; } = 9901;
        public decimal amount { get; set; }

        public string Serilize()
        {
            return $"";
        }
    }
    public class Req获取密码分次 : IReqBase
    {
        public string serviceName { get; set; } = "获取密码分次";
        public int transCode { get; set; } = 9902;
        public decimal amount { get; set; }

        public string Serilize()
        {
            return $"";
        }
    }
    public class Req获取密码分次十六进制 : IReqBase
    {
        public string serviceName { get; set; } = "获取密码分次十六进制";
        public int transCode { get; set; } = 9903;
        public decimal amount { get; set; }

        public string Serilize()
        {
            return $"";
        }
    }
    public class Req密码修改或重置 : IReqBase
    {
        public string serviceName { get; set; } = "密码修改或重置";
        public int transCode { get; set; } = 1325;
        public decimal amount { get; set; }
        public string 修改类型 { get; set; }
        public string 修改对象 { get; set; }
        public string 姓名 { get; set; }
        public string 身份证号码 { get; set; }

        public string Serilize()
        {
            return $"{修改类型}{修改对象}{姓名}{身份证号码}";
        }
    }
    public class Req密码修改或重置分次 : IReqBase
    {
        public string serviceName { get; set; } = "密码修改或重置分次";
        public int transCode { get; set; } = 81325;
        public decimal amount { get; set; }
        public string 修改类型 { get; set; }
        public string 修改对象 { get; set; }
        public string 姓名 { get; set; }
        public string 身份证号码 { get; set; }
        public string 原密码 { get; set; }
        public string 新密码 { get; set; }

        public string Serilize()
        {
            return $"{修改类型}{修改对象}{姓名}{身份证号码}{原密码}{新密码}";
        }
    }
    public class Req密码修改或重置分次十六进制 : IReqBase
    {
        public string serviceName { get; set; } = "密码修改或重置分次十六进制";
        public int transCode { get; set; } = 91325;
        public decimal amount { get; set; }
        public string 修改类型 { get; set; }
        public string 修改对象 { get; set; }
        public string 姓名 { get; set; }
        public string 身份证号码 { get; set; }
        public string 原密码 { get; set; }
        public string 新密码 { get; set; }

        public string Serilize()
        {
            return $"{修改类型}{修改对象}{姓名}{身份证号码}{原密码}{新密码}";
        }
    }
    public class Req余额查询密码外入版 : IReqBase
    {
        public string serviceName { get; set; } = "余额查询密码外入版";
        public int transCode { get; set; } = 91025;
        public decimal amount { get; set; }
        public string 密码 { get; set; }

        public string Serilize()
        {
            return $"{密码}";
        }
    }
}

namespace YuanTu.XiaoShanZYY
{
    public partial class DataHandler
    {
        public static Result<Res初始化> 初始化(Req初始化 req)
        {
            return MisposHelper.Query<Res初始化, Req初始化>(req);
        }
        public static Result<Res签到> 签到(Req签到 req)
        {
            return MisposHelper.Query<Res签到, Req签到>(req);
        }
        public static Result<Res签退> 签退(Req签退 req)
        {
            return MisposHelper.Query<Res签退, Req签退>(req);
        }
        public static Result<Res检验卡是否插入> 检验卡是否插入(Req检验卡是否插入 req)
        {
            return MisposHelper.Query<Res检验卡是否插入, Req检验卡是否插入>(req);
        }
        public static Result<Res读非接触卡号> 读非接触卡号(Req读非接触卡号 req)
        {
            return MisposHelper.Query<Res读非接触卡号, Req读非接触卡号>(req);
        }
        public static Result<Res读接触卡号> 读接触卡号(Req读接触卡号 req)
        {
            return MisposHelper.Query<Res读接触卡号, Req读接触卡号>(req);
        }
        public static Result<Res读接触非接卡号> 读接触非接卡号(Req读接触非接卡号 req)
        {
            return MisposHelper.Query<Res读接触非接卡号, Req读接触非接卡号>(req);
        }
        public static Result<Res读证卡卡号> 读证卡卡号(Req读证卡卡号 req)
        {
            return MisposHelper.Query<Res读证卡卡号, Req读证卡卡号>(req);
        }
        public static Result<Res可扣查询_JKK> 可扣查询_JKK(Req可扣查询_JKK req)
        {
            return MisposHelper.Query<Res可扣查询_JKK, Req可扣查询_JKK>(req);
        }
        public static Result<Res可扣查询_SMK> 可扣查询_SMK(Req可扣查询_SMK req)
        {
            return MisposHelper.Query<Res可扣查询_SMK, Req可扣查询_SMK>(req);
        }
        public static Result<Res健康卡信息查询> 健康卡信息查询(Req健康卡信息查询 req)
        {
            return MisposHelper.Query<Res健康卡信息查询, Req健康卡信息查询>(req);
        }
        public static Result<Res市民卡账户开通> 市民卡账户开通(Req市民卡账户开通 req)
        {
            return MisposHelper.Query<Res市民卡账户开通, Req市民卡账户开通>(req);
        }
        public static Result<Res智慧医疗开通> 智慧医疗开通(Req智慧医疗开通 req)
        {
            return MisposHelper.Query<Res智慧医疗开通, Req智慧医疗开通>(req);
        }
        public static Result<Res账户医疗开通> 账户医疗开通(Req账户医疗开通 req)
        {
            return MisposHelper.Query<Res账户医疗开通, Req账户医疗开通>(req);
        }
        public static Result<Res儿童医疗开通> 儿童医疗开通(Req儿童医疗开通 req)
        {
            return MisposHelper.Query<Res儿童医疗开通, Req儿童医疗开通>(req);
        }
        public static Result<Res市民卡账户充值> 市民卡账户充值(Req市民卡账户充值 req)
        {
            return MisposHelper.Query<Res市民卡账户充值, Req市民卡账户充值>(req);
        }
        public static Result<Res消费> 消费(Req消费 req)
        {
            return MisposHelper.Query<Res消费, Req消费>(req);
        }
        public static Result<Res密码回显> 密码回显(Req密码回显 req)
        {
            return MisposHelper.Query<Res密码回显, Req密码回显>(req);
        }
        public static Result<Res读十进制卡号> 读十进制卡号(Req读十进制卡号 req)
        {
            return MisposHelper.Query<Res读十进制卡号, Req读十进制卡号>(req);
        }
        public static Result<Res获取密码分次> 获取密码分次(Req获取密码分次 req)
        {
            return MisposHelper.Query<Res获取密码分次, Req获取密码分次>(req);
        }
        public static Result<Res获取密码分次十六进制> 获取密码分次十六进制(Req获取密码分次十六进制 req)
        {
            return MisposHelper.Query<Res获取密码分次十六进制, Req获取密码分次十六进制>(req);
        }
        public static Result<Res密码修改或重置> 密码修改或重置(Req密码修改或重置 req)
        {
            return MisposHelper.Query<Res密码修改或重置, Req密码修改或重置>(req);
        }
        public static Result<Res密码修改或重置分次> 密码修改或重置分次(Req密码修改或重置分次 req)
        {
            return MisposHelper.Query<Res密码修改或重置分次, Req密码修改或重置分次>(req);
        }
        public static Result<Res密码修改或重置分次十六进制> 密码修改或重置分次十六进制(Req密码修改或重置分次十六进制 req)
        {
            return MisposHelper.Query<Res密码修改或重置分次十六进制, Req密码修改或重置分次十六进制>(req);
        }
        public static Result<Res余额查询密码外入版> 余额查询密码外入版(Req余额查询密码外入版 req)
        {
            return MisposHelper.Query<Res余额查询密码外入版, Req余额查询密码外入版>(req);
        }
    }
}