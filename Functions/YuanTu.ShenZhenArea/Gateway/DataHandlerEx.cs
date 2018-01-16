using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Gateway.Base;


namespace YuanTu.ShenZhenArea.Gateway
{
    public class SZDataHandler
    {
		public static DataHandler szHandler=new DataHandler();
        public static res医保个人基本信息查询 医保个人基本信息查询(req医保个人基本信息查询 req)
        {
            return szHandler.Query<res医保个人基本信息查询, req医保个人基本信息查询>(req);
        }

		public static res医保个人基本信息查询 医保个人基本信息查询(req医保个人基本信息查询 req,Uri url)
        {
            return szHandler.Query<res医保个人基本信息查询, req医保个人基本信息查询>(req,url);
        }
        public static res医保门诊挂号 医保门诊挂号(req医保门诊挂号 req)
        {
            return szHandler.Query<res医保门诊挂号, req医保门诊挂号>(req);
        }

		public static res医保门诊挂号 医保门诊挂号(req医保门诊挂号 req,Uri url)
        {
            return szHandler.Query<res医保门诊挂号, req医保门诊挂号>(req,url);
        }
        public static res医保门诊登记 医保门诊登记(req医保门诊登记 req)
        {
            return szHandler.Query<res医保门诊登记, req医保门诊登记>(req);
        }

		public static res医保门诊登记 医保门诊登记(req医保门诊登记 req,Uri url)
        {
            return szHandler.Query<res医保门诊登记, req医保门诊登记>(req,url);
        }
        public static res医保门诊费用 医保门诊费用(req医保门诊费用 req)
        {
            return szHandler.Query<res医保门诊费用, req医保门诊费用>(req);
        }

		public static res医保门诊费用 医保门诊费用(req医保门诊费用 req,Uri url)
        {
            return szHandler.Query<res医保门诊费用, req医保门诊费用>(req,url);
        }
        public static res医保门诊退费 医保门诊退费(req医保门诊退费 req)
        {
            return szHandler.Query<res医保门诊退费, req医保门诊退费>(req);
        }

		public static res医保门诊退费 医保门诊退费(req医保门诊退费 req,Uri url)
        {
            return szHandler.Query<res医保门诊退费, req医保门诊退费>(req,url);
        }
        public static res医保门诊支付确认 医保门诊支付确认(req医保门诊支付确认 req)
        {
            return szHandler.Query<res医保门诊支付确认, req医保门诊支付确认>(req);
        }

		public static res医保门诊支付确认 医保门诊支付确认(req医保门诊支付确认 req,Uri url)
        {
            return szHandler.Query<res医保门诊支付确认, req医保门诊支付确认>(req,url);
        }
        public static res排班信息查询 排班信息查询(req排班信息查询 req)
        {
            return szHandler.Query<res排班信息查询, req排班信息查询>(req);
        }

		public static res排班信息查询 排班信息查询(req排班信息查询 req,Uri url)
        {
            return szHandler.Query<res排班信息查询, req排班信息查询>(req,url);
        }
        public static res交易记账 交易记账(req交易记账 req)
        {
            return szHandler.Query<res交易记账, req交易记账>(req);
        }

		public static res交易记账 交易记账(req交易记账 req,Uri url)
        {
            return szHandler.Query<res交易记账, req交易记账>(req,url);
        }
        public static res医保消费记账 医保消费记账(req医保消费记账 req)
        {
            return szHandler.Query<res医保消费记账, req医保消费记账>(req);
        }

		public static res医保消费记账 医保消费记账(req医保消费记账 req,Uri url)
        {
            return szHandler.Query<res医保消费记账, req医保消费记账>(req,url);
        }
    }

    
    public class req医保个人基本信息查询 : GatewayRequest
    {
        /// <summary>
        /// 医保个人基本信息查询
        /// </summary>
        public req医保个人基本信息查询()
        {
            service = "yuantu.wap.yb.query.patient.info";
            _serviceName = "医保个人基本信息查询";
        }
        

        public string data { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
          dic[nameof(data)] = data;

            return dic;
        }

    }
    
    public class req医保门诊挂号 : GatewayRequest
    {
        /// <summary>
        /// 医保门诊挂号
        /// </summary>
        public req医保门诊挂号()
        {
            service = "yuantu.wap.yb.save.reg";
            _serviceName = "医保门诊挂号";
        }
        

