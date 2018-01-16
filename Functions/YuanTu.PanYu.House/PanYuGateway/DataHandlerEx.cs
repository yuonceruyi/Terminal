
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.PanYu.House.PanYuGateway.Base;

namespace YuanTu.PanYu.House.PanYuGateway
{
    public partial class DataHandler
    {
        public static res病人信息查询 病人信息查询(req病人信息查询 req)
        {
            return Query<res病人信息查询, req病人信息查询>(req);
        }

        public static res病人建档发卡 病人建档发卡(req病人建档发卡 req)
        {
            return Query<res病人建档发卡, req病人建档发卡>(req);
        }

        public static res发卡 发卡(req发卡 req)
        {
            return Query<res发卡, req发卡>(req);
        }

        public static res诊疗卡账户修改密码 诊疗卡账户修改密码(req诊疗卡账户修改密码 req)
        {
            return Query<res诊疗卡账户修改密码, req诊疗卡账户修改密码>(req);
        }

        public static res诊疗卡密码校验 诊疗卡密码校验(req诊疗卡密码校验 req)
        {
            return Query<res诊疗卡密码校验, req诊疗卡密码校验>(req);
        }

        public static res自助绑定银行卡 自助绑定银行卡(req自助绑定银行卡 req)
        {
            return Query<res自助绑定银行卡, req自助绑定银行卡>(req);
        }

        public static res自助绑定银行卡解绑 自助绑定银行卡解绑(req自助绑定银行卡解绑 req)
        {
            return Query<res自助绑定银行卡解绑, req自助绑定银行卡解绑>(req);
        }

        public static res病人基本信息修改 病人基本信息修改(req病人基本信息修改 req)
        {
            return Query<res病人基本信息修改, req病人基本信息修改>(req);
        }

        public static res就诊情况记录查询 就诊情况记录查询(req就诊情况记录查询 req)
        {
            return Query<res就诊情况记录查询, req就诊情况记录查询>(req);
        }

        public static res就诊满意度 就诊满意度(req就诊满意度 req)
        {
            return Query<res就诊满意度, req就诊满意度>(req);
        }

        public static res获取缴费概要信息 获取缴费概要信息(req获取缴费概要信息 req)
        {
            return Query<res获取缴费概要信息, req获取缴费概要信息>(req);
        }

        public static res获取缴费明细信息 获取缴费明细信息(req获取缴费明细信息 req)
        {
            return Query<res获取缴费明细信息, req获取缴费明细信息>(req);
        }

        public static res缴费预结算 缴费预结算(req缴费预结算 req)
        {
            return Query<res缴费预结算, req缴费预结算>(req);
        }

        public static res缴费结算 缴费结算(req缴费结算 req)
        {
            return Query<res缴费结算, req缴费结算>(req);
        }

        public static res获取已结算记录 获取已结算记录(req获取已结算记录 req)
        {
            return Query<res获取已结算记录, req获取已结算记录>(req);
        }

        public static res预缴金充值 预缴金充值(req预缴金充值 req)
        {
            return Query<res预缴金充值, req预缴金充值>(req);
        }

        public static res查询预缴金充值记录 查询预缴金充值记录(req查询预缴金充值记录 req)
        {
            return Query<res查询预缴金充值记录, req查询预缴金充值记录>(req);
        }

        public static res查询预缴金账户余额 查询预缴金账户余额(req查询预缴金账户余额 req)
        {
            return Query<res查询预缴金账户余额, req查询预缴金账户余额>(req);
        }

        public static res排班科室信息查询 排班科室信息查询(req排班科室信息查询 req)
        {
            return Query<res排班科室信息查询, req排班科室信息查询>(req);
        }

        public static res排班医生信息查询 排班医生信息查询(req排班医生信息查询 req)
        {
            return Query<res排班医生信息查询, req排班医生信息查询>(req);
        }

        public static res排班信息查询 排班信息查询(req排班信息查询 req)
        {
            return Query<res排班信息查询, req排班信息查询>(req);
        }

        public static res号源明细查询 号源明细查询(req号源明细查询 req)
        {
            return Query<res号源明细查询, req号源明细查询>(req);
        }

        public static res锁号 锁号(req锁号 req)
        {
            return Query<res锁号, req锁号>(req);
        }

        public static res解锁 解锁(req解锁 req)
        {
            return Query<res解锁, req解锁>(req);
        }

        public static res当天挂号 当天挂号(req当天挂号 req)
        {
            return Query<res当天挂号, req当天挂号>(req);
        }

        public static res预约挂号 预约挂号(req预约挂号 req)
        {
            return Query<res预约挂号, req预约挂号>(req);
        }

        public static res预约取号 预约取号(req预约取号 req)
        {
            return Query<res预约取号, req预约取号>(req);
        }

        public static res取消预约或挂号 取消预约或挂号(req取消预约或挂号 req)
        {
            return Query<res取消预约或挂号, req取消预约或挂号>(req);
        }

        public static res挂号预约记录查询 挂号预约记录查询(req挂号预约记录查询 req)
        {
            return Query<res挂号预约记录查询, req挂号预约记录查询>(req);
        }

        public static res取号预结算 取号预结算(req取号预结算 req)
        {
            return Query<res取号预结算, req取号预结算>(req);
        }

        public static res取号结算 取号结算(req取号结算 req)
        {
            return Query<res取号结算, req取号结算>(req);
        }

        public static res挂号退号 挂号退号(req挂号退号 req)
        {
            return Query<res挂号退号, req挂号退号>(req);
        }

        public static res住院患者信息查询 住院患者信息查询(req住院患者信息查询 req)
        {
            return Query<res住院患者信息查询, req住院患者信息查询>(req);
        }

        public static res住院患者费用明细查询 住院患者费用明细查询(req住院患者费用明细查询 req)
        {
            return Query<res住院患者费用明细查询, req住院患者费用明细查询>(req);
        }

        public static res住院预缴金充值 住院预缴金充值(req住院预缴金充值 req)
        {
            return Query<res住院预缴金充值, req住院预缴金充值>(req);
        }

        public static res住院预缴金充值记录查询 住院预缴金充值记录查询(req住院预缴金充值记录查询 req)
        {
            return Query<res住院预缴金充值记录查询, req住院预缴金充值记录查询>(req);
        }

        public static res虚拟账户开通 虚拟账户开通(req虚拟账户开通 req)
        {
            return Query<res虚拟账户开通, req虚拟账户开通>(req);
        }

        public static res检验基本信息查询 检验基本信息查询(req检验基本信息查询 req)
        {
            return Query<res检验基本信息查询, req检验基本信息查询>(req);
        }

        public static res更新打印次数 更新打印次数(req更新打印次数 req)
        {
            return Query<res更新打印次数, req更新打印次数>(req);
        }

        public static res检验结果明细查询 检验结果明细查询(req检验结果明细查询 req)
        {
            return Query<res检验结果明细查询, req检验结果明细查询>(req);
        }

        public static res检查结果查询 检查结果查询(req检查结果查询 req)
        {
            return Query<res检查结果查询, req检查结果查询>(req);
        }

        public static res医生信息查询 医生信息查询(req医生信息查询 req)
        {
            return Query<res医生信息查询, req医生信息查询>(req);
        }

        public static res科室信息查询 科室信息查询(req科室信息查询 req)
        {
            return Query<res科室信息查询, req科室信息查询>(req);
        }

        public static res民生卡开卡 民生卡开卡(req民生卡开卡 req)
        {
            return Query<res民生卡开卡, req民生卡开卡>(req);
        }

        public static res民生卡终端签到 民生卡终端签到(req民生卡终端签到 req)
        {
            return Query<res民生卡终端签到, req民生卡终端签到>(req);
        }

        public static res民生卡余额查询 民生卡余额查询(req民生卡余额查询 req)
        {
            return Query<res民生卡余额查询, req民生卡余额查询>(req);
        }

        public static res民生卡交易明细查询 民生卡交易明细查询(req民生卡交易明细查询 req)
        {
            return Query<res民生卡交易明细查询, req民生卡交易明细查询>(req);
        }

        public static res民生卡充值 民生卡充值(req民生卡充值 req)
        {
            return Query<res民生卡充值, req民生卡充值>(req);
        }

        public static res民生卡充值冲正 民生卡充值冲正(req民生卡充值冲正 req)
        {
            return Query<res民生卡充值冲正, req民生卡充值冲正>(req);
        }

        public static res民生卡消费 民生卡消费(req民生卡消费 req)
        {
            return Query<res民生卡消费, req民生卡消费>(req);
        }

        public static res民生卡消费冲正 民生卡消费冲正(req民生卡消费冲正 req)
        {
            return Query<res民生卡消费冲正, req民生卡消费冲正>(req);
        }

        public static res银联卡消费登记 银联卡消费登记(req银联卡消费登记 req)
        {
            return Query<res银联卡消费登记, req银联卡消费登记>(req);
        }

        public static res民生卡退费 民生卡退费(req民生卡退费 req)
        {
            return Query<res民生卡退费, req民生卡退费>(req);
        }

        public static res民生卡工本费 民生卡工本费(req民生卡工本费 req)
        {
            return Query<res民生卡工本费, req民生卡工本费>(req);
        }

        public static res民生卡客户信息更新 民生卡客户信息更新(req民生卡客户信息更新 req)
        {
            return Query<res民生卡客户信息更新, req民生卡客户信息更新>(req);
        }

        public static res民生卡重置密码 民生卡重置密码(req民生卡重置密码 req)
        {
            return Query<res民生卡重置密码, req民生卡重置密码>(req);
        }

        public static res民生卡卡片信息查询 民生卡卡片信息查询(req民生卡卡片信息查询 req)
        {
            return Query<res民生卡卡片信息查询, req民生卡卡片信息查询>(req);
        }

        public static res民生卡密码修改 民生卡密码修改(req民生卡密码修改 req)
        {
            return Query<res民生卡密码修改, req民生卡密码修改>(req);
        }

        public static res民生卡CPU卡密码设置 民生卡CPU卡密码设置(req民生卡CPU卡密码设置 req)
        {
            return Query<res民生卡CPU卡密码设置, req民生卡CPU卡密码设置>(req);
        }

        public static res医保信息查询 医保信息查询(req医保信息查询 req)
        {
            return Query<res医保信息查询, req医保信息查询>(req);
        }

        public static res医生信息快速查询 医生信息快速查询(req医生信息快速查询 req)
        {
            return Query<res医生信息快速查询, req医生信息快速查询>(req);
        }

        public static res系统签到 系统签到(req系统签到 req)
        {
            return Query<res系统签到, req系统签到>(req);
        }

        public static res信息上报 信息上报(req信息上报 req)
        {
            return Query<res信息上报, req信息上报>(req);
        }

        public static res创建扫码订单 创建扫码订单(req创建扫码订单 req)
        {
            return Query<res创建扫码订单, req创建扫码订单>(req);
        }

        public static res取消扫码订单 取消扫码订单(req取消扫码订单 req)
        {
            return Query<res取消扫码订单, req取消扫码订单>(req);
        }

        public static res查询订单状态 查询订单状态(req查询订单状态 req)
        {
            return Query<res查询订单状态, req查询订单状态>(req);
        }

        public static res支付宝支付成功上报 支付宝支付成功上报(req支付宝支付成功上报 req)
        {
            return Query<res支付宝支付成功上报, req支付宝支付成功上报>(req);
        }

        public static res查询将要接种的针次 查询将要接种的针次(req查询将要接种的针次 req)
        {
            return Query<res查询将要接种的针次, req查询将要接种的针次>(req);
        }

