using YuanTu.QDArea.QingDaoSiPay.Common;

namespace YuanTu.QDArea.QingDaoSiPay.Models.Receive
{
    /// <summary>
    /// 人员基本信息
    /// </summary>
    public class BasPerInfo
    {
        #region 构造
        public BasPerInfo()
        {
            personNo = string.Empty;//个人编号
            IDCard = string.Empty;//身份证
            name = string.Empty;//姓名
            sex = string.Empty;//性别       1：男；2：女；
            birthday = string.Empty;//出生日期
            unitNO = string.Empty;//单位编号
            unitName = string.Empty;//单位名称
            /// <summary>
            /// 待遇信息
            /// </summary>
            treatInfo.area = string.Empty;//参保病人区划
            treatInfo.perType = string.Empty;//医疗人员类别
            /// <summary>
            /// 定点信息
            /// </summary>
            ddxx.DDYY1 = string.Empty;
            ddxx.DDLX1 = string.Empty;
            ddxx.DDYY2 = string.Empty;
            ddxx.DDLX2 = string.Empty;
            ddxx.DDYY3 = string.Empty;
            ddxx.DDLX3 = string.Empty;
            /// <summary>
            /// 在院状态
            /// </summary>
            inState.hosName = string.Empty;
            inState.type = string.Empty;
            inState.inDate = string.Empty;
            /// <summary>
            /// 缴费情况
            /// </summary>
            payInfo.JanInfo = string.Empty;
            payInfo.FebInfo = string.Empty;
            payInfo.MarInfo = string.Empty;
            payInfo.AprInfo = string.Empty;
            payInfo.MayInfo = string.Empty;
            payInfo.JunInfo = string.Empty;
            payInfo.JulInfo = string.Empty;
            payInfo.AugInfo = string.Empty;
            payInfo.SepInfo = string.Empty;
            payInfo.OctInfo = string.Empty;
            payInfo.NovInfo = string.Empty;
            payInfo.DecInfo = string.Empty;
            /// <summary>
            /// 门诊签约信息
            /// </summary>
            outSignInfo.cliniCode = string.Empty; //签约社区编码     
            outSignInfo.signType = string.Empty; //签约类型1：家庭医生；2：社区门诊；3：农民工；4：大学生集体 
            outSignInfo.signBeginDate = string.Empty; //签约开始日期	时间日期格式B      
            outSignInfo.signEndDate = string.Empty; //签约截止日期	8	时间日期格式B       
            outSignInfo.treatBeginDate = string.Empty; //待遇享受开始日期	8	时间日期格式B  
            outSignInfo.treatEndDate = string.Empty; //待遇享受截止日期	8	时间日期格式B  


            length = 0;
            for (int i = 0; i < memberLen.Length; i++)
            {
                length = length + memberLen[i];
            }
        }
        //构造
        public BasPerInfo(string messBody)
        {
            int startChar = 0;
            messBody.Trim();

            personNo = Comm.GetSubString(messBody, startChar, memberLen[0]).Trim();//个人编号


            startChar = startChar + memberLen[0];
            IDCard = Comm.GetSubString(messBody, startChar, memberLen[1]).Trim();//身份证

            startChar = startChar + memberLen[1];
            name = Comm.GetSubString(messBody, startChar, memberLen[2]).Trim();//姓名

            startChar = startChar + memberLen[2];
            sex = Comm.GetSubString(messBody, startChar, memberLen[3]).Trim();//性别       1：男；2：女；

            startChar = startChar + memberLen[3];
            birthday = Comm.GetSubString(messBody, startChar, memberLen[4]).Trim();//出生日期

            startChar = startChar + memberLen[4];
            unitNO = Comm.GetSubString(messBody, startChar, memberLen[5]).Trim();//单位编号

            startChar = startChar + memberLen[5];
            unitName = Comm.GetSubString(messBody, startChar, memberLen[6]).Trim();//单位名称

            /// <summary>
            /// 待遇信息
            /// </summary>
            startChar = startChar + memberLen[6];
            treatInfo.area = Comm.GetSubString(messBody, startChar, memberLen[7]).Trim();//参保病人区划
            startChar = startChar + memberLen[7];
            treatInfo.perType = Comm.GetSubString(messBody, startChar, memberLen[8]).Trim();//医疗人员类别

            /// <summary>
            /// 定点信息
            /// </summary>
            startChar = startChar + memberLen[8];
            ddxx.DDYY1 = Comm.GetSubString(messBody, startChar, memberLen[9]).Trim();
            startChar = startChar + memberLen[9];
            ddxx.DDLX1 = Comm.GetSubString(messBody, startChar, memberLen[10]).Trim();
            startChar = startChar + memberLen[10];
            ddxx.DDYY2 = Comm.GetSubString(messBody, startChar, memberLen[11]).Trim();
            startChar = startChar + memberLen[11];
            ddxx.DDLX2 = Comm.GetSubString(messBody, startChar, memberLen[12]).Trim();
            startChar = startChar + memberLen[12];
            ddxx.DDYY3 = Comm.GetSubString(messBody, startChar, memberLen[13]).Trim();
            startChar = startChar + memberLen[13];
            ddxx.DDLX3 = Comm.GetSubString(messBody, startChar, memberLen[14]).Trim();

            /// <summary>
            /// 在院状态
            /// </summary>
            startChar = startChar + memberLen[14];
            inState.hosName = Comm.GetSubString(messBody, startChar, memberLen[15]).Trim();
            startChar = startChar + memberLen[15];
            inState.type = Comm.GetSubString(messBody, startChar, memberLen[16]).Trim();
            startChar = startChar + memberLen[16];
            inState.inDate = Comm.GetSubString(messBody, startChar, memberLen[17]).Trim();
            startChar = startChar + memberLen[17];
            inState.treatNo = Comm.GetSubString(messBody, startChar, memberLen[18]).Trim();

            /// <summary>
            /// 缴费情况
            /// </summary>
            startChar = startChar + memberLen[18];
            payInfo.JanInfo = Comm.GetSubString(messBody, startChar, memberLen[19]).Trim();
            startChar = startChar + memberLen[19];
            payInfo.FebInfo = Comm.GetSubString(messBody, startChar, memberLen[20]).Trim();
            startChar = startChar + memberLen[20];
            payInfo.MarInfo = Comm.GetSubString(messBody, startChar, memberLen[21]).Trim();
            startChar = startChar + memberLen[21];
            payInfo.AprInfo = Comm.GetSubString(messBody, startChar, memberLen[22]).Trim();
            startChar = startChar + memberLen[22];
            payInfo.MayInfo = Comm.GetSubString(messBody, startChar, memberLen[23]).Trim();
            startChar = startChar + memberLen[23];
            payInfo.JunInfo = Comm.GetSubString(messBody, startChar, memberLen[24]).Trim();
            startChar = startChar + memberLen[24];
            payInfo.JulInfo = Comm.GetSubString(messBody, startChar, memberLen[25]).Trim();
            startChar = startChar + memberLen[25];
            payInfo.AugInfo = Comm.GetSubString(messBody, startChar, memberLen[26]).Trim();
            startChar = startChar + memberLen[26];
            payInfo.SepInfo = Comm.GetSubString(messBody, startChar, memberLen[27]).Trim();
            startChar = startChar + memberLen[27];
            payInfo.OctInfo = Comm.GetSubString(messBody, startChar, memberLen[28]).Trim();
            startChar = startChar + memberLen[28];
            payInfo.NovInfo = Comm.GetSubString(messBody, startChar, memberLen[29]).Trim();
            startChar = startChar + memberLen[29];
            payInfo.DecInfo = Comm.GetSubString(messBody, startChar, memberLen[30]).Trim();

            /// <summary>
            /// 门诊签约信息
            /// </summary>
            startChar = startChar + memberLen[30];
            outSignInfo.cliniCode = Comm.GetSubString(messBody, startChar, memberLen[31]).Trim(); //签约社区编码     
            startChar = startChar + memberLen[31];
            outSignInfo.signType = Comm.GetSubString(messBody, startChar, memberLen[32]).Trim(); //签约类型1：家庭医生；2：社区门诊；3：农民工；4：大学生集体 
            startChar = startChar + memberLen[32];
            outSignInfo.signBeginDate = Comm.GetSubString(messBody, startChar, memberLen[33]).Trim(); //签约开始日期	时间日期格式B      
            startChar = startChar + memberLen[33];
            outSignInfo.signEndDate = Comm.GetSubString(messBody, startChar, memberLen[34]).Trim(); //签约截止日期	8	时间日期格式B       
            startChar = startChar + memberLen[34];
            outSignInfo.treatBeginDate = Comm.GetSubString(messBody, startChar, memberLen[35]).Trim(); //待遇享受开始日期	8	时间日期格式B  
            startChar = startChar + memberLen[35];
            outSignInfo.treatEndDate = Comm.GetSubString(messBody, startChar, memberLen[36]).Trim(); //待遇享受截止日期	8	时间日期格式B  

            length = 0;
            for (int i = 0; i < memberLen.Length; i++)
            {
                length = length + memberLen[i];
            }
        }
        #endregion

