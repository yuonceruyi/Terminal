using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using YuanTu.Consts;

namespace YuanTu.QDArea.QingDaoSiPay.Common
{
    public class SiSet
    {
        #region 变量
        private static string _sisBaseDirectory;
        #region 基础参数
        public  static string SiHosCode { get;  set; }
        public static string terminalCode { get;  set; }
        public static string operatorCode { get;  set; }
        public static Dictionary<string, string> SiMessage { get;  set; }
        public static Dictionary<string, string> YLState { get;  set; }
        public static Dictionary<string, string> YLResult { get; set; }
        public static Dictionary<string, string> DeptCompare { get; set; }
        #endregion

        #endregion

        public static void init()
        {
            _sisBaseDirectory = Path.Combine(FrameworkConst.RootDirectory, "CurrentResource");
            LoadBaseSet();
            LoadMessage();
            LoadYLState();
            LoadYLResult();
            //LoadDeptCompare();
        }
        public static void LoadBaseSet()
        {
            var xmlpath = Path.Combine(_sisBaseDirectory, "SiSetbaseSet.xml");
            XElement xmlTree = XElement.Load(xmlpath);

            SiHosCode = xmlTree.Element("SiHosCode").Value;
            terminalCode = xmlTree.Element("terminalCode").Value;
            operatorCode = xmlTree.Element("operatorCode").Value;

        }
        /// <summary>
        /// 医保错误信息
        /// </summary>
        public static void LoadMessage()
        {
            SiMessage = new Dictionary<string, string>();
            var xmlpath = Path.Combine(_sisBaseDirectory, "SiSetMessage.xml");
            XElement xmlTree = XElement.Load(xmlpath);
            foreach (XElement item in xmlTree.Elements("ROW"))
            {
                string errCode = item.Element("ERRCODE").Value;
                string errMessage = item.Element("ERRMESSINFO").Value;
                SiMessage.Add(errCode, errMessage);
            }
        }
        public static void LoadYLState()
        {
            YLState = new Dictionary<string, string>();
            var xmlpath = Path.Combine(_sisBaseDirectory, "SiSetYLState.xml");
            XElement xmlTree = XElement.Load(xmlpath);
            foreach (XElement item in xmlTree.Elements("ROW"))
            {
                string errCode = item.Element("ERRCODE").Value;
                string errMessage = item.Element("ERRMESSINFO").Value;
                YLState.Add(errCode, errMessage);
            }
        }

        public static void LoadYLResult()
        {
            YLResult = new Dictionary<string, string>();
            var xmlpath = Path.Combine(_sisBaseDirectory, "SiSetYLResult.xml");
            XElement xmlTree = XElement.Load(xmlpath);
            foreach (XElement item in xmlTree.Elements("ROW"))
            {
                string errCode = item.Element("ERRCODE").Value;
                string errMessage = item.Element("ERRMESSINFO").Value;
                YLResult.Add(errCode, errMessage);
            }
        }

        //        public static void LoadDeptCompare()
        //        {
        //            List<Type> typeList = ReflectionManager.GetTypesByInterfaceWithOutAbstract(typeof(iExternal), TypeOfType.Class);
        //            if (typeList.Count <= 0)
        //            {
        //                return;
        //            }
        //            Type type = typeList[0];
        //            iExternal extFun = Activator.CreateInstance(type) as iExternal;
        //            DeptCompare = extFun.GetGetDeptCompare();
        //        }

    }
}
