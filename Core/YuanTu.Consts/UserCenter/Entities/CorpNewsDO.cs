namespace YuanTu.Consts.UserCenter.Entities
{
    public class CorpNewsDO
    {
        /// <summary>
        /// </summary>
        public long id { get; set; }

        /// <summary>
        ///     栏目ID
        /// </summary>
        public long classifyId { get; set; }

        /// <summary>
        ///     医院id
        /// </summary>
        public long hospitalId { get; set; }

        /// <summary>
        ///     医联体ID
        /// </summary>
        public long unionId { get; set; }

        /// <summary>
        ///     标题 可以不写title，直接图片
        /// </summary>
        public string title { get; set; }

        /// <summary>
        ///     标题图片
        /// </summary>
        public string titleImg { get; set; }

        /// <summary>
        ///     首页轮播图URL地址
        /// </summary>
        public string homeUrl { get; set; }

        /// <summary>
        ///     资讯id 可以为空，为空代表没有内容
        /// </summary>
        public long newsId { get; set; }

        /// <summary>
        ///     排序字段
        /// </summary>
        public int sort { get; set; }

        /// <summary>
        ///     1: 广告信息{get;set;} 2 ：医院健康信息；3:新闻动态；4：通知公告；
        /// </summary>
        public int type { get; set; }

        /// <summary>
        ///     1：草稿：2：正式稿；3：删除
        /// </summary>
        public char status { get; set; }

        /// <summary>
        ///     文章摘要
        /// </summary>
        public string summary { get; set; }
    }
}