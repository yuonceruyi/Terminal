using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace YuanTu.BJArea.BeiJingSiPay
{
    public class Req
    {
        [XmlIgnore]
        public virtual string ServiceName => string.Empty;
    }

    public class Res
    {
        [XmlIgnore]
        public virtual string ServiceName => string.Empty;
        
        [XmlIgnore]
        public virtual string ErrorMsg {
            get {
                string m = "";
                foreach (var v in state.error)
                {
                    m += v.info + "\r\n";
                }
                return m;
            }
        }
        [XmlIgnore]
        public virtual string WarningMsg
        {
            get
            {
                string m = "";
                foreach (var v in state.warning)
                {
                    m += v.info + "\r\n";
                }
                return m;
            }
        }
        public State state { get; set; }
    }

    [XmlType("state")]
    public class State
    {
        [XmlAttribute]
        public string success { get; set; }

        [XmlElement("error")]
        public Error[] error { get; set; }

        [XmlElement("warning")]
        public Warning[] warning { get; set; }
    }
    public class Error
    {
        [XmlAttribute]
        public string no { get; set; }

        [XmlAttribute]
        public string info { get; set; }

    }

    public class Warning
    {
        [XmlAttribute]
        public string no { get; set; }

        [XmlAttribute]
        public string info { get; set; }
    }
}
