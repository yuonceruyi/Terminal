

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;


namespace YuanTu.YuHangArea.CYHIS.WebService
{
	public static class DataHandler
	{
		public static bool YIYUANPBXX(YIYUANPBXX_IN req, out YIYUANPBXX_OUT res)
		{
		     res = HisConnection.Handle<YIYUANPBXX_OUT>(req);
			 /*
		    if (res == null)
				return false;
			if (res.OUTMSG.ERRNO != "0")
			 {
			  Logger.log.Error("调用YIYUANPBXX错误\n" + res.OUTMSG.ERRMSG);
			  throw new Exception(res.OUTMSG.ERRMSG);
			 }*/
			return res!=null;
		}
		public static bool GUAHAOYSXX(GUAHAOYSXX_IN req, out GUAHAOYSXX_OUT res)
		{
		     res = HisConnection.Handle<GUAHAOYSXX_OUT>(req);
			 /*
		    if (res == null)
				return false;
			if (res.OUTMSG.ERRNO != "0")
			 {
			  Logger.log.Error("调用GUAHAOYSXX错误\n" + res.OUTMSG.ERRMSG);
			  throw new Exception(res.OUTMSG.ERRMSG);
			 }*/
			return res!=null;
		}
		public static bool GUAHAOHYXX(GUAHAOHYXX_IN req, out GUAHAOHYXX_OUT res)
		{
		     res = HisConnection.Handle<GUAHAOHYXX_OUT>(req);
			 /*
		    if (res == null)
				return false;
			if (res.OUTMSG.ERRNO != "0")
			 {
			  Logger.log.Error("调用GUAHAOHYXX错误\n" + res.OUTMSG.ERRMSG);
			  throw new Exception(res.OUTMSG.ERRMSG);
			 }*/
			return res!=null;
		}
		public static bool MENZHENFYMX(MENZHENFYMX_IN req, out MENZHENFYMX_OUT res)
		{
		     res = HisConnection.Handle<MENZHENFYMX_OUT>(req);
			 /*
		    if (res == null)
				return false;
			if (res.OUTMSG.ERRNO != "0")
			 {
			  Logger.log.Error("调用MENZHENFYMX错误\n" + res.OUTMSG.ERRMSG);
			  throw new Exception(res.OUTMSG.ERRMSG);
			 }*/
			return res!=null;
		}
		public static bool CLINICORDERD(CLINICORDERD_IN req, out CLINICORDERD_OUT res)
		{
		     res = HisConnection.Handle<CLINICORDERD_OUT>(req);
			 /*
		    if (res == null)
				return false;
			if (res.OUTMSG.ERRNO != "0")
			 {
			  Logger.log.Error("调用CLINICORDERD错误\n" + res.OUTMSG.ERRMSG);
			  throw new Exception(res.OUTMSG.ERRMSG);
			 }*/
			return res!=null;
		}
		public static bool GUAHAOYYTH(GUAHAOYYTH_IN req, out GUAHAOYYTH_OUT res)
		{
		     res = HisConnection.Handle<GUAHAOYYTH_OUT>(req);
			 /*
		    if (res == null)
				return false;
			if (res.OUTMSG.ERRNO != "0")
			 {
			  Logger.log.Error("调用GUAHAOYYTH错误\n" + res.OUTMSG.ERRMSG);
			  throw new Exception(res.OUTMSG.ERRMSG);
			 }*/
			return res!=null;
		}
		public static bool CashInfo(CashInfo_IN req, out CashInfo_OUT res)
		{
		     res = HisConnection.Handle<CashInfo_OUT>(req);
			 /*
		    if (res == null)
				return false;
			if (res.OUTMSG.ERRNO != "0")
			 {
			  Logger.log.Error("调用CashInfo错误\n" + res.OUTMSG.ERRMSG);
			  throw new Exception(res.OUTMSG.ERRMSG);
			 }*/
			return res!=null;
		}
		public static bool ZXPOSInfo(ZXPOSInfo_IN req, out ZXPOSInfo_OUT res)
		{
		     res = HisConnection.Handle<ZXPOSInfo_OUT>(req);
			 /*
		    if (res == null)
				return false;
			if (res.OUTMSG.ERRNO != "0")
			 {
			  Logger.log.Error("调用ZXPOSInfo错误\n" + res.OUTMSG.ERRMSG);
			  throw new Exception(res.OUTMSG.ERRMSG);
			 }*/
			return res!=null;
		}
		public static bool RechargeInfo(RechargeInfo_IN req, out RechargeInfo_OUT res)
		{
		     res = HisConnection.Handle<RechargeInfo_OUT>(req);
			 /*
		    if (res == null)
				return false;
			if (res.OUTMSG.ERRNO != "0")
			 {
			  Logger.log.Error("调用RechargeInfo错误\n" + res.OUTMSG.ERRMSG);
			  throw new Exception(res.OUTMSG.ERRMSG);
			 }*/
			return res!=null;
		}
	}
}

	