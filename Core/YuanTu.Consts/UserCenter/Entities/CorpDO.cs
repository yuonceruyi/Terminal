namespace YuanTu.Consts.UserCenter.Entities
{
    public class CorpDO : BaseDO
    {
        /// <summary>
        /// </summary>
        public long id { get; set; }

        /// <summary>
        ///     手动设置 医院的id
        /// </summary>
        public long corpId { get; set; }

        /// <summary>
        ///     手动设置 医院的排序序号
        /// </summary>
        public long serialNum { get; set; }

        /// <summary>
        ///     医院类型 1：省级医院； 2：市级医院
        /// </summary>
        public byte type { get; set; }

        /// <summary>
        ///     医院名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        ///     医院标签
        /// </summary>
        public string corpTags { get; set; }

        /// <summary>
        /// </summary>
        public string city { get; set; }

        /// <summary>
        /// </summary>
        public string province { get; set; }

        /// <summary>
        ///     医院区域 如上城区 下城区
        /// </summary>
        public string area { get; set; }

        /// <summary>
        ///     详细地址
        /// </summary>
        public string address { get; set; }

        /// <summary>
        ///     银行类型
        /// </summary>
        public int bankCode { get; set; }

        /// <summary>
        ///     银行账户
        /// </summary>
        public string bankAccount { get; set; }

        /// <summary>
        ///     组织结构代码
        /// </summary>
        public string corpCode { get; set; }

        /// <summary>
        ///     医院缩写简称
        /// </summary>
        public string abbreviationName { get; set; }

        /// <summary>
        ///     医联体id
        /// </summary>
        public long corpUnionId { get; set; }

        /// <summary>
        ///     医院logo
        /// </summary>
        public string corpLogo { get; set; }

        /// <summary>
        ///     医院网关ip
        /// </summary>
        public string corpIp { get; set; }

        /// <summary>
        ///     医院端口
        /// </summary>
        public string corpPort { get; set; }

        /// <summary>
        ///     交易代码
        /// </summary>
        public string transCode { get; set; }

        /// <summary>
        ///     分院编号
        /// </summary>
        public string hisCode { get; set; }

        /// <summary>
        ///     操作者ID
        /// </summary>
        public string operId { get; set; }

        /// <summary>
        ///     终端设备信息
        /// </summary>
        public string deviceInfo { get; set; }

        /// <summary>
        ///     功能内容：json 数组，包含的内容 如 [1,2] ，里面的每个数字代表具体功能 1 : 挂号；2：预约挂号结构查询；3：充值；4：缴费； 5： 取报告单；6：医院导航；7：排班叫号；8:住院预缴{get;set;}
        ///     9:住院清单
        /// </summary>
        public string corpFunction { get; set; }

        /// <summary>
        ///     手动配置的json数据，供APP调用
        /// </summary>
        public string functionJson { get; set; }

        /// <summary>
        ///     预约json数据
        /// </summary>
        public string appointmentJson { get; set; }

        /// <summary>
        ///     挂号json数据
        /// </summary>
        public string registerJson { get; set; }

        /// <summary>
        ///     支付方式，json 数组，包含的内容 如 [1,2,3]：1，支付宝；2，微信支付；3，余额
        /// </summary>
        public string payType { get; set; }

        /// <summary>
        ///     付款顺序：1，先支付后挂号；2，先挂号后支付
        /// </summary>
        public int paySequence { get; set; }

        /// <summary>
        ///     1普通挂号，2专家挂号，3名医挂号，4普通预约，5专家预约，6名医预约
        /// </summary>
        public string corpRegister { get; set; }

        /// <summary>
        ///     挂号托收 1：挂号托收，2其他不托收
        /// </summary>
        public int corpGuaEntrust { get; set; }

        /// <summary>
        ///     医院详情链接，通过拼链接访问
        /// </summary>
        public long corpNewsId { get; set; }

        /// <summary>
        ///     1: 正常；2：不正常
        /// </summary>
        public int status { get; set; }

        /// <summary>
        ///     0：未上线{get;set;}1：上线；
        /// </summary>
        public int online { get; set; }

        /// <summary>
        ///     预约时间文案
        /// </summary>
        public string timeCopyJson { get; set; }

        /// <summary>
        ///     线上测试科室，以逗号分隔，5个左右
        /// </summary>
        public string testDepts { get; set; }

        /// <summary>
        ///     是否支持密码取号,1需要密码，2不需要
        /// </summary>
        public int needPassword { get; set; } = 1;

        /// <summary>
        ///     排班获取规则,1通过科室获取,2通过医生获取
        /// </summary>
        public int scheduleRule { get; set; } = 1;

        /// <summary>
        ///     网关医院编号挂靠的医院id，如果为0不存在分院
        /// </summary>
        public long parentCorpId { get; set; } = 0;

        /// <summary>
        ///     挂号文案
        /// </summary>
        public string registerTimeCopy { get; set; }

        /// <summary>
        ///     预约文案
        /// </summary>
        public string appointmentTimeCopy { get; set; }

        /// <summary>
        ///     医院电话
        /// </summary>
        public string corpPhone { get; set; }

        /// <summary>
        ///     显示在APP上的引导性文案信息
        /// </summary>
        public string guideCopyJson { get; set; }

        /// <summary>
        ///     经度  默认为空字符串
        /// </summary>
        public string lng { get; set; }

        /// <summary>
        ///     纬度  默认为空字符串
        /// </summary>
        public string lat { get; set; }
    }
}