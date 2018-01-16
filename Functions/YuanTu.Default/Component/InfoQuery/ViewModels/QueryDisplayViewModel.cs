using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Component.InfoQuery.ViewModels
{
    public class QueryDisplayViewModel : ViewModelBase
    {
        private string _key;
        private string _searchText=String.Empty;
        private ListDataGrid.PageDataEx _medicineItemsData;
        private string _hint;
        public override string Title => "搜索并显示";

        public string Hint
        {
            get
            {
               
                return _hint;
            }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        [Dependency]
        public IMedicineModel MedicineModel { get; set; }
            
        public ListDataGrid.PageDataEx MedicineItemsData
        {
            get { return _medicineItemsData; }
            set { _medicineItemsData = value;OnPropertyChanged(); }
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            HideNavigating = true;

            base.OnEntered(navigationContext);
            if (MedicineModel.Res药品项目查询?.success!=true)
            {
                DoCommand(lp =>
                {
                    MedicineModel.Req药品项目查询 = new req药品项目查询
                    {
                        pinyinCode = null
                    };
                    MedicineModel.Res药品项目查询 = DataHandlerEx.药品项目查询(MedicineModel.Req药品项目查询);
                    if (MedicineModel.Res药品项目查询.success)
                    {
                        if (MedicineModel.Res药品项目查询.data == null || MedicineModel.Res药品项目查询.data.Count == 0)
                        {
                            ShowAlert(false, "药品信息查询", "未查询到相关药品的信息(列表为空)");
                            return;
                        }
                        ReflashData();
                    }
                    else
                    {
                        ShowAlert(false, "药品信息查询", "未查询到相关药品的信息", debugInfo: MedicineModel.Res药品项目查询.msg);
                    }
                });
            }
        }

        private void ReflashData()
        {
            var data = MedicineModel.Res药品项目查询.data.Where(
                p => _searchText.IsNullOrWhiteSpace() || GetSpellCode(p.medicineName).ToUpper().Contains(_searchText.ToUpper()));
            MedicineItemsData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = data,
                Tag = ""
            };
            Hint = "药品信息"+(_searchText.IsNullOrWhiteSpace()?"":$" 搜索:{_searchText}");
        }

        public string Key
        {
           
            set
            {
                _key = value;
                if (_key== "←")
                {
                    if (_searchText.Length>0)
                    {
                        _searchText = _searchText.Remove(_searchText.Length - 1);
                    }
                }
                else
                {
                    _searchText += _key;
                }
                ReflashData();
            }
        }


        /// <summary>
        /// 在指定的字符串列表CnStr中检索符合拼音索引字符串
        /// </summary>
        /// <param name="CnStr">汉字字符串</param>
        /// <returns>相对应的汉语拼音首字母串</returns>
        public static string GetSpellCode(string CnStr)
        {

            string strTemp = "";

            int iLen = CnStr.Length;

            int i = 0;

            for (i = 0; i <= iLen - 1; i++)
            {

                strTemp += GetCharSpellCode(CnStr.Substring(i, 1));
            }

            return strTemp;

        }
        /// <summary>
        /// 截取字符长度
        /// </summary>
        /// <param name="str">被截取的字符串</param>
        /// <param name="len">所截取的长度</param>
        /// <returns>子字符串</returns>
        public static string CutString(string str, int len)
        {
            if (string.IsNullOrEmpty(str) || len <= 0)
            {
                return string.Empty;
            }

            int l = str.Length;


            #region 计算长度
            int clen = 0;
            while (clen < len && clen < l)
            {
                //每遇到一个中文，则将目标长度减一。
                if ((int)str[clen] > 128) { len--; }
                clen++;
            }
            #endregion

            if (clen < l)
            {
                return str.Substring(0, clen) + "...";
            }
            else
            {
                return str;
            }
        }
        /// <summary>
        /// 得到一个汉字的拼音第一个字母，如果是一个英文字母则直接返回大写字母
        /// </summary>
        /// <param name="CnChar">单个汉字</param>
        /// <returns>单个大写字母</returns>
        private static string GetCharSpellCode(string CnChar)
        {

            long iCnChar;

            byte[] ZW = System.Text.Encoding.Default.GetBytes(CnChar);

            //如果是字母，则直接返回首字母

            if (ZW.Length == 1)
            {

                return CutString(CnChar.ToUpper(), 1);

            }
            else
            {

                // get the array of byte from the single char

                int i1 = (short)(ZW[0]);

                int i2 = (short)(ZW[1]);

                iCnChar = i1 * 256 + i2;

            }

            // iCnChar match the constant

            if ((iCnChar >= 45217) && (iCnChar <= 45252))
            {

                return "A";

            }

            else if ((iCnChar >= 45253) && (iCnChar <= 45760))
            {

                return "B";

            }
            else if ((iCnChar >= 45761) && (iCnChar <= 46317))
            {

                return "C";

            }
            else if ((iCnChar >= 46318) && (iCnChar <= 46825))
            {

                return "D";

            }
            else if ((iCnChar >= 46826) && (iCnChar <= 47009))
            {

                return "E";

            }
            else if ((iCnChar >= 47010) && (iCnChar <= 47296))
            {

                return "F";

            }
            else if ((iCnChar >= 47297) && (iCnChar <= 47613))
            {

                return "G";

            }
            else if ((iCnChar >= 47614) && (iCnChar <= 48118))
            {

                return "H";

            }
            else if ((iCnChar >= 48119) && (iCnChar <= 49061))
            {

                return "J";

            }
            else if ((iCnChar >= 49062) && (iCnChar <= 49323))
            {

                return "K";

            }
            else if ((iCnChar >= 49324) && (iCnChar <= 49895))
            {

                return "L";

            }
            else if ((iCnChar >= 49896) && (iCnChar <= 50370))
            {

                return "M";

            }
            else if ((iCnChar >= 50371) && (iCnChar <= 50613))
            {

                return "N";

            }
            else if ((iCnChar >= 50614) && (iCnChar <= 50621))
            {

                return "O";

            }
            else if ((iCnChar >= 50622) && (iCnChar <= 50905))
            {

                return "P";

            }
            else if ((iCnChar >= 50906) && (iCnChar <= 51386))
            {

                return "Q";

            }
            else if ((iCnChar >= 51387) && (iCnChar <= 51445))
            {

                return "R";

            }
            else if ((iCnChar >= 51446) && (iCnChar <= 52217))
            {

                return "S";

            }
            else if ((iCnChar >= 52218) && (iCnChar <= 52697))
            {

                return "T";

            }
            else if ((iCnChar >= 52698) && (iCnChar <= 52979))
            {

                return "W";

            }
            else if ((iCnChar >= 52980) && (iCnChar <= 53640))
            {

                return "X";

            }
            else if ((iCnChar >= 53689) && (iCnChar <= 54480))
            {

                return "Y";

            }
            else if ((iCnChar >= 54481) && (iCnChar <= 55289))
            {

                return "Z";

            }
            else

                return ("?");

        }
    }


}
