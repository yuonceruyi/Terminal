using System;
using System.Threading;
using System.Windows.Forms;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Devices.CashBox;
using Timer = System.Timers.Timer;

namespace YuanTu.ShenZhenArea.Nv200
{
    public class Nv200CashBox : ICashInputBox
    {
        public static volatile bool CashState; //钱箱处理通道是否有钱
        public static volatile bool CashRemoved; //钱箱是否被移走
        public static volatile bool CashFulled; //钱箱已满
        private static readonly Cashbox _cashbox = new Cashbox();
        /// <summary>
        /// 需要人工处理钱箱的事件
        /// </summary>
        public static bool NeedResetCashBox = false;

        private readonly IConfigurationManager _configurationManager;
        /// <summary>
        /// 重新连接次数
        /// </summary>
        private readonly int reconnectionAttempts = 5;
        /// <summary>
        /// 重新连接间隔时间（秒）
        /// </summary>
        private readonly int reconnectionInterval = 3;
        private readonly Timer reconnectionTimer = new Timer();
        private volatile bool Connected;
        private volatile bool ConnectionFail;
        private Thread ConnectionThread;

        public Nv200CashBox(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
            _cashbox.NeedResetCash += _cashbox_NeedResetCash;
        }

        private void _cashbox_NeedResetCash(byte commands)
        {
            Logger.Device.Error($"[钱箱{DeviceId}]发生异常{commands}，需要手动处理");
            NeedResetCashBox = true;
        }

        private int Port { get; set; }
        private bool Running { get; set; }
        public string DeviceName { get; } = "NV200";
        public string DeviceId => DeviceName;

        public Result<DeviceStatus> GetDeviceStatus()
        {
            return Result<DeviceStatus>.Success(CashState ? DeviceStatus.Busy : DeviceStatus.Idle);
        }

        public Result Connect()
        {
            if (Nv200CashBox.NeedResetCashBox)
            {
                Logger.Device.Error($"[钱箱{DeviceId}]发生异常(具体异常请看前文日志)，需要手动处理");
                return Result.Fail("钱箱发生异常，需要手动处理；请关掉程序并处理钱箱后重新开启程序");
            }

            _cashbox.NumberOfNotesStacked = 0; //记录进钱状态
            _cashbox.StopIdleLogger = false; //避免重复打“idle”日志状态

            try
            {
                Port = _configurationManager.GetValueInt("NV200:Port");
                _cashbox.CommandStructure.BaudRate = 9600;
                _cashbox.CommandStructure.ComPort = $"Com{Port}";
                _cashbox.CommandStructure.SSPAddress = 0;
                _cashbox.CommandStructure.Timeout = 3000;

                var result = ConnectToCashBox();
                if (!result.IsSuccess)
                {
                    Logger.Device.Error($"[钱箱{DeviceId}]:打开钱箱失败,重试");

                    DisConnect();

                    result = ConnectToCashBox();
                    if (!result.IsSuccess)
                    {
                        Logger.Device.Error($"[钱箱{DeviceId}]:再次打开钱箱失败,端口号:{Port}");
                        return Result.Fail($"钱箱打开失败");
                    }
                }
                Logger.Device.Error($"[钱箱{DeviceId}]:打开钱箱成功");
                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.Device.Error($"[钱箱{DeviceId}]:打开钱箱异常,{ex.Message} {ex.StackTrace}");
                return Result.Fail($"钱箱打开异常");
            }
        }

        public Result Initialize()
        {
            return Result.Success();
        }

        public Result UnInitialize()
        {
            Running = false;
            CashState = false;
            CashRemoved = false;
            CashFulled = false;
            _cashbox.DisableValidator();
            return Result.Success();
        }

        public Result DisConnect()
        {
            return !_cashbox.SSPComms.CloseComPort() ? Result.Fail(_cashbox.SSPComms?.GetLastException().Message): Result.Success();
        }

        public void StartPoll(Action<int> billcallBack, Action<byte, string> statuscallback = null)
        {
            Cashbox.updateCountDelegate = billcallBack;
            if (NeedResetCashBox)
                return;
            try
            {
                lock (_cashbox)
                {
                    Running = true;
                    while (Running)
                        if (!_cashbox.DoPoll())
                        {
                            Logger.Device.Info("Poll failed,atttempting to reconnect...\r\n");
                            Connected = false;
                            //重新连接的过程不使用线程
                            //ConnectionThread = new Thread(ConnectToValidatorThreaded);
                            //ConnectionThread.Start();

                            ConnectToValidator();

                            if (!Connected)   //不循环等待
                            {
                                Logger.Device.Info("Failed to reconnect to Cashbox\r\n");
                                return;
                            }
                            Logger.Device.Info("Reconnected successfully\r\n");
                        }
                }
            }
            catch (Exception ex)
            {
                Logger.Device.Error($"[钱箱{DeviceId}]DoPoll异常,{ex.Message}{ex.StackTrace}");
            }
        }

