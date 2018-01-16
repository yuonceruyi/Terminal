using ID003ProtocolManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.DB;
using YuanTu.Core.Log;
using static YuanTu.Devices.UnSafeMethods;
using System.Linq;

namespace YuanTu.Devices.CashBox
{
    /// <summary>
    ///     只进钱不找零的钱箱
    /// </summary>
    public interface ICashInputBox : IDevice
    {
        /// <summary>
        ///     打开钱箱，执行入钞
        /// </summary>
        /// <param name="billcallBack">有入钞时，回调入钞币值</param>
        /// <param name="statuscallback">回调钱箱当前状态</param>
        void StartPoll(Action<int> billcallBack, Action<byte, string> statuscallback = null);
    }

    public class CashInputBox : ICashInputBox
    {
        [Flags]
        public enum Bills
        {
            Bill_1 = 0x01,
            Bill_2 = 0x02,
            Bill_5 = 0x04,
            Bill_10 = 0x08,
            Bill_20 = 0x10,
            Bill_50 = 0x20,
            Bill_100 = 0x40,
            Bill_Other = 0x80,
            All = 0xFF
        }

        private static readonly byte ADDR_FL = 0x03;
        private static Guid _rollSeed = Guid.Empty;
        private static readonly int[] Values = {1, 2, 5, 10, 20, 50, 100};
        private static readonly byte[] unDoneCode = { 0x11, 0x12,0x41, 0x43, 0x47 };
        private readonly IConfigurationManager _configurationManager;
        private DeviceStatus _currentStatus = DeviceStatus.UnInitialized;
        private int _handle;
        private bool _isConnectd;
        private bool _isInit;
        private PollResults prePollret = new PollResults();
        public static byte _pollStatus { get; private set; }

        private int _port;

