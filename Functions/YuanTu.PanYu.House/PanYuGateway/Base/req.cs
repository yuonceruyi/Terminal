using System.Collections.Generic;


namespace YuanTu.PanYu.House.PanYuGateway.Base
{
    public class req
    {
        protected string _serviceName;
        public string serviceName
        {
            get { return _serviceName; }
        }
        public string service { get; set; }
        public string hospitalId { get; set; }
        public string operId { get; set; }
        public string flowId { get; set; }
        public string hospCode { get; set; }
        public string terminalNo { get; set; }

        public req()
        {
            hospitalId = DataHandler.HospitalId;
            hospCode = DataHandler.HospCode;

            terminalNo = DataHandler.terminalNo;
            operId = DataHandler.OperId;
        }
        public virtual Dictionary<string, string> GetParams()
        {
            return new Dictionary<string, string>
            {
                {nameof(service), service},
                {nameof(hospitalId), hospitalId},
                {nameof(hospCode), hospCode},
                {nameof(operId), operId},
                {nameof(flowId), flowId},
                {nameof(terminalNo), terminalNo},
            };
        }
    }
}