using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.ZheJiangZhongLiuHospital.Component.QueueSignIn
{
    public class RegisterInfo
    {
        public int id { get; set; }
        public int orderNo { get; set; }
        public object orderType { get; set; }
        public object weight { get; set; }
        public string username { get; set; }
        public object sex { get; set; }
        public object patientNo { get; set; }
        public string doctorName { get; set; }
        public string diagRoom { get; set; }
        public int callTimes { get; set; }
        public int status { get; set; }
        public string createTime { get; set; }
        public string modifyTime { get; set; }
        public object age { get; set; }
        public string mobile { get; set; }
        public int isBack { get; set; }
        public string corpId { get; set; }
        public string corpName { get; set; }
        public object waitingTime { get; set; }
        public object preTime { get; set; }
        public object callingTime { get; set; }
        public object completeTime { get; set; }
        public object passTime { get; set; }
        public object guardNo { get; set; }
        public string smallDeptCode { get; set; }
        public string smallDeptName { get; set; }
        public string bigDeptCode { get; set; }
        public string bigDeptName { get; set; }
        public string cardNo { get; set; }
        public int cardType { get; set; }
        public string regId { get; set; }
        public int intervalFlag { get; set; }
        public string regTime { get; set; }
        public object idNo { get; set; }
        public string doctorNo { get; set; }
        public object doctorCode { get; set; }
        public int doctorChoose { get; set; }
        public object area { get; set; }
        public string queueCode { get; set; }
        public string queueName { get; set; }
        public int patientType { get; set; }
        public object backType { get; set; }
        public int count { get; set; }
        public bool doctorSel { get; set; }
        public string doctorAddress { get; set; }
        public object regType { get; set; }
        public object patientDOs { get; set; }
        public int waitNum { get; set; }
        public int currentOrderNo { get; set; }
        public string deptAddress { get; set; }
        public bool signFlag { get; set; }
        public int barWaitNum { get; set; }
        public bool canSelectDoctor { get; set; }
        public bool canConfirm { get; set; }
        public object doctorDOs { get; set; }
        public bool backFlag { get; set; }
    }

}