        public CashInputBox(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public static Bills AcceptBills { get; set; } = Bills.Bill_10 | Bills.Bill_20 | Bills.Bill_50 | Bills.Bill_100 |
                                                        Bills.Bill_Other;

        #region Implementation of IDevice

        /// <summary>
        ///     硬件名称
        /// </summary>
        public string DeviceName { get; } = "CashCode";

        /// <summary>
        ///     硬件唯一标识(整个系统中唯一)
        /// </summary>
        public string DeviceId => DeviceName;

        /// <summary>
        ///     获取设备实时状态
        /// </summary>
        /// <returns></returns>
        public Result<DeviceStatus> GetDeviceStatus()
        {
            if (_pollStatus == 0x15 ||
                _pollStatus == 0x16 ||
                _pollStatus == 0x17 ||
                _pollStatus == 0x80 ||
                _pollStatus == 0x81)
            {
                //不给_currentStatus赋值，无法判断用户是否已停止入钞
                Logger.Device.Info($"[钱箱{DeviceId}]设备状态:{DeviceStatus.Busy.ToString()}");
                return Result<DeviceStatus>.Success(DeviceStatus.Busy);
            }
            else
            {
                Logger.Device.Info($"[钱箱{DeviceId}]设备状态:{_currentStatus.ToString()}");
                return Result<DeviceStatus>.Success(_currentStatus);
            }
        }

        /// <summary>
        ///     1.连接设备
        /// </summary>
        /// <returns></returns>
        public Result Connect()
        {
            Logger.Device.Info($"[钱箱{DeviceId}]开始连接:{_isConnectd}");
            if (_isConnectd)
            {
                return Result.Success();
            }
            _port = _configurationManager.GetValueInt("CashCode:Port");
            Logger.Device.Info($"[钱箱{DeviceId}]开始连接:端口号{_port}");
            var ret = UnSafeMethods.InitDevice(_port, 3000);
            if (ret)
            {
                Logger.Device.Error($"[钱箱{DeviceId}]连接成功， 端口:{_port}");
            }
            else
            {
                Logger.Device.Error($"[钱箱{DeviceId}]连接异常， 端口:{_port}");
                return Result.Fail("钱箱打开失败");
            }
            _isConnectd = true;
            _currentStatus = DeviceStatus.Connected;
            return Result.Success();
        }

        /// <summary>
        ///     2.初始化
        /// </summary>
        /// <returns></returns>
        public Result Initialize()
        {
            if (!_isConnectd)
            {
                return Result.Fail("钱箱未连接");
            }          
            _rollSeed = Guid.NewGuid();
                        
            UnSafeMethods.CmdPoll(ADDR_FL);
            var pollret = UnSafeMethods.GetPollResult();
            prePollret = pollret;
            if (unDoneCode.Contains(pollret.z1))
            {
                return Result.Fail("钱箱存在没有处理完成的业务，请于服务人员联系");
                cashcodeLog(pollret, "钱箱存在没有处理完成的业务");               
            }
            if (!_isInit)
            {
                UnSafeMethods.CmdReset(ADDR_FL);
            }
            UnSafeMethods.CmdIdentification(ADDR_FL);
            UnSafeMethods.CmdPoll(ADDR_FL);
            UnSafeMethods.CmdSetSecurity(0, ADDR_FL);
            UnSafeMethods.CmdBillType((int) AcceptBills, 0xff, ADDR_FL);
            _isInit = true;
            return Result.Success();
        }

        /// <summary>
        ///     3.反初始化
        /// </summary>
        /// <returns></returns>
        public Result UnInitialize()
        {
            if (!_isConnectd)
            {
                return Result.Fail("钱箱未连接");
            }
            _rollSeed = Guid.NewGuid();
            _currentStatus = DeviceStatus.UnInitialized;
            return Result.Success();
        }

        /// <summary>
        ///     4.断开连接
        /// </summary>
        /// <returns></returns>
        public Result DisConnect()
        {
            if (!_isConnectd)
            {
                return Result.Fail("钱箱未连接");
            }
            Logger.Device.Error($"[钱箱{DeviceId}]断开连接成功");
            _rollSeed = Guid.NewGuid();
            _isConnectd = false;
            _currentStatus = DeviceStatus.Disconnect;
            return Result.Success();
        }

        /// <summary>
        ///     打开钱箱，执行入钞
        /// </summary>
        /// <param name="billcallBack">有入钞时，回调入钞币值</param>
        /// <param name="statuscallback"></param>
        public void StartPoll(Action<int> billcallBack, Action<byte, string> statuscallback = null)
        {
            _rollSeed = Guid.NewGuid();
            var currentSeed = _rollSeed;
            while (currentSeed == _rollSeed)
            {
                try
                {
                    UnSafeMethods.CmdPoll(ADDR_FL);
                    var pollret = UnSafeMethods.GetPollResult();                    
                    cashcodeLog(pollret);
                    if (_pollStatus != pollret.z1)
                    {
                        _pollStatus = pollret.z1;
                        //statuscallback?.Invoke(pollret.z1, "未定义");
                    }

                    if (pollret.z1 == 0x81)
                    {
                        if (prePollret.z1 == 0x15 ||
                           prePollret.z1 == 0x17)
                        {
                            var money = Values[pollret.z2];
                            Logger.Device.Info($"钱箱[{DeviceId}]入钞成功，金额(元):{money}");
                            DBManager.Insert(new CashInputInfo
                            {
                                TotalSeconds = money * 100
                            });
                            billcallBack?.Invoke(money);
                        }
                        else
                        {
                            writeLog("可能是压钞不成功或者卡币的纸币");
                            //statuscallback?.Invoke(pollret.z1, "钱箱可能存在没有处理完成的业务，请于服务人员联系");
                        }
                    }
                    else if (pollret.z1 == 0x80)
                    {
                        UnSafeMethods.CmdPack(ADDR_FL);
                    }
                    else if (unDoneCode.Contains(pollret.z1))
                    {
                        _currentStatus = DeviceStatus.Error;
                        prePollret = pollret;
                        statuscallback?.Invoke(pollret.z1, "钱箱可能存在没有处理完成的业务，请于服务人员联系");
                        break;
                    }
                    prePollret = pollret;
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    _currentStatus = DeviceStatus.Error;
                    Logger.Device.Error($"钱箱[{DeviceId}]发生异常，异常内容:{ex.Message + "\n" + ex.StackTrace}");
                }
            }
            var ret = UnSafeMethods.CloseDevice();
            if (!ret)
            {
                _currentStatus = DeviceStatus.Error;
                Logger.Device.Error($"钱箱[{DeviceId}]关闭失败");
            }
        }
        #endregion Implementation of IDevice

        #region cashcodelog
        public static void cashcodeLog(PollResults pollret,string msg=null)
        {
            writeLog("Z1["+pollret.z1+"]Z2["+ pollret .z2+"]:"+ msgCode[pollret.z1]);
            if (pollret.z1 == 0x81)
            {
                var money = Values[pollret.z2];
                writeLog("入钞" + money);
            }
            if (string.IsNullOrWhiteSpace(msg))
            {
                writeLog(msg);
            }
        }
        static Dictionary<byte, string> msgCode = new Dictionary <byte, string>
        {
            {0x10,"POWER UP "},
            {0x13,"INITIALIZE "},
            {0x14,"IDLING "},
            {0x15,"ACCEPTING "},
            {0x17,"STACKING "},
            {0x18,"RETURNING "},
            {0x19,"DISABLED "},
            {0x1A,"OLDING "},
            {0x1B,"BUSY"},
            {0x1C,"REJECTING"},
            {0x1D,"DISPENSING "},
            {0x1E,"UNLOADING "},
            {0x21,"SETTING TYPE CASSETTE "},
            {0x25,"DISPENSED "},
            {0x26,"UNLOADED "},
            {0x28,"INVALID BILL NUMBER "},
            {0x29,"SET CASSETTE TYPE "},
            {0x30,"INVALID COMMAND "},
            {0x41,"DROP CASSETTE FULL "},
            {0x42,"DROP CASSETTE REMOVED "},
            {0x43,"JAM IN ACCEPTOR "},
            {0x44,"JAM IN STACKER "},
            {0x45,"CEATED "},
            {0x47,"Generic BB ERROR codes. Followed by failure description bytes."},
            {0x80,"ESCROW."},
            {0x81,"PACKED, STACKED"},
            {0x82,"RETURNED."}
        };
        /// <summary>
        /// 记录cashcode日志
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static void writeLog(string message)
        {
            string dirName = AppDomain.CurrentDomain.BaseDirectory + "\\cashcode";
            string fileName = dirName + "\\" + DateTimeCore.Now.ToString("yyyy-MM-dd") + ".log";
            FileStream file;

            message = DateTimeCore.Now + "\t" + message;
            try
            {
                if (!Directory.Exists(dirName)) //判断目录是否存在
                {
                    Directory.CreateDirectory(dirName);//生成目录
                }

                if (!File.Exists(fileName)) //判断文件是否存在
                {
                    file = File.Create(fileName);
                    file.Close();
                }

                file = File.Open(fileName, FileMode.Append);
                using (StreamWriter writer = new StreamWriter(file, Encoding.Default))
                {
                    writer.WriteLine(message);
                    writer.Close();
                };
                file.Close();
            }
            catch
            {
                //MessageBox.Show("日志 文件操作失败！");
            }
        }
        #endregion
    }

