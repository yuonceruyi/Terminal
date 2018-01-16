using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts.UserCenter.Entities;

namespace YuanTu.Consts.UserCenter
{
    public partial class DataHandlerEx
    {
        public static IDataHandler Handler { get; set; }

        public static res医院列表 医院列表(req医院列表 req,string url=null)
        {
            return Handler.Query<res医院列表, req医院列表>(req,url);
        }

        public static res医生主页 医生主页(req医生主页 req,string url=null)
        {
            return Handler.Query<res医生主页, req医生主页>(req,url);
        }

        public static res医生排班主页 医生排班主页(req医生排班主页 req,string url=null)
        {
            return Handler.Query<res医生排班主页, req医生排班主页>(req,url);
        }

        public static res按医生排班列表 按医生排班列表(req按医生排班列表 req,string url=null)
        {
            return Handler.Query<res按医生排班列表, req按医生排班列表>(req,url);
        }

        public static res按日期排班列表 按日期排班列表(req按日期排班列表 req,string url=null)
        {
            return Handler.Query<res按日期排班列表, req按日期排班列表>(req,url);
        }

        public static res查询排班号量 查询排班号量(req查询排班号量 req,string url=null)
        {
            return Handler.Query<res查询排班号量, req查询排班号量>(req,url);
        }

        public static res查询就诊人 查询就诊人(req查询就诊人 req,string url=null)
        {
            return Handler.Query<res查询就诊人, req查询就诊人>(req,url);
        }

        public static res确认挂号 确认挂号(req确认挂号 req,string url=null)
        {
            return Handler.Query<res确认挂号, req确认挂号>(req,url);
        }

        public static res确认预约 确认预约(req确认预约 req,string url=null)
        {
            return Handler.Query<res确认预约, req确认预约>(req,url);
        }

        public static res挂号支付 挂号支付(req挂号支付 req,string url=null)
        {
            return Handler.Query<res挂号支付, req挂号支付>(req,url);
        }

        public static res科室列表 科室列表(req科室列表 req,string url=null)
        {
            return Handler.Query<res科室列表, req科室列表>(req,url);
        }

        public static res获取挂号应付金额 获取挂号应付金额(req获取挂号应付金额 req,string url=null)
        {
            return Handler.Query<res获取挂号应付金额, req获取挂号应付金额>(req,url);
        }

        public static res获取支付方式列表 获取支付方式列表(req获取支付方式列表 req,string url=null)
        {
            return Handler.Query<res获取支付方式列表, req获取支付方式列表>(req,url);
        }

        public static res生成登录二维码 生成登录二维码(req生成登录二维码 req,string url=null)
        {
            return Handler.Query<res生成登录二维码, req生成登录二维码>(req,url);
        }

        public static res根据uuid获取绑定就诊人信息 根据uuid获取绑定就诊人信息(req根据uuid获取绑定就诊人信息 req,string url=null)
        {
            return Handler.Query<res根据uuid获取绑定就诊人信息, req根据uuid获取绑定就诊人信息>(req,url);
        }

        public static res取消扫码 取消扫码(req取消扫码 req,string url=null)
        {
            return Handler.Query<res取消扫码, req取消扫码>(req,url);
        }

        public static res获取token 获取token(req获取token req,string url=null)
        {
            return Handler.Query<res获取token, req获取token>(req,url);
        }

        public static res获取deviceSecret 获取deviceSecret(req获取deviceSecret req,string url=null)
        {
            return Handler.Query<res获取deviceSecret, req获取deviceSecret>(req,url);
        }

    }

    
    public class req医院列表 : UserCenterRequest
    {
        public override string UrlPath => "/restapi/common/corp/allList";

        public override string ServiceName => "医院列表";

