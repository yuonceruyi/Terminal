using System;
using System.Collections.Generic;
using System.Text;
using YuanTu.QDArea.QingDaoSiPay.Common;
using YuanTu.QDArea.QingDaoSiPay.Models;
using YuanTu.QDArea.QingDaoSiPay.Models.DownLoad;
using YuanTu.QDArea.QingDaoSiPay.Models.Receive;
using YuanTu.QDArea.QingDaoSiPay.Models.Send;

namespace YuanTu.QDArea.QingDaoSiPay
{
    /// <summary>
    /// 外部函数类
    /// </summary>
    public class Function
    {
        #region 变量、属性
        private static string errMsg = string.Empty;
        public static string ErrMsg
        {
            get
            {
                return errMsg;
            }
        }

        /// <summary>
        /// 回滚撤队列
        /// </summary>
        private static List<BizParam> rollBackLine = new List<BizParam>();
        #endregion

        #region 基础函数
        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            SiSet.init();
        }
        public static long Commit()
        {
            //回滚撤队列
            rollBackLine.Clear();
            return 1;
        }
        public static long Rollback()
        {
            string messageType = string.Empty;           
            string receiveMess = string.Empty;
            string exchangeMess = string.Empty;          
            //调用回滚列表
            if (rollBackLine.Count > 0)
            {
                //调用各撤消业务列表
                foreach (BizParam param in rollBackLine)
                {
                    messageType = param["ID"].ToString();                   
                    switch (messageType)
                    {
                        case "YL001":
                            {
                                BillPayCancelSend billPayCancelSend = param["BillPayCancelSend"] as BillPayCancelSend;
                                bool isSuccess = Comm.SendMessage("YL003", billPayCancelSend.toMessage(), ref receiveMess, ref exchangeMess);
                                if (!isSuccess)
                                {
                                    Comm.writeLog("订单支付-->撤销失败！" + receiveMess); //日志记录
                                    rollBackLine.Clear();
                                    return -1;
                                }
                                else
                                {
                                    BillPayReceive billPayReceive = new BillPayReceive(receiveMess);
                                    if (billPayReceive.State != "0001")
                                    {
                                        Comm.writeLog("订单支付-->撤销失败！" + receiveMess); //日志记录
                                        rollBackLine.Clear();
                                        return -1;
                                    }
                                    else
                                    {
                                        Comm.writeLog("订单支付-->撤销成功！" + receiveMess); //日志记录
                                    }
                                }
                            }
                            break;                       

                    }

                }
            }
            //清队列
            rollBackLine.Clear();
            return 0;
        }
        #endregion 基础函数

