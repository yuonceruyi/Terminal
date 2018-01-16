using System;
using System.Collections.Generic;

namespace YuanTu.QDArea.QingDaoSiPay.Models
{
    /// <summary>
    /// 【功能说明：回滚字典信息】
    /// 【创建人：郭存磊】
    /// 【创建时间：2015-12-13】
    /// </summary>
    public class BizParam
    {
        /// <summary>
        /// 参数字典
        /// </summary>
        private Dictionary<string, Object> paramDic = new Dictionary<string, Object>();

        #region IBizParam 成员

        public Object this[string key]
        {
            get
            {
                if (this.paramDic.ContainsKey(key))
                {
                    return this.paramDic[key];
                }
                else
                {
                    return null;
                }

            }
        }

        #endregion

        #region IBizParam 成员


        public void SetParam(string key, Object value)
        {
            if (this.paramDic.ContainsKey(key))
            {
                this.paramDic[key] = value;
            }
            else
            {
                this.paramDic.Add(key, value);
            }
        }

        #endregion
    }
}
