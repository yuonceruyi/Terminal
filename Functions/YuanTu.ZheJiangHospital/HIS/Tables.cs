using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YuanTu.ZheJiangHospital.HIS
{
    internal class Req
    {
        public virtual string 业务类型 => null;
    }

    public class Res
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public virtual bool Parse(string s)
        {
            var p = s.IndexOf("^", StringComparison.Ordinal);
            if (p < 0)
            {
                Success = false;
                Message = "解析出错:" + s;
                return false;
            }
            int ret;
            if (!int.TryParse(s.Substring(0, p), out ret) || ret != 0)
            {
                Success = false;
                Message = s.Substring(p + 1);
                return false;
            }
            Success = true;
            return true;
        }
    }

    class Req建档 : Req
    {
        public override string 业务类型 => "F142";
        public string 卡号 { get; set; }
        public string 姓名 { get; set; }
        public string 性别 { get; set; }
        public string 民族 { get; set; }
        public string 生日 { get; set; }
        public string 身份证号 { get; set; }
        public string 地址 { get; set; }
        public string 电话 { get; set; }
        public string 医保类别 { get; set; }
        public override string ToString()
        {
            return $"{业务类型}^{卡号}^{姓名}^{性别}^{民族}^{生日}^{身份证号}^{地址}^{电话}^{医保类别}";
        }
    }
    public class Res建档 : Res
    {
        public string 银医通账号 { get; set; }
        public string 就诊卡号 { get; set; }
        public override bool Parse(string s)
        {
            if (!base.Parse(s))
                return false;
            var list = s.Split('^');
            if (list.Length != 3)
            {
                Success = false;
                Message = "返回域数量不对:" + s;
                return false;
            }
            银医通账号 = list[1];
            就诊卡号 = list[2];
            return true;
        }
    }

    class Req建档读卡 : Req
    {
        public override string 业务类型 => "F199";
        public string 医保类别 { get; set; }
        public override string ToString()
        {
            return $"{业务类型}^{医保类别}^0000000";
        }
    }

    public class Res建档读卡 : Res
    {
        public string 卡号 { get; set; }
        public string 姓名 { get; set; }
        public string 性别 { get; set; }
        public string 民族 { get; set; }
        public string 生日 { get; set; }
        public string 身份证号 { get; set; }
        public string 地址 { get; set; }
        public override bool Parse(string s)
        {
            if (!base.Parse(s))
                return false;
            var list = s.Split('^');
            if (list.Length != 8)
            {
                Success = false;
                Message = "返回域数量不对:" + s;
                return false;
            }
            卡号 = list[1];
            姓名 = list[2];
            性别 = list[3];
            民族 = list[4];
            生日 = list[5];
            身份证号 = list[6];
            地址 = list[7];
            return true;
        }
    }

    class Req读卡 : Req
    {
        public override string 业务类型 => "F200";
        public string 医保类别 { get; set; }
        public override string ToString()
        {
            return $"{业务类型}^{医保类别}^0000000";
        }
    }

    public class Res读卡 : Res
    {
        public string 门诊号 { get; set; }
        public string 身份证号 { get; set; }
        public string 姓名 { get; set; }
        public string 银行账号 { get; set; }
        public string 就诊卡号 { get; set; }
        public string 卡面号 { get; set; }
        public string 卡内号 { get; set; }
        public override bool Parse(string s)
        {
            if (!base.Parse(s))
                return false;
            var list = s.Split('^');
            if (list.Length != 8)
            {
                Success = false;
                Message = "返回域数量不对:" + s;
                return false;
            }
            门诊号 = list[1];
            身份证号 = list[2];
            姓名 = list[3];
            银行账号 = list[4];
            就诊卡号 = list[5];
            卡面号 = list[6];
            卡内号 = list[7];
            return true;
        }
    }

    internal class Req挂号 : Req
    {
        public override string 业务类型 => "F144";
        public string 医保类别 { get; set; }
        public string 医保卡号 { get; set; }
        public string 科室代码 { get; set; }
        public string 医生代码 { get; set; }
        public string 值班类别 { get; set; }

        public override string ToString()
        {
            return $"{业务类型}^{医保类别}^{医保卡号}^{科室代码}^{医生代码}^{值班类别}";
        }
    }

    internal class Res挂号 : Res
    {
        public string 医院名称 { get; set; }
        public string 就诊号码 { get; set; }
        public string 门诊号码 { get; set; }
        public string 病人姓名 { get; set; }
        public string 挂号日期 { get; set; }
        public string 挂号费总额 { get; set; }
        public string 病人账户支付 { get; set; }
        public string 医保支付 { get; set; }
        public string 挂号科室 { get; set; }
        public string 挂号医生 { get; set; }
        public string 就诊序号 { get; set; }
        public string 挂号班别 { get; set; }
        public string 就诊卡号 { get; set; }
        public string 就诊地址 { get; set; }

        public override bool Parse(string s)
        {
            if (!base.Parse(s))
                return false;
            var list = s.Split('^');
            if (list.Length != 15)
            {
                Success = false;
                Message = "返回域数量不对:" + s;
                return false;
            }
            医院名称 = list[1];
            就诊号码 = list[2];
            门诊号码 = list[3];
            病人姓名 = list[4];
            挂号日期 = list[5];
            挂号费总额 = list[6];
            病人账户支付 = list[7];
            医保支付 = list[8];
            挂号科室 = list[9];
            挂号医生 = list[10];
            就诊序号 = list[11];
            挂号班别 = list[12];
            就诊卡号 = list[13];
            就诊地址 = list[14];
            return true;
        }
    }

    internal class Req预约挂号 : Req
    {
        public override string 业务类型 => "F147";
        public string 医保类别 { get; set; }
        public string 医保卡号 { get; set; }
        public string 科室代码 { get; set; }
        public string 医生代码 { get; set; }
        public string 值班类别 { get; set; }
        public string 预约日期 { get; set; }

        public override string ToString()
        {
            return $"{业务类型}^{医保类别}^{医保卡号}^{科室代码}^{医生代码}^{值班类别}^{预约日期}";
        }
    }

    internal class Res预约挂号 : Res
    {
        public string 医院名称 { get; set; }
        public string 自助预约单 { get; set; }
        public string 门诊号码 { get; set; }
        public string 病人姓名 { get; set; }
        public string 预约日期 { get; set; }
        public string 预约科室 { get; set; }
        public string 预约医生 { get; set; }
        public string 预约序号 { get; set; }
        public string 预约班别 { get; set; }
        public override bool Parse(string s)
        {
            if (!base.Parse(s))
                return false;
            var list = s.Split('^');
            if (list.Length != 10)
            {
                Success = false;
                Message = "返回域数量不对:" + s;
                return false;
            }
            医院名称 = list[1];
            自助预约单 = list[2];
            门诊号码 = list[3];
            病人姓名 = list[4];
            预约日期 = list[5];
            预约科室 = list[6];
            预约医生 = list[7];
            预约序号 = list[8];
            预约班别 = list[9];
            return true;
        }
    }

    internal class Req取预约号 : Req
    {
        public override string 业务类型 => "F149";
        public string 医保类别 { get; set; }
        public string 身份证号 { get; set; }
        public override string ToString()
        {
            return $"{业务类型}^{医保类别}^{身份证号}";
        }
    }

    public class Res取预约号 : Res
    {
        public string 医院名称 { get; set; }
        public string 预约取号单 { get; set; }
        public string 门诊号码 { get; set; }
        public string 病人姓名 { get; set; }
        public string 挂号日期 { get; set; }
        public string 挂号费总额 { get; set; }
        public string 病人账户支付 { get; set; }
        public string 医保支付 { get; set; }
        public string 挂号科室 { get; set; }
        public string 挂号医生 { get; set; }
        public string 就诊序号 { get; set; }
        public string 挂号班别 { get; set; }
        public string 就诊卡号 { get; set; }

        public override bool Parse(string s)
        {
            if (!base.Parse(s))
                return false;
            var list = s.Split('^');
            if (list.Length != 14)
            {
                Success = false;
                Message = "返回域数量不对:" + s;
                return false;
            }
            医院名称 = list[1];
            预约取号单 = list[2];
            门诊号码 = list[3];
            病人姓名 = list[4];
            挂号日期 = list[5];
            挂号费总额 = list[6];
            病人账户支付 = list[7];
            医保支付 = list[8];
            挂号科室 = list[9];
            挂号医生 = list[10];
            就诊序号 = list[11];
            挂号班别 = list[12];
            就诊卡号 = list[13];
            return true;
        }
    }

    class Req自助预结算 : Req自助结算
    {
        public override string 业务类型 => "F201";
    }
    class Res自助预结算 : Res
    {
        public 预结算信息 预结算信息 { get; set; }
        public override bool Parse(string s)
        {
            if (!base.Parse(s))
                return false;
            var list = s.Split('^');
            if (list.Length != 2)
            {
                Success = false;
                Message = "返回域数量不对:" + s;
                return false;
            }
            预结算信息 = new 预结算信息().Parse(list[1]);
            return true;
        }
    }

    class Req自助结算 : Req
    {
        public override string 业务类型 => "F202";
        public string 医保类别 { get; set; }
        public long 病人编号 { get; set; }
        public string 医技序号 { get; set; } = "-1";
        public string 处方识别 { get; set; } = "-1";
        public override string ToString()
        {
            return $"{业务类型}^{医保类别}^{病人编号}^{处方识别}^{医技序号}";
        }
    }
    class Req自助结算支付宝 : Req自助结算
    {
        public override string 业务类型 => "F203";
        public string 支付金额 { get; set; }
        public string 支付宝流水号 { get; set; }
        public string 远图流水号 { get; set; }
        public override string ToString()
        {
            return $"{base.ToString()}^{支付金额}^{支付宝流水号}^{远图流水号}";
        }
    }
    class Res自助结算 : Res
    {
        public string 医院名称 { get; set; }
        public string 发票号码 { get; set; }
        public string 门诊号码 { get; set; }
        public string 病人姓名 { get; set; }
        public string 总金额 { get; set; }
        public string 医保支付 { get; set; }
        public string 自费支付 { get; set; }
        public string 银医通余额 { get; set; }
        public string 操作工号 { get; set; }
        public string 就诊卡号 { get; set; }
        public string 医保卡号 { get; set; }
        public string 医保支付信息 { get; set; }

        public string 发药窗口 { get; set; }
        public string 处方信息 { get; set; }
        public List<处方信息> 处方信息List { get; set; }
        public string 医技信息 { get; set; }
        public List<医技信息> 医技信息List { get; set; }
        public override bool Parse(string s)
        {
            if (!base.Parse(s))
                return false;
            var list = s.Split('^');
            if (list.Length != 16)
            {
                Success = false;
                Message = "返回域数量不对:" + s;
                return false;
            }
            医院名称 = list[1];
            发票号码 = list[2];
            门诊号码 = list[3];
            病人姓名 = list[4];
            总金额 = list[5];
            医保支付 = list[6];
            自费支付 = list[7];
            银医通余额 = list[8];
            操作工号 = list[9];
            就诊卡号 = list[10];
            医保卡号 = list[11];
            医保支付信息 = list[12];
            发药窗口 = list[13];
            处方信息 = list[14];
            医技信息 = list[15];

            处方信息List = new List<处方信息>();
            var l1 = 处方信息.Split(new[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var l in l1)
                处方信息List.Add(new 处方信息().Parse(l));

            医技信息List = new List<医技信息>();
            var l2 = 医技信息.Split(new[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var l in l2)
                医技信息List.Add(new 医技信息().Parse(l));

            return true;
        }
    }

    class 预结算信息
    {
        public string 总计金额 { get; set; }
        public string 医保支付 { get; set; }
        public string 银行支付 { get; set; }
        public string 还需支付 { get; set; }

        public 预结算信息 Parse(string s)
        {
            var l = s.Split('|');
            总计金额 = l[0];
            医保支付 = l[1];
            银行支付 = l[2];
            还需支付 = l[3];
            return this;
        }

        public override string ToString()
        {
            return $"{总计金额}={医保支付}+{银行支付}+{还需支付}";
        }
    }

    class 处方信息
    {
        public string 项目名称 { get; set; }
        public string 报销类型 { get; set; }
        public string 数量 { get; set; }
        public string 金额 { get; set; }

        public 处方信息 Parse(string s)
        {
            var l = s.Split('|');
            项目名称 = l[0];
            报销类型 = l[1];
            数量 = l[2];
            金额 = l[3];
            return this;
        }

        public override string ToString()
        {
            return $"[{报销类型}]{项目名称}x{数量}@{金额}";
        }
    }

    class 医技信息
    {
        public string 姓名 { get; set; }
        public string 医保类型 { get; set; }
        public string 性别 { get; set; }
        public string 年龄 { get; set; }
        public string 发票号码 { get; set; }
        public string 条码 { get; set; }
        public string 检查科室 { get; set; }
        public string Unknown2 { get; set; }
        public string 检查地址 { get; set; }
        public string 开单医生 { get; set; }
        public string 检查名称 { get; set; }

        public 医技信息 Parse(string s)
        {
            var l = s.Split('|');
            姓名 = l[0];
            医保类型 = l[1];
            性别 = l[2];
            年龄 = l[3];
            发票号码 = l[4];
            条码 = l[5];
            检查科室 = l[6];
            Unknown2 = l[7];
            检查地址 = l[8];
            开单医生 = l[9];
            检查名称 = l[10];
            return this;
        }

        public override string ToString()
        {
            return $"{检查名称}@{检查科室}";
        }
    }
}