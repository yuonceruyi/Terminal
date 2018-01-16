using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using YuanTu.ChongQingArea.Component.Auth.ViewModels;
using YuanTu.ChongQingArea.Component.Auth.Views;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Systems;
using static YuanTu.Core.Systems.WindowHelper;

namespace YuanTu.ChongQingArea.SiHandler
{
    public class SiContext:IDisposable
    {
        #region Input

        public string 经办人 { get; set; }
        public string 住院号_门诊号 { get; set; }
        public string 社保卡卡号 { get; set; }
      //  [Obsolete("已经从extend中获取BILLDEPTID")]
        public string 科室编码 { get; set; }
        [Obsolete("已经从extend中获取DOCZJH")]
        public string 医生编码 { get; set; }
        public string 本次结算总金额 { get; set; }
        public string 本次结算明细总条数 { get; set; }
        public string 入院日期 { get; set; }
        public string 职工医疗类别 { get; set; }
        public string 居民医疗类别 { get; set; }
        public string 入院诊断 { get; set; }
        public List<ISiItem> itemList { get; set; }

        #endregion Input

        #region Actions
        
        private Req获取人员基本信息 req获取人员基本信息;
        public Res获取人员基本信息 res获取人员基本信息 { get; set; }

        public Result 获取人员基本信息()
        {
            // 获取人员基本信息

            req获取人员基本信息 = new Req获取人员基本信息()
            {
                险种类别 = "1",
                社保卡卡号 = 社保卡卡号,
            };
            var result获取人员基本信息 = SiInterface.获取人员基本信息(req获取人员基本信息);
            if (!result获取人员基本信息.IsSuccess)
                return Result.Fail(result获取人员基本信息.Message);

            res获取人员基本信息 = result获取人员基本信息.Value;
            return Result.Success();
        }

        private Req获取人员账户基础信息 req获取人员账户基础信息;
        public Res获取人员账户基础信息 res获取人员账户基础信息 { get; set; }

        public Result 获取人员账户基础信息()
        {
            // 获取人员账户基础信息
            req获取人员账户基础信息 = new Req获取人员账户基础信息()
            {
                险种类别 = "1",
                社保卡卡号 = 社保卡卡号,
            };
            var result获取人员账户基础信息 = SiInterface.获取人员账户基础信息(req获取人员账户基础信息);
            if (!result获取人员账户基础信息.IsSuccess)
                return Result.Fail(result获取人员账户基础信息.Message);
            res获取人员账户基础信息 = result获取人员账户基础信息.Value;
            return Result.Success();
        }

        private Req获取医保特殊病审批信息 req获取医保特殊病审批信息;
        public Res获取医保特殊病审批信息 res获取医保特殊病审批信息 { get; set; }

        public Result 获取医保特殊病审批信息()
        {
            // 获取医保特殊病审批信息
            req获取医保特殊病审批信息 = new Req获取医保特殊病审批信息()
            {
                社保卡卡号 = 社保卡卡号,
            };
            var result获取医保特殊病审批信息 = SiInterface.获取医保特殊病审批信息(req获取医保特殊病审批信息);
            if (!result获取医保特殊病审批信息.IsSuccess)
                return Result.Fail(result获取医保特殊病审批信息.Message);
            res获取医保特殊病审批信息 = result获取医保特殊病审批信息.Value;
            return Result.Success();
        }

        private Req就诊登记 req就诊登记;
        public Res就诊登记 res就诊登记 { get; set; }
        private bool 就诊登记Success;

        public Result 就诊登记()
        {
            var doctCode = (itemList.FirstOrDefault()?.DOCZJH).BackNotNullOrEmpty(医生编码);
            var deptCode = 科室编码;//(itemList.FirstOrDefault()?.BILLDEPTID).BackNotNullOrEmpty(科室编码);
            // 就诊登记
            req就诊登记 = new Req就诊登记()
            {
                险种类别 = "1",
                社会保障识别号 = 社保卡卡号,
                住院号_门诊号 = 住院号_门诊号,

                医疗类别 = 职工医疗类别,
                居民特殊就诊标记 = 居民医疗类别,

                病案号 = "",

                入院科室编码 = deptCode,
                入院医师编码 = doctCode,

                操作员 = 经办人,

                并发症信息 = "",

                入院日期 = 入院日期,
                入院诊断 = 入院诊断,
                急诊转住院发生时间 = "",

                工伤个人编号 = "",
                工伤单位编号 = "",

                新生儿出生日期 = "",
                生育证号码 = "",
            };
            var result就诊登记 = SiInterface.就诊登记(req就诊登记);
            if (!result就诊登记.IsSuccess)
                return Result.Fail(result就诊登记.Message);
            res就诊登记 = result就诊登记.Value;
            就诊登记Success = true;
            return Result.Success();
        }

