using YuanTu.Consts.FrameworkBase;
using YuanTu.FuYangRMYY.HisNative.Models.Base;
namespace YuanTu.FuYangRMYY.HisNative.Models{
		public class 社保读卡:InsuranceResponseBase<社保读卡>{
			public override char  DataSplit=>'^';
			 public override char OriginSplit=>'|';
			public string 个人编号{get;set;}
			public string 卡号{get;set;}
			public string 社保序列号{get;set;}
			public string 姓名{get;set;}
			public string 性别{get;set;}
			public string 民族{get;set;}
			public string 生日{get;set;}
			public string 身份证号{get;set;}
			public string 公司名称{get;set;}
			public string 住址{get;set;}
			public string 联系电话{get;set;}
			public string 人员类别{get;set;}
			public string 参保类别{get;set;}
			public string 账户余额{get;set;}
			public string IPTimes{get;set;}
			public string CardStatus{get;set;}
			public string Spesic{get;set;}
			public string IPStatus{get;set;}
			public string Transflag{get;set;}
			public string DQ{get;set;}
			public override Result Format(string msg){
				var arr=msg.Split(DataSplit);
				if(arr.Length<20){
					return Result.Fail(-1,"返回的社保信息不正确");
				}
				个人编号=arr[0];
				卡号=arr[1];
				社保序列号=arr[2];
				姓名=arr[3];
				性别=arr[4];
				民族=arr[5];
				生日=arr[6];
				身份证号=arr[7];
				公司名称=arr[8];
				住址=arr[9];
				联系电话=arr[10];
				人员类别=arr[11];
				参保类别=arr[12];
				账户余额=arr[13];
				IPTimes=arr[14];
				CardStatus=arr[15];
				Spesic=arr[16];
				IPStatus=arr[17];
				Transflag=arr[18];
				DQ=arr[19];
				return Result.Success();
			}
		}
   		public class 社保挂号结算:InsuranceResponseBase<社保挂号结算>{
			public override char  DataSplit=>'\u0002';
			 public override char OriginSplit=>'!';
			public string 自费金额{get;set;}
			public string 医保基金{get;set;}
			public string 医保账户{get;set;}
			public override Result Format(string msg){
				var arr=msg.Split(DataSplit);
				if(arr.Length<3){
					return Result.Fail(-1,"返回的社保信息不正确");
				}
				自费金额=arr[0];
				医保基金=arr[1];
				医保账户=arr[2];
				return Result.Success();
			}
		}
   		public class 社保挂号冲销:InsuranceResponseBase<社保挂号冲销>{
			public override char  DataSplit=>'^';
			 public override char OriginSplit=>'|';
			public override Result Format(string msg){
				return Result.Success();
				var arr=msg.Split(DataSplit);
				if(arr.Length<0){
					return Result.Fail(-1,"返回的社保信息不正确");
				}
				return Result.Success();
			}
		}
   		public class 社保缴费结算:InsuranceResponseBase<社保缴费结算>{
			public override char  DataSplit=>'^';
			 public override char OriginSplit=>'|';
			public string 医保结算表RowId{get;set;}
			public string 个人支付_起付标准{get;set;}
			public string 发票rowId{get;set;}
			public string 基金支付{get;set;}
			public string 医保账户支付{get;set;}
			public override Result Format(string msg){
				var arr=msg.Split(DataSplit);
				if(arr.Length<5){
					return Result.Fail(-1,"返回的社保信息不正确");
				}
				医保结算表RowId=arr[0];
				个人支付_起付标准=arr[1];
				发票rowId=arr[2];
				基金支付=arr[3];
				医保账户支付=arr[4];
				return Result.Success();
			}
		}
   }