        public string data { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
          dic[nameof(data)] = data;

            return dic;
        }

    }
    
    public class req医保门诊登记 : GatewayRequest
    {
        /// <summary>
        /// 医保门诊登记
        /// </summary>
        public req医保门诊登记()
        {
            service = "yuantu.wap.yb.save.sign.info";
            _serviceName = "医保门诊登记";
        }
        

        public string data { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
          dic[nameof(data)] = data;

            return dic;
        }

    }
    
    public class req医保门诊费用 : GatewayRequest
    {
        /// <summary>
        /// 医保门诊费用
        /// </summary>
        public req医保门诊费用()
        {
            service = "yuantu.wap.yb.save.balance.bill";
            _serviceName = "医保门诊费用";
        }
        

        public string data { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
          dic[nameof(data)] = data;

            return dic;
        }

    }
    
    public class req医保门诊退费 : GatewayRequest
    {
        /// <summary>
        /// 医保门诊退费
        /// </summary>
        public req医保门诊退费()
        {
            service = "yuantu.wap.yb.back.fee";
            _serviceName = "医保门诊退费";
        }
        

        public string data { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
          dic[nameof(data)] = data;

            return dic;
        }

    }
    
    public class req医保门诊支付确认 : GatewayRequest
    {
        /// <summary>
        /// 医保门诊支付确认
        /// </summary>
        public req医保门诊支付确认()
        {
            service = "yuantu.wap.yb.pay.confirmed";
            _serviceName = "医保门诊支付确认";
        }
        

        public string data { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
          dic[nameof(data)] = data;

            return dic;
        }

    }
    
    public class req排班信息查询 : GatewayRequest
    {
        /// <summary>
        /// 排班信息查询
        /// </summary>
        public req排班信息查询()
        {
            service = "yuantu.wap.query.registration.schedule.info.list";
            _serviceName = "排班信息查询";
        }
        

        public string regMode { get; set; }
        

        public string regType { get; set; }
        

        public string medAmPm { get; set; }
        

        public string deptCode { get; set; }
        

        public string parentDeptCode { get; set; }
        

        public string doctCode { get; set; }
        

        public string startDate { get; set; }
        

        public string endDate { get; set; }
        

        public string PatientId { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
          dic[nameof(regMode)] = regMode;

          dic[nameof(regType)] = regType;

          dic[nameof(medAmPm)] = medAmPm;

          dic[nameof(deptCode)] = deptCode;

          dic[nameof(parentDeptCode)] = parentDeptCode;

          dic[nameof(doctCode)] = doctCode;

          dic[nameof(startDate)] = startDate;

          dic[nameof(endDate)] = endDate;

          dic[nameof(PatientId)] = PatientId;

            return dic;
        }

    }
    
    public class req交易记账 : GatewayRequest
    {
        /// <summary>
        /// 交易记账
        /// </summary>
        public req交易记账()
        {
            service = "yuantu.wap.jizhang";
            _serviceName = "交易记账";
        }
        

        /// <summary>
        /// 交易状态 成功 失败
        /// </summary>
        public string zhuangtai { get; set; }
        

        /// <summary>
        /// 交易码  PRE：预交金充值 DEP：住院押金 INV：消费
        /// </summary>
        public string jiaoyima { get; set; }
        

        /// <summary>
        /// 卡号
        /// </summary>
        public string kahao { get; set; }
        

        /// <summary>
        /// 交易金额
        /// </summary>
        public string jiaoyijine { get; set; }
        

        /// <summary>
        /// 银联批次号
        /// </summary>
        public string yinlianpicihao { get; set; }
        

        /// <summary>
        /// 银联凭证号
        /// </summary>
        public string yinlianpingzhenghao { get; set; }
        

        /// <summary>
        /// 终端号
        /// </summary>
        public string zhongduanhao { get; set; }
        

        /// <summary>
        /// 银联卡类型ID
        /// </summary>
        public string yinliankaleixingID { get; set; }
        

        /// <summary>
        /// 银联交易流水号
        /// </summary>
        public string yinlianjiaoyiliushuihao { get; set; }
        

        /// <summary>
        /// 银行卡号
        /// </summary>
        public string yinhangkahao { get; set; }
        

        /// <summary>
        /// 银联交易日期
        /// </summary>
        public string yinlianjiaoyiriqi { get; set; }
        

        /// <summary>
        /// 交易日期
        /// </summary>
        public string jiaoyiriqi { get; set; }
        

