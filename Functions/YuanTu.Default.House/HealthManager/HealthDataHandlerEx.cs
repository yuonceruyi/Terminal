using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Default.House.HealthManager.Base;

namespace YuanTu.Default.House.HealthManager
{
   
    public partial class HealthDataHandlerEx
    {
       
        public static res查询是否已建档 查询是否已建档(req查询是否已建档 req)
        {
            return DataHandler.Query<res查询是否已建档, req查询是否已建档>(req);
        }

        public static res修改手机号 修改手机号(req修改手机号 req)
        {
            return DataHandler.Query<res修改手机号, req修改手机号>(req);
        }

        public static res上传体检报告 上传体检报告(req上传体检报告 req)
        {
            return DataHandler.Query<res上传体检报告, req上传体检报告>(req);
        }

        public static res查询体检报告单 查询体检报告单(req查询体检报告单 req)
        {
            return DataHandler.Query<res查询体检报告单, req查询体检报告单>(req);
        }

        public static res获取服务器当前时间 获取服务器当前时间(req获取服务器当前时间 req)
        {
            return DataHandler.Query<res获取服务器当前时间, req获取服务器当前时间>(req);
        }

    }

    
    public partial class req查询是否已建档 : RequestBase
    {
        /// <summary>
        /// 查询是否已建档
        /// </summary>
        public req查询是否已建档()
        {
            service = "/restapi/common/health/queryUser";
            serviceName = "查询是否已建档";
        }

        public string name { get; set; }



        public string idNo { get; set; }



        public string cardNo { get; set; }



        public string cardType { get; set; }



        public string sex { get; set; }



        public string age { get; set; }



        public string birthday { get; set; }



        public string nation { get; set; }



        public string addr { get; set; }



        public string expire { get; set; }



        public string photo { get; set; }



        public string phone { get; set; }


        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
	            dic[nameof(name)] = name;
	            dic[nameof(idNo)] = idNo;
	            dic[nameof(cardNo)] = cardNo;
	            dic[nameof(cardType)] = cardType;
	            dic[nameof(sex)] = sex;
	            dic[nameof(age)] = age;
	            dic[nameof(birthday)] = birthday;
	            dic[nameof(nation)] = nation;
	            dic[nameof(addr)] = addr;
	            dic[nameof(expire)] = expire;
	            dic[nameof(photo)] = photo;
	            dic[nameof(phone)] = phone;

            return dic;
        }

    }
    
    public partial class req修改手机号 : RequestBase
    {
        /// <summary>
        /// 修改手机号
        /// </summary>
        public req修改手机号()
        {
            service = "/restapi/common/health/upateUser";
            serviceName = "修改手机号";
        }

        public string healthUserId { get; set; }



        public string phone { get; set; }



        public string idNo { get; set; }


        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
	            dic[nameof(healthUserId)] = healthUserId;
	            dic[nameof(phone)] = phone;
	            dic[nameof(idNo)] = idNo;

            return dic;
        }

    }
    
    public partial class req上传体检报告 : RequestBase
    {
        /// <summary>
        /// 上传体检报告
        /// </summary>
        public req上传体检报告()
        {
            service = "/restapi/common/health/commitReport";
            serviceName = "上传体检报告";
        }

        public string healthUserId { get; set; }



        public string idNo { get; set; }



        public string icpcode { get; set; }



        public List<测量组> groupList { get; set; }


        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
	            dic[nameof(healthUserId)] = healthUserId;
	            dic[nameof(idNo)] = idNo;
	            dic[nameof(icpcode)] = icpcode;
	            if (groupList != null)
            {
				int num = 0;
                for (int i = 0; i < groupList.Count; i++)
                {
                    var groupName = groupList[i].groupName;
                    var childList= groupList[i].childList;
                    if (childList != null)
                    {
                        for (int j = 0; j < childList.Count; j++)
                        {
                            dic[$"childList[{num}].groupName"] = groupName;
                            dic[$"childList[{num}].childName"] = childList[j].childName;
                            dic[$"childList[{num}].dataStr"] = childList[j].dataStr;
							dic[$"childList[{num}].unit"] = childList[j].unit;
							num++;
                        }
                    }
                }
            }
          
            return dic;
        }

    }
    
    public partial class req查询体检报告单 : RequestBase
    {
        /// <summary>
        /// 查询体检报告单
        /// </summary>
        public req查询体检报告单()
        {
            service = "/restapi/common/health/queryReport";
            serviceName = "查询体检报告单";
        }

        public string healthUserId { get; set; }



        public string idNo { get; set; }



        public string beginTime { get; set; }



        public string endTime { get; set; }


        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
	            dic[nameof(healthUserId)] = healthUserId;
	            dic[nameof(idNo)] = idNo;
	            dic[nameof(beginTime)] = beginTime;
	            dic[nameof(endTime)] = endTime;

            return dic;
        }

    }
    
    public partial class req获取服务器当前时间 : RequestBase
    {
        /// <summary>
        /// 获取服务器当前时间
        /// </summary>
        public req获取服务器当前时间()
        {
            service = "/restapi/common/selfDevice/getSystemTime";
            serviceName = "获取服务器当前时间";
        }

        public string idNo { get; set; }


        
        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
	            dic[nameof(idNo)] = idNo;

            return dic;
        }

    }


    public class res查询是否已建档 : ResponseBase
    {
        public 是否建档信息 data { get; set; }
    }

    public class res修改手机号 : ResponseBase
    {
        public bool data { get; set; }
    }

    public class res上传体检报告 : ResponseBase
    {
        public string data { get; set; }
    }

    public class res查询体检报告单 : ResponseBase
    {
        public 查询体检报告单分页数据 data { get; set; }
    }

    public class res获取服务器当前时间 : ResponseBase
    {
        public long data { get; set; }
    }
#pragma warning restore 612
}