        #region 功能函数
        /// <summary>
        /// 脱机读卡
        /// </summary>
        /// <param name="strIdenno"></param>
        /// <param name="strName"></param>
        /// <returns></returns>
        public static int ReadCard(ref string strIdenno, ref string strName)
        {
            StringBuilder idenno = new StringBuilder(19);
            StringBuilder name = new StringBuilder(31);
            Comm.HookStop();
            Comm.HookStart();
            int sign = ExternFun.ReadCard(idenno, name);
            Comm.HookStop();
            strIdenno = idenno.ToString().Trim();
            strName = name.ToString().Trim();            
            return sign;
        }
        /// <summary>
        /// 在线读卡
        /// </summary>
        /// <param name="basPerInfo"></param>
        /// <returns></returns>
        public static int ReadCard(ref BasPerInfo basPerInfo)
        {
            string receiveMess = string.Empty;
            string exchangeMess = string.Empty;
            CardInfo cardInfo = new CardInfo();
            bool isSuccess;

            //获取个人信息
            string messageType = "CX001";
            //JiangShuaiTEST
            //cardInfo.cardNo = "2217101";
            //isSuccess = Comm.SendMessage(messageType, cardInfo.toMessage(), ref receiveMess, ref exchangeMess);
            isSuccess = Comm.SendMessage(messageType, string.Empty, ref receiveMess, ref exchangeMess);

            //选择结算方式等登记信息
            if (isSuccess)
            {
                basPerInfo = new BasPerInfo(receiveMess);
            }
            else
            {
                errMsg = exchangeMess;
                return -1;
            }
            return 0;
        }
        /// <summary>
        /// 联合读卡(先联机，联机失败后再脱机)
        /// </summary>
        /// <param name="strIdenno"></param>
        /// <param name="strName"></param>
        /// <returns></returns>
        public static int ReadCardUnion(ref string strIdenno, ref string strName)
        {
            string receiveMess = string.Empty;
            string exchangeMess = string.Empty;
            string backCode = string.Empty;
            BasPerInfo basPerInfo = new BasPerInfo();
            bool isSuccess = false;

            //获取个人信息
            string messageType = "CX001";
            //JiangShuaiTEST
            //CardInfo cardInfo = new CardInfo();
            //cardInfo.cardNo = "2217101";
            //isSuccess = Comm.SendMessage(messageType, cardInfo.toMessage(), ref receiveMess, ref exchangeMess);
            isSuccess = Comm.SendMessage(messageType, string.Empty, ref receiveMess, ref exchangeMess, ref backCode);

            //选择结算方式等登记信息
            if (isSuccess)//联机成功
            {
                basPerInfo = new BasPerInfo(receiveMess);
                strIdenno = basPerInfo.IDCard;
                strName = basPerInfo.name;
            }
            else if (backCode == "P0091" || backCode == "P0092") //联机失败，脱机读卡
            {
                StringBuilder idenno = new StringBuilder(19);
                StringBuilder name = new StringBuilder(31);
                int sign = ExternFun.ReadCard(idenno, name);
                strIdenno = idenno.ToString().Trim();
                strName = name.ToString().Trim();
                return sign;
            }
            else
            {
                errMsg = exchangeMess;
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// 联合读卡(先联机，联机失败后再脱机)
        /// </summary>
        /// <param name="strIdenno"></param>
        /// <param name="strName"></param>
        /// <returns></returns>
        public static int ReadCardUnionExtend(ref string strIdenno, ref string strName,ref string strPerson, ref string strUnitName)
        {
            string receiveMess = string.Empty;
            string exchangeMess = string.Empty;
            string backCode = string.Empty;
            BasPerInfo basPerInfo = new BasPerInfo();
            bool isSuccess = false;

            //获取个人信息
            string messageType = "CX001";
            //JiangShuaiTEST
//            CardInfo cardInfo = new CardInfo();
//            cardInfo.cardNo = "84905775";
//            isSuccess = Comm.SendMessage(messageType, cardInfo.toMessage(), ref receiveMess, ref exchangeMess);
            isSuccess = Comm.SendMessage(messageType, string.Empty, ref receiveMess, ref exchangeMess, ref backCode);

            //选择结算方式等登记信息
            if (isSuccess)//联机成功
            {
                basPerInfo = new BasPerInfo(receiveMess);
                strIdenno = basPerInfo.IDCard;
                strName = basPerInfo.name;
                strPerson = basPerInfo.personNo;
                strUnitName = basPerInfo.unitName;
            }
            else if (backCode == "P0091" || backCode == "P0092") //联机失败，脱机读卡
            {
                StringBuilder idenno = new StringBuilder(19);
                StringBuilder name = new StringBuilder(31);
                int sign = ExternFun.ReadCard(idenno, name);
                strIdenno = idenno.ToString().Trim();
                strName = name.ToString().Trim();
                strPerson = string.Empty;
                strUnitName = string.Empty;
                return sign;
            }
            else
            {
                errMsg = exchangeMess;
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// 余额查询
        /// </summary>
        /// <param name="personNo"></param>
        /// <returns></returns>
        public static bool GetRemain(string personNo, ref decimal remain)
        {
            RemainSend remainSend = new RemainSend { CardNo = personNo };
            RemainReceive remainReceive = new RemainReceive();
            var sign = GetRemain(remainSend, ref remainReceive);
            if (sign != 0)
            {
                return false;
            }
            remain = remainReceive.Remain;
            return true;
        }
        /// <summary>
        /// 余额查询
        /// </summary>
        /// <param name="basPerInfo"></param>
        /// <returns></returns>
        public static int GetRemain(RemainSend remainSend, ref RemainReceive remainReceive)
        {
            string receiveMess = string.Empty;
            string exchangeMess = string.Empty;
            bool isSuccess;

            //获取个人信息
            string messageType = "YL002";

            isSuccess = Comm.SendMessage(messageType, remainSend.toMessage(), ref receiveMess, ref exchangeMess);

            //选择结算方式等登记信息
            if (isSuccess)
            {
                remainReceive = new RemainReceive(receiveMess);
                if (remainReceive.State != "0001")
                {
                    errMsg = remainReceive.Result;
                    return -1;
                }
            }
            else
            {
                errMsg = exchangeMess;
                return -1;
            }
            return 0;
        }
        /// <summary>
        /// 订单支付
        /// </summary>
        /// <param name="basPerInfo"></param>
        /// <returns></returns>
        public static int BillPay(BillPaySend billPaySend ,ref BillPayReceive billPayReceive)
        {
            string messageType = "YL001";//交易类型
            string receiveMess = string.Empty;
            string exchangeMess = string.Empty;

            if (!SiSet.DeptCompare.ContainsKey(billPaySend.HisDeptCode))
            {
                errMsg = billPaySend.HisDeptCode+"该科室未对照，无法和医保进行结算交易";
                return -1;
            }
            billPaySend.DeptCode = SiSet.DeptCompare[billPaySend.HisDeptCode];
            bool isSuccess = Comm.SendMessage(messageType, billPaySend.toMessage(), ref receiveMess, ref exchangeMess);
            //调用接口成功
            if (isSuccess)
            {
                billPayReceive = new BillPayReceive(receiveMess);
                if (billPayReceive.State != "0001")
                {
                    if (SiSet.YLResult.ContainsKey(billPayReceive.Result.Substring(4)))
                    {
                        errMsg = SiSet.YLResult[billPayReceive.Result.Substring(4)];
                    }
                    else
                    {
                        errMsg = billPayReceive.Result;
                    }
                    return -1;
                }
                #region 设置回滚参数: 撤消登记-->登记
                BizParam rollBackParam = new BizParam();
                BillPayCancelSend cancelSend = new BillPayCancelSend();
                cancelSend.CardNo = billPaySend.CardNo;
                cancelSend.SeqNo = billPayReceive.SeqNo;
                cancelSend.TransCost = billPayReceive.Cost;
                cancelSend.TransNo = billPayReceive.TransNo;
                rollBackParam.SetParam("ID", messageType);
                rollBackParam.SetParam("BillPayCancelSend", cancelSend);
                rollBackLine.Add(rollBackParam);
                #endregion
            }
            else
            {
                errMsg = exchangeMess;
                return -1;
            }
            return 0;
        }
        public static bool ReadCard(string IDCard, ref string personNo)
        {
            SimPerInfo simPerInfo = new SimPerInfo();
            var sign = ReadCard(IDCard, ref simPerInfo);
            if (sign < 0)
            {
                return false;
            }
            else
            {
                personNo = simPerInfo.personNo;
                return true;
            }         
        }
        /// <summary>
        /// 根据身份证获取医保卡号信息
        /// </summary>
        /// <param name="cardNo"></param>
        /// <param name="basPerInfo"></param>
        /// <param name="show"></param>
        /// <returns></returns>
        public static int ReadCard(string IDCard, ref SimPerInfo simPerInfo)
        {
            string receiveMess = string.Empty;
            string exchangeMess = string.Empty;
            string backCode = string.Empty;
            bool isSuccess;


            //获取个人信息
            string messageType = "CX002";

            isSuccess = Comm.SendMessage(messageType, IDCard, ref receiveMess, ref exchangeMess,ref backCode);

            //选择结算方式等登记信息
            if (isSuccess)
            {
                simPerInfo = new SimPerInfo(receiveMess);
            }
            else
            {
                errMsg = exchangeMess;
                return -1;
            }
            return 1;
        }
        public static bool isDiseaseSpByIdNo(string IDCard)
        {
            SimPerInfo simPerInfo = new SimPerInfo();
            int sign = ReadCard(IDCard,ref simPerInfo);
            if (sign == -1)
            {
                return false;
            }
            DiseaseSpPrintLoop printLoop = new DiseaseSpPrintLoop();

            printLoop.cardNo = simPerInfo.personNo;
            printLoop.searchDate = DateTime.Now;
            //第一次传为空，后续要传前次的审批编号
            printLoop.approveNo = "";           
            sign = diseaseSpPrint(ref printLoop);
            if (sign > 0 && printLoop.listDiseaseSpPrints.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }           
        }
        public static bool isDiseaseSp(string personNo)
        {           
            DiseaseSpPrintLoop printLoop = new DiseaseSpPrintLoop();

            printLoop.cardNo = personNo;
            printLoop.searchDate = DateTime.Now;
            //第一次传为空，后续要传前次的审批编号
            printLoop.approveNo = "";
            int sign = diseaseSpPrint(ref printLoop);
            if (sign > 0 && printLoop.listDiseaseSpPrints.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 门诊大病证打印信息查询
        /// </summary>
        public static int diseaseSpPrint(ref DiseaseSpPrintLoop diseaseSpPrintLoop)
        {
            //局部变量
            bool isSuccess;
            string receiveMess = string.Empty;
            string exchangeMess = string.Empty;
            string messageType = "CX005";
            string backCode = string.Empty;

            //调用接口
            isSuccess = Comm.SendMessage(messageType, diseaseSpPrintLoop.toMessage(), ref receiveMess, ref exchangeMess,ref backCode);
            if (isSuccess)
            {
                string approveNo = string.Empty;
                bool isEnd = false;
                diseaseSpPrintLoop.setReceiveInfo(receiveMess);
                diseaseSpPrintLoop.decompData(ref approveNo, ref isEnd);
            }
            else
            {
                errMsg = exchangeMess;
                return -1;
            }
            errMsg = exchangeMess;
            return 1;
        }
        #endregion 功能函数
    }
}
