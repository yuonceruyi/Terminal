using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.Services;

namespace YuanTu.XiaoShanZYY.HIS
{
    public class Req
    {
        public virtual string Service { get; }
        public BASEINFO BASEINFO { get; set; }

        public Req()
        {
            BASEINFO = new BASEINFO()
            {
                CAOZUOYDM = FrameworkConst.OperatorId,
                CAOZUOYXM = FrameworkConst.OperatorName,
                CAOZUORQ = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                FENYUANDM = "",
                JESHOUJGDM = "",
                JIGOUDM = "",
                MessageID = "",
                XITONGBS = "",
                ZHONGDUANJBH = FrameworkConst.OperatorId,
                ZHONGDUANLSH = "",
            };
            try
            {
                var bis = ServiceLocator.Current.GetInstance<IBusinessConfigManager>();
                BASEINFO.ZHONGDUANLSH = bis.GetFlowId("ReqDll");
            }
            catch (Exception)
            {
                BASEINFO.ZHONGDUANLSH = "ERR" + DateTimeCore.Now.Ticks;
            }
        }
    }

    public class Res
    {
        public virtual string Service { get; }
        public OUTMSG OUTMSG { get; set; }
    }

    public class BASEINFO
    {
        public string CAOZUOYDM { get; set; }
        public string CAOZUOYXM { get; set; }
        public string CAOZUORQ { get; set; }
        public string XITONGBS { get; set; }
        public string FENYUANDM { get; set; }
        public string JIGOUDM { get; set; }
        public string JESHOUJGDM { get; set; }
        public string ZHONGDUANJBH { get; set; }
        public string ZHONGDUANLSH { get; set; }
        public string MessageID { get; set; }
    }

    public class OUTMSG
    {
        public string ERRNO { get; set; }
        public string ERRMSG { get; set; }
        public string ZHONGDUANJBH { get; set; }
        public string ZHONGDUANLSH { get; set; }
    }

    public class DllRes
    {
        public string 错误信息 { get; set; }
        public virtual bool Parse(string s)
        {
            return true;
        }
    }

    public partial class ReqDll
    {
        public ReqDll()
        {
            调用接口ID = "";
            调用类型 = "";

            操作工号 = FrameworkConst.OperatorId;
            try
            {
                var bis = ServiceLocator.Current.GetInstance<IBusinessConfigManager>();
                系统序号 = bis.GetFlowId("ReqDll");
            }
            catch (Exception)
            {
                系统序号 = "ERR" + DateTimeCore.Now.Ticks;
            }
        }
    }

    public class XmlHelper
    {
        private static readonly Encoding encoding = new UTF8Encoding(false);
        private static readonly XmlWriterSettings settings;
        private static readonly XmlSerializerNamespaces ns;
        static XmlHelper()
        {
            settings = new XmlWriterSettings
            {
                Encoding = encoding,
                Indent = false,
                OmitXmlDeclaration = true
            };
            ns = new XmlSerializerNamespaces();
            ns.Add("", "");
        }

        public static string Serilize(object o)
        {
            var serializer = new XmlSerializer(o.GetType());
            var sb = new StringBuilder();
            using (var stringWriter = new StringWriter(sb))
            using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                serializer.Serialize(xmlWriter, o, ns);
            }
            return sb.ToString();
        }
    }
}