        #region 变量
        private int[] memberLen = { 16, 18, 20, 1, 8, 10, 60, 20, 20, 5, 1, 5, 1, 5, 1, 60, 20, 8, 16, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 5, 1, 8, 8, 8, 8 };
        static public int length = 0;

        public string personNo;//个人编号
        public string IDCard;//身份证
        public string name;//姓名
        public string sex;//性别       1：男；2：女；
        public string birthday;//出生日期
        public string unitNO;//单位编号
        public string unitName;//单位名称

        /// <summary>
        /// 待遇信息
        /// </summary>
        public class DYXX
        {
            public string area;//参保病人区划
            public string perType;//参保人员类别  
        };
        /// <summary>
        /// 待遇信息
        /// </summary>
        public DYXX treatInfo = new DYXX();

        /// <summary>
        /// 定点信息
        /// </summary>
        public class DDXX
        {
            public string DDYY1;//定点医院1
            public string DDLX1;//定点类型1（1：离休定点；（目前仅离休定点一种情况））
            public string DDYY2;//定点医院1
            public string DDLX2;//定点类型1
            public string DDYY3;//定点医院1
            public string DDLX3;//定点类型1
        };
        public DDXX ddxx = new DDXX();

        /// <summary>
        /// 在院状态
        /// </summary>
        public class InState
        {
            public string hosName;//医院名称
            public string type;//医疗类别
            public string inDate;//就诊日期
            public string treatNo;//就诊编号
        };
        public InState inState = new InState();

