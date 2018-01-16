namespace YuanTu.Consts.UserCenter
{
    public abstract class UserCenterResponse
    {
        public bool success { get; set; }
        public string msg { get; set; }
        /// <summary>
        /// 服务器时间(1970.1.1到现在的毫秒数)
        /// </summary>
        public long startTime { get; set; }
        /// <summary>
        /// 该操作服务端耗时(毫秒)
        /// </summary>
        public long timeConsum { get; set; }

        /// <summary>
        /// 消息码，标志异常来源，当code为0时，其异常信息是可信的
        /// </summary>
        public string resultCode { get; set; }
    }
}
