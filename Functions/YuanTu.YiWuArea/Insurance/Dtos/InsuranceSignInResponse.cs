using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.YiWuArea.Insurance.Dtos
{
    public class InsuranceSignInResponse
    {
        /// <summary>
        /// 请求的操作员号
        /// </summary>
        public string SiOperator { get; set; }
        /// <summary>
        /// 业务周期号
        /// </summary>
        public string SiToken { get; set; }
    }
}
