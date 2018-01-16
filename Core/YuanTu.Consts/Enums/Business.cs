﻿using System;

namespace YuanTu.Consts.Enums
{
    public enum Business
    {
        未定义 = 0,
        建档 = 5,
        挂号 = 3,
        预约 = 6,
        取号 = 4,
        缴费 = 2,
        充值 = 1,
        查询 = 7,
        住院押金 = 8,
        出院结算 = 9,

        补卡 = 30,
        外院卡注册 = 12,
        实名认证 = 14,
        生物信息录入 = 15,

        补打 = 13,
        出院清单打印 = 32,

        签到 = 29,
        收银 = 28,
        输液费 = 35,

        科室信息 = 31,
        先诊疗后付费 = 36,
        切换终端 = 37,
        远程问诊 = 38,

        #region 查询业务

        药品查询 = 16,
        项目查询 = 17,
        已缴费明细 = 18,
        检验结果 = 19,
        影像结果 = 20,
        住院一日清单 = 21,
        住院押金查询 = 22,
        住院床位查询 = 23,
        执业资格查询 = 24,
        交易记录查询 = 25,
        门诊排班查询 = 26,
        材料费查询 = 27,

        药品变动价格查询 = 33,
        项目变动价格查询 = 34,

        #endregion

        #region 健康小屋业务

        健康服务 = 10,
        体测查询 = 11,

        #endregion
    }

    //[Flags]
    //public enum HouseBusiness
    //{
    //    健康服务=1,
    //    体测查询=2,
    //    预约挂号=3,
    //    建档发卡=4,
    //    现场挂号=5
    //}
}