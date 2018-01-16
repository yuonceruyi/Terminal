using System;
using System.Collections.Generic;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.PanYu.House.PanYuGateway;
using CardType = YuanTu.PanYu.House.PanYuGateway.CardType;
using DataHandler = YuanTu.PanYu.House.PanYuGateway.DataHandler;
using regMode = YuanTu.PanYu.House.PanYuGateway.regMode;
using regType = YuanTu.PanYu.House.PanYuGateway.regType;
using streak = YuanTu.PanYu.House.PanYuGateway.streak;

namespace YuanTu.PanYu.House.PanYuService
{
    public interface IHisService : IService
    {
        bool NeedCreat { get; set; }
        CardType CardType { get; set; }
        string CardNo { get; set; }
        string Name { get; set; }

        string Gender { get; set; }
        string Birthday { get; set; }
        string IDNo { get; set; }
        string Nation { get; set; }
        string Address { get; set; }
        string Phone { get; set; }

        Result 病人信息查询();

        res病人信息查询 Res病人信息查询 { get; set; }

        Result 病人建档发卡();

        res病人建档发卡 Res病人建档发卡 { get; set; }
        regMode RegMode { get; set; }
        regType RegType { get; set; }

        Result Run排班科室信息查询();

        res排班科室信息查询 Res排班科室信息查询 { get; set; }
        排班科室信息 排班科室信息 { get; set; }

        Result Run排班医生信息查询();

        res排班医生信息查询 Res排班医生信息查询 { get; set; }

        排班医生信息 排班医生信息 { get; set; }

        Result Run排班信息查询();

        res排班信息查询 Res排班信息查询 { get; set; }

        排班信息 排班信息 { get; set; }
        string PatientId { get; set; }

        Result Run号源明细查询();

        res号源明细查询 Res号源明细查询 { get; set; }

        号源明细 号源明细 { get; set; }

        Result Run预约挂号();

        res预约挂号 Res预约挂号 { get; set; }
        List<排班信息> 排班信息列表 { get; set; }
    }

    public class HisService : ServiceBase, IHisService
    {
        public regMode RegMode { get; set; }

        public regType RegType { get; set; }

        public string MedDate { get; set; }
        public streak Streak { get; set; }

        #region 查询病人信息

        public CardType CardType { get; set; }
        public string CardNo { get; set; }
        public bool NeedCreat { get; set; }

        public res病人信息查询 Res病人信息查询 { get; set; }

        public Result 病人信息查询()
        {
            NeedCreat = false;
            var req = new req病人信息查询
            {
                cardType = ((int)CardType).ToString(),
                cardNo = CardNo,
                flowId = GetFlowId()
            };
            var res = DataHandler.病人信息查询(req);
            Res病人信息查询 = res;
            if (!(res?.success ?? false))
                return Result.Fail("信息查询失败\n" + res?.msg);
            if (res.data == null || res.data.Count == 0)
            {
                NeedCreat = true;
                return Result.Fail("未查询到对应病人信息");
            }

            return Result.Success();
        }

        #endregion 查询病人信息

        #region 病人建档发卡

        public string Name { get; set; }