        /// <summary>
        /// 医联体id
        /// </summary>
        public string unionId { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(unionId)] = unionId;
            return dic;
        }

    }
    
    public class req医生主页 : UserCenterRequest
    {
        public override string UrlPath => "/restapi/common/doctor/getDoctAccountInfo";

        public override string ServiceName => "医生主页";

        /// <summary>
        /// 医院id
        /// </summary>
        public string corpId { get; set; }
        /// <summary>
        /// 科室编码
        /// </summary>
        public string deptCode { get; set; }
        /// <summary>
        /// 医生编码
        /// </summary>
        public string doctCode { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(corpId)] = corpId;
            dic[nameof(deptCode)] = deptCode;
            dic[nameof(doctCode)] = doctCode;
            return dic;
        }

    }
    
    public class req医生排班主页 : UserCenterRequest
    {
        public override string UrlPath => "/ws/query/doct/schedule";

        public override string ServiceName => "医生排班主页";

        /// <summary>
        /// 医院id
        /// </summary>
        public string corpId { get; set; }
        /// <summary>
        /// 科室编码
        /// </summary>
        public string deptCode { get; set; }
        /// <summary>
        /// 医生编码
        /// </summary>
        public string doctCode { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(corpId)] = corpId;
            dic[nameof(deptCode)] = deptCode;
            dic[nameof(doctCode)] = doctCode;
            return dic;
        }

    }
    
    public class req按医生排班列表 : UserCenterRequest
    {
        public override string UrlPath => "/restapi/common/reservation/listScheduleinfoByDoct";

        public override string ServiceName => "按医生排班列表";

        /// <summary>
        /// 医院id
        /// </summary>
        public string corpId { get; set; }
        /// <summary>
        /// 科室编码
        /// </summary>
        public string deptCode { get; set; }
        /// <summary>
        /// 父科室编码
        /// </summary>
        public string parentDeptCode { get; set; }
        /// <summary>
        /// 1：预约 2：挂号
        /// </summary>
        public string regMode { get; set; }
        /// <summary>
        /// 1:普通 2:专家
        /// </summary>
        public string regType { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(corpId)] = corpId;
            dic[nameof(deptCode)] = deptCode;
            dic[nameof(parentDeptCode)] = parentDeptCode;
            dic[nameof(regMode)] = regMode;
            dic[nameof(regType)] = regType;
            return dic;
        }

    }
    
    public class req按日期排班列表 : UserCenterRequest
    {
        public override string UrlPath => "/restapi/common/reservation/listScheduleinfoByDate";

        public override string ServiceName => "按日期排班列表";

        /// <summary>
        /// 医院id
        /// </summary>
        public string corpId { get; set; }
        /// <summary>
        /// 科室编码
        /// </summary>
        public string deptCode { get; set; }
        /// <summary>
        /// 1：预约 2：挂号
        /// </summary>
        public string regMode { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(corpId)] = corpId;
            dic[nameof(deptCode)] = deptCode;
            dic[nameof(regMode)] = regMode;
            return dic;
        }

    }
    
    public class req查询排班号量 : UserCenterRequest
    {
        public override string UrlPath => "/restapi/common/reservation/numbersource";

        public override string ServiceName => "查询排班号量";

        /// <summary>
        /// 医院id
        /// </summary>
        public string corpId { get; set; }
        /// <summary>
        /// 科室编码
        /// </summary>
        public string deptCode { get; set; }
        /// <summary>
        /// 医生编码
        /// </summary>
        public string doctCode { get; set; }
        /// <summary>
        /// 上下午标志 1：上午 2：下午
        /// </summary>
        public string medAmPm { get; set; }
        /// <summary>
        /// 就诊日期
        /// </summary>
        public string medDate { get; set; }
        /// <summary>
        /// 1：预约 2：挂号
        /// </summary>
        public string regMode { get; set; }
        /// <summary>
        /// 挂号类别1普通，2专家，3名医，4急诊，5便民，6视频问诊
        /// </summary>
        public string regType { get; set; }
        /// <summary>
        /// 排班id
        /// </summary>
        public string scheduleId { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(corpId)] = corpId;
            dic[nameof(deptCode)] = deptCode;
            dic[nameof(doctCode)] = doctCode;
            dic[nameof(medAmPm)] = medAmPm;
            dic[nameof(medDate)] = medDate;
            dic[nameof(regMode)] = regMode;
            dic[nameof(regType)] = regType;
            dic[nameof(scheduleId)] = scheduleId;
            return dic;
        }

    }
    
    public class req查询就诊人 : UserCenterRequest
    {
        public override string UrlPath => "/restapi/common/selfDevice/queryPatient";

        public override string ServiceName => "查询就诊人";

        /// <summary>
        /// 卡号
        /// </summary>
        public string cardNo { get; set; }
        /// <summary>
        /// 卡类型
        /// </summary>
        public string cardType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string unionId { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(cardNo)] = cardNo;
            dic[nameof(cardType)] = cardType;
            dic[nameof(unionId)] = unionId;
            return dic;
        }

    }
    
    public class req确认挂号 : UserCenterRequest
    {
        public override string UrlPath => "/restapi/common/selfDevice/regCreateOrder";

        public override string ServiceName => "确认挂号";

        /// <summary>
        /// 医院id
        /// </summary>
        public string corpId { get; set; }
        /// <summary>
        /// 科室编码
        /// </summary>
        public string deptCode { get; set; }
        /// <summary>
        /// 医生编码
        /// </summary>
        public string doctCode { get; set; }
        /// <summary>
        /// 上下午标志 1：上午 2：下午
        /// </summary>
        public string medAmPm { get; set; }
        /// <summary>
        /// 就诊日期
        /// </summary>
        public string medDate { get; set; }
        /// <summary>
        /// 1：预约 2：挂号
        /// </summary>
        public string regMode { get; set; }
        /// <summary>
        /// 挂号类别1普通，2专家，3名医，4急诊，5便民，6视频问诊
        /// </summary>
        public string regType { get; set; }
        /// <summary>
        /// 排班id
        /// </summary>
        public string scheduleId { get; set; }
        /// <summary>
        /// 就诊人id
        /// </summary>
        public string patientId { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(corpId)] = corpId;
            dic[nameof(deptCode)] = deptCode;
            dic[nameof(doctCode)] = doctCode;
            dic[nameof(medAmPm)] = medAmPm;
            dic[nameof(medDate)] = medDate;
            dic[nameof(regMode)] = regMode;
            dic[nameof(regType)] = regType;
            dic[nameof(scheduleId)] = scheduleId;
            dic[nameof(patientId)] = patientId;
            return dic;
        }

    }
    
    public class req确认预约 : UserCenterRequest
    {
        public override string UrlPath => "/restapi/common/selfDevice/appointCreateOrder";

        public override string ServiceName => "确认预约";

        /// <summary>
        /// 医院id
        /// </summary>
        public string corpId { get; set; }
        /// <summary>
        /// 科室编码
        /// </summary>
        public string deptCode { get; set; }
        /// <summary>
        /// 医生编码
        /// </summary>
        public string doctCode { get; set; }
        /// <summary>
        /// 上下午标志 1：上午 2：下午
        /// </summary>
        public string medAmPm { get; set; }
        /// <summary>
        /// 就诊日期
        /// </summary>
        public string medDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string medBegTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string medEndTime { get; set; }
        /// <summary>
        /// 挂号序号 int 号源
        /// </summary>
        public string appoNo { get; set; }
        /// <summary>
        /// 1：预约 2：挂号
        /// </summary>
        public string regMode { get; set; }
        /// <summary>
        /// 挂号类别1普通，2专家，3名医，4急诊，5便民，6视频问诊
        /// </summary>
        public string regType { get; set; }
        /// <summary>
        /// 排班id
        /// </summary>
        public string scheduleId { get; set; }
        /// <summary>
        /// 就诊人id
        /// </summary>
        public string patientId { get; set; }
        /// <summary>
        /// 付费通道：1、支付宝 2、微信 3、预缴金(余额)4、到院支付
        /// </summary>
        public string feeChannel { get; set; }
        /// <summary>
        /// 3：挂号 6：预约
        /// </summary>
        public string optType { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(corpId)] = corpId;
            dic[nameof(deptCode)] = deptCode;
            dic[nameof(doctCode)] = doctCode;
            dic[nameof(medAmPm)] = medAmPm;
            dic[nameof(medDate)] = medDate;
            dic[nameof(medBegTime)] = medBegTime;
            dic[nameof(medEndTime)] = medEndTime;
            dic[nameof(appoNo)] = appoNo;
            dic[nameof(regMode)] = regMode;
            dic[nameof(regType)] = regType;
            dic[nameof(scheduleId)] = scheduleId;
            dic[nameof(patientId)] = patientId;
            dic[nameof(feeChannel)] = feeChannel;
            dic[nameof(optType)] = optType;
            return dic;
        }

    }
    
    public class req挂号支付 : UserCenterRequest
    {
        public override string UrlPath => "/restapi/common/selfDevice/account/preCharge";

        public override string ServiceName => "挂号支付";

        /// <summary>
        /// 医院id
        /// </summary>
        public string corpId { get; set; }
        /// <summary>
        /// 付费通道：1、支付宝 2、微信 3、预缴金(余额)4、到院支付
        /// </summary>
        public string feeChannel { get; set; }
        /// <summary>
        /// 3：挂号 6：预约
        /// </summary>
        public string optType { get; set; }
        /// <summary>
        /// 挂号记录的主键id
        /// </summary>
        public string outId { get; set; }
        /// <summary>
        /// 就诊人id
        /// </summary>
        public string patientId { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(corpId)] = corpId;
            dic[nameof(feeChannel)] = feeChannel;
            dic[nameof(optType)] = optType;
            dic[nameof(outId)] = outId;
            dic[nameof(patientId)] = patientId;
            return dic;
        }

    }
    
    public class req科室列表 : UserCenterRequest
    {
        public override string UrlPath => "/restapi/common/reservation/multiDeptsList2";

        public override string ServiceName => "科室列表";

        /// <summary>
        /// 医院id
        /// </summary>
        public string corpId { get; set; }
        /// <summary>
        /// 1：预约 2：挂号
        /// </summary>
        public string regMode { get; set; }
        /// <summary>
        /// 挂号类别1普通，2专家，3名医，4急诊，5便民，6视频问诊
        /// </summary>
        public string regType { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(corpId)] = corpId;
            dic[nameof(regMode)] = regMode;
            dic[nameof(regType)] = regType;
            return dic;
        }

    }
    
    public class req获取挂号应付金额 : UserCenterRequest
    {
        public override string UrlPath => "/restapi/common/selfDevice/getAppointRegBenefit";

        public override string ServiceName => "获取挂号应付金额";

        /// <summary>
        /// 医院id
        /// </summary>
        public string corpId { get; set; }
        /// <summary>
        /// 科室编码
        /// </summary>
        public string deptCode { get; set; }
        /// <summary>
        /// 医生编码
        /// </summary>
        public string doctCode { get; set; }
        /// <summary>
        /// 上下午标志 1：上午 2：下午
        /// </summary>
        public string medAmPm { get; set; }
        /// <summary>
        /// 就诊日期
        /// </summary>
        public string medDate { get; set; }
        /// <summary>
        /// 1：预约 2：挂号
        /// </summary>
        public string regMode { get; set; }
        /// <summary>
        /// 挂号类别1普通，2专家，3名医，4急诊，5便民，6视频问诊
        /// </summary>
        public string regType { get; set; }
        /// <summary>
        /// 排班id
        /// </summary>
        public string scheduleId { get; set; }
        /// <summary>
        /// 就诊人id
        /// </summary>
        public string patientId { get; set; }
        /// <summary>
        /// 挂号金额
        /// </summary>
        public string regAmount { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(corpId)] = corpId;
            dic[nameof(deptCode)] = deptCode;
            dic[nameof(doctCode)] = doctCode;
            dic[nameof(medAmPm)] = medAmPm;
            dic[nameof(medDate)] = medDate;
            dic[nameof(regMode)] = regMode;
            dic[nameof(regType)] = regType;
            dic[nameof(scheduleId)] = scheduleId;
            dic[nameof(patientId)] = patientId;
            dic[nameof(regAmount)] = regAmount;
            return dic;
        }

    }
    
    public class req获取支付方式列表 : UserCenterRequest
    {
        public override string UrlPath => "/restapi/common/selfDevice/pay/type";

        public override string ServiceName => "获取支付方式列表";

        /// <summary>
        /// 医院id
        /// </summary>
        public string corpId { get; set; }
        /// <summary>
        /// 1充值 2缴费 3挂号 6预约
        /// </summary>
        public string optType { get; set; }
        /// <summary>
        /// 就诊人id
        /// </summary>
        public string patientId { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(corpId)] = corpId;
            dic[nameof(optType)] = optType;
            dic[nameof(patientId)] = patientId;
            return dic;
        }

    }
    
    public class req生成登录二维码 : UserCenterRequest
    {
        public override string UrlPath => "/restapi/common/ytPatient/getQRCode";

        public override string ServiceName => "生成登录二维码";

        public string corpId { get; set; }
        public string deviceMac { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(corpId)] = corpId;
            dic[nameof(deviceMac)] = deviceMac;
            return dic;
        }

    }
    
    public class req根据uuid获取绑定就诊人信息 : UserCenterRequest
    {
        public override string UrlPath => "/restapi/common/ytPatient/getPatientListByUUID";

        public override string ServiceName => "根据uuid获取绑定就诊人信息";

        public string uuid { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(uuid)] = uuid;
            return dic;
        }

    }
    
    public class req取消扫码 : UserCenterRequest
    {
        public override string UrlPath => "/restapi/common/ytPatient/cancelScan";

        public override string ServiceName => "取消扫码";

        public string uuid { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(uuid)] = uuid;
            return dic;
        }

    }
    
    public class req获取token : UserCenterRequest
    {
        public override string UrlPath => "/api/device/getAccessToken";

        public override string ServiceName => "获取token";

        public string deviceSecret { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(deviceSecret)] = deviceSecret;
            return dic;
        }

    }
    
    public class req获取deviceSecret : UserCenterRequest
    {
        public override string UrlPath => "/api/device/getSecret";

        public override string ServiceName => "获取deviceSecret";

        public string deviceMac { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(deviceMac)] = deviceMac;
            return dic;
        }

    }


    public class res医院列表 : UserCenterResponse
    {
        public List<CorpVO> data { get; set; }
    }

    public class res医生主页 : UserCenterResponse
    {
        public Doct data { get; set; }
    }

    public class res医生排班主页 : UserCenterResponse
    {
        public DoctSchdulesVO data { get; set; }
    }

    public class res按医生排班列表 : UserCenterResponse
    {
        public ScheduleVO data { get; set; }
    }

    public class res按日期排班列表 : UserCenterResponse
    {
        public List<ScheduleVO> data { get; set; }
    }

    public class res查询排班号量 : UserCenterResponse
    {
        public SourceVO data { get; set; }
    }

    public class res查询就诊人 : UserCenterResponse
    {
        public PatientVO data { get; set; }
    }

    public class res确认挂号 : UserCenterResponse
    {
        public AppointRegLogVO data { get; set; }
    }

    public class res确认预约 : UserCenterResponse
    {
        public AppointRegLogVO data { get; set; }
    }

    public class res挂号支付 : UserCenterResponse
    {
        public ResPrePayVO data { get; set; }
    }

    public class res科室列表 : UserCenterResponse
    {
        public ResScheduleDeptList data { get; set; }
    }

    public class res获取挂号应付金额 : UserCenterResponse
    {
        public BenefitInfo data { get; set; }
    }

    public class res获取支付方式列表 : UserCenterResponse
    {
        public PayInfo data { get; set; }
    }

    public class res生成登录二维码 : UserCenterResponse
    {
        public ScanDataVO data { get; set; }
    }

    public class res根据uuid获取绑定就诊人信息 : UserCenterResponse
    {
        public List<PatientVO> data { get; set; }
    }

    public class res取消扫码 : UserCenterResponse
    {
        public List<object> data { get; set; }
    }

    public class res获取token : UserCenterResponse
    {
        public string data { get; set; }
    }

    public class res获取deviceSecret : UserCenterResponse
    {
        public string data { get; set; }
    }
}