        private Req添加处方明细 req添加处方明细;
        private Res添加处方明细 res添加处方明细;
        private List<Tuple<Req添加处方明细, Res添加处方明细>> 添加处方明细Pairs;
        private bool 添加处方明细Success;

        public Result 添加处方明细()
        {
            添加处方明细Pairs = new List<Tuple<Req添加处方明细, Res添加处方明细>>();
            // 添加处方明细
            foreach (var item in itemList)
            {
                req添加处方明细 = new Req添加处方明细()
                {
                    住院号_门诊号 = 住院号_门诊号,
                    Items = new List<添加处方明细Item>()
                };
                req添加处方明细.Items.Add(new 添加处方明细Item()
                {
                    处方号 = item.RECEIPTNO,
                    开方日期 = item.RECEIPTTIME,
                    项目医保流水号 = item.CENTERCODE,
                    医院内码 = item.CENTERCODE, //TODO 暂时
                    项目名称 = item.ITEMNAME,
                    单价 = item.PRICE, //
                    数量 = item.NUM, //

                    急诊标志 = "0", //非急诊
                    处方医生名字 = item.MARKDESC.BackNotNullOrEmpty(item.DOCTOR,""),

                    经办人 = 经办人,
                    单位 = item.ITEMUNIT,
                    规格 = "",
                    剂型 = "",
                    冲消明细流水号 = "", //留空
                    金额 = "",
                    科室编码 = 科室编码,
                    科室名称 = "",
                    医师编码 = item.DOCZJH??"",
                    每次用量 = "",
                    用法标准 = "",
                    执行周期 = "",

                    险种类别 = "1", //医疗保险
                    转自费标识 = "",
                });
                var result添加处方明细 = SiInterface.添加处方明细(req添加处方明细);
                if (!result添加处方明细.IsSuccess)
                    return Result.Fail(result添加处方明细.Message);
                添加处方明细Success = true;
                res添加处方明细 = result添加处方明细.Value;

                添加处方明细Pairs.Add(new Tuple<Req添加处方明细, Res添加处方明细>(req添加处方明细, res添加处方明细));
            }
            return Result.Success();
        }

        private Req预结算 req预结算;
        public Res预结算 res预结算 { get; set; }
        private bool 预结算Sucess;

        public Result 预结算()
        {
            var doctCode = (itemList.FirstOrDefault()?.DOCZJH).BackNotNullOrEmpty(医生编码);
            var deptCode = 科室编码;//(itemList.FirstOrDefault()?.BILLDEPTID).BackNotNullOrEmpty(科室编码);

            // 预结算
            req预结算 = new Req预结算()
            {
                险种类别 = "1",
                住院号_门诊号 = 住院号_门诊号,

                本次结算总金额 = 本次结算总金额,
                本次结算明细总条数 = 本次结算明细总条数,
                账户余额支付标志 = "",//为空时，默认为用账户余额支付

                截止日期 = "", //为空时表示结算当前时间前所有费用

                住院床日 = "", //门诊类型的结算可以为空
                工伤认定编号 = "",
                工伤认定疾病编码 = "",
                尘肺结算类型 = "",

                出院科室编码 = deptCode,
                出院医师编码 = doctCode,
            };
            var result预结算 = SiInterface.预结算(req预结算);
            if (!result预结算.IsSuccess)
            {
                return Result.Fail(result预结算.Message);
            }
            res预结算 = result预结算.Value;
            预结算Sucess = true;
            return Result.Success();
        }

        public Res结算 res结算 { get; set; }
        private Req结算 req结算;
        private bool 结算Sucess;

