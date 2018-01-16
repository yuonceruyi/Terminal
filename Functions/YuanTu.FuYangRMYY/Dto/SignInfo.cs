using System;

namespace YuanTu.FuYangRMYY
{
    

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute("Response",Namespace = "", IsNullable = false)]
    public partial class SignInfoResponse: ResponseBase
    {


        /// <remarks/>
        public string CardNo { get; set; }

        /// <remarks/>
        public string PatNo { get; set; }

        /// <remarks/>
        public string PatName { get; set; }

        /// <remarks/>
        public string RegDep { get; set; }

        /// <remarks/>
        public string SessionType { get; set; }

        /// <remarks/>
        public string MarkDesc { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime AdmDate { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "time")]
        public System.DateTime RegTime { get; set; }

        /// <remarks/>
        public string SeqNo { get; set; }

        /// <remarks/>
        public string UserCode { get; set; }
    }



}
