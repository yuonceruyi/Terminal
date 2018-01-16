using System;
using System.Xml.Serialization;

namespace YuanTu.FuYangRMYY
{

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot("Response", Namespace = "", IsNullable = false)]
    public partial class RegisterListResponse : ResponseBase
    {
        
        /// <remarks/>
        public int RecordCount { get; set; }

        /// <remarks/>
        [XmlArrayItem("Order", IsNullable = false)]
        public ResponseOrder[] Orders { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType = true)]
    public partial class ResponseOrder
    {

        private string regIDField;

        private System.DateTime regDateField;

        private string statusField;

        private string patNameField;

        private string patientIDField;

        private System.DateTime admitDateField;

        private string hospitalNameField;

        private string departmentField;

        private string doctorField;

        private string doctorTitleField;

        private string regFeeField;

        private string seqCodeField;

        private string sessionNameField;

        private string admitRangeField;

        private string payModeCodeField;

        private string queueIDField;

        private string queueStatusField;

        /// <remarks/>
        public string RegID
        {
            get
            {
                return this.regIDField;
            }
            set
            {
                this.regIDField = value;
            }
        }

        /// <remarks/>
        [XmlElement(DataType = "date")]
        public System.DateTime RegDate
        {
            get
            {
                return this.regDateField;
            }
            set
            {
                this.regDateField = value;
            }
        }

        /// <remarks/>
        public string Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        public string PatName
        {
            get
            {
                return this.patNameField;
            }
            set
            {
                this.patNameField = value;
            }
        }

        /// <remarks/>
        public string PatientID
        {
            get
            {
                return this.patientIDField;
            }
            set
            {
                this.patientIDField = value;
            }
        }

        /// <remarks/>
        [XmlElement(DataType = "date")]
        public System.DateTime AdmitDate
        {
            get
            {
                return this.admitDateField;
            }
            set
            {
                this.admitDateField = value;
            }
        }

        /// <remarks/>
        public string HospitalName
        {
            get
            {
                return this.hospitalNameField;
            }
            set
            {
                this.hospitalNameField = value;
            }
        }

        /// <remarks/>
        public string Department
        {
            get
            {
                return this.departmentField;
            }
            set
            {
                this.departmentField = value;
            }
        }

        /// <remarks/>
        public string Doctor
        {
            get
            {
                return this.doctorField;
            }
            set
            {
                this.doctorField = value;
            }
        }

        /// <remarks/>
        public string DoctorTitle
        {
            get
            {
                return this.doctorTitleField;
            }
            set
            {
                this.doctorTitleField = value;
            }
        }

        /// <remarks/>
        public string RegFee
        {
            get
            {
                return this.regFeeField;
            }
            set
            {
                this.regFeeField = value;
            }
        }

        /// <remarks/>
        public string SeqCode
        {
            get
            {
                return this.seqCodeField;
            }
            set
            {
                this.seqCodeField = value;
            }
        }

        /// <remarks/>
        public string SessionName
        {
            get
            {
                return this.sessionNameField;
            }
            set
            {
                this.sessionNameField = value;
            }
        }

        /// <remarks/>
        public string AdmitRange
        {
            get
            {
                return this.admitRangeField;
            }
            set
            {
                this.admitRangeField = value;
            }
        }

        /// <remarks/>
        public string PayModeCode
        {
            get
            {
                return this.payModeCodeField;
            }
            set
            {
                this.payModeCodeField = value;
            }
        }

        /// <remarks/>
        public string QueueID
        {
            get
            {
                return this.queueIDField;
            }
            set
            {
                this.queueIDField = value;
            }
        }

        /// <remarks/>
        public string QueueStatus
        {
            get
            {
                return this.queueStatusField;
            }
            set
            {
                this.queueStatusField = value;
            }
        }
    }



}