        public Result 结算()
        {
            var doctCode = (itemList.FirstOrDefault()?.DOCZJH).BackNotNullOrEmpty(医生编码);
            var deptCode = 科室编码;//(itemList.FirstOrDefault()?.BILLDEPTID).BackNotNullOrEmpty(科室编码);

            // 结算
            req结算 = new Req结算()
            {
                险种类别 = "1",
                住院号_门诊号 = 住院号_门诊号,

                结算类别 = "0",
                经办人 = 经办人,

                本次结算总金额 = 本次结算总金额,
                本次结算明细总条数 = 本次结算明细总条数,
                账户余额支付标志 = "",//为空时，默认为用账户余额支付

                住院床日 = "", //门诊类型的结算可以为空
                中途结算终止日期 = "",

                工伤认定编号 = "",
                工伤认定疾病编码 = "",
                尘肺结算类型 = "",

                HIS开发商编码 = "",

                出院科室编码 = deptCode,
                出院医师编码 = doctCode,
            };
            var result结算 = SiInterface.结算(req结算);
            Logger.Main.Info($"R236社保结算{result结算.IsSuccess.ToString()}:{result结算.Message}");
            if (!result结算.IsSuccess)
            {
                string message = result结算.Message;
                if(message == "用户密码输入超过三次,请检查密码是否有效")
                    return Result.Fail("密码错误");
                return Result.Fail(result结算.Message);
            }
            res结算 = result结算.Value;
            结算Sucess = true;
            return Result.Success();
        }

        #endregion

        public Result 冲正交易()
        {
            bool fail = false;
            if (添加处方明细Success)
            {
                if (结算Sucess)
                {
                    var result冲正 = SiInterface.冲正交易(new Req冲正交易()
                    {
                        住院号_门诊号 = 住院号_门诊号,
                        险种类别 = "1",
                        交易流水号 = res结算.交易流水号,
                        冲账类型 = "0",
                        经办人 = 经办人,
                    });
                    if (!result冲正.IsSuccess)
                    {
                        // TODO 冲正失败处理
                        Logger.Main.Error($"冲正交易 失败:res结算.交易流水号:{res结算.交易流水号}");
                        fail = true;
                    }
                }
                else
                {
                    foreach (var pair in 添加处方明细Pairs)
                    {
                        for (var i = 0; i < pair.Item2.处方明细.Count; i++)
                        {
                            var item = pair.Item2.处方明细[i];
                            var result冲正处方明细 = SiInterface.冲正交易(new Req冲正交易()
                            {
                                住院号_门诊号 = 住院号_门诊号,
                                险种类别 = "1",
                                交易流水号 = item.交易流水号,
                                冲账类型 = "0",
                                经办人 = 经办人,
                            });
                            if (!result冲正处方明细.IsSuccess)
                            {
                                // TODO 冲正失败处理
                                Logger.Main.Error($"冲正交易 失败:item.交易流水号:{item.交易流水号}");
                                fail = true;
                            }
                        }
                    }
                }
            }
            if (就诊登记Success)
            {
                var result冲正 = SiInterface.冲正交易(new Req冲正交易()
                {
                    住院号_门诊号 = 住院号_门诊号,
                    险种类别 = "1",
                    交易流水号 = res就诊登记.交易流水号,
                    冲账类型 = "0",
                    经办人 = 经办人,
                });
                if (!result冲正.IsSuccess)
                {
                    // TODO 冲正失败处理
                    Logger.Main.Error("res就诊登记.交易流水号:" + res就诊登记.交易流水号);
                }
            }

            待冲正 = false;

            if (fail)
                return Result.Fail("医保冲正失败");
            return Result.Success();
        }

