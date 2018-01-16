using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YuanTu.Consts;
using System.IO;
using YuanTu.Core.Extension;
using YuanTu.QDQLYY.Current.Models;

namespace YuanTu.QDQLYY.Current.Services
{
    public class QualificationService
    {
        public static res获取执业资格信息 执业资格信息列表 { get; set; }

        private static TRes InQualificationInit<TRes>()
            where TRes : res获取执业资格信息, new()
        {
            var res = "";
            var baseDirectory = Path.Combine(FrameworkConst.RootDirectory, "CurrentResource", FrameworkConst.HospitalId);
            var file = Path.Combine(baseDirectory, "执业资格证书.json");

            if (File.Exists(file))
            {
                using (var sr = new StreamReader(file))
                    res = sr.ReadToEnd();
            }
            return res.ToJsonObject<TRes>();
        }

        public static void QualificationInit()
        {
            执业资格信息列表 = InQualificationInit<res获取执业资格信息>();
        }
    }
}
