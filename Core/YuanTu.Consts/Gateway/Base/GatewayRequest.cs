using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Practices.ServiceLocation;
using Newtonsoft.Json;
using YuanTu.Consts.Services;

namespace YuanTu.Consts.Gateway.Base
{
    public class GatewayRequest
    {
        protected string _serviceName;

        [JsonIgnore]
        public string serviceName
        {
            get { return _serviceName; }
        }

        public GatewayRequest()
        {
            hospitalId = FrameworkConst.HospitalId;
            terminalNo= deviceInfo = operId = FrameworkConst.OperatorId;
            hospCode = FrameworkConst.HospitalAreaCode;
            //deviceIp = FrameworkConst.LocalIp;
            //deviceMac = FrameworkConst.LocalMac;
            deviceVersion = FrameworkConst.DeviceType;
            fundCustodian = FrameworkConst.FundCustodian;
            try
            {
                var bis = ServiceLocator.Current.GetInstance<IBusinessConfigManager>();
                flowId= bis.GetFlowId(serviceName);
            }
            catch (Exception)
            {
                flowId= "ERR" + DateTimeCore.Now.Ticks;
            }
        }

        [JsonIgnore]
        public TimeSpan Timeout { get; set; } = new TimeSpan(0, 3, 0);

        public string service { get; set; }

        public string hospitalId { get; set; }

        public string operId { get; set; }

        public string tradeTime { get; set; } = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss");

        public string flowId { get; }

        public string hospCode { get; set; }

        public string sourceCode { get; set; } = "ZZJ";

        /// <summary>
        /// 默认自助机编号，一般用于银联终端号
        /// </summary>
        public string deviceInfo { get; set; }

        /// <summary>
        /// 默认自助机编号，用于特殊需求
        /// </summary>
        public string terminalNo { get; set; }

        /// <summary>
        /// 扩展字段，存放项目特殊必要数据
        /// </summary>
        public string extend { get; set; }

        /// <summary>
        /// 设备MAC地址
        /// </summary>
        public string deviceMac { get; set; }
        /// <summary>
        /// 设备IP
        /// </summary>
        public string deviceIp { get; set; }

        /// <summary>
        /// 设备型号（同deviceType）
        /// </summary>
        public string deviceVersion { get; set; }

        /// <summary>
        /// 第三方支付多账号区分Code(具体值由平台分配)
        /// </summary>
        public string fundCustodian { get; set; }

        public virtual Dictionary<string, string> GetParams()
        {
            return new Dictionary<string, string>
            {
                {nameof(service), service},
                {nameof(hospitalId), hospitalId},
                {nameof(operId), operId},
                {nameof(tradeTime), tradeTime},
                {nameof(flowId), flowId},
                {nameof(deviceInfo), deviceInfo},
                {nameof(sourceCode), sourceCode},
                {nameof(hospCode), hospCode},
                {nameof(terminalNo),terminalNo},
                {nameof(extend),extend},
                {nameof(deviceIp),deviceIp },
                {nameof(deviceMac),deviceMac},
                {nameof(deviceVersion),deviceVersion},
                {nameof(fundCustodian),fundCustodian},
        };
        }

        private static Dictionary<Type, PropertyInfo[]> _cacheDic = new Dictionary<Type, PropertyInfo[]>();

        protected  Dictionary<string, string> Build(string prefix, object obj)
        {
            var dic = new Dictionary<string, string>();
            if (obj != null)
            {
                var tp = obj.GetType();
                PropertyInfo[] props = null;
                if (!_cacheDic.ContainsKey(tp))
                {
                    _cacheDic[tp] = tp.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                }
                props = _cacheDic[tp];

                foreach (var info in props)
                {
                    if (info.PropertyType == typeof(string))//字符串
                    {
                        dic[$"{prefix}.{info.Name}"] = info.GetValue(obj) as string;
                    }
                    else if (typeof(IList).IsAssignableFrom(info.PropertyType))//数组集合
                    {
                        var val = info.GetValue(obj) as IList;
                        if (val != null)
                        {
                            for (int i = 0; i < val.Count; i++)
                            {
                                if (val[i] is string)
                                {
                                    dic[$"{prefix}.{info.Name}[{i}]"] = val[i] as string;
                                }
                                else
                                {
                                    var innDic = Build($"{prefix}.{info.Name}[{i}]", val[i]);
                                    foreach (var kv in innDic)
                                    {
                                        dic[kv.Key] = kv.Value;
                                    }
                                }
                            }
                        }
                    }
                    else//自定义实体
                    {
                        var val = info.GetValue(obj);
                        if (val != null)
                        {
                            var innDic = Build($"{prefix}.{info.Name}", val);
                            foreach (var kv in innDic)
                            {
                                dic[kv.Key] = kv.Value;
                            }
                        }
                    }
                }

            }

            return dic;
        }
    }
}