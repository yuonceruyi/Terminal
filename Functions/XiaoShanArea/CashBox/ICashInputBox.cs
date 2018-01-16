using System;
using System.Globalization;
using System.Text;
using MPOST;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Devices.CashBox;

namespace YuanTu.YuHangArea.CashBox
{
    public class MeiBox : ICashInputBox
    {
        private bool _isConnectd;
        private int _port;
        private readonly IConfigurationManager _configurationManager;
        private DeviceStatus _currentStatus = DeviceStatus.UnInitialized;
        private readonly Acceptor _meiBox = new Acceptor();
        private bool _enableAcceptance;
        private bool _autoStack;
        private Action<int> _stackAction;

        public MeiBox(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        #region Implementation of IDevice

        public string DeviceName { get; } = "MEI";
        public string DeviceId => DeviceName;

        public Result<DeviceStatus> GetDeviceStatus()
        {
            if (!_isConnectd)
            {
                return Result<DeviceStatus>.Success(DeviceStatus.Disconnect);
            }
            return Result<DeviceStatus>.Success(_meiBox.DeviceBusy ?
                DeviceStatus.Busy :
                DeviceStatus.Idle);
        }

        public Result Connect()
        {
            if (_isConnectd)
            {
                return Result.Success();
            }
            _port = _configurationManager.GetValueInt("MEI:Port");
            try
            {
                _meiBox.Open($"COM{_port}", PowerUp.A);
                if (_meiBox.DeviceState == State.Disconnected)
                {
                    Logger.Device.Error($"[钱箱{DeviceId}]连接异常， 端口:{_port}，钱箱状态：{_meiBox.DeviceState}");
                    return Result.Fail("钱箱打开失败");
                }
            }
            catch (Exception ex)
            {
                Logger.Device.Error($"[钱箱{DeviceId}]连接异常， 端口:{_port}，{ex.Message} {ex.StackTrace}");
                return Result.Fail("钱箱打开失败");
            }

            _isConnectd = true;
            _currentStatus = DeviceStatus.Connected;
            return Result.Success();
        }

        public Result Initialize()
        {
            if (!_isConnectd)
            {
                return Result.Fail("钱箱未连接");
            }
            // Connect to the events.
            _meiBox.OnCashBoxCleanlinessDetected += HandleCashBoxCleanlinessEvent;
            _meiBox.OnCashBoxAttached += HandleCashBoxAttachedEvent;
            _meiBox.OnCashBoxRemoved += HandleCashBoxRemovedEvent;
            _meiBox.OnCheated += HandleCheatedEvent;
            _meiBox.OnClearAuditComplete += HandleClearAuditEvent;
            _meiBox.OnConnected += HandleConnectedEvent;
            _meiBox.OnDisconnected += HandleDisconnectedEvent;
            _meiBox.OnSendMessageFailure += HandleSendMessageErrorEvent;
            _meiBox.OnEscrow += HandleEscrowedEvent;
            _meiBox.OnFailureCleared += HandleFailureClearedEvent;
            _meiBox.OnFailureDetected += HandleFailureDetectedEvent;
            _meiBox.OnInvalidCommand += HandleInvalidCommandEvent;
            _meiBox.OnJamCleared += HandleJamClearedEvent;
            _meiBox.OnJamDetected += HandleJamDetectedEvent;
            _meiBox.OnNoteRetrieved += HandleNoteRetrievedEvent;
            _meiBox.OnPauseCleared += HandlePauseClearedEvent;
            _meiBox.OnPauseDetected += HandlePauseDetectedEvent;
            _meiBox.OnPowerUpComplete += HandlePowerUpCompleteEvent;
            _meiBox.OnPowerUp += HandlePowerUpEvent;
            _meiBox.OnPUPEscrow += HandlePUPEscrowEvent;
            _meiBox.OnRejected += HandleRejectedEvent;
            _meiBox.OnReturned += HandleReturnedEvent;
            _meiBox.OnStacked += HandleStackedEvent;
            _meiBox.OnStackerFullCleared += HandleStackerFullClearedEvent;
            _meiBox.OnStackerFull += HandleStackerFullEvent;
            _meiBox.OnStallCleared += HandleStallClearedEvent;
            _meiBox.OnStallDetected += HandleStallDetectedEvent;
            _enableAcceptance = true;
            _autoStack = true;
            return Result.Success();
        }

        public Result UnInitialize()
        {
            if (!_isConnectd)
            {
                return Result.Fail("钱箱未连接");
            }
            // Connect to the events.
            _meiBox.OnCashBoxCleanlinessDetected -= HandleCashBoxCleanlinessEvent;
            _meiBox.OnCashBoxAttached -= HandleCashBoxAttachedEvent;
            _meiBox.OnCashBoxRemoved -= HandleCashBoxRemovedEvent;
            _meiBox.OnCheated -= HandleCheatedEvent;
            _meiBox.OnClearAuditComplete -= HandleClearAuditEvent;
            _meiBox.OnConnected -= HandleConnectedEvent;
            _meiBox.OnDisconnected -= HandleDisconnectedEvent;
            _meiBox.OnSendMessageFailure -= HandleSendMessageErrorEvent;
            _meiBox.OnEscrow -= HandleEscrowedEvent;
            _meiBox.OnFailureCleared -= HandleFailureClearedEvent;
            _meiBox.OnFailureDetected -= HandleFailureDetectedEvent;
            _meiBox.OnInvalidCommand -= HandleInvalidCommandEvent;
            _meiBox.OnJamCleared -= HandleJamClearedEvent;
            _meiBox.OnJamDetected -= HandleJamDetectedEvent;
            _meiBox.OnNoteRetrieved -= HandleNoteRetrievedEvent;
            _meiBox.OnPauseCleared -= HandlePauseClearedEvent;
            _meiBox.OnPauseDetected -= HandlePauseDetectedEvent;
            _meiBox.OnPowerUpComplete -= HandlePowerUpCompleteEvent;
            _meiBox.OnPowerUp -= HandlePowerUpEvent;
            _meiBox.OnPUPEscrow -= HandlePUPEscrowEvent;
            _meiBox.OnRejected -= HandleRejectedEvent;
            _meiBox.OnReturned -= HandleReturnedEvent;
            _meiBox.OnStacked -= HandleStackedEvent;
            _meiBox.OnStackerFullCleared -= HandleStackerFullClearedEvent;
            _meiBox.OnStackerFull -= HandleStackerFullEvent;
            _meiBox.OnStallCleared -= HandleStallClearedEvent;
            _meiBox.OnStallDetected -= HandleStallDetectedEvent;
            _enableAcceptance = false;
            _autoStack = false;
            return Result.Success();
        }

        public Result DisConnect()
        {
            if (!_isConnectd)
            {
                return Result.Fail("钱箱未连接");
            }
            try
            {
                _meiBox.Close();
                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.Device.Error($"[钱箱{DeviceId}]关闭异常， 端口:{_port}，{ex.Message} {ex.StackTrace}");
                return Result.Fail("钱箱关闭失败");
            }
        }

        public void StartPoll(Action<int> billcallBack, Action<byte, string> statuscallback = null)
        {
            _stackAction = billcallBack;
        }

        #endregion Implementation of IDevice

        #region Event Handlers

        private void HandleCashBoxCleanlinessEvent(object sender, CashBoxCleanlinessEventArgs e)
        {
            Logger.Device.Info($"Event: Cashbox Cleanliness - {e.Value}");
        }

        private void HandleCashBoxAttachedEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Cassette Installed.");
        }

        private void HandleCashBoxRemovedEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Cassette Removed.");
        }

        private void HandleCheatedEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Cheat Detected.");
        }

        private void HandleClearAuditEvent(object sender, ClearAuditEventArgs e)
        {
            Logger.Device.Info(e.Success ? "Event: Clear Audit Complete: Success" : "Event: Clear Audit Complete: FAILED");
        }

        private void HandleConnectedEvent(object sender, EventArgs e)
        {
            _meiBox.EnableAcceptance = _enableAcceptance;
            _meiBox.AutoStack = _autoStack;

            Logger.Device.Info("Event: Connected.");
            if (!_meiBox.Connected)
                return;
            var b = new[] { false, false, false, false, false, true, true };
            _meiBox.SetBillValueEnables(ref b);
        }

        private void HandleDisconnectedEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Disconnected.");
        }

        private void HandleSendMessageErrorEvent(object sender, AcceptorMessageEventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append("Event: Error in send message. ");
            sb.Append(e.Msg.Description);
            sb.Append("  ");

            foreach (var b in e.Msg.Payload)
            {
                sb.Append(b.ToString("X2") + " ");
            }

            Logger.Device.Info(sb.ToString());
        }

        private void HandleEscrowedEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Escrowed: " + DocInfoToString());
        }

        private void HandleFailureClearedEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Device Failure Cleared. ");
        }

        private void HandleFailureDetectedEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Device Failure Detected. ");
        }

        private void HandleInvalidCommandEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Invalid Command.");
        }

        private void HandleJamClearedEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Jam Cleared.");
        }

        private void HandleJamDetectedEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Jam Detected.");
        }

        private void HandleNoteRetrievedEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Note Retreived.");
        }

        private void HandlePauseClearedEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Pause Cleared.");
        }

        private void HandlePauseDetectedEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Pause Detected.");
        }

        private void HandlePowerUpCompleteEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Power Up Complete.");
        }

        private void HandlePowerUpEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Power Up.");
        }

        private void HandlePUPEscrowEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Power Up with Escrow: " + DocInfoToString());
        }

        private void HandleRejectedEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Rejected.");
        }

        private void HandleReturnedEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Returned.");
        }

        private void HandleStackedEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Stacked: " + DocInfoToString());

            double value = 0;

            if (_meiBox.DocType == DocumentType.Bill && _meiBox.Bill != null)
                value = _meiBox.Bill.Value;
            _stackAction?.Invoke(int.Parse(value.ToString(CultureInfo.InvariantCulture)));
        }

        private void HandleStackerFullClearedEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Cassette Full Cleared.");
        }

        private void HandleStackerFullEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Cassette Full.");
        }

        private void HandleStallClearedEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Stall Cleared.");
        }

        private void HandleStallDetectedEvent(object sender, EventArgs e)
        {
            Logger.Device.Info("Event: Stall Detected.");
        }

        private string DocInfoToString()
        {
            switch (_meiBox.DocType)
            {
                case DocumentType.None:
                    return "Doc Type: None";

                case DocumentType.NoValue:
                    return "Doc Type: No Value";

                case DocumentType.Bill:
                    if (_meiBox.Bill == null)
                        return "Doc Type Bill = null";
                    if (!_meiBox.CapOrientationExt)
                        return "Doc Type Bill = " + _meiBox.Bill;
                    return "Doc Type Bill = " + _meiBox.Bill + " (" + _meiBox.EscrowOrientation + ")";

                case DocumentType.Barcode:
                    if (_meiBox.BarCode == null)
                        return "Doc Type Bar Code = null";
                    return "Doc Type Bar Code = " + _meiBox.BarCode;

                case DocumentType.Coupon:
                    if (_meiBox.Coupon == null)
                        return "Doc Type Coupon = null";
                    return "Doc Type Coupon = " + _meiBox.Coupon;

                default:
                    return "Unknown Doc Type Error";
            }
        }

        #endregion Event Handlers
    }
}