using System.Net;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts.Services;
using YuanTu.ISO8583;
using YuanTu.ShenZhenArea.Enums;
using YuanTu.Consts.Models.Print;
using System.Drawing;
using YuanTu.ShenZhenArea.Services;
using YuanTu.Core.Log;
using YuanTu.Consts;
using YuanTu.ShenZhenArea.Models.DepartModel;
using System.Collections.Generic;

namespace YuanTu.ShenZhen.BaoAnCenterHospital
{
    public class ConfigBaoAnCenterHospital
    {
        /// <summary>
        ///退出系统的密码
        /// </summary>
        public const string ExitPass = "123123123";

        /// <summary>
        ///清钱箱的密码
        /// </summary>
        public const string ClearCashboxPass = "123698745";

        /// <summary>
        ///是否开启社保卡输入密码
        /// </summary>
        public static bool OpenSheBaoCardPasswordEnter { set; get; }

        /// <summary>
        /// 是否关闭打印机检测
        /// </summary>
        public static bool ClosePrintStatusCheck { set; get; }


        /// <summary>
        ///     银联配置
        /// </summary>
        public static Config PosConfig { get; set; }


        public static List<BaoAnCenterHospitalDepartment> ListDepartments = new List<BaoAnCenterHospitalDepartment>();

        public static int LisTimeOut { get; set; }
        public static string LisFolderPath { get; set; }


        public static void Init()
        {
            LoadPrinterConfig();
            LoadPosConfig();
            LoadSheBaoConfig();
            LoadPrintConfig();
            LoadAccountingConfig();
            LoadLISConfig();
            LoadDepartmentsConfig();
        }

        /// <summary>
        /// 记账地址配置
        /// </summary>
        private static void LoadAccountingConfig()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            AccountingBase.JiaoYiAccountingUrl = config.GetValue("AccountingAddress:jyjz");
            AccountingBase.InsuranceAccountingUrl = config.GetValue("AccountingAddress:ybjz");

            HttpPost.Log += Logger.Net.Info;
        }

