using System.Xml.Serialization;

namespace YuanTu.BJArea.BeiJingSiPay
{
    #region 初始化读卡设备
    [XmlRoot("root")]
    public class Req初始化读卡设备 : Req
    {
        public override string ServiceName => "Open";
    }

    [XmlRoot("root")]
    public class Res初始化读卡设备 : Res
    {
        public override string ServiceName => "Open";
    }
    #endregion

    #region 关闭读卡设备
    [XmlRoot("root")]
    public class Req关闭读卡设备 : Req
    {
        public override string ServiceName => "Close";
    }

    [XmlRoot("root")]
    public class Res关闭读卡设备 : Res
    {
        public override string ServiceName => "Close";

    }
    #endregion

    #region 获取个人信息
    [XmlRoot("root")]
    public class Req获取个人信息 : Req
    {
        public override string ServiceName => "GetPersonInfo";
    }

    [XmlRoot("root")]
    public class Res获取个人信息 : Res
    {
        public override string ServiceName => "GetPersonInfo";

        public Output output { get; set; }


        [XmlType("output")]
        public class Output
        {
            [XmlAttribute]
            public string name { get; set; }
            public Ic ic { get; set; }
            public Net net { get; set; }

            [XmlType("ic")]
            public class Ic
            {
                public string card_no { get; set; }
                public string ic_no { get; set; }
                public string id_no { get; set; }
                public string personname { get; set; }
                public string sex { get; set; }
                public string birthday { get; set; }
                public string fromhosp { get; set; }
                public string fromhospdate { get; set; }
                public string fundtype { get; set; }
                public string isyt { get; set; }
                public string jclevel { get; set; }
                public string hospflag { get; set; }

            }

            [XmlRoot(ElementName = "net")]
            public class Net
            {
                public string persontype { get; set; }
                public string isinredlist { get; set; }
                public string isspecifiedhosp { get; set; }
                public string ischronichosp { get; set; }
                public string personcount { get; set; }
                public string chroniccode { get; set; }
            }
        }

    }
    #endregion

    #region 卡内基本信息

    [XmlRoot("root")]
    public class Req获取卡内个人信息 : Req
    {
        public override string ServiceName => "GetCardInfo";
    }

    [XmlRoot("root")]
    public class Res获取卡内个人信息 : Res
    {
        public Output output { get; set; }

        [XmlType("output")]
        public class Output
        {
            [XmlAttribute]
            public string name { get; set; }
            public Ic ic { get; set; }
            [XmlType("ic")]
            public class Ic
            {
                public string card_no { get; set; }
                public string ic_no { get; set; }
                public string id_no { get; set; }
                public string personname { get; set; }
                public string sex { get; set; }
                public string birthday { get; set; }

            }
        }
    }
    #endregion
    #region 费用分解

    [XmlRoot("root")]
    public class Req费用分解 : Req
    {
        public override string ServiceName => "Divide";
        public Input input { get; set; }
        [XmlType("input")]
        public class Input
        {
            public Tradeinfo tradeinfo { get; set; }
            [XmlElement("recipearray")]
            public Recipe[] recipearray { get; set; }
            [XmlElement("feeitemarray")]
            public Feeitem[] feeitemarray { get; set; }
            
            [XmlType("tradeinfo")]
            public class Tradeinfo
            {
                public string curetype { get; set; }
                public string illtype { get; set; }
                public string feeno { get; set; }

                [XmlElement(ElementName = "operator")]
                public string operator2 { get; set; }
            }
            [XmlType("recipe")]
            public class Recipe
            {
                public string diagnoseno { get; set; }
                public string recipeno { get; set; }
                public string recipedate { get; set; }
                public string diagnosename { get; set; }
                public string diagnosecode { get; set; }
                public string medicalrecord { get; set; }
                public string sectioncode { get; set; }
                public string sectionname { get; set; }
                public string hissectionname { get; set; }
                public string drid { get; set; }
                public string drname { get; set; }
                public string recipetype { get; set; }
                public string helpmedicineflag { get; set; }
                public string remark { get; set; }
                public string registertradeno { get; set; }
                public string billstype { get; set; }
            }
            [XmlType("feeitem")]
            public class Feeitem {
                [XmlAttribute]
                public string itemno { get; set; }
                [XmlAttribute]
                public string recipeno { get; set; }
                [XmlAttribute]
                public string hiscode { get; set; }
                [XmlAttribute]
                public string itemname { get; set; }
                [XmlAttribute]
                public string itemtype { get; set; }
                [XmlAttribute]
                public string unitprice { get; set; }
                [XmlAttribute]
                public string count { get; set; }
                [XmlAttribute]
                public string fee { get; set; }
                [XmlAttribute]
                public string dose { get; set; }
                [XmlAttribute]
                public string specification { get; set; }
                [XmlAttribute]
                public string unit { get; set; }
                [XmlAttribute]
                public string howtouse { get; set; }
                [XmlAttribute]
                public string dosage { get; set; }
                [XmlAttribute]
                public string packaging { get; set; }
                [XmlAttribute]
                public string minpackage { get; set; }
                [XmlAttribute]
                public string conversion { get; set; }
                [XmlAttribute]
                public string days { get; set; }
                [XmlAttribute]
                public string babyflag { get; set; }
                [XmlAttribute]
                public string drugapprovalnumber { get; set; }
            }

        }
    }

