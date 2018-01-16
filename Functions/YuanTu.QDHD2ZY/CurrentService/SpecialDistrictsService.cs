using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using YuanTu.Consts;

namespace YuanTu.QDHD2ZY.CurrentService
{
    public class SpecialDistrictsService
    {
        /// <summary>
        /// 黄岛二中医需求背景：持医保卡人员，挂号时，根据医保卡读出的地址，匹配静态配置，确定归属地，标记给HIS
        /// </summary>
        #region 变量
        private static string _baseDirectory;
        #endregion

        #region 基础参数
        public static Dictionary<string, string> DicSpecialDistricts { get; set; }
        #endregion

        public static void Init()
        {
            _baseDirectory = Path.Combine(FrameworkConst.RootDirectory, "CurrentResource");
            InnerLoad();
        }

        private static void InnerLoad()
        {
            DicSpecialDistricts = new Dictionary<string, string>();
            var xmlpath = Path.Combine(_baseDirectory,FrameworkConst.HospitalId, "SpecialDistricts.xml");
            var xmlTree = XElement.Load(xmlpath);
            foreach (var item in xmlTree.Elements("ROW"))
            {
                var address = item.Element("ADDRESS")?.Value;
                var districts = item.Element("DISTRICTS")?.Value;
                if (address != null) DicSpecialDistricts.Add(address, districts);
            }
        }
    }
}