        public Result Run()
        {
            var result获取人员基本信息 = 获取人员基本信息();
            if (!result获取人员基本信息.IsSuccess)
                return result获取人员基本信息;

            var result获取人员账户基础信息 = 获取人员账户基础信息();
            if (!result获取人员账户基础信息.IsSuccess)
                return result获取人员账户基础信息;

            var result获取医保特殊病审批信息 = 获取医保特殊病审批信息();
            if (!result获取医保特殊病审批信息.IsSuccess)
                return result获取医保特殊病审批信息;

            var result就诊登记 = 就诊登记();
            if (!result就诊登记.IsSuccess)
                return result就诊登记;

            var result添加处方明细 = 添加处方明细();
            if (!result添加处方明细.IsSuccess)
                return result添加处方明细;

            var result预结算 = 预结算();
            if (!result预结算.IsSuccess)
                return result预结算;

            running = true;
            Task.Factory.StartNew(CheckInputWindow);
            try
            {
                var result结算 = 结算();
                if (!result结算.IsSuccess)
                    return result结算;
            }
            finally
            {
                StopCheckInputWindow();
            }

            var reqHisRegister = new ReqHisRegister()
            {
                医保号 = 社保卡卡号,
                个人编号 = 社保卡卡号,

                姓名 = res获取人员基本信息.姓名,
                性别 = res获取人员基本信息.性别,
                年龄 = res获取人员基本信息.实足年龄,
                身份证号 = res获取人员基本信息.身份证号,
                民族 = res获取人员基本信息.民族,
                住址 = res获取人员基本信息.住址,
                人员类别 = res获取人员基本信息.人员类别,
                是否享受公务员待遇 = res获取人员基本信息.是否享受公务员待遇,
                单位名称 = res获取人员基本信息.单位名称,
                //统筹区编号 = res1.,
                民政人员类别 = res获取人员基本信息.民政人员类别,
                居民缴费档次 = res获取人员基本信息.居民缴费档次,
                参保类别 = res获取人员基本信息.参保类别,
                就诊时间 = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),//当前时间
                险种类别 = "1",//医疗保险
                居民档次 = res获取人员基本信息.居民缴费档次,

                住院门诊号 = 住院号_门诊号,
                就诊流水号 = res就诊登记.交易流水号,

                帐户余额 = res获取人员账户基础信息.帐户余额,

                //病种名称 = res1.,
                //并发症 = res1.,
                医疗类别 = "11", //普通门诊
                结算后个人帐户余额 = res结算.帐户余额,
                结算流水号 = res结算.交易流水号,
                统筹支付 = res结算.统筹支付,
                帐户支付 = res结算.帐户支付,
                公务员补助 = res结算.公务员补助,
                大额理赔金额 = res结算.大额理赔金额,
                公务员返还 = res结算.历史起付线公务员返还,
                //单病种医院垫支 = res3.,
                民政救助金额 = res结算.民政救助金额,
                民政救助门诊余额 = res结算.民政救助门诊余额,
                耐多药项目支付金额 = res结算.耐多药项目支付金额,
                一般诊疗支付数 = res结算.一般诊疗支付数,
                神华救助基金支付数 = res结算.神华救助基金支付数,
                本年统筹支付累计 = res结算.本年统筹支付累计,
                本年大额支付累计 = res结算.本年大额支付累计,
                特病起付线支付累计 = res结算.特病起付线支付累计,
                耐多药项目累计 = res结算.耐多药项目累计,
                民政救助住院支付累计 = res结算.本年民政救助住院支付累计,
                结算时间 = res结算.中心结算时间,
                起付线支付金额 = res结算.本次起付线支付金额,
                符合报销费用金额 = res结算.本次进入医保范围费用,
                //区县公务员支付 = res3.,
                生育统筹支付 = res结算.生育基金支付,
                生育现金支付 = res结算.生育现金支付,
                工伤统筹支付 = res结算.工伤基金支付,
                工伤现金支付 = res结算.工伤现金支付,
            };
            Logger.Main.Info(reqHisRegister.ToJsonString());
            return Result.Success();
        }

        public Result RunFirstHalf()
        {
            Logger.Main.Info("获取人员基本信息");
            var result获取人员基本信息 = 获取人员基本信息();
            if (!result获取人员基本信息.IsSuccess)
                return result获取人员基本信息;

            Logger.Main.Info("获取人员账户基础信息");
            var result获取人员账户基础信息 = 获取人员账户基础信息();
            if (!result获取人员账户基础信息.IsSuccess)
                return result获取人员账户基础信息;

            Logger.Main.Info("获取医保特殊病审批信息");
            var result获取医保特殊病审批信息 = 获取医保特殊病审批信息();
            if (!result获取医保特殊病审批信息.IsSuccess)
                return result获取医保特殊病审批信息;

            Logger.Main.Info("就诊登记");
            var result就诊登记 = 就诊登记();
            if (!result就诊登记.IsSuccess)
                return result就诊登记;

            待冲正 = true;

            Logger.Main.Info("添加处方明细");
            var result添加处方明细 = 添加处方明细();
            if (!result添加处方明细.IsSuccess)
                return result添加处方明细;

            待冲正 = true;

            Logger.Main.Info("预结算");
            var result预结算 = 预结算();
            if (!result预结算.IsSuccess)
                return result预结算;

            Logger.Main.Info("return Result.Success");
            return Result.Success();
        }

