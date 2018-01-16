using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.XiaoShanArea.CYHIS.WebService;
using YuanTu.XiaoShanHealthStation.Component.Auth.Models;
using YuanTu.XiaoShanHealthStation.Component.Register.Models;
using  DataHandler=YuanTu.XiaoShanArea.CYHIS.WebService.DataHandler;
using HAOYUANXX = YuanTu.XiaoShanArea.CYHIS.WebService.HAOYUANXX;

namespace YuanTu.XiaoShanHealthStation.Component.Register.ViewModels
{
    public class SourceViewModel:Default.Component.Register.ViewModels.SourceViewModel
    {
        [Dependency]
        public IChaKaModel ChaKaModel { get; set; }
        [Dependency]
        public IGuaHaoModel GuaHaoModel { get; set; }
        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }
        [Dependency]
        public IPrintModel PrintModel { get; set; }
        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }
        [Dependency]
        public IPrintManager PrintManager { get; set; }
        [Dependency]
        public IRegTypesModel RegTypesModel { get; set; }
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = GuaHaoModel.号源信息.Select(p => new InfoMore
            {
                Title = $"{p.JIUZHENSJ}",
                Type = $"序号 {p.GUAHAOXH}",
                Tag = p,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<InfoMore>(list);
            
            
            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号号源 : SoundMapping.选择预约号源);
        }
        protected override void Confirm(Info i)
        {
            GuaHaoModel.所选号源 = i.Tag.As<HAOYUANXX>();
            ChangeNavigationContent(i.Title);
            var schedulInfo = GuaHaoModel.所选排班;
            PaymentModel.Self = decimal.Parse(schedulInfo.ZHENLIAOF);
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Total = decimal.Parse(schedulInfo.ZHENLIAOJSF);
            PaymentModel.NoPay = true; //默认预约或者自费金额为0时不支付
            PaymentModel.ConfirmAction = Confirm;

            PaymentModel.LeftList = new List<PayInfoItem>()
            {
                new PayInfoItem("日期：",schedulInfo.PAIBANRQ),
                new PayInfoItem("时间：",GuaHaoModel.所选号源?.JIUZHENSJ),
                new PayInfoItem("科室：",schedulInfo.KESHIMC),
                new PayInfoItem("医生：",GuaHaoModel.所选医生?.YISHENGXM),
            };

            PaymentModel.RightList = new List<PayInfoItem>()
            {
                new PayInfoItem("挂号序号：",$"{GuaHaoModel.所选号源?.GUAHAOXH}"),
                new PayInfoItem("诊疗费：",$"{PaymentModel.Self}元"),
                new PayInfoItem("诊疗费加收：",$"{PaymentModel.Total}元"),
                new PayInfoItem("挂号金额：",$"{PaymentModel.Total+PaymentModel.Self}元",true),
            };
          
            Next();
        }
        protected virtual Result Confirm()
        {
            return DoCommand(p =>
            {

                CLINICORDERD_OUT res;
                var req = new CLINICORDERD_IN()
                {
                    BASEINFO = Instance.Baseinfo,
                    PATIENTID = ChaKaModel.查询建档Out?.就诊卡号,
                    OPERATOR = FrameworkConst.OperatorId,
                    ORDERDATE = GuaHaoModel.RegDate.ToString("yyyy-MM-dd"),
                    REGDEPTID = GuaHaoModel.所选号源?.KESHIDM,
                    DOCTORID = GuaHaoModel.所选号源?.YISHENGDM,
                    CLINICTTYPE = GuaHaoModel.所选号源?.GUAHAOLB,
                    DUTYTYPE = GuaHaoModel.所选号源?.GUAHAOBC,
                    SEQUENCENUM = RegTypesModel.SelectRegType.RegType!=RegType.普通门诊?GuaHaoModel.所选号源?.GUAHAOXH:GuaHaoModel.所选号源?.DANGTIANPBID,
                    TELNO = GuaHaoModel.AppointPhone,
                };
                if (!DataHandler.CLINICORDERD(req, out res))
                    {
                        return Result.Fail("预约失败");
                    }
                if (res.OUTMSG.ERRNO != "0")
                {
                    return Result.Fail($"预约失败:{res.OUTMSG?.ERRMSG}");
                }
                    GuaHaoModel.预约结果 = res;
                    ExtraPaymentModel.Complete = true;
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg =  "预约成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分预约",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        PrintablesList =new List<Queue<IPrintable>> {AppointPrintables()},
                        TipImage = "提示_凭条"
                    });
                    Navigate( A.YY.Print);
                    return Result.Success();
              

            }).Result;
        }

        protected virtual Queue<IPrintable> AppointPrintables()
        {

            var queue = PrintManager.NewQueue("预约挂号单");
            var appoint =GuaHaoModel.预约结果;
            var doctor = GuaHaoModel.所选医生;
            var schedule = GuaHaoModel.所选排班;
            var patientInfo = ChaKaModel.查询建档Out;
            var source = GuaHaoModel.所选号源;
            var sb = new StringBuilder();
            sb.Append($"状态：预约成功\n");
            sb.Append($"姓名：{patientInfo?.病人姓名}\n");
            sb.Append($"门诊号：{patientInfo?.就诊卡号}\n");
            sb.Append($"交易类型：预约挂号\n");
            sb.Append($"预约序号：{appoint?.SEQUENCENUM}\n");
            sb.Append($"取号密码：{appoint?.ORDERNUM}\n");
            sb.Append($"预约日期：{GuaHaoModel.RegDate.ToString("yyyy-MM-dd")}\n");
            sb.Append($"预约科室：{schedule?.KESHIMC}\n");
            sb.Append($"预约医生：{doctor?.YISHENGXM}\n");
            sb.Append($"取号日期：{source?.RIQI}\n");
            sb.Append($"候诊时间：{source?.JIUZHENSJ}\n");
            sb.Append($"就诊地址：{schedule?.JIUZHENDD}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请于当日取号就诊。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

    }
}
