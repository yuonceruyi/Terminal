namespace YuanTu.Consts.UserCenter.Entities
{
    public class Doct : BaseDO
    {
        /// <summary>
        ///     手动设置 排序序号
        /// </summary>
        public int serialNum { get; set; } = 999;

        /// <summary>
        ///     医生代码
        /// </summary>
        public string doctCode { get; set; }

        /// <summary>
        ///     医生姓名
        /// </summary>
        public string doctName { get; set; }

        /// <summary>
        ///     科室名称的全拼
        /// </summary>
        public string doctPY { get; set; }

        /// <summary>
        ///     科室名称的简拼
        /// </summary>
        public string doctSimplePY { get; set; }

        /// <summary>
        ///     性别  汉字：男、女
        /// </summary>
        public string sex { get; set; }

        /// <summary>
        ///     医生头像  短路径
        /// </summary>
        public string doctLogo { get; set; }

        /// <summary>
        ///     医生级别： 教授
        /// </summary>
        public string doctLevel { get; set; }

        /// <summary>
        ///     医生职称  对应 HisScheduleInfoItemDO  和 ScheduleInfoItemDO 里的 doctTech
        /// </summary>
        public string doctProfe { get; set; }

        /// <summary>
        ///     医生特长
        /// </summary>
        public string doctSpec { get; set; }

        /// <summary>
        ///     医生介绍
        /// </summary>
        public string doctIntro { get; set; }

        /// <summary>
        ///     医院id
        /// </summary>
        public long corpId { get; set; }

        /// <summary>
        ///     医院id在排班系统中的编号
        /// </summary>
        public string corpCode { get; set; }

        /// <summary>
        ///     医院名称   表里面无此字段，只在接口返回数据时，加上该字段的数据即可
        /// </summary>
        public string corpName { get; set; }

        /// <summary>
        ///     科室代码
        /// </summary>
        public string deptCode { get; set; }

        /// <summary>
        ///     科室名称
        /// </summary>
        public string deptName { get; set; }

        /// <summary>
        ///     状态 1：正常；2：不正常
        /// </summary>
        public int status { get; set; }

        /// <summary>
        ///     医生电话
        /// </summary>
        public string doctPhoneNum { get; set; }

        /// <summary>
        ///     医生工号
        /// </summary>
        public string doctEmployeeNum { get; set; }

        /// <summary>
        ///     医生头像全路径  接口返回时使用  阿里云地址
        /// </summary>
        public string doctPictureUrl { get; set; }

        /// <summary>
        ///     图片代理地址，用于 自助机无法访问 外网的情况
        ///     具体使用：各个医联体机房部署一个 nginx 代理；
        ///     医院自助机 访问 nginx 的代理地址，nginx 代理进行转换，指向阿里云的图片服务器
        ///     医生头像全路径  接口返回时使用  nginx 代理地址
        /// </summary>
        public string doctPictureIntranetUrl { get; set; }

        /// <summary>
        ///     职称，为了取到网关的数据，所以加这个字段，和doctProfe性质一样
        /// </summary>
        public string doctTech { get; set; }
    }
}