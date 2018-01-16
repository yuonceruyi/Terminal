namespace YuanTu.Consts.UserCenter.Entities
{
    public class RegTypeOutParams
    {
        /// <summary>
        ///     预约挂号类型
        /// </summary>
        public int regType { get; set; }

        /// <summary>
        ///     预约挂号类别描述
        /// </summary>
        public string desc { get; set; }

        /// <summary>
        ///     剩余号源数量
        /// </summary>
        public long remainderNumber { get; set; }
    }
}