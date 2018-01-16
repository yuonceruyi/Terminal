namespace YuanTu.YiWuBeiYuan.Models
{



    public class YbOpPayHisInfo
    {
        public string FEIYONGMXTS { get; set; }
        /// <summary>
        /// 00 普通 02 特病
        /// </summary>
        public string YILIAOLB { get; set; }

        public string SHOUFEIWWCBZ{ get; set; }
        public string FEIYONGMX { get; set; }
        public Preoppayjson[] preOpPayJSON { get; set; }
        public string[] preOpPayArgs { get; set; }
    }

    public class Preoppayjson
    {
        public string outpatientNo { get; set; }
        public string diseaseName { get; set; }
        public string billCount { get; set; }
        public string chargeType { get; set; }
        public string diseaseCode { get; set; }
        public string ICinfo { get; set; }
        public Billitem[] billItems { get; set; }
        public Chargeitem[] chargeItems { get; set; }
        public string accountBalNo { get; set; }
        public string diseaseDesc { get; set; }
        public string isPersonPay { get; set; }
        public string diseaseApprovalCode { get; set; }
    }

    public class Billitem
    {
        public string billNo { get; set; }
        public string chargeType { get; set; }
        public string deptName { get; set; }
        public string diseaseCode { get; set; }
        public string diseaseDesc { get; set; }
        public string doctorIdNo { get; set; }
        public string doctorName { get; set; }
        public string feeCount { get; set; }
        public string noInsuranceFee { get; set; }
        public string outpatientNo { get; set; }
        public string prescriptionNo { get; set; }
        public string visitDate { get; set; }
    }

    public class Chargeitem
    {
        public string amount { get; set; }
        public string billNo { get; set; }
        public string countOfDay { get; set; }
        public string diseaseCode { get; set; }

        public string docOrderNo { get; set; }
        public string docOrderTime { get; set; }
        public string restrictedMark { get; set; }
        public string importSelfRatio { get; set; }
        public string itemCount { get; set; }
        public string itemHospCode { get; set; }
        public string itemHospName { get; set; }
        public string itemYBCode { get; set; }

        public string itemLiquid { get; set; }
        public string itemMinUnit { get; set; }
        public string itemPackageCount { get; set; }
        public string itemQty { get; set; }
        public string itemSpecs { get; set; }
        public string itemUnit { get; set; }
        public string price { get; set; }
        public string selfRatio { get; set; }
        public string singleMark { get; set; }
        public string treatType { get; set; }
        public string unitOfDay { get; set; }
        public string useDay { get; set; }
    }
}