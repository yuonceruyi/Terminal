namespace YuanTu.YuHangArea.CYHIS.DLL
{
	public static partial class DataHandler
	{
		public static bool 查询建档(Req查询建档 req,out 查询建档_OUT res,int operation = 1)
		{
		      string recv;
			  var ret = RunExe(req.Serilize(),out recv,operation);
			  if(ret == 0)
			  {
			     res = 查询建档_OUT.Deserilize(recv);
		      }
			 else
			 {
			   res = null;
			   //throw new Exception("错误："+ ret);
			 }
			 
			return res != null;
		}
		public static bool 挂号取号(Req挂号取号 req,out 挂号取号_OUT res,int operation = 2)
		{
		      string recv;
			  var ret = RunExe(req.Serilize(),out recv,operation);
			  if(ret == 0)
			  {
			     res = 挂号取号_OUT.Deserilize(recv);
		      }
			 else
			 {
			   res = null;
			   //throw new Exception("错误："+ ret);
			 }
			 
			return res != null;
		}
		public static bool 缴费预结算(Req缴费预结算 req,out 缴费预结算_OUT res,int operation = 3)
		{
		      string recv;
			  var ret = RunExe(req.Serilize(),out recv,operation);
			  if(ret == 0)
			  {
			     res = 缴费预结算_OUT.Deserilize(recv);
		      }
			 else
			 {
			   res = null;
			   //throw new Exception("错误："+ ret);
			 }
			 
			return res != null;
		}
		public static bool 缴费结算(Req缴费结算 req,out 缴费结算_OUT res,int operation = 4)
		{
		      string recv;
			  var ret = RunExe(req.Serilize(),out recv,operation);
			  if(ret == 0)
			  {
			     res = 缴费结算_OUT.Deserilize(recv);
		      }
			 else
			 {
			   res = null;
			   //throw new Exception("错误："+ ret);
			 }
			 
			return res != null;
		}
		public static bool 签退(Req签退 req,out 签退_OUT res,int operation = 5)
		{
		      string recv;
			  var ret = RunExe(req.Serilize(),out recv,operation);
			  if(ret == 0)
			  {
			     res = 签退_OUT.Deserilize(recv);
		      }
			 else
			 {
			   res = null;
			   //throw new Exception("错误："+ ret);
			 }
			 
			return res != null;
		}
	}
}