        /// <summary>
        /// 交易时间
        /// </summary>
        public string jiaoyishijian { get; set; }
        

        /// <summary>
        /// 收费ID
        /// </summary>
        public string shoufeiID { get; set; }
        

        /// <summary>
        /// 操作员号
        /// </summary>
        public string caozuoyuanhao { get; set; }
        

        /// <summary>
        /// 自费部分的交易方式
        /// </summary>
        public string tradeMode { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
          dic[nameof(zhuangtai)] = zhuangtai;

          dic[nameof(jiaoyima)] = jiaoyima;

          dic[nameof(kahao)] = kahao;

          dic[nameof(jiaoyijine)] = jiaoyijine;

          dic[nameof(yinlianpicihao)] = yinlianpicihao;

          dic[nameof(yinlianpingzhenghao)] = yinlianpingzhenghao;

          dic[nameof(zhongduanhao)] = zhongduanhao;

          dic[nameof(yinliankaleixingID)] = yinliankaleixingID;

          dic[nameof(yinlianjiaoyiliushuihao)] = yinlianjiaoyiliushuihao;

          dic[nameof(yinhangkahao)] = yinhangkahao;

          dic[nameof(yinlianjiaoyiriqi)] = yinlianjiaoyiriqi;

          dic[nameof(jiaoyiriqi)] = jiaoyiriqi;

          dic[nameof(jiaoyishijian)] = jiaoyishijian;

          dic[nameof(shoufeiID)] = shoufeiID;

          dic[nameof(caozuoyuanhao)] = caozuoyuanhao;

          dic[nameof(tradeMode)] = tradeMode;

            return dic;
        }

    }
    
    public class req医保消费记账 : GatewayRequest
    {
        /// <summary>
        /// 医保消费记账
        /// </summary>
        public req医保消费记账()
        {
            service = "yuantu.wap.ybjizhang";
            _serviceName = "医保消费记账";
        }
        

        /// <summary>
        /// 医疗证号
        /// </summary>
        public string ylzh { get; set; }
        

        /// <summary>
        /// 病人姓名
        /// </summary>
        public string name { get; set; }
        

        /// <summary>
        /// 登记单据号
        /// </summary>
        public string danjuhao { get; set; }
        

        /// <summary>
        /// 费用总额
        /// </summary>
        public string feiyongheji { get; set; }
        

        /// <summary>
        /// 记账金额-医保报销金额
        /// </summary>
        public string jizhang { get; set; }
        

        /// <summary>
        /// 现金-自费金额
        /// </summary>
        public string xianjin { get; set; }
        

        /// <summary>
        /// 操作员编码
        /// </summary>
        public string czybm { get; set; }
        

        /// <summary>
        /// 操作员姓名
        /// </summary>
        public string czyxm { get; set; }
        

        /// <summary>
        /// 结算时间
        /// </summary>
        public string jstime { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
          dic[nameof(ylzh)] = ylzh;

          dic[nameof(name)] = name;

          dic[nameof(danjuhao)] = danjuhao;

          dic[nameof(feiyongheji)] = feiyongheji;

          dic[nameof(jizhang)] = jizhang;

          dic[nameof(xianjin)] = xianjin;

          dic[nameof(czybm)] = czybm;

          dic[nameof(czyxm)] = czyxm;

          dic[nameof(jstime)] = jstime;

            return dic;
        }

    }


    
    public class res医保个人基本信息查询 : GatewayResponse
    {
        public 医保个人基本信息 data { get; set; }
    }
    
    public class res医保门诊挂号 : GatewayResponse
    {
        public List<医保门诊挂号结果> data { get; set; }
    }
    
    public class res医保门诊登记 : GatewayResponse
    {
        public string data { get; set; }
    }
    
    public class res医保门诊费用 : GatewayResponse
    {
        public 医保门诊费用结果 data { get; set; }
    }
    
    public class res医保门诊退费 : GatewayResponse
    {
        public 医保门诊退费结果 data { get; set; }
    }
    
    public class res医保门诊支付确认 : GatewayResponse
    {
        public string data { get; set; }
    }
    
    public class res排班信息查询 : GatewayResponse
    {
        public List<排班信息> data { get; set; }
    }
    
    public class res交易记账 : GatewayResponse
    {
        public string data { get; set; }
    }
    
    public class res医保消费记账 : GatewayResponse
    {
        public string data { get; set; }
    }

}