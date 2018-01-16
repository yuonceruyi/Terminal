namespace YuanTu.Consts
{
    public static class A
    {
        public static readonly string Separator = "->";
        public static string Home => "主页";
        public static string AdminPart => "小后台";
        public static string Maintenance => "维护";
        public static string JianDang_Context => "自助建档";
        public static string ChaKa_Context => "个人信息";
        public static string XianChang_Context => "当天挂号";
        public static string YuYue_Context => "预约挂号";
        public static string QuHao_Context => "自助取号";
        public static string JiaoFei_Context => "自助缴费";
        public static string ChongZhi_Context => "自助充值";
        public static string BuDa_Context => "补打凭条";
        public static string YinYi_Context => "银行卡绑定解绑";
        public static string ZhuYuan_Context => "住院人员";
        public static string RealAuth_Context => "实名认证";

        public static string Biometric_Context => "生物信息采集";

        public static string ThirdPay => "第三方支付";
        public static string PayCostQuery => "已缴费记录查询";
        public static string ReChargeQuery => "充值记录查询";
        public static string MedicineQuery => "药品信息查询";
        public static string ChargeItemsQuery => "收费项目查询";
        public static string DiagReportQuery => "检验报告查询";
        public static string PacsReportQuery => "影像报告查询";
        public static string InDayDetailList_Context => "住院一日清单";
        public static string QueryChoice => "查询选择";
        public static string IpRecharge_Context => "住院充值";
        public static string QueneSelect_Context => "分诊签到";
        public static string InPrePayRecordQuery_Context => "住院押金查询";
        public static string InBedInfoQuery_Context => "床位查询";
        public static string PrintAgainChoice => "补打选择";
        public static string BillPrint => "缴费补打";



        public class Third
        {
            public static string Confirm => ThirdPay + Separator + nameof(Confirm);
            public static string PosUnion => ThirdPay + Separator + nameof(PosUnion);
            public static string Cash => ThirdPay + Separator + nameof(Cash);
            public static string AtmCash => ThirdPay + Separator + nameof(AtmCash);
            public static string JCMCash => ThirdPay + Separator + nameof(JCMCash);
            public static string ScanQrCode => ThirdPay + Separator + nameof(ScanQrCode);
            public static string SiPay => ThirdPay + Separator + nameof(SiPay);
        }

        public class JD
        {
            public static string Print => ChaKa_Context + Separator + nameof(Print);
        }

        public class CK
        {
            public static string Choice => ChaKa_Context + Separator + nameof(Choice);
            public static string Card => ChaKa_Context + Separator + nameof(Card);
            public static string IDCard => ChaKa_Context + Separator + nameof(IDCard);
            public static string InputIDCard => ChaKa_Context + Separator + nameof(InputIDCard);
            public static string HICard => ChaKa_Context + Separator + nameof(HICard);
            public static string QrCode => ChaKa_Context + Separator + nameof(QrCode);
            public static string FaceRec => ChaKa_Context + Separator + nameof(FaceRec);
            public static string Select => ChaKa_Context + Separator + nameof(Select);
            public static string Cash => ChaKa_Context + Separator + nameof(Cash);
            public static string Info => ChaKa_Context + Separator + nameof(Info);
            public static string InfoEx => ChaKa_Context + Separator + nameof(InfoEx);
        }
        public class ZY
        {
            public static string Choice => ZhuYuan_Context + Separator + nameof(Choice);
            public static string Card => ZhuYuan_Context + Separator + nameof(Card);
            public static string IDCard => ZhuYuan_Context + Separator + nameof(IDCard);
            public static string InputIDCard => ZhuYuan_Context + Separator + nameof(InputIDCard);
            public static string HICard => ZhuYuan_Context + Separator + nameof(HICard);
            public static string InPatientNo => ZhuYuan_Context + Separator + nameof(InPatientNo);
            public static string InPatientInfo => ZhuYuan_Context + Separator + nameof(InPatientInfo);

        }

        public class ZYCZ
        {
            public static string RechargeWay => ZhuYuan_Context + Separator + nameof(RechargeWay);
            public static string InputAmount => ZhuYuan_Context + Separator + nameof(InputAmount);
            public static string Print => ZhuYuan_Context + Separator + nameof(Print);
            public static string PayerName => ZhuYuan_Context + Separator + nameof(PayerName);
        }

        public class XC
        {
            public static string Wether => XianChang_Context + Separator + nameof(Wether);
            public static string AMPM => XianChang_Context + Separator + nameof(AMPM);
            public static string ParentDept => XianChang_Context + Separator + nameof(ParentDept);
            public static string Dept => XianChang_Context + Separator + nameof(Dept);
            public static string Doctor => XianChang_Context + Separator + nameof(Doctor);
            public static string Schedule => XianChang_Context + Separator + nameof(Schedule);
            public static string Time => XianChang_Context + Separator + nameof(Time);
            public static string Confirm => XianChang_Context + Separator + nameof(Confirm);
            public static string Print => XianChang_Context + Separator + nameof(Print);
        }

        public class YY
        {
            public static string Date => YuYue_Context + Separator + nameof(Date);
            public static string AMPM => YuYue_Context + Separator + nameof(AMPM);
            public static string Wether => YuYue_Context + Separator + nameof(Wether);
            public static string ParentDept => YuYue_Context + Separator + nameof(ParentDept);
            public static string Dept => YuYue_Context + Separator + nameof(Dept);
            public static string Doctor => YuYue_Context + Separator + nameof(Doctor);
            public static string Schedule => YuYue_Context + Separator + nameof(Schedule);
            public static string Time => YuYue_Context + Separator + nameof(Time);
            public static string Confirm => YuYue_Context + Separator + nameof(Confirm);
            public static string Print => YuYue_Context + Separator + nameof(Print);
        }

        public class QH
        {
            public static string Query => QuHao_Context + Separator + nameof(Query);
            public static string Record => QuHao_Context + Separator + nameof(Record);
            public static string TakeNum => QuHao_Context + Separator + nameof(TakeNum);
            public static string Confirm => QuHao_Context + Separator + nameof(Confirm);
            public static string Print => QuHao_Context + Separator + nameof(Print);
        }

        public class JF
        {
            public static string BillRecord => JiaoFei_Context + Separator + nameof(BillRecord);
            public static string Confirm => JiaoFei_Context + Separator + nameof(Confirm);
            public static string Print => JiaoFei_Context + Separator + nameof(Print);
        }

        public class CZ
        {
            public static string RechargeWay => ChongZhi_Context + Separator + nameof(RechargeWay);
            public static string Print => ChongZhi_Context + Separator + nameof(Print);
            public static string InputAmount => ChongZhi_Context + Separator + nameof(InputAmount);
        }

        public class JFJL
        {
            public static string Date => PayCostQuery + Separator + nameof(Date);
            public static string PayCostRecord => PayCostQuery + Separator + nameof(PayCostRecord);
        }
        public class CZJL
        {
            public static string Date => ReChargeQuery + Separator + nameof(Date);
            public static string ReChargeRecord => ReChargeQuery + Separator + nameof(ReChargeRecord);
        }
        public class YP
        {
            public static string Query => MedicineQuery + Separator + nameof(Query);
            public static string QueryDisplay => MedicineQuery + Separator + nameof(QueryDisplay);
            public static string Medicine => MedicineQuery + Separator + nameof(Medicine);
        }

        public class XM
        {
            public static string Query => MedicineQuery + Separator + nameof(Query);
            public static string ChargeItems => ChargeItemsQuery + Separator + nameof(ChargeItems);
        }
        public class ZYYRQD
        {
            public static string Date => InDayDetailList_Context + Separator + nameof(Date);
            public static string DailyDetail => InDayDetailList_Context + Separator + nameof(DailyDetail);
        }
        public class JYJL
        {
            public static string Date => DiagReportQuery + Separator + nameof(Date);
            public static string DiagReport => DiagReportQuery + Separator + nameof(DiagReport);
        }
        public class YXBG
        {
            public static string Date => PacsReportQuery + Separator + nameof(Date);
            public static string PacsReport => PacsReportQuery + Separator + nameof(PacsReport);
        }
        public class ZYYJ
        {
            public static string Date => InPrePayRecordQuery_Context + Separator + nameof(Date);
            public static string InPrePayRecord => InPrePayRecordQuery_Context + Separator + nameof(InPrePayRecord);
        }
        public class CW
        {
            public static string InBedInfo => InBedInfoQuery_Context + Separator + nameof(InBedInfo);
        }
        public class JFBD
        {
            public static string Date => BillPrint + Separator + nameof(Date);
            public static string PayCostRecord => BillPrint + Separator + nameof(PayCostRecord);
            public static string Print => BillPrint + Separator + nameof(Print);
        }

        public class SMRZ
        {
            //插卡->个人信息展示+校验密码->刷身份证->结束
            public static string Card => RealAuth_Context + Separator + nameof(Card);
            public static string PatientInfo => RealAuth_Context + A.Separator + nameof(PatientInfo);
            public static string CheckPwd => RealAuth_Context + Separator + nameof(CheckPwd);
            public static string IDCard => ZhuYuan_Context + Separator + nameof(IDCard);
        }

        public static class  SignIn
        {
            public static string RegisterInfoSelect => QueneSelect_Context + Separator + nameof(RegisterInfoSelect);


        }

        public class Bio
        {
            public static string Choice => Biometric_Context + Separator + nameof(Choice);

            public static string FaceRec => Biometric_Context + Separator + nameof(FaceRec);

            public static string FingerPrint => Biometric_Context + Separator + nameof(FingerPrint);
        }



        
    }
}