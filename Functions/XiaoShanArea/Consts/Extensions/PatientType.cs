using System;
using System.Collections.Generic;
using YuanTu.Core.Log;

namespace YuanTu.YuHangArea.Consts.Extensions
{
      public  class PatientTypeParse
    {
        private static readonly Dictionary<string, string> SmkTypeDictionary = new Dictionary<string, string>
        {
            {"330100", "市医保"},
            {"330199", "市医保"},
            {"330102", "市医保"},
            {"330103", "市医保"},
            {"330104", "市医保"},
            {"330105", "市医保"},
            {"330106", "市医保"},
            {"330108", "市医保"},
            {"339900", "省医保"},
            {"330109", "萧山医保"},
            {"330185", "临安市医保"},
            {"330183", "富阳市医保"},
            {"330182", "建德市医保"},
            {"330127", "淳安县医保"},
            {"330122", "桐庐县医保"},
            {"330110", "余杭医保"},
        };
          private static readonly Dictionary<string, string> StTypeDictionary = new Dictionary<string, string>
          {
              {"健康卡", "1"}, //自费
              {"省医保", "15"},
              {"市医保", "84"},
              {"省异地", "55"},   //省一卡通
              {"市异地", "56"},  //市一卡通
              {"萧山医保", "88"},
          };

          private static readonly Dictionary<string, string> CyTypeDictionary = new Dictionary<string, string>
          {
              {"健康卡", "100"},
              {"省医保", "15"},
              {"市医保", "56"},//84改成56
              {"省异地", "55"},//省一卡通
              {"市异地", "56"},//市一卡通
              {"萧山医保", "88"},
          };

        public static string SMK_PatintTypePare(string code)
        {

            if (SmkTypeDictionary.ContainsKey(code))
            {
                return SmkTypeDictionary[code];
            }
            else if (code.Substring(0, 4) == "3301")
            {
                return "市异地";
            }
            else if (code.Substring(0, 2) == "33")
            {
                return "省异地";
            }

            return "自费";


        }

        public static string ST_PatintTypePare(string code)
        {
            try
            {
                return StTypeDictionary[code];
            }
            catch (Exception ex)
            {
                Logger.Main.Error(ex.Message + "\n" + ex.StackTrace);
                return null;
            }
        }

        public static string CY_PatintTypePare(string code)
        {
            try
            {
                return CyTypeDictionary[code];
            }
            catch (Exception ex)
            {
                Logger.Main.Error(ex.Message + "\n" + ex.StackTrace);
                return null;
            }
        }

    }
}