        public static res查询将要接种的清单 查询将要接种的清单(req查询将要接种的清单 req)
        {
            return Query<res查询将要接种的清单, req查询将要接种的清单>(req);
        }

        public static res查询公费人员信息 查询公费人员信息(req查询公费人员信息 req)
        {
            return Query<res查询公费人员信息, req查询公费人员信息>(req);
        }

        public static res验证公费人员信息 验证公费人员信息(req验证公费人员信息 req)
        {
            return Query<res验证公费人员信息, req验证公费人员信息>(req);
        }

        public static res停车计费查询 停车计费查询(req停车计费查询 req)
        {
            return Query<res停车计费查询, req停车计费查询>(req);
        }

        public static res停车订单生成 停车订单生成(req停车订单生成 req)
        {
            return Query<res停车订单生成, req停车订单生成>(req);
        }

        public static res停车订单支付 停车订单支付(req停车订单支付 req)
        {
            return Query<res停车订单支付, req停车订单支付>(req);
        }

        public static res病人已有卡查询 病人已有卡查询(req病人已有卡查询 req)
        {
            return Query<res病人已有卡查询, req病人已有卡查询>(req);
        }

        public static res病人绑卡 病人绑卡(req病人绑卡 req)
        {
            return Query<res病人绑卡, req病人绑卡>(req);
        }

    }

    
    public class req病人信息查询 : req
    {
        /// <summary>
        /// 病人信息查询
        /// </summary>
        public req病人信息查询()
        {
            service = "yuantu.wap.query.patient.info";
            _serviceName = "病人信息查询";
        }
        public string cardNo { get; set; }
        public string cardType { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(cardNo)] = cardNo;
            dic[nameof(cardType)] = cardType;
            return dic;
        }

    }
    
    public class req病人建档发卡 : req
    {
        /// <summary>
        /// 病人建档发卡
        /// </summary>
        public req病人建档发卡()
        {
            service = "yuantu.wap.set.patient.info";
            _serviceName = "病人建档发卡";
        }
        public string idNo { get; set; }
        public string idType { get; set; }
        public string patientId { get; set; }
        public string cardNo { get; set; }
        public string cardType { get; set; }
        public string guardianNo { get; set; }
        public string patientType { get; set; }
        public string name { get; set; }
        public string nation { get; set; }
        public string sex { get; set; }
        public string birthday { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public string tradeMode { get; set; }
        public string tradeAccountNo { get; set; }
        public string pwd { get; set; }
        public string accountNo { get; set; }
        public string posTransNo { get; set; }
        public string bankTansNo { get; set; }
        public string bankDate { get; set; }
        public string bankTime { get; set; }
        public string bankSettlementTime { get; set; }
        public string bankCardNo { get; set; }
        public string deviceInfo { get; set; }
        public string posIndexNo { get; set; }
        public string sellerAccountNo { get; set; }
        public string cash { get; set; }
        public string setupType { get; set; }
        public string guarderIdNo { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(idNo)] = idNo;
            dic[nameof(idType)] = idType;
            dic[nameof(patientId)] = patientId;
            dic[nameof(cardNo)] = cardNo;
            dic[nameof(cardType)] = cardType;
            dic[nameof(guardianNo)] = guardianNo;
            dic[nameof(patientType)] = patientType;
            dic[nameof(name)] = name;
            dic[nameof(nation)] = nation;
            dic[nameof(sex)] = sex;
            dic[nameof(birthday)] = birthday;
            dic[nameof(address)] = address;
            dic[nameof(phone)] = phone;
            dic[nameof(tradeMode)] = tradeMode;
            dic[nameof(tradeAccountNo)] = tradeAccountNo;
            dic[nameof(pwd)] = pwd;
            dic[nameof(accountNo)] = accountNo;
            dic[nameof(posTransNo)] = posTransNo;
            dic[nameof(bankTansNo)] = bankTansNo;
            dic[nameof(bankDate)] = bankDate;
            dic[nameof(bankTime)] = bankTime;
            dic[nameof(bankSettlementTime)] = bankSettlementTime;
            dic[nameof(bankCardNo)] = bankCardNo;
            dic[nameof(deviceInfo)] = deviceInfo;
            dic[nameof(posIndexNo)] = posIndexNo;
            dic[nameof(sellerAccountNo)] = sellerAccountNo;
            dic[nameof(cash)] = cash;
            dic[nameof(setupType)] = setupType;
            dic[nameof(guarderIdNo)] = guarderIdNo;
            return dic;
        }

    }
    
    public class req发卡 : req
    {
        /// <summary>
        /// 发卡
        /// </summary>
        public req发卡()
        {
            service = "yuantu.wap.set.patient.info";
            _serviceName = "发卡";
        }
        public string cardNo { get; set; }
        public string cardType { get; set; }
        public string caseNo { get; set; }
        public string patientid { get; set; }
        public string platformCardNo { get; set; }
        public string platCardId { get; set; }
        public string pwd { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(cardNo)] = cardNo;
            dic[nameof(cardType)] = cardType;
            dic[nameof(caseNo)] = caseNo;
            dic[nameof(patientid)] = patientid;
            dic[nameof(platformCardNo)] = platformCardNo;
            dic[nameof(platCardId)] = platCardId;
            dic[nameof(pwd)] = pwd;
            return dic;
        }

    }
    
    public class req诊疗卡账户修改密码 : req
    {
        /// <summary>
        /// 诊疗卡账户修改密码
        /// </summary>
        public req诊疗卡账户修改密码()
        {
            service = "yuantu.wap.modify.card.pwd";
            _serviceName = "诊疗卡账户修改密码";
        }
        public string patientId { get; set; }
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(oldPassword)] = oldPassword;
            dic[nameof(newPassword)] = newPassword;
            return dic;
        }

    }
    
    public class req诊疗卡密码校验 : req
    {
        /// <summary>
        /// 诊疗卡密码校验
        /// </summary>
        public req诊疗卡密码校验()
        {
            service = "yuantu.wap.validate.card.pwd";
            _serviceName = "诊疗卡密码校验";
        }
        public string patientId { get; set; }
        public string password { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(password)] = password;
            return dic;
        }

    }
    
    public class req自助绑定银行卡 : req
    {
        /// <summary>
        /// 自助绑定银行卡
        /// </summary>
        public req自助绑定银行卡()
        {
            service = "yuantu.wap.query.patient.info";
            _serviceName = "自助绑定银行卡";
        }
        public string patientId { get; set; }
        public string cardNo { get; set; }
        public string name { get; set; }
        public string operatorId { get; set; }
        public string idNo { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(cardNo)] = cardNo;
            dic[nameof(name)] = name;
            dic[nameof(operatorId)] = operatorId;
            dic[nameof(idNo)] = idNo;
            return dic;
        }

    }
    
    public class req自助绑定银行卡解绑 : req
    {
        /// <summary>
        /// 自助绑定银行卡解绑
        /// </summary>
        public req自助绑定银行卡解绑()
        {
            service = "yuantu.wap.query.patient.info";
            _serviceName = "自助绑定银行卡解绑";
        }
        public string patientId { get; set; }
        public string cardNo { get; set; }
        public string idNo { get; set; }
        public string name { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(cardNo)] = cardNo;
            dic[nameof(idNo)] = idNo;
            dic[nameof(name)] = name;
            return dic;
        }

    }
    
    public class req病人基本信息修改 : req
    {
        /// <summary>
        /// 病人基本信息修改
        /// </summary>
        public req病人基本信息修改()
        {
            service = "yuantu.wap.modify.patient.info";
            _serviceName = "病人基本信息修改";
        }
        public string patientId { get; set; }
        public string cardType { get; set; }
        public string cardNo { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }
        public string accountNo { get; set; }
        public string idType { get; set; }
        public string idNo { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(cardType)] = cardType;
            dic[nameof(cardNo)] = cardNo;
            dic[nameof(phone)] = phone;
            dic[nameof(mobile)] = mobile;
            dic[nameof(accountNo)] = accountNo;
            dic[nameof(idType)] = idType;
            dic[nameof(idNo)] = idNo;
            dic[nameof(name)] = name;
            dic[nameof(address)] = address;
            return dic;
        }

    }
    
    public class req就诊情况记录查询 : req
    {
        /// <summary>
        /// 就诊情况记录查询
        /// </summary>
        public req就诊情况记录查询()
        {
            service = "yuantu.wap.query.patient.info";
            _serviceName = "就诊情况记录查询";
        }
        public string patientId { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(startDate)] = startDate;
            dic[nameof(endDate)] = endDate;
            return dic;
        }

    }
    
    public class req就诊满意度 : req
    {
        /// <summary>
        /// 就诊满意度
        /// </summary>
        public req就诊满意度()
        {
            service = "yuantu.wap.query.patient.info";
            _serviceName = "就诊满意度";
        }
        public string doctCode { get; set; }
        public string deptCode { get; set; }
        public string level { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(doctCode)] = doctCode;
            dic[nameof(deptCode)] = deptCode;
            dic[nameof(level)] = level;
            return dic;
        }

    }
    
    public class req获取缴费概要信息 : req
    {
        /// <summary>
        /// 获取缴费概要信息
        /// </summary>
        public req获取缴费概要信息()
        {
            service = "yuantu.wap.query.wait.balance.bill.list";
            _serviceName = "获取缴费概要信息";
        }
        public string patientId { get; set; }
        public string billType { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(billType)] = billType;
            return dic;
        }

    }
    
    public class req获取缴费明细信息 : req
    {
        /// <summary>
        /// 获取缴费明细信息
        /// </summary>
        public req获取缴费明细信息()
        {
            service = "yuantu.wap.query.wait.balance.bill.item.list";
            _serviceName = "获取缴费明细信息";
        }
        public string patientId { get; set; }
        public string billType { get; set; }
        public string billNo { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(billType)] = billType;
            dic[nameof(billNo)] = billNo;
            return dic;
        }

    }
    
    public class req缴费预结算 : req
    {
        /// <summary>
        /// 缴费预结算
        /// </summary>
        public req缴费预结算()
        {
            service = "yuantu.wap.pre.balance.bill";
            _serviceName = "缴费预结算";
        }
        public string patientId { get; set; }
        public string billNo { get; set; }
        public string billItems { get; set; }
        public string cardNo { get; set; }
        public string cardType { get; set; }
        public string gfCardNo { get; set; }
        public string tradeMode { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(billNo)] = billNo;
            dic[nameof(billItems)] = billItems;
            dic[nameof(cardNo)] = cardNo;
            dic[nameof(cardType)] = cardType;
            dic[nameof(gfCardNo)] = gfCardNo;
            dic[nameof(tradeMode)] = tradeMode;
            return dic;
        }

    }
    
    public class req缴费结算 : req
    {
        /// <summary>
        /// 缴费结算
        /// </summary>
        public req缴费结算()
        {
            service = "yuantu.wap.balance.and.pay.bill";
            _serviceName = "缴费结算";
        }
        public string patientId { get; set; }
        public string billNo { get; set; }
        public string posTransNo { get; set; }
        public string bankTansNo { get; set; }
        public string bankDate { get; set; }
        public string bankTime { get; set; }
        public string bankSettlementTime { get; set; }
        public string bankCardNo { get; set; }
        public string deviceInfo { get; set; }
        public string posIndexNo { get; set; }
        public string sellerAccountNo { get; set; }
        public string cash { get; set; }
        public string tradeMode { get; set; }
        public string accountNo { get; set; }
        public string regId { get; set; }
        public string cardNo { get; set; }
        public string cardTansNo { get; set; }
        public string transSeq { get; set; }
        public string terminalNO { get; set; }
        public string operMac { get; set; }
        public string ybInfo { get; set; }
        public string extend { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(billNo)] = billNo;
            dic[nameof(posTransNo)] = posTransNo;
            dic[nameof(bankTansNo)] = bankTansNo;
            dic[nameof(bankDate)] = bankDate;
            dic[nameof(bankTime)] = bankTime;
            dic[nameof(bankSettlementTime)] = bankSettlementTime;
            dic[nameof(bankCardNo)] = bankCardNo;
            dic[nameof(deviceInfo)] = deviceInfo;
            dic[nameof(posIndexNo)] = posIndexNo;
            dic[nameof(sellerAccountNo)] = sellerAccountNo;
            dic[nameof(cash)] = cash;
            dic[nameof(tradeMode)] = tradeMode;
            dic[nameof(accountNo)] = accountNo;
            dic[nameof(regId)] = regId;
            dic[nameof(cardNo)] = cardNo;
            dic[nameof(cardTansNo)] = cardTansNo;
            dic[nameof(transSeq)] = transSeq;
            dic[nameof(terminalNO)] = terminalNO;
            dic[nameof(operMac)] = operMac;
            dic[nameof(ybInfo)] = ybInfo;
            dic[nameof(extend)] = extend;
            return dic;
        }

    }
    
    public class req获取已结算记录 : req
    {
        /// <summary>
        /// 获取已结算记录
        /// </summary>
        public req获取已结算记录()
        {
            service = "yuantu.wap.query.bill.balance.and.pay.record";
            _serviceName = "获取已结算记录";
        }
        public string patientId { get; set; }
        public string beginDate { get; set; }
        public string endDate { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(beginDate)] = beginDate;
            dic[nameof(endDate)] = endDate;
            return dic;
        }

    }
    
    public class req预缴金充值 : req
    {
        /// <summary>
        /// 预缴金充值
        /// </summary>
        public req预缴金充值()
        {
            service = "yuantu.wap.recharge.virtual.settlement";
            _serviceName = "预缴金充值";
        }
        public string patientId { get; set; }
        public string tradeMode { get; set; }
        public string posTransNo { get; set; }
        public string bankTansNo { get; set; }
        public string bankDate { get; set; }
        public string bankTime { get; set; }
        public string bankSettlementTime { get; set; }
        public string bankCardNo { get; set; }
        public string deviceInfo { get; set; }
        public string posIndexNo { get; set; }
        public string sellerAccountNo { get; set; }
        public string cash { get; set; }
        public string tradeTime { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(tradeMode)] = tradeMode;
            dic[nameof(posTransNo)] = posTransNo;
            dic[nameof(bankTansNo)] = bankTansNo;
            dic[nameof(bankDate)] = bankDate;
            dic[nameof(bankTime)] = bankTime;
            dic[nameof(bankSettlementTime)] = bankSettlementTime;
            dic[nameof(bankCardNo)] = bankCardNo;
            dic[nameof(deviceInfo)] = deviceInfo;
            dic[nameof(posIndexNo)] = posIndexNo;
            dic[nameof(sellerAccountNo)] = sellerAccountNo;
            dic[nameof(cash)] = cash;
            dic[nameof(tradeTime)] = tradeTime;
            return dic;
        }

    }
    
    public class req查询预缴金充值记录 : req
    {
        /// <summary>
        /// 查询预缴金充值记录
        /// </summary>
        public req查询预缴金充值记录()
        {
            service = "yuantu.wap.query.patient.vs.record";
            _serviceName = "查询预缴金充值记录";
        }
        public string patientId { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            return dic;
        }

    }
    
    public class req查询预缴金账户余额 : req
    {
        /// <summary>
        /// 查询预缴金账户余额
        /// </summary>
        public req查询预缴金账户余额()
        {
            service = "yuantu.wap.query.virtual.settlement";
            _serviceName = "查询预缴金账户余额";
        }
        public string patientId { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            return dic;
        }

    }
    
    public class req排班科室信息查询 : req
    {
        /// <summary>
        /// 排班科室信息查询
        /// </summary>
        public req排班科室信息查询()
        {
            service = "yuantu.wap.query.registration.dep.list";
            _serviceName = "排班科室信息查询";
        }
        public string regMode { get; set; }
        public string regType { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(regMode)] = regMode;
            dic[nameof(regType)] = regType;
            dic[nameof(startDate)] = startDate;
            dic[nameof(endDate)] = endDate;
            return dic;
        }

    }
    
    public class req排班医生信息查询 : req
    {
        /// <summary>
        /// 排班医生信息查询
        /// </summary>
        public req排班医生信息查询()
        {
            service = "yuantu.wap.query.registration.doc.list";
            _serviceName = "排班医生信息查询";
        }
        public string regMode { get; set; }
        public string regType { get; set; }
        public string deptCode { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(regMode)] = regMode;
            dic[nameof(regType)] = regType;
            dic[nameof(deptCode)] = deptCode;
            return dic;
        }

    }
    
    public class req排班信息查询 : req
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
        public string medDate { get; set; }
        public string medAmPm { get; set; }
        public string deptCode { get; set; }
        public string parentDeptCode { get; set; }
        public string doctCode { get; set; }
        public string sourceCode { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string patientId { get; set; }
        public string gfFlag { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(regMode)] = regMode;
            dic[nameof(regType)] = regType;
            dic[nameof(medDate)] = medDate;
            dic[nameof(medAmPm)] = medAmPm;
            dic[nameof(deptCode)] = deptCode;
            dic[nameof(parentDeptCode)] = parentDeptCode;
            dic[nameof(doctCode)] = doctCode;
            dic[nameof(sourceCode)] = sourceCode;
            dic[nameof(startDate)] = startDate;
            dic[nameof(endDate)] = endDate;
            dic[nameof(patientId)] = patientId;
            dic[nameof(gfFlag)] = gfFlag;
            return dic;
        }

    }
    
    public class req号源明细查询 : req
    {
        /// <summary>
        /// 号源明细查询
        /// </summary>
        public req号源明细查询()
        {
            service = "yuantu.wap.query.registration.sources";
            _serviceName = "号源明细查询";
        }
        public string regMode { get; set; }
        public string medDate { get; set; }
        public string medAmPm { get; set; }
        public string regType { get; set; }
        public string deptCode { get; set; }
        public string secondDeptCode { get; set; }
        public string doctCode { get; set; }
        public string scheduleId { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(regMode)] = regMode;
            dic[nameof(medDate)] = medDate;
            dic[nameof(medAmPm)] = medAmPm;
            dic[nameof(regType)] = regType;
            dic[nameof(deptCode)] = deptCode;
            dic[nameof(secondDeptCode)] = secondDeptCode;
            dic[nameof(doctCode)] = doctCode;
            dic[nameof(scheduleId)] = scheduleId;
            return dic;
        }

    }
    
    public class req锁号 : req
    {
        /// <summary>
        /// 锁号
        /// </summary>
        public req锁号()
        {
            service = "yuantu.wap.lock.registration.source";
            _serviceName = "锁号";
        }
        public string regDate { get; set; }
        public string amPm { get; set; }
        public string regType { get; set; }
        public string deptCode { get; set; }
        public string doctCode { get; set; }
        public string scheduleId { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(regDate)] = regDate;
            dic[nameof(amPm)] = amPm;
            dic[nameof(regType)] = regType;
            dic[nameof(deptCode)] = deptCode;
            dic[nameof(doctCode)] = doctCode;
            dic[nameof(scheduleId)] = scheduleId;
            return dic;
        }

    }
    
    public class req解锁 : req
    {
        /// <summary>
        /// 解锁
        /// </summary>
        public req解锁()
        {
            service = "yuantu.wap.un.lock.registration.source";
            _serviceName = "解锁";
        }
        public string lockid { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(lockid)] = lockid;
            return dic;
        }

    }
    
    public class req当天挂号 : req
    {
        /// <summary>
        /// 当天挂号
        /// </summary>
        public req当天挂号()
        {
            service = "yuantu.wap.appointment.or.registration";
            _serviceName = "当天挂号";
        }
        public string cardNo { get; set; }
        public string patientId { get; set; }
        public string idNo { get; set; }
        public string name { get; set; }
        public string scheduleId { get; set; }
        public string regDate { get; set; }
        public string deptCode { get; set; }
        public string doctCode { get; set; }
        public string amPm { get; set; }
        public string regType { get; set; }
        public string lockid { get; set; }
        public string terminalNo { get; set; }
        public string tradeMode { get; set; }
        public string bankNo { get; set; }
        public string transSeq { get; set; }
        public string cardTansNo { get; set; }
        public string cash { get; set; }
        public string tradeTime { get; set; }
        public string regMode { get; set; }
        public string gfFlag { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(cardNo)] = cardNo;
            dic[nameof(patientId)] = patientId;
            dic[nameof(idNo)] = idNo;
            dic[nameof(name)] = name;
            dic[nameof(scheduleId)] = scheduleId;
            dic[nameof(regDate)] = regDate;
            dic[nameof(deptCode)] = deptCode;
            dic[nameof(doctCode)] = doctCode;
            dic[nameof(amPm)] = amPm;
            dic[nameof(regType)] = regType;
            dic[nameof(lockid)] = lockid;
            dic[nameof(terminalNo)] = terminalNo;
            dic[nameof(tradeMode)] = tradeMode;
            dic[nameof(bankNo)] = bankNo;
            dic[nameof(transSeq)] = transSeq;
            dic[nameof(cardTansNo)] = cardTansNo;
            dic[nameof(cash)] = cash;
            dic[nameof(tradeTime)] = tradeTime;
            dic[nameof(regMode)] = regMode;
            dic[nameof(gfFlag)] = gfFlag;
            return dic;
        }

    }
    
    public class req预约挂号 : req
    {
        /// <summary>
        /// 预约挂号
        /// </summary>
        public req预约挂号()
        {
            service = "yuantu.wap.appointment.or.registration";
            _serviceName = "预约挂号";
        }
        public string patientId { get; set; }
        public string regMode { get; set; }
        public string regType { get; set; }
        public string medDate { get; set; }
        public string medTime { get; set; }
        public string scheduleId { get; set; }
        public string deptCode { get; set; }
        public string doctCode { get; set; }
        public string medAmPm { get; set; }
        public string appoNo { get; set; }
        public string tradeMode { get; set; }
        public string tradeAccountNo { get; set; }
        public string posTransNo { get; set; }
        public string bankTansNo { get; set; }
        public string bankDate { get; set; }
        public string bankTime { get; set; }
        public string bankSettlementTime { get; set; }
        public string bankCardNo { get; set; }
        public string deviceInfo { get; set; }
        public string posIndexNo { get; set; }
        public string sellerAccountNo { get; set; }
        public string cash { get; set; }
        public string lockid { get; set; }
        public string cardNo { get; set; }
        public string transSeq { get; set; }
        public string terminalNo { get; set; }
        public string medBegTime { get; set; }
        public string medEndTime { get; set; }
        public string gfFlag { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(regMode)] = regMode;
            dic[nameof(regType)] = regType;
            dic[nameof(medDate)] = medDate;
            dic[nameof(medTime)] = medTime;
            dic[nameof(scheduleId)] = scheduleId;
            dic[nameof(deptCode)] = deptCode;
            dic[nameof(doctCode)] = doctCode;
            dic[nameof(medAmPm)] = medAmPm;
            dic[nameof(appoNo)] = appoNo;
            dic[nameof(tradeMode)] = tradeMode;
            dic[nameof(tradeAccountNo)] = tradeAccountNo;
            dic[nameof(posTransNo)] = posTransNo;
            dic[nameof(bankTansNo)] = bankTansNo;
            dic[nameof(bankDate)] = bankDate;
            dic[nameof(bankTime)] = bankTime;
            dic[nameof(bankSettlementTime)] = bankSettlementTime;
            dic[nameof(bankCardNo)] = bankCardNo;
            dic[nameof(deviceInfo)] = deviceInfo;
            dic[nameof(posIndexNo)] = posIndexNo;
            dic[nameof(sellerAccountNo)] = sellerAccountNo;
            dic[nameof(cash)] = cash;
            dic[nameof(lockid)] = lockid;
            dic[nameof(cardNo)] = cardNo;
            dic[nameof(transSeq)] = transSeq;
            dic[nameof(terminalNo)] = terminalNo;
            dic[nameof(medBegTime)] = medBegTime;
            dic[nameof(medEndTime)] = medEndTime;
            dic[nameof(gfFlag)] = gfFlag;
            return dic;
        }

    }
    
    public class req预约取号 : req
    {
        /// <summary>
        /// 预约取号
        /// </summary>
        public req预约取号()
        {
            service = "yuantu.wap.take.registration.no";
            _serviceName = "预约取号";
        }
        public string patientId { get; set; }
        public string medDate { get; set; }
        public string scheduleId { get; set; }
        public string medAmPm { get; set; }
        public string posTransNo { get; set; }
        public string bankTansNo { get; set; }
        public string bankDate { get; set; }
        public string bankTime { get; set; }
        public string bankSettlementTime { get; set; }
        public string bankCardNo { get; set; }
        public string deviceInfo { get; set; }
        public string posIndexNo { get; set; }
        public string sellerAccountNo { get; set; }
        public string cash { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(medDate)] = medDate;
            dic[nameof(scheduleId)] = scheduleId;
            dic[nameof(medAmPm)] = medAmPm;
            dic[nameof(posTransNo)] = posTransNo;
            dic[nameof(bankTansNo)] = bankTansNo;
            dic[nameof(bankDate)] = bankDate;
            dic[nameof(bankTime)] = bankTime;
            dic[nameof(bankSettlementTime)] = bankSettlementTime;
            dic[nameof(bankCardNo)] = bankCardNo;
            dic[nameof(deviceInfo)] = deviceInfo;
            dic[nameof(posIndexNo)] = posIndexNo;
            dic[nameof(sellerAccountNo)] = sellerAccountNo;
            dic[nameof(cash)] = cash;
            return dic;
        }

    }
    
    public class req取消预约或挂号 : req
    {
        /// <summary>
        /// 取消预约或挂号
        /// </summary>
        public req取消预约或挂号()
        {
            service = "yuantu.wap.cancel.appointment.or.registration";
            _serviceName = "取消预约或挂号";
        }
        public string patientId { get; set; }
        public string regMode { get; set; }
        public string appoNo { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(regMode)] = regMode;
            dic[nameof(appoNo)] = appoNo;
            return dic;
        }

    }
    
    public class req挂号预约记录查询 : req
    {
        /// <summary>
        /// 挂号预约记录查询
        /// </summary>
        public req挂号预约记录查询()
        {
            service = "yuantu.wap.query.appointment.and.registration.record";
            _serviceName = "挂号预约记录查询";
        }
        public string patientId { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string searchType { get; set; }
        public string status { get; set; }
        public string regMode { get; set; }
        public string idNo { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(startDate)] = startDate;
            dic[nameof(endDate)] = endDate;
            dic[nameof(searchType)] = searchType;
            dic[nameof(status)] = status;
            dic[nameof(regMode)] = regMode;
            dic[nameof(idNo)] = idNo;
            return dic;
        }

    }
    
    public class req取号预结算 : req
    {
        /// <summary>
        /// 取号预结算
        /// </summary>
        public req取号预结算()
        {
            service = "yuantu.wap.query.appointment.and.registration.record";
            _serviceName = "取号预结算";
        }
        public string patientId { get; set; }
        public string orderNo { get; set; }
        public string idNo { get; set; }
        public string searchType { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(orderNo)] = orderNo;
            dic[nameof(idNo)] = idNo;
            dic[nameof(searchType)] = searchType;
            return dic;
        }

    }
    
    public class req取号结算 : req
    {
        /// <summary>
        /// 取号结算
        /// </summary>
        public req取号结算()
        {
            service = "yuantu.wap.take.registration.no";
            _serviceName = "取号结算";
        }
        public string patientId { get; set; }
        public string idNo { get; set; }
        public string orderNo { get; set; }
        public string operateNo { get; set; }
        public string payMethod { get; set; }
        public string bankNo { get; set; }
        public string amount { get; set; }
        public string transSeq { get; set; }
        public string tradeMode { get; set; }
        public string cash { get; set; }
        public string terminalNo { get; set; }
        public string tradeTime { get; set; }
        public string gfFlag { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(idNo)] = idNo;
            dic[nameof(orderNo)] = orderNo;
            dic[nameof(operateNo)] = operateNo;
            dic[nameof(payMethod)] = payMethod;
            dic[nameof(bankNo)] = bankNo;
            dic[nameof(amount)] = amount;
            dic[nameof(transSeq)] = transSeq;
            dic[nameof(tradeMode)] = tradeMode;
            dic[nameof(cash)] = cash;
            dic[nameof(terminalNo)] = terminalNo;
            dic[nameof(tradeTime)] = tradeTime;
            dic[nameof(gfFlag)] = gfFlag;
            return dic;
        }

    }
    
    public class req挂号退号 : req
    {
        /// <summary>
        /// 挂号退号
        /// </summary>
        public req挂号退号()
        {
            service = "yuantu.wap.take.registration.no";
            _serviceName = "挂号退号";
        }
        public string patientId { get; set; }
        public string orderNo { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(orderNo)] = orderNo;
            return dic;
        }

    }
    
    public class req住院患者信息查询 : req
    {
        /// <summary>
        /// 住院患者信息查询
        /// </summary>
        public req住院患者信息查询()
        {
            service = "yuantu.wap.query.inhos.patient.info";
            _serviceName = "住院患者信息查询";
        }
        public string patientId { get; set; }
        public string name { get; set; }
        public string cardNo { get; set; }
        public string cardType { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(name)] = name;
            dic[nameof(cardNo)] = cardNo;
            dic[nameof(cardType)] = cardType;
            return dic;
        }

    }
    
    public class req住院患者费用明细查询 : req
    {
        /// <summary>
        /// 住院患者费用明细查询
        /// </summary>
        public req住院患者费用明细查询()
        {
            service = "yuantu.wap.query.inhos.bill.item.list";
            _serviceName = "住院患者费用明细查询";
        }
        public string patientId { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(startDate)] = startDate;
            dic[nameof(endDate)] = endDate;
            return dic;
        }

    }
    
    public class req住院预缴金充值 : req
    {
        /// <summary>
        /// 住院预缴金充值
        /// </summary>
        public req住院预缴金充值()
        {
            service = "yuantu.wap.recharge.inhos.virtual.settlement";
            _serviceName = "住院预缴金充值";
        }
        public string admissNo { get; set; }
        public string patientId { get; set; }
        public string tradeMode { get; set; }
        public string cardNo { get; set; }
        public string phone { get; set; }
        public string tradeAccountNo { get; set; }
        public string accountNo { get; set; }
        public string deviceInfo { get; set; }
        public string posTransNo { get; set; }
        public string bankTransNo { get; set; }
        public string bankDate { get; set; }
        public string bankTime { get; set; }
        public string bankSettlementTime { get; set; }
        public string bankCardNo { get; set; }
        public string posIndexNo { get; set; }
        public string sellerAccountNo { get; set; }
        public string cash { get; set; }
        public string terminalNo { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(admissNo)] = admissNo;
            dic[nameof(patientId)] = patientId;
            dic[nameof(tradeMode)] = tradeMode;
            dic[nameof(cardNo)] = cardNo;
            dic[nameof(phone)] = phone;
            dic[nameof(tradeAccountNo)] = tradeAccountNo;
            dic[nameof(accountNo)] = accountNo;
            dic[nameof(deviceInfo)] = deviceInfo;
            dic[nameof(posTransNo)] = posTransNo;
            dic[nameof(bankTransNo)] = bankTransNo;
            dic[nameof(bankDate)] = bankDate;
            dic[nameof(bankTime)] = bankTime;
            dic[nameof(bankSettlementTime)] = bankSettlementTime;
            dic[nameof(bankCardNo)] = bankCardNo;
            dic[nameof(posIndexNo)] = posIndexNo;
            dic[nameof(sellerAccountNo)] = sellerAccountNo;
            dic[nameof(cash)] = cash;
            dic[nameof(terminalNo)] = terminalNo;
            return dic;
        }

    }
    
    public class req住院预缴金充值记录查询 : req
    {
        /// <summary>
        /// 住院预缴金充值记录查询
        /// </summary>
        public req住院预缴金充值记录查询()
        {
            service = "yuantu.wap.query.inhos.vs.record";
            _serviceName = "住院预缴金充值记录查询";
        }
        public string patientId { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            return dic;
        }

    }
    
    public class req虚拟账户开通 : req
    {
        /// <summary>
        /// 虚拟账户开通
        /// </summary>
        public req虚拟账户开通()
        {
            service = "yuantu.wap.appointment.or.registration";
            _serviceName = "虚拟账户开通";
        }
        public string patientId { get; set; }
        public string idNo { get; set; }
        public string name { get; set; }
        public string sex { get; set; }
        public string birthday { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            dic[nameof(idNo)] = idNo;
            dic[nameof(name)] = name;
            dic[nameof(sex)] = sex;
            dic[nameof(birthday)] = birthday;
            dic[nameof(phone)] = phone;
            dic[nameof(address)] = address;
            return dic;
        }

    }
    
    public class req检验基本信息查询 : req
    {
        /// <summary>
        /// 检验基本信息查询
        /// </summary>
        public req检验基本信息查询()
        {
            service = "yuantu.wap.query.lis.report";
            _serviceName = "检验基本信息查询";
        }
        public string cardNo { get; set; }
        public string cardType { get; set; }
        public string type { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(cardNo)] = cardNo;
            dic[nameof(cardType)] = cardType;
            dic[nameof(type)] = type;
            dic[nameof(startTime)] = startTime;
            dic[nameof(endTime)] = endTime;
            return dic;
        }

    }
    
    public class req更新打印次数 : req
    {
        /// <summary>
        /// 更新打印次数
        /// </summary>
        public req更新打印次数()
        {
            service = "yuantu.wap.query.lis.print.report";
            _serviceName = "更新打印次数";
        }
        public string cardNo { get; set; }
        public string cardType { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(cardNo)] = cardNo;
            dic[nameof(cardType)] = cardType;
            return dic;
        }

    }
    
    public class req检验结果明细查询 : req
    {
        /// <summary>
        /// 检验结果明细查询
        /// </summary>
        public req检验结果明细查询()
        {
            service = "yuantu.wap.appointment.or.registration";
            _serviceName = "检验结果明细查询";
        }
        public string examId { get; set; }
        public string idNo { get; set; }
        public string name { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(examId)] = examId;
            dic[nameof(idNo)] = idNo;
            dic[nameof(name)] = name;
            return dic;
        }

    }
    
    public class req检查结果查询 : req
    {
        /// <summary>
        /// 检查结果查询
        /// </summary>
        public req检查结果查询()
        {
            service = "yuantu.wap.appointment.or.registration";
            _serviceName = "检查结果查询";
        }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string visitFrom { get; set; }
        public string patientId { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(startDate)] = startDate;
            dic[nameof(endDate)] = endDate;
            dic[nameof(visitFrom)] = visitFrom;
            dic[nameof(patientId)] = patientId;
            return dic;
        }

    }
    
    public class req医生信息查询 : req
    {
        /// <summary>
        /// 医生信息查询
        /// </summary>
        public req医生信息查询()
        {
            service = "yuantu.wap.appointment.or.registration";
            _serviceName = "医生信息查询";
        }
        public string doctCode { get; set; }
        public string deptCode { get; set; }
        public string doctName { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(doctCode)] = doctCode;
            dic[nameof(deptCode)] = deptCode;
            dic[nameof(doctName)] = doctName;
            return dic;
        }

    }
    
    public class req科室信息查询 : req
    {
        /// <summary>
        /// 科室信息查询
        /// </summary>
        public req科室信息查询()
        {
            service = "yuantu.wap.appointment.or.registration";
            _serviceName = "科室信息查询";
        }
        public string deptType { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(deptType)] = deptType;
            return dic;
        }

    }
    
    public class req民生卡开卡 : req
    {
        /// <summary>
        /// 民生卡开卡
        /// </summary>
        public req民生卡开卡()
        {
            service = "yuantu.wap.send.out.patient.card";
            _serviceName = "民生卡开卡";
        }
        public string TxnCode { get; set; }
        public string TxnChnl { get; set; }
        public string MerchantId { get; set; }
        public string TellerNo { get; set; }
        public string TerminalNO { get; set; }
        public string TxnDate { get; set; }
        public string TxnTime { get; set; }
        public string ChnlSsn { get; set; }
        public string Mac { get; set; }
        public string CardNo { get; set; }
        public string CustName { get; set; }
        public string Sex { get; set; }
        public string Nation { get; set; }
        public string CertType { get; set; }
        public string CertNum { get; set; }
        public string ParentName { get; set; }
        public string BirthDay { get; set; }
        public string PhoneNum { get; set; }
        public string Adrr { get; set; }
        public string PassWord { get; set; }
        public string Amt { get; set; }
        public string CardUid { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(TxnCode)] = TxnCode;
            dic[nameof(TxnChnl)] = TxnChnl;
            dic[nameof(MerchantId)] = MerchantId;
            dic[nameof(TellerNo)] = TellerNo;
            dic[nameof(TerminalNO)] = TerminalNO;
            dic[nameof(TxnDate)] = TxnDate;
            dic[nameof(TxnTime)] = TxnTime;
            dic[nameof(ChnlSsn)] = ChnlSsn;
            dic[nameof(Mac)] = Mac;
            dic[nameof(CardNo)] = CardNo;
            dic[nameof(CustName)] = CustName;
            dic[nameof(Sex)] = Sex;
            dic[nameof(Nation)] = Nation;
            dic[nameof(CertType)] = CertType;
            dic[nameof(CertNum)] = CertNum;
            dic[nameof(ParentName)] = ParentName;
            dic[nameof(BirthDay)] = BirthDay;
            dic[nameof(PhoneNum)] = PhoneNum;
            dic[nameof(Adrr)] = Adrr;
            dic[nameof(PassWord)] = PassWord;
            dic[nameof(Amt)] = Amt;
            dic[nameof(CardUid)] = CardUid;
            return dic;
        }

    }
    
    public class req民生卡终端签到 : req
    {
        /// <summary>
        /// 民生卡终端签到
        /// </summary>
        public req民生卡终端签到()
        {
            service = "yuantu.wap.zjj.sign";
            _serviceName = "民生卡终端签到";
        }
        public string TxnCode { get; set; }
        public string TxnChnl { get; set; }
        public string MerchantId { get; set; }
        public string TellerNo { get; set; }
        public string TerminalNO { get; set; }
        public string TxnDate { get; set; }
        public string TxnTime { get; set; }
        public string ChnlSsn { get; set; }
        public string Mac { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(TxnCode)] = TxnCode;
            dic[nameof(TxnChnl)] = TxnChnl;
            dic[nameof(MerchantId)] = MerchantId;
            dic[nameof(TellerNo)] = TellerNo;
            dic[nameof(TerminalNO)] = TerminalNO;
            dic[nameof(TxnDate)] = TxnDate;
            dic[nameof(TxnTime)] = TxnTime;
            dic[nameof(ChnlSsn)] = ChnlSsn;
            dic[nameof(Mac)] = Mac;
            return dic;
        }

    }
    
    public class req民生卡余额查询 : req
    {
        /// <summary>
        /// 民生卡余额查询
        /// </summary>
        public req民生卡余额查询()
        {
            service = "yuantu.wap.query.patient.info.vs.cash";
            _serviceName = "民生卡余额查询";
        }
        public string TxnCode { get; set; }
        public string TxnChnl { get; set; }
        public string MerchantId { get; set; }
        public string TellerNo { get; set; }
        public string TerminalNO { get; set; }
        public string TxnDate { get; set; }
        public string TxnTime { get; set; }
        public string ChnlSsn { get; set; }
        public string Mac { get; set; }
        public string CardNo { get; set; }
        public string PassWord { get; set; }
        public string CardSeq { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(TxnCode)] = TxnCode;
            dic[nameof(TxnChnl)] = TxnChnl;
            dic[nameof(MerchantId)] = MerchantId;
            dic[nameof(TellerNo)] = TellerNo;
            dic[nameof(TerminalNO)] = TerminalNO;
            dic[nameof(TxnDate)] = TxnDate;
            dic[nameof(TxnTime)] = TxnTime;
            dic[nameof(ChnlSsn)] = ChnlSsn;
            dic[nameof(Mac)] = Mac;
            dic[nameof(CardNo)] = CardNo;
            dic[nameof(PassWord)] = PassWord;
            dic[nameof(CardSeq)] = CardSeq;
            return dic;
        }

    }
    
    public class req民生卡交易明细查询 : req
    {
        /// <summary>
        /// 民生卡交易明细查询
        /// </summary>
        public req民生卡交易明细查询()
        {
            service = "yuantu.wap.query.patient.vs.record";
            _serviceName = "民生卡交易明细查询";
        }
        public string TxnCode { get; set; }
        public string TxnChnl { get; set; }
        public string MerchantId { get; set; }
        public string TellerNo { get; set; }
        public string TerminalNO { get; set; }
        public string TxnDate { get; set; }
        public string TxnTime { get; set; }
        public string ChnlSsn { get; set; }
        public string Mac { get; set; }
        public string CardNo { get; set; }
        public string PassWord { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string PageNum { get; set; }
        public string TxnNum { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(TxnCode)] = TxnCode;
            dic[nameof(TxnChnl)] = TxnChnl;
            dic[nameof(MerchantId)] = MerchantId;
            dic[nameof(TellerNo)] = TellerNo;
            dic[nameof(TerminalNO)] = TerminalNO;
            dic[nameof(TxnDate)] = TxnDate;
            dic[nameof(TxnTime)] = TxnTime;
            dic[nameof(ChnlSsn)] = ChnlSsn;
            dic[nameof(Mac)] = Mac;
            dic[nameof(CardNo)] = CardNo;
            dic[nameof(PassWord)] = PassWord;
            dic[nameof(StartDate)] = StartDate;
            dic[nameof(EndDate)] = EndDate;
            dic[nameof(PageNum)] = PageNum;
            dic[nameof(TxnNum)] = TxnNum;
            return dic;
        }

    }
    
    public class req民生卡充值 : req
    {
        /// <summary>
        /// 民生卡充值
        /// </summary>
        public req民生卡充值()
        {
            service = "yuantu.wap.recharge.virtual.settlement";
            _serviceName = "民生卡充值";
        }
        public string TxnCode { get; set; }
        public string TxnChnl { get; set; }
        public string MerchantId { get; set; }
        public string TellerNo { get; set; }
        public string TerminalNO { get; set; }
        public string TxnDate { get; set; }
        public string TxnTime { get; set; }
        public string ChnlSsn { get; set; }
        public string Mac { get; set; }
        public string CardNo { get; set; }
        public string TxnAmt { get; set; }
        public string ChargeType { get; set; }
        public string CupDate { get; set; }
        public string CupSsn { get; set; }
        public string sBankCardNo { get; set; }
        public string CardSeq { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(TxnCode)] = TxnCode;
            dic[nameof(TxnChnl)] = TxnChnl;
            dic[nameof(MerchantId)] = MerchantId;
            dic[nameof(TellerNo)] = TellerNo;
            dic[nameof(TerminalNO)] = TerminalNO;
            dic[nameof(TxnDate)] = TxnDate;
            dic[nameof(TxnTime)] = TxnTime;
            dic[nameof(ChnlSsn)] = ChnlSsn;
            dic[nameof(Mac)] = Mac;
            dic[nameof(CardNo)] = CardNo;
            dic[nameof(TxnAmt)] = TxnAmt;
            dic[nameof(ChargeType)] = ChargeType;
            dic[nameof(CupDate)] = CupDate;
            dic[nameof(CupSsn)] = CupSsn;
            dic[nameof(sBankCardNo)] = sBankCardNo;
            dic[nameof(CardSeq)] = CardSeq;
            return dic;
        }

    }
    
    public class req民生卡充值冲正 : req
    {
        /// <summary>
        /// 民生卡充值冲正
        /// </summary>
        public req民生卡充值冲正()
        {
            service = "yuantu.wap.appointment.or.registration";
            _serviceName = "民生卡充值冲正";
        }
        public string TxnCode { get; set; }
        public string TxnChnl { get; set; }
        public string MerchantId { get; set; }
        public string TellerNo { get; set; }
        public string TerminalNO { get; set; }
        public string TxnDate { get; set; }
        public string TxnTime { get; set; }
        public string ChnlSsn { get; set; }
        public string Mac { get; set; }
        public string CardNo { get; set; }
        public string TxnAmt { get; set; }
        public string OriChnlSsn { get; set; }
        public string OriTxnDate { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(TxnCode)] = TxnCode;
            dic[nameof(TxnChnl)] = TxnChnl;
            dic[nameof(MerchantId)] = MerchantId;
            dic[nameof(TellerNo)] = TellerNo;
            dic[nameof(TerminalNO)] = TerminalNO;
            dic[nameof(TxnDate)] = TxnDate;
            dic[nameof(TxnTime)] = TxnTime;
            dic[nameof(ChnlSsn)] = ChnlSsn;
            dic[nameof(Mac)] = Mac;
            dic[nameof(CardNo)] = CardNo;
            dic[nameof(TxnAmt)] = TxnAmt;
            dic[nameof(OriChnlSsn)] = OriChnlSsn;
            dic[nameof(OriTxnDate)] = OriTxnDate;
            return dic;
        }

    }
    
    public class req民生卡消费 : req
    {
        /// <summary>
        /// 民生卡消费
        /// </summary>
        public req民生卡消费()
        {
            service = "yuantu.wap.consume.vs.cash";
            _serviceName = "民生卡消费";
        }
        public string TxnCode { get; set; }
        public string TxnChnl { get; set; }
        public string MerchantId { get; set; }
        public string TellerNo { get; set; }
        public string TerminalNO { get; set; }
        public string TxnDate { get; set; }
        public string TxnTime { get; set; }
        public string ChnlSsn { get; set; }
        public string Mac { get; set; }
        public string CardNo { get; set; }
        public string TxnAmt { get; set; }
        public string PassWord { get; set; }
        public string PayFlag { get; set; }
        public string PlanAmt { get; set; }
        public string CardSeq { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(TxnCode)] = TxnCode;
            dic[nameof(TxnChnl)] = TxnChnl;
            dic[nameof(MerchantId)] = MerchantId;
            dic[nameof(TellerNo)] = TellerNo;
            dic[nameof(TerminalNO)] = TerminalNO;
            dic[nameof(TxnDate)] = TxnDate;
            dic[nameof(TxnTime)] = TxnTime;
            dic[nameof(ChnlSsn)] = ChnlSsn;
            dic[nameof(Mac)] = Mac;
            dic[nameof(CardNo)] = CardNo;
            dic[nameof(TxnAmt)] = TxnAmt;
            dic[nameof(PassWord)] = PassWord;
            dic[nameof(PayFlag)] = PayFlag;
            dic[nameof(PlanAmt)] = PlanAmt;
            dic[nameof(CardSeq)] = CardSeq;
            return dic;
        }

    }
    
    public class req民生卡消费冲正 : req
    {
        /// <summary>
        /// 民生卡消费冲正
        /// </summary>
        public req民生卡消费冲正()
        {
            service = "yuantu.wap.consume.flushes.vs.cash";
            _serviceName = "民生卡消费冲正";
        }
        public string TxnCode { get; set; }
        public string TxnChnl { get; set; }
        public string MerchantId { get; set; }
        public string TellerNo { get; set; }
        public string TerminalNO { get; set; }
        public string TxnDate { get; set; }
        public string TxnTime { get; set; }
        public string ChnlSsn { get; set; }
        public string Mac { get; set; }
        public string CardNo { get; set; }
        public string TxnAmt { get; set; }
        public string OriChnlSsn { get; set; }
        public string OriTxnDate { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(TxnCode)] = TxnCode;
            dic[nameof(TxnChnl)] = TxnChnl;
            dic[nameof(MerchantId)] = MerchantId;
            dic[nameof(TellerNo)] = TellerNo;
            dic[nameof(TerminalNO)] = TerminalNO;
            dic[nameof(TxnDate)] = TxnDate;
            dic[nameof(TxnTime)] = TxnTime;
            dic[nameof(ChnlSsn)] = ChnlSsn;
            dic[nameof(Mac)] = Mac;
            dic[nameof(CardNo)] = CardNo;
            dic[nameof(TxnAmt)] = TxnAmt;
            dic[nameof(OriChnlSsn)] = OriChnlSsn;
            dic[nameof(OriTxnDate)] = OriTxnDate;
            return dic;
        }

    }
    
    public class req银联卡消费登记 : req
    {
        /// <summary>
        /// 银联卡消费登记
        /// </summary>
        public req银联卡消费登记()
        {
            service = "yuantu.wap.consume.register";
            _serviceName = "银联卡消费登记";
        }
        public string TxnCode { get; set; }
        public string TxnChnl { get; set; }
        public string MerchantId { get; set; }
        public string TellerNo { get; set; }
        public string TerminalNO { get; set; }
        public string TxnDate { get; set; }
        public string TxnTime { get; set; }
        public string ChnlSsn { get; set; }
        public string Mac { get; set; }
        public string sBankCardNo { get; set; }
        public string TxnAmt { get; set; }
        public string CupSsn { get; set; }
        public string CupDate { get; set; }
        public string CardNo { get; set; }
        public string PlanAmt { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(TxnCode)] = TxnCode;
            dic[nameof(TxnChnl)] = TxnChnl;
            dic[nameof(MerchantId)] = MerchantId;
            dic[nameof(TellerNo)] = TellerNo;
            dic[nameof(TerminalNO)] = TerminalNO;
            dic[nameof(TxnDate)] = TxnDate;
            dic[nameof(TxnTime)] = TxnTime;
            dic[nameof(ChnlSsn)] = ChnlSsn;
            dic[nameof(Mac)] = Mac;
            dic[nameof(sBankCardNo)] = sBankCardNo;
            dic[nameof(TxnAmt)] = TxnAmt;
            dic[nameof(CupSsn)] = CupSsn;
            dic[nameof(CupDate)] = CupDate;
            dic[nameof(CardNo)] = CardNo;
            dic[nameof(PlanAmt)] = PlanAmt;
            return dic;
        }

    }
    
    public class req民生卡退费 : req
    {
        /// <summary>
        /// 民生卡退费
        /// </summary>
        public req民生卡退费()
        {
            service = "yuantu.wap.back.vs.cash";
            _serviceName = "民生卡退费";
        }
        public string TxnCode { get; set; }
        public string TxnChnl { get; set; }
        public string MerchantId { get; set; }
        public string TellerNo { get; set; }
        public string TerminalNO { get; set; }
        public string TxnDate { get; set; }
        public string TxnTime { get; set; }
        public string ChnlSsn { get; set; }
        public string Mac { get; set; }
        public string OriChnlSsn { get; set; }
        public string OriTxnDate { get; set; }
        public string CardNo { get; set; }
        public string RetAmt { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(TxnCode)] = TxnCode;
            dic[nameof(TxnChnl)] = TxnChnl;
            dic[nameof(MerchantId)] = MerchantId;
            dic[nameof(TellerNo)] = TellerNo;
            dic[nameof(TerminalNO)] = TerminalNO;
            dic[nameof(TxnDate)] = TxnDate;
            dic[nameof(TxnTime)] = TxnTime;
            dic[nameof(ChnlSsn)] = ChnlSsn;
            dic[nameof(Mac)] = Mac;
            dic[nameof(OriChnlSsn)] = OriChnlSsn;
            dic[nameof(OriTxnDate)] = OriTxnDate;
            dic[nameof(CardNo)] = CardNo;
            dic[nameof(RetAmt)] = RetAmt;
            return dic;
        }

    }
    
    public class req民生卡工本费 : req
    {
        /// <summary>
        /// 民生卡工本费
        /// </summary>
        public req民生卡工本费()
        {
            service = "yuantu.wap.calc.send.card.cost.fee";
            _serviceName = "民生卡工本费";
        }
        public string TxnCode { get; set; }
        public string TxnChnl { get; set; }
        public string MerchantId { get; set; }
        public string TellerNo { get; set; }
        public string TerminalNO { get; set; }
        public string TxnDate { get; set; }
        public string TxnTime { get; set; }
        public string ChnlSsn { get; set; }
        public string Mac { get; set; }
        public string CertType { get; set; }
        public string CertNum { get; set; }
        public string CustName { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(TxnCode)] = TxnCode;
            dic[nameof(TxnChnl)] = TxnChnl;
            dic[nameof(MerchantId)] = MerchantId;
            dic[nameof(TellerNo)] = TellerNo;
            dic[nameof(TerminalNO)] = TerminalNO;
            dic[nameof(TxnDate)] = TxnDate;
            dic[nameof(TxnTime)] = TxnTime;
            dic[nameof(ChnlSsn)] = ChnlSsn;
            dic[nameof(Mac)] = Mac;
            dic[nameof(CertType)] = CertType;
            dic[nameof(CertNum)] = CertNum;
            dic[nameof(CustName)] = CustName;
            return dic;
        }

    }
    
    public class req民生卡客户信息更新 : req
    {
        /// <summary>
        /// 民生卡客户信息更新
        /// </summary>
        public req民生卡客户信息更新()
        {
            service = "yuantu.wap.modify.card.info";
            _serviceName = "民生卡客户信息更新";
        }
        public string TxnCode { get; set; }
        public string TxnChnl { get; set; }
        public string MerchantId { get; set; }
        public string TellerNo { get; set; }
        public string TerminalNO { get; set; }
        public string TxnDate { get; set; }
        public string TxnTime { get; set; }
        public string ChnlSsn { get; set; }
        public string Mac { get; set; }
        public string CardNo { get; set; }
        public string CustName { get; set; }
        public string CertType { get; set; }
        public string CertNum { get; set; }
        public string BirthDay { get; set; }
        public string PhoneNum { get; set; }
        public string Adrr { get; set; }
        public string CardUid { get; set; }
        public string CardSeq { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(TxnCode)] = TxnCode;
            dic[nameof(TxnChnl)] = TxnChnl;
            dic[nameof(MerchantId)] = MerchantId;
            dic[nameof(TellerNo)] = TellerNo;
            dic[nameof(TerminalNO)] = TerminalNO;
            dic[nameof(TxnDate)] = TxnDate;
            dic[nameof(TxnTime)] = TxnTime;
            dic[nameof(ChnlSsn)] = ChnlSsn;
            dic[nameof(Mac)] = Mac;
            dic[nameof(CardNo)] = CardNo;
            dic[nameof(CustName)] = CustName;
            dic[nameof(CertType)] = CertType;
            dic[nameof(CertNum)] = CertNum;
            dic[nameof(BirthDay)] = BirthDay;
            dic[nameof(PhoneNum)] = PhoneNum;
            dic[nameof(Adrr)] = Adrr;
            dic[nameof(CardUid)] = CardUid;
            dic[nameof(CardSeq)] = CardSeq;
            return dic;
        }

    }
    
    public class req民生卡重置密码 : req
    {
        /// <summary>
        /// 民生卡重置密码
        /// </summary>
        public req民生卡重置密码()
        {
            service = "yuantu.wap.modify.card.pwd";
            _serviceName = "民生卡重置密码";
        }
        public string TxnCode { get; set; }
        public string TxnChnl { get; set; }
        public string MerchantId { get; set; }
        public string TellerNo { get; set; }
        public string TerminalNO { get; set; }
        public string TxnDate { get; set; }
        public string TxnTime { get; set; }
        public string ChnlSsn { get; set; }
        public string Mac { get; set; }
        public string CardNo { get; set; }
        public string NewPassWord { get; set; }
        public string CardSeq { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(TxnCode)] = TxnCode;
            dic[nameof(TxnChnl)] = TxnChnl;
            dic[nameof(MerchantId)] = MerchantId;
            dic[nameof(TellerNo)] = TellerNo;
            dic[nameof(TerminalNO)] = TerminalNO;
            dic[nameof(TxnDate)] = TxnDate;
            dic[nameof(TxnTime)] = TxnTime;
            dic[nameof(ChnlSsn)] = ChnlSsn;
            dic[nameof(Mac)] = Mac;
            dic[nameof(CardNo)] = CardNo;
            dic[nameof(NewPassWord)] = NewPassWord;
            dic[nameof(CardSeq)] = CardSeq;
            return dic;
        }

    }
    
    public class req民生卡卡片信息查询 : req
    {
        /// <summary>
        /// 民生卡卡片信息查询
        /// </summary>
        public req民生卡卡片信息查询()
        {
            service = "yuantu.wap.query.card.info";
            _serviceName = "民生卡卡片信息查询";
        }
        public string TxnCode { get; set; }
        public string TxnChnl { get; set; }
        public string MerchantId { get; set; }
        public string TellerNo { get; set; }
        public string TerminalNO { get; set; }
        public string TxnDate { get; set; }
        public string TxnTime { get; set; }
        public string ChnlSsn { get; set; }
        public string Mac { get; set; }
        public string CardNo { get; set; }
        public string CardSeq { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(TxnCode)] = TxnCode;
            dic[nameof(TxnChnl)] = TxnChnl;
            dic[nameof(MerchantId)] = MerchantId;
            dic[nameof(TellerNo)] = TellerNo;
            dic[nameof(TerminalNO)] = TerminalNO;
            dic[nameof(TxnDate)] = TxnDate;
            dic[nameof(TxnTime)] = TxnTime;
            dic[nameof(ChnlSsn)] = ChnlSsn;
            dic[nameof(Mac)] = Mac;
            dic[nameof(CardNo)] = CardNo;
            dic[nameof(CardSeq)] = CardSeq;
            return dic;
        }

    }
    
    public class req民生卡密码修改 : req
    {
        /// <summary>
        /// 民生卡密码修改
        /// </summary>
        public req民生卡密码修改()
        {
            service = "yuantu.wap.modify.card.pwd";
            _serviceName = "民生卡密码修改";
        }
        public string TxnCode { get; set; }
        public string TxnChnl { get; set; }
        public string MerchantId { get; set; }
        public string TellerNo { get; set; }
        public string TerminalNO { get; set; }
        public string TxnDate { get; set; }
        public string TxnTime { get; set; }
        public string ChnlSsn { get; set; }
        public string Mac { get; set; }
        public string CardNo { get; set; }
        public string PassWord { get; set; }
        public string NewPassWord { get; set; }
        public string CardSeq { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(TxnCode)] = TxnCode;
            dic[nameof(TxnChnl)] = TxnChnl;
            dic[nameof(MerchantId)] = MerchantId;
            dic[nameof(TellerNo)] = TellerNo;
            dic[nameof(TerminalNO)] = TerminalNO;
            dic[nameof(TxnDate)] = TxnDate;
            dic[nameof(TxnTime)] = TxnTime;
            dic[nameof(ChnlSsn)] = ChnlSsn;
            dic[nameof(Mac)] = Mac;
            dic[nameof(CardNo)] = CardNo;
            dic[nameof(PassWord)] = PassWord;
            dic[nameof(NewPassWord)] = NewPassWord;
            dic[nameof(CardSeq)] = CardSeq;
            return dic;
        }

    }
    
    public class req民生卡CPU卡密码设置 : req
    {
        /// <summary>
        /// 民生卡CPU卡密码设置
        /// </summary>
        public req民生卡CPU卡密码设置()
        {
            service = "yuantu.wap.modify.cpu.card.pwd";
            _serviceName = "民生卡CPU卡密码设置";
        }
        public string TxnCode { get; set; }
        public string TxnChnl { get; set; }
        public string MerchantId { get; set; }
        public string TellerNo { get; set; }
        public string TerminalNO { get; set; }
        public string TxnDate { get; set; }
        public string TxnTime { get; set; }
        public string ChnlSsn { get; set; }
        public string Mac { get; set; }
        public string CardNo { get; set; }
        public string NewPassWord { get; set; }
        public string CardSeq { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(TxnCode)] = TxnCode;
            dic[nameof(TxnChnl)] = TxnChnl;
            dic[nameof(MerchantId)] = MerchantId;
            dic[nameof(TellerNo)] = TellerNo;
            dic[nameof(TerminalNO)] = TerminalNO;
            dic[nameof(TxnDate)] = TxnDate;
            dic[nameof(TxnTime)] = TxnTime;
            dic[nameof(ChnlSsn)] = ChnlSsn;
            dic[nameof(Mac)] = Mac;
            dic[nameof(CardNo)] = CardNo;
            dic[nameof(NewPassWord)] = NewPassWord;
            dic[nameof(CardSeq)] = CardSeq;
            return dic;
        }

    }
    
    public class req医保信息查询 : req
    {
        /// <summary>
        /// 医保信息查询
        /// </summary>
        public req医保信息查询()
        {
            service = "yuantu.wap.query.yb.med.type";
            _serviceName = "医保信息查询";
        }
        public string productCode  { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(productCode )] = productCode ;
            return dic;
        }

    }
    
    public class req医生信息快速查询 : req
    {
        /// <summary>
        /// 医生信息快速查询
        /// </summary>
        public req医生信息快速查询()
        {
            service = "yuantu.wap.query.registration.doc.list.by.name";
            _serviceName = "医生信息快速查询";
        }
        public string regType { get; set; }
        public string nameSm { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(regType)] = regType;
            dic[nameof(nameSm)] = nameSm;
            return dic;
        }

    }
    
    public class req系统签到 : req
    {
        /// <summary>
        /// 系统签到
        /// </summary>
        public req系统签到()
        {
            service = "yuantu.wap.monitor.service";
            _serviceName = "系统签到";
        }

        /// <summary>
        /// 调用方法名
        /// </summary>
        public string method { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string deviceNo { get; set; }

        /// <summary>
        /// 设备IP
        /// </summary>
        public string deviceIp { get; set; }

        /// <summary>
        /// 设备物理地址
        /// </summary>
        public string deviceMac { get; set; }

        /// <summary>
        /// 设备型号
        /// </summary>
        public string deviceType { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(method)] = method;
            dic[nameof(deviceNo)] = deviceNo;
            dic[nameof(deviceIp)] = deviceIp;
            dic[nameof(deviceMac)] = deviceMac;
            dic[nameof(deviceType)] = deviceType;
            return dic;
        }

    }
    
    public class req信息上报 : req
    {
        /// <summary>
        /// 信息上报
        /// </summary>
        public req信息上报()
        {
            service = "yuantu.wap.monitor.service";
            _serviceName = "信息上报";
        }

        /// <summary>
        /// 调用方法名
        /// </summary>
        public string method { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string deviceNo { get; set; }

        /// <summary>
        /// 异常编码
        /// </summary>
        public string errorCode { get; set; }

        /// <summary>
        /// 异常内容
        /// </summary>
        public string errorMsg { get; set; }

        /// <summary>
        /// 异常级别
        /// </summary>
        public string errorLevel { get; set; }

        /// <summary>
        /// 异常详情
        /// </summary>
        public string errorDetail { get; set; }

        /// <summary>
        /// 异常解决方案
        /// </summary>
        public string errorSolution { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(method)] = method;
            dic[nameof(deviceNo)] = deviceNo;
            dic[nameof(errorCode)] = errorCode;
            dic[nameof(errorMsg)] = errorMsg;
            dic[nameof(errorLevel)] = errorLevel;
            dic[nameof(errorDetail)] = errorDetail;
            dic[nameof(errorSolution)] = errorSolution;
            return dic;
        }

    }
    
    public class req创建扫码订单 : req
    {
        /// <summary>
        /// 创建扫码订单
        /// </summary>
        public req创建扫码订单()
        {
            service = "yuantu.wap.sao.ma.create.order";
            _serviceName = "创建扫码订单";
        }

        /// <summary>
        /// 身份号
        /// </summary>
        public string idNo { get; set; }

        /// <summary>
        /// 证件类型
        /// </summary>
        public string idType { get; set; }

        /// <summary>
        /// 病人姓名
        /// </summary>
        public string patientName { get; set; }

        /// <summary>
        /// 监护人身份证号
        /// </summary>
        public string guarderId { get; set; }

        /// <summary>
        /// 医院用户门诊Id
        /// </summary>
        public string patientId { get; set; }

        /// <summary>
        /// 外键业务关联Id(去重选项)
        /// </summary>
        public string outId { get; set; }

        /// <summary>
        /// 缴费单编号(可空)
        /// </summary>
        public string billNo { get; set; }

        /// <summary>
        /// 金额(单位：分)
        /// </summary>
        public string fee { get; set; }

        /// <summary>
        /// 1:充值 2:缴费 3:挂号
        /// </summary>
        public string optType { get; set; }

        /// <summary>
        /// 业务描述
        /// </summary>
        public string subject { get; set; }

        /// <summary>
        /// 支付渠道，1:支付宝 2:微信
        /// </summary>
        public string feeChannel { get; set; }

        /// <summary>
        /// 来源
        /// </summary>
        public string sourceCode { get; set; }

        /// <summary>
        /// 扩展信息，存放额外可选数据
        /// </summary>
        public string extendBalanceInfo { get; set; }
        public string deviceInfo { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(idNo)] = idNo;
            dic[nameof(idType)] = idType;
            dic[nameof(patientName)] = patientName;
            dic[nameof(guarderId)] = guarderId;
            dic[nameof(patientId)] = patientId;
            dic[nameof(outId)] = outId;
            dic[nameof(billNo)] = billNo;
            dic[nameof(fee)] = fee;
            dic[nameof(optType)] = optType;
            dic[nameof(subject)] = subject;
            dic[nameof(feeChannel)] = feeChannel;
            dic[nameof(sourceCode)] = sourceCode;
            dic[nameof(extendBalanceInfo)] = extendBalanceInfo;
            dic[nameof(deviceInfo)] = deviceInfo;
            return dic;
        }

    }
    
    public class req取消扫码订单 : req
    {
        /// <summary>
        /// 取消扫码订单
        /// </summary>
        public req取消扫码订单()
        {
            service = "yuantu.wap.sao.ma.cancel.order";
            _serviceName = "取消扫码订单";
        }

        /// <summary>
        /// 用户平台订单号
        /// </summary>
        public string outTradeNo { get; set; }
        public string deviceInfo { get; set; }
        public string sourceCode { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(outTradeNo)] = outTradeNo;
            dic[nameof(deviceInfo)] = deviceInfo;
            dic[nameof(sourceCode)] = sourceCode;
            return dic;
        }

    }
    
    public class req查询订单状态 : req
    {
        /// <summary>
        /// 查询订单状态
        /// </summary>
        public req查询订单状态()
        {
            service = "yuantu.wap.sao.ma.query.order";
            _serviceName = "查询订单状态";
        }

        /// <summary>
        /// 用户平台订单号
        /// </summary>
        public string outTradeNo { get; set; }
        public string deviceInfo { get; set; }
        public string sourceCode { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(outTradeNo)] = outTradeNo;
            dic[nameof(deviceInfo)] = deviceInfo;
            dic[nameof(sourceCode)] = sourceCode;
            return dic;
        }

    }
    
    public class req支付宝支付成功上报 : req
    {
        /// <summary>
        /// 支付宝支付成功上报
        /// </summary>
        public req支付宝支付成功上报()
        {
            service = "yuantu.wap.sao.ma.success.order";
            _serviceName = "支付宝支付成功上报";
        }

        /// <summary>
        /// 用户平台订单号
        /// </summary>
        public string outTradeNo { get; set; }
        public string deviceInfo { get; set; }
        public string sourceCode { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(outTradeNo)] = outTradeNo;
            dic[nameof(deviceInfo)] = deviceInfo;
            dic[nameof(sourceCode)] = sourceCode;
            return dic;
        }

    }
    
    public class req查询将要接种的针次 : req
    {
        /// <summary>
        /// 查询将要接种的针次
        /// </summary>
        public req查询将要接种的针次()
        {
            service = "yuantu.wap.vaccine.query.next.schedule";
            _serviceName = "查询将要接种的针次";
        }
        public string cardType { get; set; }
        public string cardNo { get; set; }
        public string patientName { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(cardType)] = cardType;
            dic[nameof(cardNo)] = cardNo;
            dic[nameof(patientName)] = patientName;
            return dic;
        }

    }
    
    public class req查询将要接种的清单 : req
    {
        /// <summary>
        /// 查询将要接种的清单
        /// </summary>
        public req查询将要接种的清单()
        {
            service = "yuantu.wap.vaccine.query.schedule.list";
            _serviceName = "查询将要接种的清单";
        }
        public string cardType { get; set; }
        public string cardNo { get; set; }
        public string patientName { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(cardType)] = cardType;
            dic[nameof(cardNo)] = cardNo;
            dic[nameof(patientName)] = patientName;
            return dic;
        }

    }
    
    public class req查询公费人员信息 : req
    {
        /// <summary>
        /// 查询公费人员信息
        /// </summary>
        public req查询公费人员信息()
        {
            service = "yuantu.wap.gf.query.patientinfo";
            _serviceName = "查询公费人员信息";
        }
        public string cardType { get; set; }
        public string cardNo { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(cardType)] = cardType;
            dic[nameof(cardNo)] = cardNo;
            return dic;
        }

    }
    
    public class req验证公费人员信息 : req
    {
        /// <summary>
        /// 验证公费人员信息
        /// </summary>
        public req验证公费人员信息()
        {
            service = "yuantu.wap.gf.checkin";
            _serviceName = "验证公费人员信息";
        }
        public string cardType { get; set; }
        public string cardNo { get; set; }
        public string veindata { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(cardType)] = cardType;
            dic[nameof(cardNo)] = cardNo;
            dic[nameof(veindata)] = veindata;
            return dic;
        }

    }
    
    public class req停车计费查询 : req
    {
        /// <summary>
        /// 停车计费查询
        /// </summary>
        public req停车计费查询()
        {
            service = "yuantu.wap.park.charge.get";
            _serviceName = "停车计费查询";
        }
        public string carNo { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(carNo)] = carNo;
            return dic;
        }

    }
    
    public class req停车订单生成 : req
    {
        /// <summary>
        /// 停车订单生成
        /// </summary>
        public req停车订单生成()
        {
            service = "yuantu.wap.park.order.create";
            _serviceName = "停车订单生成";
        }
        public string cash { get; set; }
        public string totalFee { get; set; }
        public string entryTime { get; set; }
        public string carNo { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(cash)] = cash;
            dic[nameof(totalFee)] = totalFee;
            dic[nameof(entryTime)] = entryTime;
            dic[nameof(carNo)] = carNo;
            return dic;
        }

    }
    
    public class req停车订单支付 : req
    {
        /// <summary>
        /// 停车订单支付
        /// </summary>
        public req停车订单支付()
        {
            service = "yuantu.wap.park.order.pay";
            _serviceName = "停车订单支付";
        }
        public string cash { get; set; }
        public string totalFee { get; set; }
        public string entryTime { get; set; }
        public string tradeMode { get; set; }
        public string carNo { get; set; }
        public string transNo { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(cash)] = cash;
            dic[nameof(totalFee)] = totalFee;
            dic[nameof(entryTime)] = entryTime;
            dic[nameof(tradeMode)] = tradeMode;
            dic[nameof(carNo)] = carNo;
            dic[nameof(transNo)] = transNo;
            return dic;
        }

    }
    
    public class req病人已有卡查询 : req
    {
        /// <summary>
        /// 病人已有卡查询
        /// </summary>
        public req病人已有卡查询()
        {
            service = "yuantu.wap.query.card.info.list";
            _serviceName = "病人已有卡查询";
        }
        public string CertType { get; set; }
        public string CertNum { get; set; }
        public string CustName { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(CertType)] = CertType;
            dic[nameof(CertNum)] = CertNum;
            dic[nameof(CustName)] = CustName;
            return dic;
        }

    }
    
    public class req病人绑卡 : req
    {
        /// <summary>
        /// 病人绑卡
        /// </summary>
        public req病人绑卡()
        {
            service = "yuantu.wap.binging.patient.card";
            _serviceName = "病人绑卡";
        }
        public string idNo { get; set; }
        public string patientName { get; set; }
        public string phone { get; set; }
        public string newCardNo { get; set; }
        public string oldCardNo { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(idNo)] = idNo;
            dic[nameof(patientName)] = patientName;
            dic[nameof(phone)] = phone;
            dic[nameof(newCardNo)] = newCardNo;
            dic[nameof(oldCardNo)] = oldCardNo;
            return dic;
        }

    }


    public class res病人信息查询 : res
    {
        public List<病人信息> data { get; set; }
    }

    public class res病人建档发卡 : res
    {
        public 建档信息 data { get; set; }
    }

    public class res发卡 : res
    {
        public 建档信息 data { get; set; }
    }

    public class res诊疗卡账户修改密码 : res
    {
        public string data { get; set; }
    }

    public class res诊疗卡密码校验 : res
    {
        public string data { get; set; }
    }

    public class res自助绑定银行卡 : res
    {
        public string data { get; set; }
    }

    public class res自助绑定银行卡解绑 : res
    {
        public string data { get; set; }
    }

    public class res病人基本信息修改 : res
    {
        public string data { get; set; }
    }

    public class res就诊情况记录查询 : res
    {
        public List<就诊情况记录> data { get; set; }
    }

    public class res就诊满意度 : res
    {
        public string data { get; set; }
    }

    public class res获取缴费概要信息 : res
    {
        public List<缴费概要信息> data { get; set; }
    }

    public class res获取缴费明细信息 : res
    {
        public List<缴费明细信息> data { get; set; }
    }

    public class res缴费预结算 : res
    {
        public 预结算结果 data { get; set; }
    }

    public class res缴费结算 : res
    {
        public 结算结果 data { get; set; }
    }

    public class res获取已结算记录 : res
    {
        public List<结算记录> data { get; set; }
    }

    public class res预缴金充值 : res
    {
        public 充值结果 data { get; set; }
    }

    public class res查询预缴金充值记录 : res
    {
        public List<充值记录> data { get; set; }
    }

    public class res查询预缴金账户余额 : res
    {
        public 账户余额 data { get; set; }
    }

    public class res排班科室信息查询 : res
    {
        public List<排班科室信息> data { get; set; }
    }

    public class res排班医生信息查询 : res
    {
        public List<排班医生信息> data { get; set; }
    }

    public class res排班信息查询 : res
    {
        public List<排班信息> data { get; set; }
    }

    public class res号源明细查询 : res
    {
        public List<号源明细> data { get; set; }
    }

    public class res锁号 : res
    {
        public 锁号结果 data { get; set; }
    }

    public class res解锁 : res
    {
        public string data { get; set; }
    }

    public class res当天挂号 : res
    {
        public 当天挂号结果 data { get; set; }
    }

    public class res预约挂号 : res
    {
        public 预约挂号结果 data { get; set; }
    }

    public class res预约取号 : res
    {
        public 取号结果 data { get; set; }
    }

    public class res取消预约或挂号 : res
    {
        public 取消预约或挂号结果 data { get; set; }
    }

    public class res挂号预约记录查询 : res
    {
        public List<挂号预约记录> data { get; set; }
    }

    public class res取号预结算 : res
    {
        public 取号预结算结果 data { get; set; }
    }

    public class res取号结算 : res
    {
        public 取号结算结果 data { get; set; }
    }

    public class res挂号退号 : res
    {
        public 挂号退号结果 data { get; set; }
    }

    public class res住院患者信息查询 : res
    {
        public 住院患者信息 data { get; set; }
    }

    public class res住院患者费用明细查询 : res
    {
        public List<住院患者费用明细> data { get; set; }
    }

    public class res住院预缴金充值 : res
    {
        public 住院充值结果 data { get; set; }
    }

    public class res住院预缴金充值记录查询 : res
    {
        public List<住院充值记录> data { get; set; }
    }

    public class res虚拟账户开通 : res
    {
        public 虚拟账户开通结果 data { get; set; }
    }

    public class res检验基本信息查询 : res
    {
        public List<检验基本信息> data { get; set; }
    }

    public class res更新打印次数 : res
    {
        public string data { get; set; }
    }

    public class res检验结果明细查询 : res
    {
        public List<检验结果明细> data { get; set; }
    }

    public class res检查结果查询 : res
    {
        public List<检查结果> data { get; set; }
    }

    public class res医生信息查询 : res
    {
        public List<医生信息> data { get; set; }
    }

    public class res科室信息查询 : res
    {
        public List<科室信息> data { get; set; }
    }

    public class res民生卡开卡 : res
    {
        public 民生卡开卡结果 data { get; set; }
    }

    public class res民生卡终端签到 : res
    {
        public 民生卡终端签到结果 data { get; set; }
    }

    public class res民生卡余额查询 : res
    {
        public 民生卡余额查询结果 data { get; set; }
    }

    public class res民生卡交易明细查询 : res
    {
        public List<民生卡交易明细查询结果> data { get; set; }
    }

    public class res民生卡充值 : res
    {
        public 民生卡充值结果 data { get; set; }
    }

    public class res民生卡充值冲正 : res
    {
        public 民生卡充值冲正结果 data { get; set; }
    }

    public class res民生卡消费 : res
    {
        public 民生卡消费结果 data { get; set; }
    }

    public class res民生卡消费冲正 : res
    {
        public 民生卡消费冲正结果 data { get; set; }
    }

    public class res银联卡消费登记 : res
    {
        public 银联卡消费登记结果 data { get; set; }
    }

    public class res民生卡退费 : res
    {
        public 民生卡退费结果 data { get; set; }
    }

    public class res民生卡工本费 : res
    {
        public 民生卡工本费 data { get; set; }
    }

    public class res民生卡客户信息更新 : res
    {
        public 民生卡客户信息更新 data { get; set; }
    }

    public class res民生卡重置密码 : res
    {
        public 民生卡重置密码 data { get; set; }
    }

    public class res民生卡卡片信息查询 : res
    {
        public 民生卡卡片信息查询 data { get; set; }
    }

    public class res民生卡密码修改 : res
    {
        public 民生卡密码修改结果 data { get; set; }
    }

    public class res民生卡CPU卡密码设置 : res
    {
        public 民生卡CPU卡密码设置结果 data { get; set; }
    }

    public class res医保信息查询 : res
    {
        public List<医保信息查询> data { get; set; }
    }

    public class res医生信息快速查询 : res
    {
        public List<医生信息快速查询> data { get; set; }
    }

    public class res系统签到 : res
    {
        public 系统签到结果 data { get; set; }
    }

    public class res信息上报 : res
    {
        public 信息上报结果 data { get; set; }
    }

    public class res创建扫码订单 : res
    {
        public 订单扫码 data { get; set; }
    }

    public class res取消扫码订单 : res
    {
        public 取消订单 data { get; set; }
    }

    public class res查询订单状态 : res
    {
        public 订单状态 data { get; set; }
    }

    public class res支付宝支付成功上报 : res
    {
        public 支付宝支付成功上报结果 data { get; set; }
    }

    public class res查询将要接种的针次 : res
    {
        public 接种针次结果 data { get; set; }
    }

    public class res查询将要接种的清单 : res
    {
        public 接种清单结果 data { get; set; }
    }

    public class res查询公费人员信息 : res
    {
        public 公费人员信息结果 data { get; set; }
    }

    public class res验证公费人员信息 : res
    {
        public 公费人员指静脉验证结果 data { get; set; }
    }

    public class res停车计费查询 : res
    {
        public 停车计费查询结果 data { get; set; }
    }

    public class res停车订单生成 : res
    {
        public 停车订单生成结果 data { get; set; }
    }

    public class res停车订单支付 : res
    {
        public string data { get; set; }
    }

    public class res病人已有卡查询 : res
    {
        public List<病人卡片信息> data { get; set; }
    }

    public class res病人绑卡 : res
    {
        public 病人绑卡结果 data { get; set; }
    }
}