        /// <summary>
        /// 社保配置
        /// </summary>
        private static void LoadSheBaoConfig()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            OpenSheBaoCardPasswordEnter = config.GetValue("SheBao:OpenSheBaoPassword") == "1";
            ShenZhenArea.Insurance.YBBase.InitSheBaoBase(config);
        }
        /// <summary>
        /// 打印机配置
        /// </summary>
        public static void LoadPrinterConfig()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            ClosePrintStatusCheck = config.GetValue("Print:CloseChecked")?.Trim() == "1";
        }
        /// <summary>
        /// 打印配置
        /// </summary>
        public static void LoadPrintConfig()
        {
            //HeaderFont = new Font("微软雅黑", (CurrentStrategyType() == DeviceType.Clinic ? 12 : 16), FontStyle.Bold);
            //HeaderFont2 = new Font("微软雅黑", (CurrentStrategyType() == DeviceType.Clinic ? 10 : 14), FontStyle.Bold);
            //DefaultFont = new Font("微软雅黑", (CurrentStrategyType() == DeviceType.Clinic ? 8 : 12), FontStyle.Regular);
            PrintConfig.HeaderFont = new Font("微软雅黑", 14, FontStyle.Bold);
            PrintConfig.HeaderFont2 = new Font("微软雅黑", 12, FontStyle.Bold);
            PrintConfig.DefaultFont = new Font("微软雅黑", 10, FontStyle.Regular);
        }
        /// <summary>
        /// POS配置
        /// </summary>
        public static void LoadPosConfig()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();

            PosConfig = new Config
            {
                Address = IPAddress.Parse(config.GetValue("Pos:IP")),
                Port = config.GetValueInt("Pos:Port"),
                TPDU = config.GetValue("Pos:TPDU"),
                Head = config.GetValue("Pos:Head"),
                TerminalId = config.GetValue("Pos:TerminalId"),
                MerchantId = config.GetValue("Pos:MerchantId")
            };
        }


        public static void LoadLISConfig()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            LisTimeOut = config.GetValueInt("HuiQiaoLis:TimeOut", 6);
            LisFolderPath = config.GetValue("HuiQiaoLis:Path");
        }

        public static void LoadDepartmentsConfig()
        {
            ListDepartments.Add(new BaoAnCenterHospitalDepartment()
            {
                DepartName = "急诊医学科",
                DepartCode = null,
                ChildDepartments = new List<BaoAnCenterHospitalDepartment>()
                {
                    new BaoAnCenterHospitalDepartment() { DepartName = "急诊内科", DepartCode="77"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "急诊外科",  DepartCode="78"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "急诊儿科", DepartCode="209"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "急诊妇产科",  DepartCode="69"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "急诊头痛",DepartCode="77"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "胸痛/腹痛联合门诊",DepartCode="78"},
                }
            });

            ListDepartments.Add(new BaoAnCenterHospitalDepartment()
            {
                DepartName = "内科",
                DepartCode = null,
                ChildDepartments = new List<BaoAnCenterHospitalDepartment>()
                {
                    new BaoAnCenterHospitalDepartment() { DepartName = "呼吸综合门诊", DepartCode="42", },
                    new BaoAnCenterHospitalDepartment() { DepartName = "哮喘及呼吸慢病门诊", DepartCode="42", },
                    new BaoAnCenterHospitalDepartment() { DepartName = "肺部结节及纵膈内科门诊", DepartCode="42", },
                    new BaoAnCenterHospitalDepartment() { DepartName = "戒烟门诊", DepartCode="42", },
                    new BaoAnCenterHospitalDepartment() { DepartName = "心血管内科综合门诊", DepartCode="45", },
                    new BaoAnCenterHospitalDepartment() { DepartName = "心律失常门诊", DepartCode="45", },
                    new BaoAnCenterHospitalDepartment() { DepartName = "消化内科综合门诊", DepartCode="43", },
                    new BaoAnCenterHospitalDepartment() { DepartName = "肝病门诊", DepartCode="43", },
                    new BaoAnCenterHospitalDepartment() { DepartName = "功能性胃肠病门诊", DepartCode="43", },
                    new BaoAnCenterHospitalDepartment() { DepartName = "胃肠早癌筛查门诊", DepartCode="43", },
                    new BaoAnCenterHospitalDepartment() { DepartName = "内分泌科", DepartCode="47", },
                    new BaoAnCenterHospitalDepartment() { DepartName = "慢性肾脏病门诊", DepartCode="46", },
                    new BaoAnCenterHospitalDepartment() { DepartName = "脑血管门诊", DepartCode="44", },
                    new BaoAnCenterHospitalDepartment() { DepartName = "癫痫病", DepartCode="44", },
                    new BaoAnCenterHospitalDepartment() { DepartName = "颅脑创伤及脑肿瘤门诊", DepartCode="49", },
                    new BaoAnCenterHospitalDepartment() { DepartName = "失眠及心理门诊", DepartCode="44", }
                }
            });



            ListDepartments.Add(new BaoAnCenterHospitalDepartment()
            {
                DepartName = "外科",
                DepartCode = null,
                ChildDepartments = new List<BaoAnCenterHospitalDepartment>()
                {

            new BaoAnCenterHospitalDepartment() { DepartName = "乳腺甲状腺专科", DepartCode = "48", },
            new BaoAnCenterHospitalDepartment() { DepartName = "肝胆胃肠外科专科", DepartCode = "48", },
            new BaoAnCenterHospitalDepartment() { DepartName = "肛肠专科", DepartCode = "48", },
            new BaoAnCenterHospitalDepartment() { DepartName = "浅表肿瘤专科", DepartCode = "48", },
            new BaoAnCenterHospitalDepartment() { DepartName = "创伤骨科", DepartCode = "50", },
            new BaoAnCenterHospitalDepartment() { DepartName = "脊柱外科", DepartCode = "50", },
            new BaoAnCenterHospitalDepartment() { DepartName = "骨关节科", DepartCode = "52", },
            new BaoAnCenterHospitalDepartment() { DepartName = "运动医学专科", DepartCode = "52", },
            new BaoAnCenterHospitalDepartment() { DepartName = "手外科", DepartCode = "52", },
            new BaoAnCenterHospitalDepartment() { DepartName = "泌尿外科门诊", DepartCode = "51", },
            new BaoAnCenterHospitalDepartment() { DepartName = "泌尿系结石", DepartCode = "51", },
            new BaoAnCenterHospitalDepartment() { DepartName = "男科", DepartCode = "51", },
            new BaoAnCenterHospitalDepartment() { DepartName = "女性及儿童泌尿门诊", DepartCode = "51", },
            new BaoAnCenterHospitalDepartment() { DepartName = "泌尿系肿瘤", DepartCode = "51", }
                }
            });




            ListDepartments.Add(new BaoAnCenterHospitalDepartment()
            {
                DepartName = "妇产科",
                DepartCode = null,
                ChildDepartments = new List<BaoAnCenterHospitalDepartment>()
                {
                    new BaoAnCenterHospitalDepartment() { DepartName = "妇科综合门诊", DepartCode="69",},
                    new BaoAnCenterHospitalDepartment() { DepartName = "宫颈专科门诊", DepartCode="69",},
                    new BaoAnCenterHospitalDepartment() { DepartName = "妇科肿瘤门诊", DepartCode="69",},
                    new BaoAnCenterHospitalDepartment() { DepartName = "计划生育及人流术后关爱诊室", DepartCode="69",},
                    new BaoAnCenterHospitalDepartment() { DepartName = "不孕症门诊", DepartCode="69",},
                    new BaoAnCenterHospitalDepartment() { DepartName = "优生门诊", DepartCode="69",},
                    new BaoAnCenterHospitalDepartment() { DepartName = "妇科（中医）", DepartCode="69",},
                    new BaoAnCenterHospitalDepartment() { DepartName = "产检门诊", DepartCode="70",},
                    new BaoAnCenterHospitalDepartment() { DepartName = "出生证办理", DepartCode="",}
                }
            });


            ListDepartments.Add(new BaoAnCenterHospitalDepartment()
            {
                DepartName = "儿科",
                DepartCode = null,
                ChildDepartments = new List<BaoAnCenterHospitalDepartment>()
                {
                    new BaoAnCenterHospitalDepartment() { DepartName = "儿科综合门诊", DepartCode="55",}
                }
            });


            ListDepartments.Add(new BaoAnCenterHospitalDepartment()
            {
                DepartName = "眼、耳鼻咽喉、皮肤美容科",
                DepartCode = null,
                ChildDepartments = new List<BaoAnCenterHospitalDepartment>()
                {
                    new BaoAnCenterHospitalDepartment() { DepartName = "眼科综合门诊", DepartCode="65"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "飞秒激光专科", DepartCode="65"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "眼视光专科（近视/斜视/弱视）", DepartCode="65"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "白内障专科", DepartCode="65"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "眼底病专科", DepartCode="65"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "耳鼻咽喉科综合门诊", DepartCode="64"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "耳聋耳鸣眩晕专科", DepartCode="64"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "皮肤病综合门诊", DepartCode="59"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "痤疮专科", DepartCode="59"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "激光美容（光子嫩肤/皮秒祛斑/ 超冰脱毛）", DepartCode="59"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "注射美容（肉毒素除皱/玻尿酸填充/埋线年轻化）", DepartCode="59"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "纤体", DepartCode="59"}
                }
            });

            ListDepartments.Add(new BaoAnCenterHospitalDepartment()
            {
                DepartName = "康复科、中医科",
                DepartCode = null,
                ChildDepartments = new List<BaoAnCenterHospitalDepartment>()
                {
                    new BaoAnCenterHospitalDepartment() { DepartName = "颈肩腰腿痛专科", DepartCode="75"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "推拿专科门诊", DepartCode="75"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "针灸专科门诊", DepartCode="75"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "麻醉疼痛专科", DepartCode="75"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "肝病门诊", DepartCode="186"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "失眠门诊", DepartCode="186"},
                    new BaoAnCenterHospitalDepartment() { DepartName = "脾胃病门诊", DepartCode="186"},
                }
            });

            ListDepartments.Add(new BaoAnCenterHospitalDepartment()
            {
                DepartName = "感染性疾病科",
                DepartCode = null,
                ChildDepartments = new List<BaoAnCenterHospitalDepartment>()
                {
                    new BaoAnCenterHospitalDepartment() { DepartName = "发热门诊", DepartCode="63"},
                }
            });

        }


    }
}



