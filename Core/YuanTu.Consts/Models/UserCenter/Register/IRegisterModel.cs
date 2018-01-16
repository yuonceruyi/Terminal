using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.UserCenter;
using YuanTu.Consts.UserCenter.Entities;

namespace YuanTu.Consts.Models.UserCenter.Register
{
    public interface IRegisterModel:IModel
    {
        res医院列表 Res医院列表 { get; set; }
        CorpVO 当前选择医院 { get; set; }

        res科室列表 Res科室列表 { get; set; }
        Dept 当前选择科室 { get; set; }
        res按医生排班列表 Res按医生排班列表 { get; set; }
        DoctorVO 当前选择医生排班 { get; set; }
        res按日期排班列表 Res按日期排班列表 { get; set; }
        res查询排班号量 Res查询排班号量 { get; set; }
        SourceDO 当前选择号源 { get; set; }
        res获取挂号应付金额 Res获取挂号应付金额 { get; set; }
        res获取支付方式列表 Res获取支付方式列表 { get; set; }
        res确认挂号 Res确认挂号 { get; set; }

        res确认预约 Res确认预约 { get; set; }

        res挂号支付 Res挂号支付 { get; set; }
    }
    public class RegisterModel:ModelBase,IRegisterModel
    {
        public res医院列表 Res医院列表 { get; set; }
        public CorpVO 当前选择医院 { get; set; }
        public res科室列表 Res科室列表 { get; set; }
        public Dept 当前选择科室 { get; set; }
        public res按医生排班列表 Res按医生排班列表 { get; set; }
        public res按日期排班列表 Res按日期排班列表 { get; set; }
        public res查询排班号量 Res查询排班号量 { get; set; }
        public res获取挂号应付金额 Res获取挂号应付金额 { get; set; }
        public res获取支付方式列表 Res获取支付方式列表 { get; set; }
        public res确认挂号 Res确认挂号 { get; set; }
        public res确认预约 Res确认预约 { get; set; }
        public DoctorVO 当前选择医生排班 { get; set; }
        public SourceDO 当前选择号源 { get; set; }
        public res挂号支付 Res挂号支付 { get; set; }
    }
}
