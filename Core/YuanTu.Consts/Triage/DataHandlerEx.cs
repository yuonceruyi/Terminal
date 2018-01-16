using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YuanTu.Consts.Gateway.Base;

namespace YuanTu.Consts.Triage
{
    #pragma warning disable 612
    public partial class DataHandlerEx
    {
        public static IDataHandler Handler{get;set;}
        public static res需预检科室信息查询 需预检科室信息查询(req需预检科室信息查询 req)
        {
            return Handler.Query<res需预检科室信息查询, req需预检科室信息查询>(req);
        }

        public static res需预检挂号类别信息查询 需预检挂号类别信息查询(req需预检挂号类别信息查询 req)
        {
            return Handler.Query<res需预检挂号类别信息查询, req需预检挂号类别信息查询>(req);
        }

        public static res患者预检记录信息查询 患者预检记录信息查询(req患者预检记录信息查询 req)
        {
            return Handler.Query<res患者预检记录信息查询, req患者预检记录信息查询>(req);
        }

    }

    
    public class req需预检科室信息查询 : GatewayRequest
    {
        /// <summary>
        /// 需预检科室信息查询
        /// </summary>
        public req需预检科室信息查询()
        {
            service = "yuantu.wap.query.triage.dept.info.list";
            _serviceName = "需预检科室信息查询";
        }
    }
    
    public class req需预检挂号类别信息查询 : GatewayRequest
    {
        /// <summary>
        /// 需预检挂号类别信息查询
        /// </summary>
        public req需预检挂号类别信息查询()
        {
            service = "yuantu.wap.query.triage.regtype.info.list";
            _serviceName = "需预检挂号类别信息查询";
        }
    }
    
    public class req患者预检记录信息查询 : GatewayRequest
    {
        /// <summary>
        /// 患者预检记录信息查询
        /// </summary>
        public req患者预检记录信息查询()
        {
            service = "yuantu.wap.query.triage.record.info.list";
            _serviceName = "患者预检记录信息查询";
        }
        /// <summary>
        /// 门诊号/病历号,患者医院唯一ID
        /// </summary>
        public string patientId { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(patientId)] = patientId;
            return dic;
        }

    }


    public class res需预检科室信息查询 : GatewayResponse
    {
        public List<预检科室信息> data { get; set; }
    }

    public class res需预检挂号类别信息查询 : GatewayResponse
    {
        public List<预检挂号类别信息> data { get; set; }
    }

    public class res患者预检记录信息查询 : GatewayResponse
    {
        public List<预检记录信息> data { get; set; }
    }
#pragma warning restore 612
}