        public string Gender { get; set; }
        public string Birthday { get; set; }
        public string IDNo { get; set; }
        public string Nation { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        public res病人建档发卡 Res病人建档发卡 { get; set; }

        public Result 病人建档发卡()
        {
            var req = new req病人建档发卡()
            {
                patientId = "",
                setupType = "1",
                guarderIdNo = "",
                cardNo = CardNo,
                cardType = "2",
                name = Name,
                sex = Gender,
                birthday = Birthday,
                idNo = IDNo,
                nation = Nation,
                address = Address,
                phone = Phone,
                cash = "0",
                tradeMode = "CA",
                pwd = "123456",
                flowId = GetFlowId(),
            };
            var res = DataHandler.病人建档发卡(req);
            Res病人建档发卡 = res;
            if (res?.success ?? false)
            {
                return Result.Success();
            }
            return Result.Fail("HIS建档失败\n" + res?.msg);
        }

        #endregion 病人建档发卡

        #region 排班科室信息查询

        public res排班科室信息查询 Res排班科室信息查询 { get; set; }
        public List<排班科室信息> 排班科室信息List { get; private set; }

        public 排班科室信息 排班科室信息 { get; set; }

        public Result Run排班科室信息查询()
        {
            var req = new req排班科室信息查询
            {
                regMode = ((int)RegMode).ToString(),
                regType = ((int)RegType).ToString(),
                flowId = GetFlowId()
            };
            var res = DataHandler.排班科室信息查询(req);
            Res排班科室信息查询 = res;
            if (res?.success ?? false)
            {
                if (Res排班科室信息查询.data == null)
                {
                    return Result.Fail("未找到排班科室信息");
                }
                return Result.Success();
            }
            return Result.Fail("排班科室信息查询失败\n" + res?.msg);
        }

        #endregion 排班科室信息查询

        #region 排班医生信息查询

        public res排班医生信息查询 Res排班医生信息查询 { get; set; }

        public 排班医生信息 排班医生信息 { get; set; }

        public Result Run排班医生信息查询()
        {
            var req = new req排班医生信息查询
            {
                regMode = ((int)RegMode).ToString(),
                regType = ((int)RegType).ToString(),
                flowId = GetFlowId(),
                deptCode = 排班科室信息.deptCode
            };
            var res = DataHandler.排班医生信息查询(req);
            Res排班医生信息查询 = res;
            if (res?.success ?? false)
            {
                return Res排班医生信息查询.data == null ? Result.Fail("此科室无排班医生，请选择其他科室")
                                                       : Result.Success();
            }
            return Result.Fail("查询排班医生失败，请重试\n" + res?.msg);
        }

        #endregion 排班医生信息查询

        #region 排班信息查询

        public res排班信息查询 Res排班信息查询 { get; set; }
        public List<排班信息> 排班信息列表 { get; set; }
        public 排班信息 排班信息 { get; set; }
        public string PatientId { get; set; }

        public Result Run排班信息查询()
        {
            排班信息列表 = new List<排班信息>();
            var req = new req排班信息查询
            {
                regMode = ((int)RegMode).ToString(),
                regType = ((int)RegType).ToString(),
                deptCode = 排班科室信息.deptCode,
                medDate = RegMode == regMode.预约 ? "" : MedDate,
                flowId = GetFlowId(),
                doctCode = 排班医生信息.doctCode,
                medAmPm = ((int)streak.上午).ToString(),
                patientId = PatientId,
                gfFlag = "0"
            };
            var res = DataHandler.排班信息查询(req);
            Res排班信息查询 = res;
            if (!(res?.success ?? false))
                return Result.Fail("医生排班查询失败\n" + res?.msg);


            if (res?.data != null || res?.data?.Count != 0)
            {
                if (res?.data != null)
                    排班信息列表.AddRange(res?.data);
            }
            req = new req排班信息查询
            {
                regMode = ((int)RegMode).ToString(),
                regType = ((int)RegType).ToString(),
                deptCode = 排班科室信息.deptCode,
                medDate = RegMode == regMode.预约 ? "" : MedDate,
                flowId = GetFlowId(),
                doctCode = 排班医生信息.doctCode,
                medAmPm = ((int)streak.下午).ToString(),
                patientId = PatientId,
                gfFlag = "0"
            };
            res = DataHandler.排班信息查询(req);
            Res排班信息查询 = res;
            if (!(res?.success ?? false))
            {
                if (排班信息列表?.Count == 0)
                    Result.Fail("医生排班查询失败\n" + res?.msg);
            }
            if (res?.data != null || res?.data?.Count != 0)
            {
                if (res?.data != null)
                    排班信息列表.AddRange(res?.data);
            }
            return 排班信息列表?.Count==0 ? Result.Fail("该医生的号已挂完，请选择其他号源") 
                                         : Result.Success();
        }

        #endregion 排班信息查询

        #region 号源明细查询

        public res号源明细查询 Res号源明细查询 { get; set; }

        public 号源明细 号源明细 { get; set; }

        public Result Run号源明细查询()
        {
            var req = new req号源明细查询
            {
                regMode = ((int)RegMode).ToString(),
                regType = ((int)RegType).ToString(),
                deptCode = 排班信息.deptCode,
                flowId = GetFlowId(),
                doctCode = 排班信息.doctCode,
                medAmPm = 排班信息.medAmPm,
                scheduleId = 排班信息.scheduleId,
                medDate = 排班信息.medDate
            }; ;
            var res = DataHandler.号源明细查询(req);
            Res号源明细查询 = res;
            if (res?.success ?? false)
            {
                if (Res号源明细查询.data == null || Res号源明细查询.data.Count == 0)
                    return Result.Fail("号源明细查询失败,返回为空");
                return Result.Success();
            }
            return Result.Fail("号源明细查询失败\n" + res?.msg);
        }

        #endregion 号源明细查询

        #region 预约挂号确认

        public res预约挂号 Res预约挂号 { get; set; }

        public Result Run预约挂号()
        {
            var req = new req预约挂号
            {
                cardNo = CardNo,
                patientId = PatientId,
                regMode = ((int)RegMode).ToString(),
                regType = ((int)RegType).ToString(),
                medAmPm = 排班信息.medAmPm,
                medDate = 排班信息.medDate,
                deptCode = 排班信息.deptCode,
                doctCode = 排班信息.doctCode,
                scheduleId = 排班信息.scheduleId,
                flowId = GetFlowId(),
                medBegTime = 号源明细.medBegTime,
                medEndTime = 号源明细.medEndTime,
                gfFlag = "0"
            }; ;
            var res = DataHandler.预约挂号(req);
            Res预约挂号 = res;
            if (res?.success ?? false)
            {
                return Result.Success();
            }
            return Result.Fail("预约挂号失败\n" + res?.msg);
        }

        #endregion 预约挂号确认

        public regType GetRegType(string str)
        {
            switch (str)
            {
                case "普通":
                    return regType.普通;

                case "专家":
                    return regType.专家;

                case "名医":
                    return regType.名医;

                case "急诊":
                    return regType.急诊;

                case "免费":
                    return regType.免费;

                default:
                    return regType.普通;
            }
        }

        public bool GetDept(string deptName)
        {
            var _deptList = Res排班科室信息查询.data;
            if (string.IsNullOrEmpty(deptName))
            {
                排班科室信息List = _deptList;
            }
            else
            {
                var _deptName = deptName.ToLower();
                排班科室信息List = _deptList.FindAll(c => c.simplePy.Contains(_deptName));
            }
            if (排班科室信息List?.Count == 0)
                return false;
            return true;
        }

        //public bool RegLimit(string deptName, out string tip)
        //{
        //    if (DataHandler.HospitalId == "549")
        //    {
        //        if (deptName.Contains("儿科") && Instance.ChaKa.PatAge >= 14)
        //        {
        //            tip = $"14岁或以上不能挂儿科相关门诊";
        //            return true;
        //        }
        //        if (deptName.Contains("门诊内科") && Instance.ChaKa.PatAge < 14)
        //        {
        //            tip = $"14岁以下不能挂内科门诊";
        //            return true;
        //        }
        //        if (deptName.Contains("妇科") && Instance.ChaKa.Gender == "男")
        //        {
        //            tip = $"男性不能挂妇科相关门诊";
        //            return true;
        //        }
        //    }
        //    tip = null;
        //    return false;

        //}

        #region 预约记录查询

        public res挂号预约记录查询 Res预约记录查询 { get; private set; }

        public Result Run预约记录查询()
        {
            var req = new req挂号预约记录查询
            {
                idNo = "",
                patientId = "",
                startDate = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                endDate = DateTimeCore.Now.AddDays(7).ToString("yyyy-MM-dd"),
                searchType = "1",
                operId = FrameworkConst.OperatorId,
                flowId = GetFlowId()
            };
            var res = DataHandler.挂号预约记录查询(req);
            Res预约记录查询 = res;
            if (!(res?.success ?? false))
                return Result.Fail("查询预约记录失败");
            if (res.data == null || res.data.Count == 0)
                return Result.Fail("未查询到预约记录");
            return Result.Success();
        }

        public bool CanYuYueReg()
        {
            var AmPM = 排班信息.medAmPm;
            var dateNow = Convert.ToDateTime(排班信息.medDate).ToString("MMdd");
            var list = Res预约记录查询.data.FindAll(c => (Convert.ToDateTime(c.medDate)).ToString("MMdd") == dateNow && c.medAmPm == AmPM);
            if (list?.Count > 0)
            {
                return false;
            }
            return true;
        }

        #endregion 预约记录查询

        public string ServiceName { get; }
    }
}