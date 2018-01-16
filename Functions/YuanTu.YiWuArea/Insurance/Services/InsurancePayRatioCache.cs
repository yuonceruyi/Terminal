using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ObjectBuilder2;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.YiWuArea.Commons;
using YuanTu.YiWuArea.Insurance.Dtos;
using YuanTu.YiWuArea.Insurance.Models;
using YuanTu.YiWuArea.Insurance.Models.Base;

namespace YuanTu.YiWuArea.Insurance.Services
{
    public static class InsurancePayRatioCache
    {
        public static string RatioApi => YiWuAreaConsts.SiPlatformUrl + "/api/InsuranceSelfRatio";

        static InsurancePayRatioCache()
        {
            InitCache();
        }

        private static readonly Dictionary<string,自负比例列表> CacheList=new Dictionary<string, 自负比例列表>(); 

        public static Result<自负比例列表[]> GetFromCache(自负比例项目列表[] list,Func<List<自负比例项目列表>, Result<自负比例列表[]>>func ,bool usecache)
        {
            var exist = usecache
                ? CacheList.Where(p => list.Any(q => q.医嘱明细序号 == p.Key))
                           .Select(p => p.Value).ToList()
                : new List<自负比例列表>()
                ;
            var notexist = usecache ?list.Where(p => CacheList.All(q => q.Key != p.医嘱明细序号)).ToList(): list.ToList();//本地没有的


            if ((usecache) &&notexist.Any())
            {
                for (int i = notexist.Count-1; i >=0 ; i--)
                {
                    var currentItm = notexist[i];
                    var ret = GetFromMiddleware(currentItm);
                    if (ret!=null)
                    {
                        exist.Add(ret);
                        CacheList[ret.医嘱明细序号]= ret;
                        notexist.Remove(currentItm);
                    }
                }

            }


            if (notexist.Any())
            {
                var ret = func?.Invoke(notexist);
                if (ret?.IsSuccess ?? false)
                {
                    foreach (var itm in ret.Value.Value)
                    {
                        CacheList[itm.医嘱明细序号] = itm;
                    }
                    exist.AddRange(ret.Value.Value);
                    var lst = ret.Value.Value;
                    var currLst = new List<自负比例项目列表>(list);
                    Task.Run(() =>
                    {
                        foreach (var item in lst)
                        {
                            var it = currLst.FirstOrDefault(p => p.医嘱明细序号 == item.医嘱明细序号);
                            if (it != null)
                            {
                                DateTime time;
                                DateTime.TryParseExact(it.医嘱时间, "yyyy.MM.dd", null, DateTimeStyles.None, out time);
                                var webRet = Tools.YiWuMiddlwHttpMethod<object>(RatioApi, HttpMethodEnum.Post, new SelfRatioRequest
                                {
                                    DetailOrder = item.医嘱明细序号,
                                    TradeType = int.Parse(it.药品诊疗类型),
                                    InsuranceCode = it.医保编码,
                                    HospitalCode = "",
                                    RestrictType = int.Parse(it.限制类标志),
                                    SelfRatio = decimal.Parse(item.自负比列),
                                    ImportSelfRatio = decimal.Parse(item.进口类自负比例),
                                    OrderDate = time
                                }.ToJsonString());
                            }

                        }

                    });
                    return Result<自负比例列表[]>.Success(exist.ToArray());
                }
                return Result<自负比例列表[]>.Fail(ret?.Message);
            }
            
            return Result<自负比例列表[]>.Success(exist.ToArray());
        }

      
        private static 自负比例列表 GetFromMiddleware(自负比例项目列表 item)
        {
            var webRet = Tools.YiWuMiddlwHttpMethod<InsuranceSelfRatio>(RatioApi + $"?DetailOrder={item.医嘱明细序号}&TradeType={item.药品诊疗类型}", HttpMethodEnum.Get, null);
            if (webRet.IsSuccess)
            {
                return new 自负比例列表()
                {
                    医嘱明细序号 = webRet.Value.DetailOrder,
                    自负比列 = webRet.Value.SelfRatio.ToString(),
                    进口类自负比例 = webRet.Value.ImportSelfRatio.ToString(),
                };
            }
            return null;
        }

        private static void InitCache()
        {
            var webRet = Tools.YiWuMiddlwHttpMethod<InsuranceSelfRatio[]>(RatioApi + "?rows=500", HttpMethodEnum.Get, null);
            if (webRet.IsSuccess)
            {
                webRet.Value.ForEach(p =>
                {
                    CacheList[p.DetailOrder] = new 自负比例列表()
                    {
                        医嘱明细序号 = p.DetailOrder,
                        自负比列 = p.SelfRatio.ToString(),
                        进口类自负比例 = p.ImportSelfRatio.ToString(),
                    };
                });
            }
        }
    }


    public static class InsuranceRatioCache
    {
        
    }
}