    [XmlRoot("root")]
    public class Res费用分解 : Res
    {
        public Output output { get; set; }

        [XmlType("output")]
        public class Output
        {
            [XmlAttribute]
            public string name { get; set; }
            [XmlElement("feeitemarray")]
            public Feeitem[] feeitemarray { get; set; }
            public Sumpay sumpay { get; set; }
            public Payinfo payinfo { get; set; }
            public Medicatalog medicatalog { get; set; }
            public Medicatalog2 medicatalog2 { get; set; }

            [XmlType("tradeinfo")]
            public class Tradeinfo
            {
                public string tradeno { get; set; }
                public string feeno { get; set; }
                public string tradedate { get; set; }
            }
            [XmlType("feeitem")]
            public class Feeitem
            {
                [XmlAttribute] public string itemno { get; set; }
                [XmlAttribute] public string recipeno { get; set; }
                [XmlAttribute] public string hiscode { get; set; }
                [XmlAttribute] public string itemcode { get; set; }
                [XmlAttribute] public string itemname { get; set; }
                [XmlAttribute] public string itemtype { get; set; }
                [XmlAttribute] public string unitprice { get; set; }
                [XmlAttribute] public string count { get; set; }
                [XmlAttribute] public string fee { get; set; }
                [XmlAttribute] public string feein { get; set; }
                [XmlAttribute] public string feeout { get; set; }
                [XmlAttribute] public string selfpay2 { get; set; }
                [XmlAttribute] public string state { get; set; }
                [XmlAttribute] public string fee_type { get; set; }
                [XmlAttribute] public string preferentialfee { get; set; }
                [XmlAttribute] public string preferentialscale { get; set; }
            }
            public class Sumpay
            {
                public string feeall { get; set; }
                public string fund { get; set; }
                public string cash { get; set; }
                public string personcountpay { get; set; }

            }
            public class Payinfo
            {
                public string mzfee { get; set; }
                public string mzfeein { get; set; }
                public string mzfeeout { get; set; }
                public string mzpayfirst { get; set; }
                public string mzselfpay2 { get; set; }
                public string mzbigpay { get; set; }
                public string mzbigselfpay { get; set; }
                public string mzoutofbig { get; set; }
                public string bcpay { get; set; }
                public string jcbz { get; set; }
            }
            public class Medicatalog
            {
                public string medicine { get; set; }
                public string tmedicine { get; set; }
                public string therb { get; set; }
                public string examine { get; set; }
                public string ct { get; set; }
                public string mri { get; set; }
                public string ultrasonic { get; set; }
                public string oxygen { get; set; }
                public string operation { get; set; }
                public string treatment { get; set; }
                public string xray { get; set; }
                public string labexam { get; set; }
                public string bloodt { get; set; }
                public string orthodontics { get; set; }
                public string prosthesis { get; set; }
                public string forensic_expertise { get; set; }
                public string material { get; set; }
                public string other { get; set; }
            }
            public class Medicatalog2
            {
                public string diagnosis { get; set; }
                public string examine { get; set; }
                public string labexam { get; set; }
                public string treatment { get; set; }
                public string operation { get; set; }
                public string material { get; set; }
                public string medicine { get; set; }
                public string therb { get; set; }
                public string tmedicine { get; set; }
                public string medicalservice { get; set; }
                public string commonservice { get; set; }
                public string registfee { get; set; }
                public string otheropfee { get; set; }
            }
        }
    }
    #endregion

    #region 交易确认

    [XmlRoot("root")]
    public class Req交易确认 : Req
    {
        public override string ServiceName => "Trade";
    }

    [XmlRoot("root")]
    public class Res交易确认 : Res
    {
        public Output output { get; set; }

        [XmlType("output")]
        public class Output
        {
            [XmlAttribute]
            public string name { get; set; }
            public string personcountaftersub { get; set; }
            public string certid { get; set; }
            public string sign { get; set; }
        }
    }
    #endregion
}