        private Result ConnectToCashBox()
        {
            // setup the timer
            reconnectionTimer.Interval = reconnectionInterval * 1000; // for ms

            // run for number of attempts specified
            for (var i = 0; i < reconnectionAttempts; i++)
            {
                // reset timer
                reconnectionTimer.Enabled = true;

                // close com port in case it was open
                _cashbox.SSPComms.CloseComPort();

                // turn encryption off for first stage
                _cashbox.CommandStructure.EncryptionStatus = false;

                // open com port and negotiate keys
                if (_cashbox.OpenComPort() && _cashbox.NegotiateKeys())
                {
                    _cashbox.CommandStructure.EncryptionStatus = true; // now encrypting

                    var maxPVersion = FindMaxProtocolVersion();
                    Logger.Device.Debug(string.Format("maxPVersion={0}", maxPVersion));
                    if (maxPVersion > 6)
                        _cashbox.SetProtocolVersion(maxPVersion);
                    else
                        return Result.Fail("This program does not support units under protocol version 6, update firmware.");
                    _cashbox.SetupRequest();

                    if (!IsUnitTypeSupported(_cashbox.UnitType))
                        return Result.Fail( "Unsupported unit type, this SDK supports the BV series and the NV series (excluding the NV11)");
                    // inhibits, this sets which channels can receive notes
                    _cashbox.SetInhibits();
                    // enable, this allows the Cashbox to receive and act on commands
                    _cashbox.EnableValidator();
                    return Result.Success();
                }
                //while (reconnectionTimer.Enabled)
                //{
                //    Thread.Sleep(50);
                //    Application.DoEvents(); // wait for reconnectionTimer to tick
                //}
            }
            return Result.Fail("ConnectToCashBox fail");
        }

        private byte FindMaxProtocolVersion()
        {
            // not dealing with protocol under level 6
            // attempt to set in validator
            var i = 0;
            byte b = 0x06;

            while (true)
            {
                _cashbox.SetProtocolVersion(b);
                if (_cashbox.CommandStructure.ResponseData[0] == Commands.SSP_RESPONSE_CMD_FAIL)
                    return --b;
                b++;
                if (b > 20)
                    return 0x06; // return default if protocol 'runs away'
                Logger.Device.Debug(string.Format("第{0}次，b={1}", i++, b));
            }
        }

        private bool IsUnitTypeSupported(char type)
        {
            if (type == (char)0x00)
                return true;
            return false;
        }


        [Obsolete]
        private void ConnectToValidatorThreaded()
        {
            // setup the timer
            reconnectionTimer.Interval = reconnectionInterval * 1000; // for ms

            // run for number of attempts specified
            for (var i = 0; i < reconnectionAttempts; i++)
            {
                // reset timer
                reconnectionTimer.Enabled = true;

                // close com port in case it was open
                _cashbox.SSPComms.CloseComPort();

                // turn encryption off for first stage
                _cashbox.CommandStructure.EncryptionStatus = false;

                // open com port and negotiate keys
                if (_cashbox.OpenComPort() && _cashbox.NegotiateKeys())
                {
                    _cashbox.CommandStructure.EncryptionStatus = true; // now encrypting
                    // find the max protocol version this validator supports
                    var maxPVersion = FindMaxProtocolVersion();
                    if (maxPVersion > 6)
                    {
                        _cashbox.SetProtocolVersion(maxPVersion);
                    }
                    else
                    {
                        //MessageBox.Show( "This program does not support units under protocol version 6, update firmware.", "ERROR");
                        Connected = false;
                        return;
                    }
                    // get info from the validator and store useful vars
                    _cashbox.SetupRequest();
                    // inhibits, this sets which channels can receive notes
                    _cashbox.SetInhibits();
                    // enable, this allows the validator to operate
                    _cashbox.EnableValidator();

                    Connected = true;
                    return;
                }
                while (reconnectionTimer.Enabled)// wait for reconnectionTimer to tick
                {
                    Application.DoEvents();
                }
            }
            Connected = false;
            ConnectionFail = true;
        }




        private void ConnectToValidator()
        {
            // setup the timer
            reconnectionTimer.Interval = reconnectionInterval * 1000; // for ms

            // run for number of attempts specified
            for (var i = 0; i < reconnectionAttempts; i++)
            {
                // reset timer
                reconnectionTimer.Enabled = true;

                // close com port in case it was open
                _cashbox.SSPComms.CloseComPort();

                // turn encryption off for first stage
                _cashbox.CommandStructure.EncryptionStatus = false;

                // open com port and negotiate keys
                if (_cashbox.OpenComPort() && _cashbox.NegotiateKeys())
                {
                    _cashbox.CommandStructure.EncryptionStatus = true; // now encrypting
                    // find the max protocol version this validator supports
                    var maxPVersion = FindMaxProtocolVersion();
                    if (maxPVersion > 6)
                    {
                        _cashbox.SetProtocolVersion(maxPVersion);
                    }
                    else
                    {
                        //MessageBox.Show( "This program does not support units under protocol version 6, update firmware.", "ERROR");
                        Connected = false;
                        return;
                    }
                    // get info from the validator and store useful vars
                    _cashbox.SetupRequest();
                    // inhibits, this sets which channels can receive notes
                    _cashbox.SetInhibits();
                    // enable, this allows the validator to operate
                    _cashbox.EnableValidator();

                    Connected = true;
                    return;
                }
            }
            Connected = false;
            ConnectionFail = true;
        }
    }
}