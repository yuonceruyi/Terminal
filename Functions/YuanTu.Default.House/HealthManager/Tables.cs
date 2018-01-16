using System.Collections.Generic;
using YuanTu.Default.House.HealthManager.Base;
namespace YuanTu.Default.House.HealthManager
{
	
    public partial class 是否建档信息 : ResponseBase
    {
        public string gmtModify { get; set; }
        public string gmtCreate { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string idNo { get; set; }
        public string cardNo { get; set; }
        public string cardType { get; set; }
        public string sex { get; set; }
        public string age { get; set; }
        public string birthday { get; set; }
        public string nation { get; set; }
        public string addr { get; set; }
        public string expire { get; set; }
        public string photo { get; set; }
        public string phone { get; set; }
        public string usesurvey { get; set; }
    }

    public partial class 测量组子集 : ResponseBase
    {
        public string childName { get; set; }
        public string dataStr { get; set; }
        public string unit { get; set; }
    }

    public partial class 测量组 : ResponseBase
    {
        public string groupName { get; set; }
        public List<测量组子集> childList { get; set; }
    }

    public partial class 查询体检报告单 : ResponseBase
    {
        public string gmtModify { get; set; }
        public string gmtCreate { get; set; }
        public string id { get; set; }
        public string healthUserId { get; set; }
        public string sourceCode { get; set; }
        public string name { get; set; }
        public string age { get; set; }
        public string sex { get; set; }
        public string cardNo { get; set; }
        public string phone { get; set; }
        public string date { get; set; }
        public List<测量组> groupList { get; set; }
    }

    public partial class 查询体检报告单分页数据 : ResponseBase
    {
        public string pageSize { get; set; }
        public string currentPage { get; set; }
        public string totalPageNum { get; set; }
        public string totalRecrodNum { get; set; }
        public List<查询体检报告单> records { get; set; }
    }


}