        public Result<ReqHisRegister> RunSecondHalf()
        {
            running = true;
            Task.Factory.StartNew(CheckInputWindow);
            try
            {
                
                var result结算 = 结算();
                if (!result结算.IsSuccess)
                    return result结算.Convert<ReqHisRegister>();
                待冲正 = true;
            }
            finally
            {
                StopCheckInputWindow();
            }

            var reqHisRegister = new ReqHisRegister()
            {
                医保号 = 社保卡卡号,
                个人编号 = 社保卡卡号,

                姓名 = res获取人员基本信息.姓名,
                性别 = res获取人员基本信息.性别,
                年龄 = res获取人员基本信息.实足年龄,
                身份证号 = res获取人员基本信息.身份证号,
                民族 = res获取人员基本信息.民族,
                住址 = res获取人员基本信息.住址,
                人员类别 = res获取人员基本信息.人员类别,
                是否享受公务员待遇 = res获取人员基本信息.是否享受公务员待遇,
                单位名称 = res获取人员基本信息.单位名称,
                //统筹区编号 = res1.,
                民政人员类别 = res获取人员基本信息.民政人员类别,
                居民缴费档次 = res获取人员基本信息.居民缴费档次,
                参保类别 = res获取人员基本信息.参保类别,
                就诊时间 = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),//当前时间
                险种类别 = "1",//医疗保险
                居民档次 = res获取人员基本信息.居民缴费档次,

                住院门诊号 = 住院号_门诊号,
                就诊流水号 = res就诊登记.交易流水号,

                帐户余额 = res获取人员账户基础信息.帐户余额,

                //病种名称 = res1.,
                //并发症 = res1.,
                医疗类别 = 职工医疗类别, //普通门诊
                结算后个人帐户余额 = res结算.帐户余额,
                结算流水号 = res结算.交易流水号,
                统筹支付 = res结算.统筹支付,
                帐户支付 = res结算.帐户支付,
                公务员补助 = res结算.公务员补助,
                大额理赔金额 = res结算.大额理赔金额,
                公务员返还 = res结算.历史起付线公务员返还,
                //单病种医院垫支 = res3.,
                民政救助金额 = res结算.民政救助金额,
                民政救助门诊余额 = res结算.民政救助门诊余额,
                耐多药项目支付金额 = res结算.耐多药项目支付金额,
                一般诊疗支付数 = res结算.一般诊疗支付数,
                神华救助基金支付数 = res结算.神华救助基金支付数,
                本年统筹支付累计 = res结算.本年统筹支付累计,
                本年大额支付累计 = res结算.本年大额支付累计,
                特病起付线支付累计 = res结算.特病起付线支付累计,
                耐多药项目累计 = res结算.耐多药项目累计,
                民政救助住院支付累计 = res结算.本年民政救助住院支付累计,
                结算时间 = res结算.中心结算时间,
                起付线支付金额 = res结算.本次起付线支付金额,
                符合报销费用金额 = res结算.本次进入医保范围费用,
                //区县公务员支付 = res3.,
                生育统筹支付 = res结算.生育基金支付,
                生育现金支付 = res结算.生育现金支付,
                工伤统筹支付 = res结算.工伤基金支付,
                工伤现金支付 = res结算.工伤现金支付,
            };
            ReqHisRegister = reqHisRegister;
            Logger.Main.Info(reqHisRegister.ToJsonString());
            return Result<ReqHisRegister>.Success(reqHisRegister);
        }

        public bool 待冲正 { get; set; }

        public ReqHisRegister ReqHisRegister { get; set; }
        
        #region Password

        private bool running;
        public string 社保卡密码 { get; set; } = "123456";
        public Action<bool> OnSiPassword { get; set; }
        public int InputPWD = 0;//0 初始化  1开始输入 2输入完毕  -1取消
        IntPtr InputWindow= IntPtr.Zero;
        async void CheckInputWindow()
        {
            var firstTime = true;
            while (running)
            {
                await Task.Delay(500);
                var window = WindowHelper.FindWindow("#32770", null);
                if (window == IntPtr.Zero)
                    continue;

                var edit0 = WindowHelper.FindWindowEx(window, IntPtr.Zero, "Edit", null);
                if (edit0 == IntPtr.Zero)
                    continue;

                var edit = WindowHelper.FindWindowEx(window, edit0, "Edit", null);
                if (edit == IntPtr.Zero)
                    continue;
                InputWindow = edit;
                Logger.Main.Info($"发现窗口:{edit} 密码状态{InputPWD.ToString()}=>1");
                InputPWD = 1 ;
                WindowHelper.SetWindowPos(window, WindowHelper.WindowFlags.Top, 1, 1, 0, 0, WindowHelper.SetWindowPosFlags.IgnoreResize);
                //edit.
                OnSiPassword(firstTime);
                firstTime = false;
                if (InputPWD == 2)
                {
                    WindowHelper.SetForegroundWindow(window);
                    Logger.Main.Info($"自动输入:{社保卡密码}");
                    var p = 社保卡密码;
                    if (p.Length != 6 || !p.All(char.IsDigit))
                        return;

                    var keys = p.Select(c => (uint)c).ToArray();
                    Send(edit, keys);
                    Logger.Main.Info("密码输入完毕");
                }
                //InputPWD = false;
            }
        }
        void Send(IntPtr window, uint[] keys)
        {
            foreach (var key in keys)
            {
                Thread.Sleep(200);
                Send(window, key);
            }
            Thread.Sleep(1000);
            try
            {
                var len = WindowHelper.SendMessage(window, (uint)WindowMessage.GETTEXTLENGTH, 0, 0);
                Logger.Main.Info("密码总计输入" + len.ToString());
                StringBuilder pwd = new StringBuilder(256);
                var str = WindowHelper.SendMessage(window, (uint)WindowMessage.GETTEXT,(IntPtr)256, pwd);
                Logger.Main.Info("密码内容" + str.ToString()+"  "+ pwd.ToString());
            }
            catch (Exception ex) { Logger.Main.Info("密码输入统计异常" + ex.Message+ex.StackTrace); }
            WindowHelper.PostMessage(window, 0x100, 0x0D, 0x001C0001);
        }
        void Send(IntPtr window, uint key)
        {
            //WindowHelper.PostMessage(window, 0x100, key, 0x00020001);//keydown
            //Thread.Sleep(100);
            WindowHelper.PostMessage(window, 0x102, key, 0x00020001);//keypress
            //Thread.Sleep(100);
            //WindowHelper.PostMessage(window, 0x101, key, 0xC0020001);//keyup
        }
        void SendAll(IntPtr window, uint key)
        {
            WindowHelper.PostMessage(window, 0x100, key, 0x00020001);//keydown
            Thread.Sleep(100);
            WindowHelper.PostMessage(window, 0x102, key, 0x00020001);//keypress
            Thread.Sleep(100);
            WindowHelper.PostMessage(window, 0x101, key, 0xC0020001);//keyup
        }

        public void CloseInputWindow()
        {
            SendAll(InputWindow, 27);
            StopCheckInputWindow(); 
        }

        void StopCheckInputWindow()
        {
            InputWindow = IntPtr.Zero;
            running = false;
        }

        #endregion

        public void Dispose()
        {
            if (!待冲正)
                return;
            var result = 冲正交易();
            Logger.Main.Info("医保冲正交易:" + (!result.IsSuccess ? $"失败:{result.Message}" : "成功"));
        }
    }

    public interface ISiItem
    {
        string RECEIPTNO { get; set; }
        string RECEIPTTIME { get; set; }
        string FEESITEM { get; set; }

        string ITEMID { get; set; }
        string ITEMNAME { get; set; }
        string ITEMUNIT { get; set; }
        string NUM { get; set; }
        string PRICE { get; set; }
        string SHOULDMONEY { get; set; }
        string ACTUALMONEY { get; set; }
        string CENTERCODE { get; set; }
        string BILLDEPTID { get; set; }
        /// <summary>
        /// 医师名字（挂号）
        /// </summary>
        string MARKDESC { get; set; }
        /// <summary>
        /// 医师名字(缴费）
        /// </summary>
        string DOCTOR { get; set; }
        /// <summary>
        /// 医师身份证号
        /// </summary>
        string DOCZJH { get; set; }
    }

    public class 获取挂号号源Extend : ISiItem
    {
        public string RECEIPTNO { get; set; }
        public string RECEIPTTIME { get; set; }
        ///<summary>
        ///收入项目
        ///</summary>
        public string SRXM { get; set; }

        ///<summary>
        ///收据费目
        ///</summary>
        public string FEESITEM { get; set; }

        ///<summary>
        ///收费ID
        ///</summary>
        public string ITEMID { get; set; }

        ///<summary>
        ///收费编码
        ///</summary>
        public string ITEMCODE { get; set; }

        ///<summary>
        ///收费名称
        ///</summary>
        public string ITEMNAME { get; set; }

        ///<summary>
        ///计量单位
        ///</summary>
        public string ITEMUNIT { get; set; }

        ///<summary>
        ///数量
        ///</summary>
        public string NUM { get; set; }

        ///<summary>
        ///单价
        ///</summary>
        public string PRICE { get; set; }

        ///<summary>
        ///应收金额
        ///</summary>
        public string SHOULDMONEY { get; set; }

        ///<summary>
        ///实收金额
        ///</summary>
        public string ACTUALMONEY { get; set; }

        ///<summary>
        ///医保编码，用于医保结算
        ///</summary>
        public string CENTERCODE { get; set; }

        ///<summary>
        ///开单科室编码,用于医保结算
        ///</summary>
        public string BILLDEPTID { get; set; }

        public string MARKDESC { get; set; }
        public string DOCTOR { get; set; }
        public string DOCZJH { get; set; }
    }

    public class DATAPARAM
    {
        public List<ITEM> LIST { get; set; }

        public int COUNT { get; set; }
    }

    public class ITEM : ISiItem
    {
        public string RECEIPTNO { get; set; }
        public string RECEIPTTIME { get; set; }
        public string BILLDEPT { get; set; }
        public string EXECDEPT { get; set; }
        public string MARKDESC { get; set; }
        public string DOCTOR { get; set; }
        public string FEESTYPE { get; set; }
        public string FEESITEM { get; set; }
        public string GROUPID { get; set; }
        public string GROUPNAME { get; set; }
        public string ITEMID { get; set; }
        public string ITEMNAME { get; set; }
        public string ITEMUNIT { get; set; }
        public string NUM { get; set; }
        public string PRICE { get; set; }
        public string SHOULDMONEY { get; set; }
        public string ACTUALMONEY { get; set; }
        public string CENTERCODE { get; set; }
        public string BILLDEPTID { get; set; }
        /// <summary>
        /// 医师身份证号
        /// </summary>
        public string DOCZJH { get; set; }
    }

    class ResHisQueryPatientInfo
    {
        public string PatientID { get; set; }
        public string PatName { get; set; }
        public string Birthday { get; set; }
        public string Age { get; set; }
        public string PatSex { get; set; }
        public string IDCard { get; set; }
        public string IsYBBR { get; set; }
        public string InsureNo { get; set; }
        public string AccBalance { get; set; }
        public string ZyBalance { get; set; }
        public string Tel { get; set; }
        public string JZKH { get; set; }
        public string MZH { get; set; }
        public string ZYH { get; set; }
        public string DocName { get; set; }
        public string InDept { get; set; }
        public string OutDept { get; set; }
        public string MzDiagnose { get; set; }
        public string ZyMzDiagnose { get; set; }
        public string InDiagnose { get; set; }
        public string ZyInDiagnose { get; set; }
        public string OutDiagnose { get; set; }
        public string ZyOutDiagnose { get; set; }
        public string InDate { get; set; }
        public string OutDate { get; set; }
        public string InDay { get; set; }
        public string InHosp { get; set; }
        public string PageId { get; set; }
        public string Bed { get; set; }
        public string PrepaidMon { get; set; }
        public string AmountMon { get; set; }

    }

    public class ReqHisRegister
    {
        public string 医保号 { get; set; }
        public string 个人编号 { get; set; }
        public string 姓名 { get; set; }
        public string 性别 { get; set; }
        public string 年龄 { get; set; }
        public string 身份证号 { get; set; }
        public string 民族 { get; set; }
        public string 住址 { get; set; }
        public string 人员类别 { get; set; }
        public string 是否享受公务员待遇 { get; set; }
        public string 单位名称 { get; set; }
        public string 统筹区编号 { get; set; }
        public string 民政人员类别 { get; set; }
        public string 居民缴费档次 { get; set; }
        public string 参保类别 { get; set; }
        public string 就诊时间 { get; set; }
        public string 险种类别 { get; set; }
        public string 居民档次 { get; set; }
        public string 住院门诊号 { get; set; }
        public string 就诊流水号 { get; set; }
        public string 病种名称 { get; set; }
        public string 并发症 { get; set; }
        public string 医疗类别 { get; set; }
        public string 结算后个人帐户余额 { get; set; }
        public string 结算流水号 { get; set; }
        public string 统筹支付 { get; set; }
        public string 帐户支付 { get; set; }
        public string 公务员补助 { get; set; }
        public string 大额理赔金额 { get; set; }
        public string 公务员返还 { get; set; }
        public string 帐户余额 { get; set; }
        public string 单病种医院垫支 { get; set; }
        public string 民政救助金额 { get; set; }
        public string 民政救助门诊余额 { get; set; }
        public string 耐多药项目支付金额 { get; set; }
        public string 一般诊疗支付数 { get; set; }
        public string 神华救助基金支付数 { get; set; }
        public string 本年统筹支付累计 { get; set; }
        public string 本年大额支付累计 { get; set; }
        public string 特病起付线支付累计 { get; set; }
        public string 耐多药项目累计 { get; set; }
        public string 民政救助住院支付累计 { get; set; }
        public string 结算时间 { get; set; }
        public string 起付线支付金额 { get; set; }
        public string 符合报销费用金额 { get; set; }
        public string 区县公务员支付 { get; set; }
        public string 生育统筹支付 { get; set; }
        public string 生育现金支付 { get; set; }
        public string 工伤统筹支付 { get; set; }
        public string 工伤现金支付 { get; set; }

    }

    class ReqHisBillPay
    {
        public string 医保号 { get; set; }
        public string 个人编号 { get; set; }
        public string 姓名 { get; set; }
        public string 性别 { get; set; }
        public string 年龄 { get; set; }
        public string 身份证号 { get; set; }
        public string 民族 { get; set; }
        public string 住址 { get; set; }
        public string 人员类别 { get; set; }
        public string 是否享受公务员待遇 { get; set; }
        public string 单位名称 { get; set; }
        public string 统筹区编号 { get; set; }
        public string 民政人员类别 { get; set; }
        public string 居民缴费档次 { get; set; }
        public string 参保类别 { get; set; }
        public string 就诊时间 { get; set; }
        public string 险种类别 { get; set; }
        public string 居民档次 { get; set; }
        public string 住院门诊号 { get; set; }
        public string 就诊流水号 { get; set; }
        public string 病种名称 { get; set; }
        public string 并发症 { get; set; }
        public string 医疗类别 { get; set; }
        public string 结算后个人帐户余额 { get; set; }
        public string 结算流水号 { get; set; }
        public string 统筹支付 { get; set; }
        public string 帐户支付 { get; set; }
        public string 公务员补助 { get; set; }
        public string 大额理赔金额 { get; set; }
        public string 公务员返还 { get; set; }
        public string 帐户余额 { get; set; }
        public string 单病种医院垫支 { get; set; }
        public string 民政救助金额 { get; set; }
        public string 民政救助门诊余额 { get; set; }
        public string 耐多药项目支付金额 { get; set; }
        public string 一般诊疗支付数 { get; set; }
        public string 神华救助基金支付数 { get; set; }
        public string 本年统筹支付累计 { get; set; }
        public string 本年大额支付累计 { get; set; }
        public string 特病起付线支付累计 { get; set; }
        public string 耐多药项目累计 { get; set; }
        public string 民政救助住院支付累计 { get; set; }
        public string 结算时间 { get; set; }
        public string 起付线支付金额 { get; set; }
        public string 符合报销费用金额 { get; set; }
        public string 区县公务员支付 { get; set; }
        public string 生育统筹支付 { get; set; }
        public string 生育现金支付 { get; set; }
        public string 工伤统筹支付 { get; set; }
        public string 工伤现金支付 { get; set; }

    }
}