        /// <summary>
        /// 缴费情况
        /// </summary>
        public class PayInfo
        {
            public string JanInfo;//一月（1：正常交；2：未缴费；3：已退收；）
            public string FebInfo;//二月
            public string MarInfo;//三月
            public string AprInfo;//四月
            public string MayInfo;//五月
            public string JunInfo;//六月
            public string JulInfo;//七月
            public string AugInfo;//八月
            public string SepInfo;//九月
            public string OctInfo;//十月
            public string NovInfo;//十一月
            public string DecInfo;//十一月
        };
        public PayInfo payInfo = new PayInfo();

        /// <summary>
        /// 门诊签约信息
        /// </summary>
        public class OutSignInfo
        {
            public string cliniCode;//签约社区编码
            public string signType;//签约类型1：家庭医生；2：社区门诊；3：农民工；4：大学生集体
            public string signBeginDate;//签约开始日期	时间日期格式B
            public string signEndDate;//签约截止日期	8	时间日期格式B
            public string treatBeginDate;//待遇享受开始日期	8	时间日期格式B
            public string treatEndDate;//待遇享受截止日期	8	时间日期格式B
        };
        public OutSignInfo outSignInfo = new OutSignInfo();
        /// <summary>
        /// 离休账户余额
        /// </summary>
        public string accountRemain = string.Empty;
        #endregion

        #region 方法
        public string toMessage()
        {
            return null;
        }
        #endregion  
    }
}
