using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.YiWuArea.Insurance.Models;
using YuanTu.YiWuFuBao.Models;

namespace YuanTu.YiWuFuBao
{
    public static class ConstInner
    {
        #region[挂号信息墓碑]
        private static RegisterInfoTombDto _门诊挂号结算暂存;
        private const string tmpDataName = "门诊挂号结算暂存.json";
        public static RegisterInfoTombDto 门诊挂号结算暂存
        {
            get
            {
                try
                {
                    if (_门诊挂号结算暂存 == null && File.Exists(tmpDataName))
                    {
                        _门诊挂号结算暂存 = File.ReadAllText(tmpDataName, Encoding.UTF8).ToJsonObject<RegisterInfoTombDto>();
                    }
                }
                catch (Exception)
                {

                }
                return _门诊挂号结算暂存;
            }
            set
            {
                _门诊挂号结算暂存 = value;
                try
                {
                    if (value != null)
                    {
                        File.WriteAllText(tmpDataName, _门诊挂号结算暂存.ToJsonString(), Encoding.UTF8);
                    }
                    else
                    {
                        File.Delete(tmpDataName);
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        public static Result 暂存退号()
        {
            if (门诊挂号结算暂存 == null)
            {
                return Result.Success();
            }
            var res = DataHandlerEx.取消预约(new req取消预约()
            {
                appoNo = 门诊挂号结算暂存.appoNo,
                orderNo = 门诊挂号结算暂存.orderNo,
                patientId = 门诊挂号结算暂存.病人信息.patientId,
                operId = FrameworkConst.OperatorId,
                regMode = "2",
                cardNo = 门诊挂号结算暂存.CardNo,
                cardType = ((int)门诊挂号结算暂存.CardType).ToString(),
#pragma warning disable 612
                medDate = 门诊挂号结算暂存.MedDate,
                scheduleId = 门诊挂号结算暂存.ScheduleId,
                medAmPm = 门诊挂号结算暂存.MedAmPm,
                regNo = 门诊挂号结算暂存.visitNo,
#pragma warning restore 612
                extend = /*$"<regFlowId>{门诊挂号结算暂存.regFlowId}</regFlowId>" + */门诊挂号结算暂存.extend
            });
            if (res.success)
            {
                门诊挂号结算暂存 = null;
                return Result.Success();
            }
            return Result.Fail(res.msg);
        }
        #endregion


        public static bool IsOldMachine { get; set; } = FrameworkConst.DeviceType.StartsWith("YT");


        public static Dictionary<string,RegType> CurrentRegTypes=new Dictionary<string, RegType>()
        {
            ["1"]=RegType.普通门诊,
            ["2"]=RegType.专家门诊,
            ["3"]=RegType.名医,
            ["4"]=RegType.急诊门诊,
            ["5"]=RegType.便民门诊,
            ["12"]=RegType.特需,
            ["14"]=RegType.义诊,
        };
    }
}
