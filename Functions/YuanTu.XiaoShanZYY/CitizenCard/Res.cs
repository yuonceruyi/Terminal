using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace YuanTu.XiaoShanZYY.CitizenCard
{
  
    public class Res初始化 : IResBase
    {
        public string Service => "初始化";

        public string 交易码 { get; set; } = "1000";

        public string Serilize()
        {
            return string.Join(" ", new string[]{
            });
        }
        
        public bool Parse(string text)
        {
            var list = text.Split(' ');
            return true;
        }
    }
  
    public class Res检验卡是否插入 : IResBase
    {
        public string Service => "检验卡是否插入";

        public string 交易码 { get; set; } = "1010";

        public string Serilize()
        {
            return string.Join(" ", new string[]{
            });
        }
        
        public bool Parse(string text)
        {
            var list = text.Split(' ');
            return true;
        }
    }
  
    public class Res读接触卡号 : IResBase
    {
        public string Service => "读接触卡号";

        public string 交易码 { get; set; } = "1003";
        public string 卡号识别码 { get; set; }
        public string 卡类别 { get; set; }
        public string 卡号 { get; set; }
        public string 身份证号 { get; set; }
        public string 姓名 { get; set; }
        public string 性别 { get; set; }

        public string Serilize()
        {
            return string.Join("|", new string[]{
                卡号识别码,
                卡类别,
                卡号,
                身份证号,
                姓名,
                性别,
            });
        }
        
        public bool Parse(string text)
        {
            var list = text.Split('|');
            卡号识别码 = list[0];
            卡类别 = list[1];
            卡号 = list[2];
            身份证号 = list[3];
            姓名 = list[4];
            性别 = list[5];
            return true;
        }
    }
  
    public class Res读接触非接卡号 : IResBase
    {
        public string Service => "读接触非接卡号";

        public string 交易码 { get; set; } = "1004";
        public string 卡识别码 { get; set; }
        public string 卡类型 { get; set; }
        public string 卡号 { get; set; }
        public string 身份证号 { get; set; }
        public string 姓名 { get; set; }
        public string 性别 { get; set; }
        public string 保留1 { get; set; }
        public string 保留2 { get; set; }
        public string 保留3 { get; set; }
        public string 保留4 { get; set; }
        public string PSAM卡终端机编号 { get; set; }

        public string Serilize()
        {
            return string.Join("|", new string[]{
                卡识别码,
                卡类型,
                卡号,
                身份证号,
                姓名,
                性别,
                保留1,
                保留2,
                保留3,
                保留4,
                PSAM卡终端机编号,
            });
        }
        
        public bool Parse(string text)
        {
            var list = text.Split('|');
            卡识别码 = list[0];
            卡类型 = list[1];
            卡号 = list[2];
            身份证号 = list[3];
            姓名 = list[4];
            性别 = list[5];
            保留1 = list[6];
            保留2 = list[7];
            保留3 = list[8];
            保留4 = list[9];
            PSAM卡终端机编号 = list[10];
            return true;
        }
    }
  
    public class Res读证卡卡号 : IResBase
    {
        public string Service => "读证卡卡号";

        public string 交易码 { get; set; } = "1005";
        public string 卡识别码 { get; set; }
        public string 卡类型 { get; set; }
        public string 卡号 { get; set; }
        public string 身份证号 { get; set; }
        public string 姓名 { get; set; }
        public string 性别 { get; set; }
        public string 保留1 { get; set; }
        public string 保留2 { get; set; }
        public string 保留3 { get; set; }
        public string 保留4 { get; set; }
        public string PSAM卡终端机编号 { get; set; }

        public string Serilize()
        {
            return string.Join("|", new string[]{
                卡识别码,
                卡类型,
                卡号,
                身份证号,
                姓名,
                性别,
                保留1,
                保留2,
                保留3,
                保留4,
                PSAM卡终端机编号,
            });
        }
        
        public bool Parse(string text)
        {
            var list = text.Split('|');
            卡识别码 = list[0];
            卡类型 = list[1];
            卡号 = list[2];
            身份证号 = list[3];
            姓名 = list[4];
            性别 = list[5];
            保留1 = list[6];
            保留2 = list[7];
            保留3 = list[8];
            保留4 = list[9];
            PSAM卡终端机编号 = list[10];
            return true;
        }
    }
  
    public class Res获取密码分次 : IResBase
    {
        public string Service => "获取密码分次";

        public string 交易码 { get; set; } = "9902";
        public string 密码 { get; set; }

        public string Serilize()
        {
            return string.Join(" ", new string[]{
                密码,
            });
        }
        
        public bool Parse(string text)
        {
            var list = text.Split(' ');
            密码 = list[0];
            return true;
        }
    }
  
    public class Res获取密码分次十六进制 : IResBase
    {
        public string Service => "获取密码分次十六进制";

        public string 交易码 { get; set; } = "9903";
        public string 密码 { get; set; }

        public string Serilize()
        {
            return string.Join(" ", new string[]{
                密码,
            });
        }
        
        public bool Parse(string text)
        {
            var list = text.Split(' ');
            密码 = list[0];
            return true;
        }
    }
  
    public class Res读十进制卡号 : IResBase
    {
        public string Service => "读十进制卡号";

        public string 交易码 { get; set; } = "9901";
        public string 卡号 { get; set; }

        public string Serilize()
        {
            return string.Join(" ", new string[]{
                卡号,
            });
        }
        
        public bool Parse(string text)
        {
            var list = text.Split(' ');
            卡号 = list[0];
            return true;
        }
    }
  
    public class Res读非接触卡号 : IResBase
    {
        public string Service => "读非接触卡号";

        public string 交易码 { get; set; } = "1001";
        public string 卡号 { get; set; }

        public string Serilize()
        {
            return string.Join(" ", new string[]{
                卡号,
            });
        }
        
        public bool Parse(string text)
        {
            var list = text.Split(' ');
            卡号 = list[0];
            return true;
        }
    }

  
    public class Res签到 : IResBase
    {
        public string Service => "签到";

        public string 交易码 { get; set; } = "6215";
        public string 应答码 { get; set; }

        public string Serilize()
        {
            return String.Join(" ", new string[]{
            });
        }

        public bool Parse(string text)
        {
            if (!text.Contains(" "))
            {
                应答码 = text;
                return true;
            }
            var list = text.Split(' ');
            应答码 = list[0];
            return true;
        }
    }
  
    public class Res签退 : IResBase
    {
        public string Service => "签退";

        public string 交易码 { get; set; } = "6225";
        public string 应答码 { get; set; }

        public string Serilize()
        {
            return String.Join(" ", new string[]{
            });
        }

        public bool Parse(string text)
        {
            if (!text.Contains(" "))
            {
                应答码 = text;
                return true;
            }
            var list = text.Split(' ');
            应答码 = list[0];
            return true;
        }
    }
  
    public class Res健康卡信息查询 : IResBase
    {
        public string Service => "健康卡信息查询";

        public string 交易码 { get; set; } = "7020";
        public string 应答码 { get; set; }
        public string 返回金额 { get; set; }
        public string 卡面号 { get; set; }
        public string 智慧医疗开通 { get; set; }
        public string 姓名 { get; set; }
        public string 身份证 { get; set; }
        public string 手机号码 { get; set; }
        public string 地址 { get; set; }

        public string Serilize()
        {
            return String.Join("#", new string[]{
                   返回金额,
                   卡面号,
                   智慧医疗开通,
                   姓名,
                   身份证,
                   手机号码,
                   地址,
            });
        }

        public bool Parse(string text)
        {
            if (!text.Contains("#"))
            {
                应答码 = text;
                return true;
            }
            var list = text.Split('#');
            应答码 = list[0];
            返回金额 = list[1];
            卡面号 = list[2];
            智慧医疗开通 = list[3];
            姓名 = list[4];
            身份证 = list[5];
            手机号码 = list[6];
            地址 = list[7];
            return true;
        }
    }
  
    public class Res可扣查询_JKK : IResBase
    {
        public string Service => "可扣查询_JKK";

        public string 交易码 { get; set; } = "81025";
        public string 应答码 { get; set; }
        public string 返回金额 { get; set; }
        public string 卡面号 { get; set; }
        public string 智慧医疗开通 { get; set; }
        public string 姓名 { get; set; }
        public string 身份证 { get; set; }
        public string 手机号码 { get; set; }
        public string 地址 { get; set; }

        public string Serilize()
        {
            return String.Join("#", new string[]{
                   返回金额,
                   卡面号,
                   智慧医疗开通,
                   姓名,
                   身份证,
                   手机号码,
                   地址,
            });
        }

        public bool Parse(string text)
        {
            if (!text.Contains("#"))
            {
                应答码 = text;
                return true;
            }
            var list = text.Split('#');
            应答码 = list[0];
            返回金额 = list[1];
            卡面号 = list[2];
            智慧医疗开通 = list[3];
            姓名 = list[4];
            身份证 = list[5];
            手机号码 = list[6];
            地址 = list[7];
            return true;
        }
    }
  
    public class Res可扣查询_SMK : IResBase
    {
        public string Service => "可扣查询_SMK";

        public string 交易码 { get; set; } = "81025";
        public string 应答码 { get; set; }
        public string 返回金额 { get; set; }
        public string 卡面号 { get; set; }
        public string 智慧医疗开通 { get; set; }

        public string Serilize()
        {
            return String.Join("#", new string[]{
                   返回金额,
                   卡面号,
                   智慧医疗开通,
            });
        }

        public bool Parse(string text)
        {
            if (!text.Contains("#"))
            {
                应答码 = text;
                return true;
            }
            var list = text.Split('#');
            应答码 = list[0];
            返回金额 = list[1];
            卡面号 = list[2];
            智慧医疗开通 = list[3];
            return true;
        }
    }
  
    public class Res市民卡账户开通 : IResBase
    {
        public string Service => "市民卡账户开通";

        public string 交易码 { get; set; } = "57005";
        public string 应答码 { get; set; }
        public string 商户编号 { get; set; }
        public string 终端编号 { get; set; }
        public string 账户类型 { get; set; }
        public string 交易类型 { get; set; }
        public string 市民卡卡面号 { get; set; }
        public string 账户类型2 { get; set; }
        public string 交易日期 { get; set; }
        public string 交易时间 { get; set; }
        public string 交易参考号 { get; set; }
        public string 批次号 { get; set; }

        public string Serilize()
        {
            return String.Join("#", new string[]{
                   商户编号,
                   终端编号,
                   账户类型,
                   交易类型,
                   市民卡卡面号,
                   账户类型2,
                   交易日期,
                   交易时间,
                   交易参考号,
                   批次号,
            });
        }

        public bool Parse(string text)
        {
            if (!text.Contains("#"))
            {
                应答码 = text;
                return true;
            }
            var list = text.Split('#');
            应答码 = list[0];
            商户编号 = list[1];
            终端编号 = list[2];
            账户类型 = list[3];
            交易类型 = list[4];
            市民卡卡面号 = list[5];
            账户类型2 = list[6];
            交易日期 = list[7];
            交易时间 = list[8];
            交易参考号 = list[9];
            批次号 = list[10];
            return true;
        }
    }
  
    public class Res账户医疗开通 : IResBase
    {
        public string Service => "账户医疗开通";

        public string 交易码 { get; set; } = "57005";
        public string 应答码 { get; set; }
        public string 商户编号 { get; set; }
        public string 终端编号 { get; set; }
        public string 账户类型 { get; set; }
        public string 交易类型 { get; set; }
        public string 市民卡卡面号 { get; set; }
        public string 账户类型2 { get; set; }
        public string 交易日期 { get; set; }
        public string 交易时间 { get; set; }
        public string 交易参考号 { get; set; }
        public string 批次号 { get; set; }

        public string Serilize()
        {
            return String.Join("#", new string[]{
                   商户编号,
                   终端编号,
                   账户类型,
                   交易类型,
                   市民卡卡面号,
                   账户类型2,
                   交易日期,
                   交易时间,
                   交易参考号,
                   批次号,
            });
        }

        public bool Parse(string text)
        {
            if (!text.Contains("#"))
            {
                应答码 = text;
                return true;
            }
            var list = text.Split('#');
            应答码 = list[0];
            商户编号 = list[1];
            终端编号 = list[2];
            账户类型 = list[3];
            交易类型 = list[4];
            市民卡卡面号 = list[5];
            账户类型2 = list[6];
            交易日期 = list[7];
            交易时间 = list[8];
            交易参考号 = list[9];
            批次号 = list[10];
            return true;
        }
    }
  
    public class Res智慧医疗开通 : IResBase
    {
        public string Service => "智慧医疗开通";

        public string 交易码 { get; set; } = "57005";
        public string 应答码 { get; set; }
        public string 商户编号 { get; set; }
        public string 终端编号 { get; set; }
        public string 账户类型 { get; set; }
        public string 交易类型 { get; set; }
        public string 市民卡卡面号 { get; set; }
        public string 账户类型2 { get; set; }
        public string 交易日期 { get; set; }
        public string 交易时间 { get; set; }
        public string 交易参考号 { get; set; }
        public string 批次号 { get; set; }

        public string Serilize()
        {
            return String.Join("#", new string[]{
                   商户编号,
                   终端编号,
                   账户类型,
                   交易类型,
                   市民卡卡面号,
                   账户类型2,
                   交易日期,
                   交易时间,
                   交易参考号,
                   批次号,
            });
        }

        public bool Parse(string text)
        {
            if (!text.Contains("#"))
            {
                应答码 = text;
                return true;
            }
            var list = text.Split('#');
            应答码 = list[0];
            商户编号 = list[1];
            终端编号 = list[2];
            账户类型 = list[3];
            交易类型 = list[4];
            市民卡卡面号 = list[5];
            账户类型2 = list[6];
            交易日期 = list[7];
            交易时间 = list[8];
            交易参考号 = list[9];
            批次号 = list[10];
            return true;
        }
    }
  
    public class Res儿童医疗开通 : IResBase
    {
        public string Service => "儿童医疗开通";

        public string 交易码 { get; set; } = "57005";
        public string 应答码 { get; set; }
        public string 商户编号 { get; set; }
        public string 终端编号 { get; set; }
        public string 账户类型 { get; set; }
        public string 交易类型 { get; set; }
        public string 市民卡卡面号 { get; set; }
        public string 账户类型2 { get; set; }
        public string 交易日期 { get; set; }
        public string 交易时间 { get; set; }
        public string 交易参考号 { get; set; }
        public string 批次号 { get; set; }

        public string Serilize()
        {
            return String.Join("#", new string[]{
                   商户编号,
                   终端编号,
                   账户类型,
                   交易类型,
                   市民卡卡面号,
                   账户类型2,
                   交易日期,
                   交易时间,
                   交易参考号,
                   批次号,
            });
        }

        public bool Parse(string text)
        {
            if (!text.Contains("#"))
            {
                应答码 = text;
                return true;
            }
            var list = text.Split('#');
            应答码 = list[0];
            商户编号 = list[1];
            终端编号 = list[2];
            账户类型 = list[3];
            交易类型 = list[4];
            市民卡卡面号 = list[5];
            账户类型2 = list[6];
            交易日期 = list[7];
            交易时间 = list[8];
            交易参考号 = list[9];
            批次号 = list[10];
            return true;
        }
    }
  
    public class Res市民卡账户充值 : IResBase
    {
        public string Service => "市民卡账户充值";

        public string 交易码 { get; set; } = "7010";
        public string 应答码 { get; set; }
        public string 商户编号 { get; set; }
        public string 终端编号 { get; set; }
        public string 账户类型 { get; set; }
        public string 交易类型 { get; set; }
        public string 物理卡号 { get; set; }
        public string 交易日期 { get; set; }
        public string 交易时间 { get; set; }
        public string 交易参考号 { get; set; }
        public string 批次号 { get; set; }
        public string 凭证号 { get; set; }
        public string 金额 { get; set; }
        public string 卡面号凹码 { get; set; }
        public string 卡号 { get; set; }
        public string 账户余额 { get; set; }
        public string 小票余额打印限额 { get; set; }

        public string Serilize()
        {
            return String.Join("#", new string[]{
                   商户编号,
                   终端编号,
                   账户类型,
                   交易类型,
                   物理卡号,
                   交易日期,
                   交易时间,
                   交易参考号,
                   批次号,
                   凭证号,
                   金额,
                   卡面号凹码,
                   卡号,
                   账户余额,
                   小票余额打印限额,
            });
        }

        public bool Parse(string text)
        {
            if (!text.Contains("#"))
            {
                应答码 = text;
                return true;
            }
            var list = text.Split('#');
            应答码 = list[0];
            商户编号 = list[1];
            终端编号 = list[2];
            账户类型 = list[3];
            交易类型 = list[4];
            物理卡号 = list[5];
            交易日期 = list[6];
            交易时间 = list[7];
            交易参考号 = list[8];
            批次号 = list[9];
            凭证号 = list[10];
            金额 = list[11];
            卡面号凹码 = list[12];
            卡号 = list[13];
            账户余额 = list[14];
            小票余额打印限额 = list[15];
            return true;
        }
    }
  
    public class Res消费 : IResBase
    {
        public string Service => "消费";

        public string 交易码 { get; set; } = "81105";
        public string 应答码 { get; set; }
        public string 商户编号 { get; set; }
        public string 终端编号 { get; set; }
        public string 账户类型 { get; set; }
        public string 交易类型 { get; set; }
        public string 物理卡号 { get; set; }
        public string 交易日期 { get; set; }
        public string 交易时间 { get; set; }
        public string 交易参考号 { get; set; }
        public string 批次号 { get; set; }
        public string 凭证号 { get; set; }
        public string 金额 { get; set; }
        public string 卡面凹码 { get; set; }
        public string 卡号 { get; set; }
        public string 账户余额 { get; set; }
        public string 小票余额打印限额 { get; set; }

        public string Serilize()
        {
            return String.Join("#", new string[]{
                   商户编号,
                   终端编号,
                   账户类型,
                   交易类型,
                   物理卡号,
                   交易日期,
                   交易时间,
                   交易参考号,
                   批次号,
                   凭证号,
                   金额,
                   卡面凹码,
                   卡号,
                   账户余额,
                   小票余额打印限额,
            });
        }

        public bool Parse(string text)
        {
            if (!text.Contains("#"))
            {
                应答码 = text;
                return true;
            }
            var list = text.Split('#');
            应答码 = list[0];
            商户编号 = list[1];
            终端编号 = list[2];
            账户类型 = list[3];
            交易类型 = list[4];
            物理卡号 = list[5];
            交易日期 = list[6];
            交易时间 = list[7];
            交易参考号 = list[8];
            批次号 = list[9];
            凭证号 = list[10];
            金额 = list[11];
            卡面凹码 = list[12];
            卡号 = list[13];
            账户余额 = list[14];
            小票余额打印限额 = list[15];
            return true;
        }
    }
  
    public class Res密码修改或重置 : IResBase
    {
        public string Service => "密码修改或重置";

        public string 交易码 { get; set; } = "1325";
        public string 应答码 { get; set; }
        public string 商户编号 { get; set; }
        public string 终端编号 { get; set; }
        public string 账户类型 { get; set; }
        public string 交易类型 { get; set; }
        public string 市民卡卡面号 { get; set; }
        public string 交易日期 { get; set; }
        public string 交易时间 { get; set; }
        public string 交易参考号 { get; set; }
        public string 批次号 { get; set; }
        public string POS机流水号 { get; set; }

        public string Serilize()
        {
            return String.Join("#", new string[]{
                   商户编号,
                   终端编号,
                   账户类型,
                   交易类型,
                   市民卡卡面号,
                   交易日期,
                   交易时间,
                   交易参考号,
                   批次号,
                   POS机流水号,
            });
        }

        public bool Parse(string text)
        {
            if (!text.Contains("#"))
            {
                应答码 = text;
                return true;
            }
            var list = text.Split('#');
            应答码 = list[0];
            商户编号 = list[1];
            终端编号 = list[2];
            账户类型 = list[3];
            交易类型 = list[4];
            市民卡卡面号 = list[5];
            交易日期 = list[6];
            交易时间 = list[7];
            交易参考号 = list[8];
            批次号 = list[9];
            POS机流水号 = list[10];
            return true;
        }
    }
  
    public class Res密码修改或重置分次 : IResBase
    {
        public string Service => "密码修改或重置分次";

        public string 交易码 { get; set; } = "81325";
        public string 应答码 { get; set; }
        public string 商户编号 { get; set; }
        public string 终端编号 { get; set; }
        public string 账户类型 { get; set; }
        public string 交易类型 { get; set; }
        public string 市民卡卡面号 { get; set; }
        public string 交易日期 { get; set; }
        public string 交易时间 { get; set; }
        public string 交易参考号 { get; set; }
        public string 批次号 { get; set; }
        public string POS机流水号 { get; set; }

        public string Serilize()
        {
            return String.Join("#", new string[]{
                   商户编号,
                   终端编号,
                   账户类型,
                   交易类型,
                   市民卡卡面号,
                   交易日期,
                   交易时间,
                   交易参考号,
                   批次号,
                   POS机流水号,
            });
        }

        public bool Parse(string text)
        {
            if (!text.Contains("#"))
            {
                应答码 = text;
                return true;
            }
            var list = text.Split('#');
            应答码 = list[0];
            商户编号 = list[1];
            终端编号 = list[2];
            账户类型 = list[3];
            交易类型 = list[4];
            市民卡卡面号 = list[5];
            交易日期 = list[6];
            交易时间 = list[7];
            交易参考号 = list[8];
            批次号 = list[9];
            POS机流水号 = list[10];
            return true;
        }
    }
  
    public class Res密码修改或重置分次十六进制 : IResBase
    {
        public string Service => "密码修改或重置分次十六进制";

        public string 交易码 { get; set; } = "91325";
        public string 应答码 { get; set; }
        public string 商户编号 { get; set; }
        public string 终端编号 { get; set; }
        public string 账户类型 { get; set; }
        public string 交易类型 { get; set; }
        public string 市民卡卡面号 { get; set; }
        public string 交易日期 { get; set; }
        public string 交易时间 { get; set; }
        public string 交易参考号 { get; set; }
        public string 批次号 { get; set; }
        public string POS机流水号 { get; set; }

        public string Serilize()
        {
            return String.Join("#", new string[]{
                   商户编号,
                   终端编号,
                   账户类型,
                   交易类型,
                   市民卡卡面号,
                   交易日期,
                   交易时间,
                   交易参考号,
                   批次号,
                   POS机流水号,
            });
        }

        public bool Parse(string text)
        {
            if (!text.Contains("#"))
            {
                应答码 = text;
                return true;
            }
            var list = text.Split('#');
            应答码 = list[0];
            商户编号 = list[1];
            终端编号 = list[2];
            账户类型 = list[3];
            交易类型 = list[4];
            市民卡卡面号 = list[5];
            交易日期 = list[6];
            交易时间 = list[7];
            交易参考号 = list[8];
            批次号 = list[9];
            POS机流水号 = list[10];
            return true;
        }
    }
  
    public class Res余额查询密码外入版 : IResBase
    {
        public string Service => "余额查询密码外入版";

        public string 交易码 { get; set; } = "91025";
        public string 应答码 { get; set; }
        public string 金额 { get; set; }
        public string 卡面号 { get; set; }

        public string Serilize()
        {
            return String.Join("#", new string[]{
                   金额,
                   卡面号,
            });
        }

        public bool Parse(string text)
        {
            if (!text.Contains("#"))
            {
                应答码 = text;
                return true;
            }
            var list = text.Split('#');
            应答码 = list[0];
            金额 = list[1];
            卡面号 = list[2];
            return true;
        }
    }
  
    public class Res密码回显 : IResBase
    {
        public string Service => "密码回显";

        public string 交易码 { get; set; } = "9904";
        public string 应答码 { get; set; }
        public string 首位标志 { get; set; }
        public string 密码个数 { get; set; }
        public string 密码值 { get; set; }

        public string Serilize()
        {
            return String.Join("#", new string[]{
                   首位标志,
                   密码个数,
                   密码值,
            });
        }

        public bool Parse(string text)
        {
            if (!text.Contains("#"))
            {
                应答码 = text;
                return true;
            }
            var list = text.Split('#');
            应答码 = list[0];
            首位标志 = list[1];
            密码个数 = list[2];
            密码值 = list[3];
            return true;
        }
    }
}