    public class JCMCashBox : ICashInputBox
    {
        private readonly IConfigurationManager _configurationManager;
        private bool _isConnectd;
        private DeviceStatus _currentStatus = DeviceStatus.UnInitialized;
        public static int PortNumber = 5;

        private Thread comThread; //Thread to poll bill acceptor
        private SerialPort Port;  //Serial port for ID003 Communication        
        private ID003CommandCreater ComDll = new ID003CommandCreater(); //Declaring an instance of ID003CommandCreater which is a class in ID003ProtocolManager
        private byte[] buffer = new byte[255]; //Buffer to use the Dll commands
        private byte[] status = new byte[255]; //Buffer to read data from the serial port
        private int length; //length of status received or command sent
        public static event Action<string> LogEvent = writeLog;
        public Action<int> ReceviedCash;
        public Action<byte,string> NeedResetCashBox; 
        /// <summary>
        ///     硬件名称
        /// </summary>
        public string DeviceName { get; } = "JCM";

        /// <summary>
        ///     硬件唯一标识(整个系统中唯一)
        /// </summary>
        public string DeviceId => DeviceName;

        public JCMCashBox(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public Result Connect()
        {
            if (_isConnectd)
            {
                return Result.Success();
            }
            try
            {
                PortNumber = _configurationManager.GetValueInt("JCM:Port");

                Port = new SerialPort("COM" + PortNumber, 9600, Parity.Even, 8, StopBits.One); //Just initializing the serial port. Here I hard coded COM2
                _isConnectd = true; //Polling is activated
                _currentStatus = DeviceStatus.Connected;
                Port.Open(); //Opening com port for communication
                comThread = new Thread(Poll); //starting new thread that calls Poll() method

                comThread.Start(); //Starting thread used to poll
                return Result.Success();

            }
            catch (Exception ex)
            {
                LogEvent?.Invoke($"[钱箱{DeviceId}]连接异常， 端口:{PortNumber}:"+ ex.Message);                
                return Result.Fail("钱箱打开失败");
            }          
        }

        #region 暂停和开启接收钞票
        /// <summary>
        /// 暂停接收钞票【不关闭端口】
        /// </summary>
        public bool SuspendRecevice(object sender, EventArgs e)
        {
            try
            {
                if (_isConnectd)
                {
                    throw new Exception("端口已关闭");
                }
                byte inhibit = 0x01;
                comThread.Suspend();
                ComDll.SetInhibit(buffer, inhibit);
                length = (int)buffer[1]; //the length is position 1 of the array. The length is casted from byte to int.
                Port.Write(buffer, 0, length); //the buffer it is now written to the com port. offset is 0 and the length is given by length.
                Thread.Sleep(100); //Wait for 100ms.
                comThread.Resume();
                return true;
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke("Suspend failed:" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 开始接收钞票【在端口未关闭的情况下】
        /// </summary>
        public bool StartRecevice(object sender, EventArgs e)
        {
            try
            {
                if (_isConnectd)
                {
                    throw new Exception("端口已关闭");
                }
                byte inhibit = 0x00;
                comThread.Suspend();
                ComDll.SetInhibit(buffer, inhibit);
                length = (int)buffer[1]; //the length is position 1 of the array. The length is casted from byte to int.
                Port.Write(buffer, 0, length); //the buffer it is now written to the com port. offset is 0 and the length is given by length.
                Thread.Sleep(100); //Wait for 100ms.
                comThread.Resume(); return true;
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke("Recevice failed:" + ex.Message);
                return false;
            }
        }
        #endregion

        public Result DisConnect()
        {
            try
            {
                if (!_isConnectd)
                    return Result.Fail("钱箱未连接");
                //closing COM Port
                _isConnectd = false;
                _currentStatus = DeviceStatus.Disconnect;
                //comThread.Suspend();
                comThread.Abort();  //Closed thread

                //stop received
                byte inhibit = 0x01;
                ComDll.SetInhibit(buffer, inhibit);
                length = (int)buffer[1]; //the length is position 1 of the array. The length is casted from byte to int.
                Port.Write(buffer, 0, length); //the buffer it is now written to the com port. offset is 0 and the length is given by length.
                Thread.Sleep(100); //Wait for 100ms.
                //closed port
                Port.Close();
                Thread.Sleep(500);
                return Result.Success();
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke("ClosedPort failed:" + ex.Message);
                return Result.Fail("钱箱断开异常:"+ ex.Message);
            }
        }

        public Result<DeviceStatus> GetDeviceStatus()
        {
            if (status[2] == 0x13 ||
                status[2] == 0x46)
            {
                //不给_currentStatus赋值，无法判断用户是否已停止入钞
                Logger.Device.Info($"[钱箱{DeviceId}]设备状态:{DeviceStatus.Busy.ToString()}");
                return Result<DeviceStatus>.Success(DeviceStatus.Busy);
            }
            else
            {
                Logger.Device.Info($"[钱箱{DeviceId}]设备状态:{_currentStatus.ToString()}");
                return Result<DeviceStatus>.Success(_currentStatus);
            }
        }

        public Result Initialize()
        {
            if (!_isConnectd)
            {
                return Result.Fail("钱箱未连接");
            }                   
            return Result.Success();
        }

        public void StartPoll(Action<int> billcallBack, Action<byte, string> statuscallback = null)
        {
            ReceviedCash = billcallBack;
            NeedResetCashBox = statuscallback;
        }

        public Result UnInitialize()
        {
            if (!_isConnectd)
            {
                return Result.Fail("钱箱未连接");
            }       
            _currentStatus = DeviceStatus.UnInitialized;
            return Result.Success();
        }

        private void Poll()
        {
            Actions.init(buffer, length, ComDll, Port); //Function to Initialize
            while (_isConnectd)
            {
                ComDll.StatusRequest(buffer); //Here Status request command is used. The dll constructs the command into buffer
                length = (int)buffer[1]; //the length is position 1 of the array. The length is casted from byte to int.
                Port.Write(buffer, 0, length); //the buffer it is now written to the com port. offset is 0 and the length is given by length.
                Port.Read(status, 0, 255); //we now read the response of the Bill Acceptor
                Thread.Sleep(100); //Wait for 100ms.

                switch (status[2])
                {
                    //here we check the status of the bill acceptor.
                    case 0x40:
                        LogEvent?.Invoke("Power Up【电源开启】");
                        break;
                    case 0x11:
                        LogEvent?.Invoke("Idling【正常接钞】");
                        break;
                    case 0x1A:
                        LogEvent?.Invoke("Inhibit【暂停接钞】");
                        break;
                    case 0x13://if there is a bill in escrow do the following.
                        LogEvent?.Invoke("0x13【接收到钞票】面额为：" + status[3].ToString("X") + "【0x61（￥1)、0x63（￥5)、0x64（￥10)、0x65（￥20)、0x66（￥50)、0x67（￥100)】");
                        Actions.Accept(buffer, length, ComDll, Port);
                        switch (status[3])
                        {
                            case 0x61:
                                ReceviedCash.Invoke(1);
                                break;
                            case 0x63:
                                ReceviedCash?.Invoke(5);
                                break;
                            case 0x64:
                                ReceviedCash?.Invoke(10);
                                break;
                            case 0x65:
                                ReceviedCash?.Invoke(20);
                                break;
                            case 0x66:
                                ReceviedCash?.Invoke(50);
                                break;
                            case 0x67:
                                ReceviedCash?.Invoke(100);
                                break;
                        }
                        break;
                    case 0x17:
                        LogEvent?.Invoke("Rejected【不能识别的钞票】");
                        break;
                    case 0x43:
                        LogEvent?.Invoke("Stacker Full【堆栈满，应该是钱箱满的意思】");
                        NeedResetCashBox.Invoke(0x43, "堆栈满，钱箱已满");   //弹出需要人工干预的事件
                        break;
                    case 0x44:
                        LogEvent?.Invoke("Stacker Open【堆栈开，应该是钱箱未关闭的意思】");
                        NeedResetCashBox.Invoke(0x44, "堆栈开，钱箱未关闭");   //弹出需要人工干预的事件
                        break;
                    case 0x45:
                        LogEvent?.Invoke("Jam in Acceptor【卡钞了，需要打开钱箱排除异常】");
                        NeedResetCashBox.Invoke(0x45, "卡钞了，需要打开钱箱排除异常");   //弹出需要人工干预的事件
                        break;
                    case 0x46:
                        LogEvent?.Invoke("Jam in Stacker【钱箱正忙】");
                        break;
                    case 0x47:
                        LogEvent?.Invoke("Paused【暂停接钞】");
                        break;
                    case 0x48:
                        LogEvent?.Invoke("Cheated【被骗，可能有假钞】");
                        NeedResetCashBox.Invoke(0x48, "可能有假钞");   //弹出需要人工干预的事件
                        break;
                    case 0x49:
                        LogEvent?.Invoke("Major Failure【发生故障，请排除故障】");
                        NeedResetCashBox.Invoke(0x49, "发生故障，请排除故障");   //弹出需要人工干预的事件
                        break;
                    case 0x4A:
                        LogEvent?.Invoke("Communication Error【发生通信故障，请排除故障】");
                        NeedResetCashBox.Invoke(0x4A, "发生通信故障，请排除故障，检查com口连接");//弹出需要人工干预的事件
                        break;
                    /*All the other status conditions will be handled here (jams, rejections, returned notes, etc).
                     * Other cases will also include error handling.
                     */
                    default:
                        LogEvent?.Invoke("unknow status:未知的状态，代号为:" + status[2].ToString("X"));
                        //NeedResetCashBox.Invoke(status[2], "未知的状态，代号为"+ status[2]);//弹出需要人工干预的事件
                        break;
                }
            }
        }

        #region jcm日志单独记录，用系统日志会导致崩溃
        /// <summary>
        /// 记录jcm日志
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static void writeLog(string message)
        {
            string dirName = AppDomain.CurrentDomain.BaseDirectory + "\\JCM";
            string fileName = dirName + "\\" + DateTimeCore.Now.ToString("yyyy-MM-dd") + ".log";
            FileStream file;

            message = DateTimeCore.Now + "\t" + message;
            try
            {
                if (!Directory.Exists(dirName)) //判断目录是否存在
                {
                    Directory.CreateDirectory(dirName);//生成目录
                }

                if (!File.Exists(fileName)) //判断文件是否存在
                {
                    file = File.Create(fileName);
                    file.Close();
                }

                file = File.Open(fileName, FileMode.Append);
                using (StreamWriter writer = new StreamWriter(file, Encoding.Default))
                {
                    writer.WriteLine(message);
                    writer.Close();
                };
                file.Close();
            }
            catch
            {
                //MessageBox.Show("日志 文件操作失败！");
            }
        }
        #endregion
        public class Actions
        {
            public static void init(byte[] buffer, int length, ID003CommandCreater ComDll, SerialPort Port)
            {
                //This method does the BV setup in order to accept bills 
                byte enable1 = 0;
                byte enable2 = 0;

                //Sending the reset command
                ComDll.Reset(buffer);
                length = (int)buffer[1];
                Port.Write(buffer, 0, length);
                System.Threading.Thread.Sleep(100); //wait for 100ms (poll rate should be between 100ms and 200ms)

                //Enabling denominations ($1, $5, $10, $20, $50, $100)
                ComDll.SetEnableDeno(buffer, enable1, enable2);
                length = (int)buffer[1];
                Port.Write(buffer, 0, length);
                System.Threading.Thread.Sleep(100);

                //Setting standard security enable1= 0x00, enable2= 0x00
                ComDll.SetSecurity(buffer, enable1, enable2);
                length = (int)buffer[1];
                Port.Write(buffer, 0, length);
                System.Threading.Thread.Sleep(100);

                //No optional functions enable1= 0x00, enable2= 0x00
                ComDll.SetOpFunction(buffer, enable1, enable2);
                length = (int)buffer[1];
                Port.Write(buffer, 0, length);
                System.Threading.Thread.Sleep(100);

                //Enabling bill acceptor enable=0x00 (0x01 to disable)
                ComDll.SetInhibit(buffer, enable1);
                length = (int)buffer[1];
                Port.Write(buffer, 0, length);
                System.Threading.Thread.Sleep(100);

            }

            public static void Accept(byte[] buffer, int length, ID003CommandCreater ComDll, SerialPort Port)
            {
                //This method will stack the note once the vend valid message is sent by the unit

                bool vend = false; //value to check if vend valid has been sent
                byte[] status = new byte[255];  //capturing the status message from the serial port

                ComDll.Stack1(buffer); //we have detected escrow and now sending the stack command.
                length = (int)buffer[1];
                Port.Write(buffer, 0, length); //writing buffer to com port
                System.Threading.Thread.Sleep(100);

                while (!vend) //if no vend valid, keep checking status
                {
                    ComDll.StatusRequest(buffer); //checking status
                    length = (int)buffer[1];
                    Port.Write(buffer, 0, length);
                    Port.Read(status, 0, 255); //capturing status from the com port
                    System.Threading.Thread.Sleep(100);

                    if (status[2] == 0x15) //we have received vend valid response
                    {
                        ComDll.Ack(buffer); //Sending an ACK
                        length = (int)buffer[1];
                        Port.Write(buffer, 0, length);
                        vend = true;
                    }
                }

            }
        }
    }
}