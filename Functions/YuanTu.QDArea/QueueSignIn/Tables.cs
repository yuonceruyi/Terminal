using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.QDArea.QueueSignIn
{
    public interface IRes
    {
        bool success { get; set; }
        string resultCode { get; set; }
        string msg { get; set; }
        long startTime { get; set; }
        long timeConsum { get; set; }
    }

    public class Res : IRes
    {
        public bool success { get; set; }
        public string resultCode { get; set; }
        public string msg { get; set; }
        public long startTime { get; set; }
        public long timeConsum { get; set; }
    }

    public interface IReq
    {
        string serviceUrl { get; }

        Dictionary<string, string> GetParams();
    }

    public class Req : IReq
    {
        public virtual string serviceUrl => string.Empty;

        public virtual Dictionary<string, string> GetParams()
        {
            return new Dictionary<string, string>();
        }
    }

    #region GetSecret
    public class ReqGetSecret : Req
    {
        public override string serviceUrl => "device/getSecret";
        public string deviceMac { get; set; }

        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(deviceMac)] = deviceMac;
            return dic;
        }
    }

    public class ResGetSecret : Res
    {
        public string data { get; set; }
    }
    #endregion GetSecret

    #region GetAccessToken
    public class ReqGetAccessToken : Req
    {
        public override string serviceUrl => "device/getAccessToken";
        public string deviceSecret { get; set; }

        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(deviceSecret)] = deviceSecret;
            return dic;
        }
    }

    public class ResGetAccessToken : Res
    {
        public string data { get; set; }
    }
    #endregion GetAccessToken

    #region QueryQueueByDevice
    public class ReqQueryQueueByDevice : Req
    {
        public override string serviceUrl => "queryQueueByDevice";
        public string token { get; set; }
        public string cardType { get; set; }
        public string cardNo { get; set; }
        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(token)] = token;
            dic[nameof(cardType)] = cardType;
            dic[nameof(cardNo)] = cardNo;
            return dic;
        }
    }
    public class ResQueryQueueByDevice : Res
    {
        public List<Data> data { get; set; }

        public class Data
        {
            public string queueCode { get; set; } //String  队列编号
            public string queueName { get; set; } //String 队列名
            public string corpId { get; set; } //String  医院编号
            public string area { get; set; }//String 编号
            public bool hadTakeNo { get; set; }//Boolean 是否在该队列已经取号 true:在该队列已经取号(不能取号，可以查看) false:可以取号
            public string orderNoTag { get; set; }//String 已取的号
        }
    }
    #endregion QueryQueueByDevice

    #region ConfirmTakeNo
    public class ReqConfirmTakeNo : Req
    {
        public override string serviceUrl => "confirmTakeNo";
        public string token { get; set; }
        public string cardType { get; set; }
        public string cardNo { get; set; }
        public string queueCode { get; set; }
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(token)] = token;
            dic[nameof(cardType)] = cardType;
            dic[nameof(cardNo)] = cardNo;
            dic[nameof(queueCode)] = queueCode;
            return dic;
        }
    }
    public class ResConfirmTakeNo : Res
    {
        public Data data { get; set; }

        public class Data
        {
            public string username { get; set; } //String  姓名
            public string patientNo { get; set; } //String 门诊号
            public string currentOrderNo { get; set; } //String  该队列当前叫号
            public string orderNoTag { get; set; } //String 患者取的号
            public int intervalFlag { get; set; } //Interger	1:上午 2：下午
            public string corpId { get; set; } //String 医院编号
            public string area { get; set; } //String  区域编号
            public string queueCode { get; set; } //String 队列编号
            public string queueName { get; set; } //String  队列名
            public string cardNo { get; set; } //String 卡号
            public int cardType { get; set; } //Integer 卡类型
        }
    }
    #endregion QueryQueueByDevice
}
