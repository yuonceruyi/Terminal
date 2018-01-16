using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ObjectBuilder2;
using Newtonsoft.Json;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Core.Services.PrintService;
using YuanTu.Devices.Printer;
using IPrintable = YuanTu.Consts.Models.Print.IPrintable;

namespace YuanTu.ShengZhouHospital.Component.PrintAgain.Common
{
    public class SilpHandler
    {
        /// <summary>
        /// 此方法不允许解析图片信息 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Result<Queue<IPrintable>> DeserializeObject(string json)
        {
            try
            {
                var jsonObject = json.ToJsonObject<List<PrintUpLoadModel>>();
                var data = new Queue<IPrintable>();
                jsonObject.ForEach(p =>
                {
                    IPrintable table = (IPrintable)JsonConvert.DeserializeObject(p.JsonStr, p.Type);
                    data.Enqueue(table);
                });
                return Result<Queue<IPrintable>>.Success(data);
            }
            catch (Exception ex)
            {
                return Result<Queue<IPrintable>>.Fail($"凭条数据解析失败：{ex.Message}");
            }

        }

        /// <summary>
        /// 此方法不允许上传图片信息图片请额外上传解析
        /// </summary>
        /// <param name="queque"></param>
        /// <returns></returns>
        public static Result<string> Serialize(Queue<IPrintable> queque)
        {
            try
            {
                var data = new List<PrintUpLoadModel>();
                foreach (var item in queque)
                {
                    data.Add(new PrintUpLoadModel
                    {
                        Type = item.GetType(),
                        JsonStr = item.ToJsonString()
                    });
                }
                return Result<string>.Success(data.ToJsonString());
            }
            catch (Exception ex)
            {
                return Result<string>.Fail($"凭条数据序列化失败：{ex.Message}");
            }

        }
    }
}
