using System;
using System.Collections.Generic;
using YuanTu.Core.Log;
using YuanTu.ShenZhenArea.Nv200.ITLlib;

namespace YuanTu.ShenZhenArea.Nv200
{
    public delegate void CashEventHandler(byte commands);

    public class Cashbox
	{
        /// <summary>
        /// 需要人工处理钱箱的事件
        /// </summary>
        public event CashEventHandler NeedResetCash;
		public static Action<int> updateCountDelegate;
        private readonly ChannelData d = new ChannelData();
		private readonly SSP_KEYS keys;
		private readonly List<ChannelData> m_UnitDataList;
		private SSP_FULL_KEY sspKey;
		private SSP_COMMAND storedCmd;
	    public  bool StopIdleLogger { get; set; } = false;
        public Cashbox()
		{
			SSPComms = new SSPComms();
			CommandStructure = new SSP_COMMAND();
			storedCmd = new SSP_COMMAND();
			keys = new SSP_KEYS();
			sspKey = new SSP_FULL_KEY();
			InfoStructure = new SSP_COMMAND_INFO();

			NumberOfChannels = 0;
			Multiplier = 1;
			UnitType = (char) 0xFF;
			m_UnitDataList = new List<ChannelData>();
		}

		public SSPComms SSPComms { get; set; }
		// a pointer to the command structure, this struct is filled with info and then compiled into
		// a packet by the library and sent to the cashbox
		public SSP_COMMAND CommandStructure { get; set; }
		// pointer to an information structure which accompanies the command structure
		public SSP_COMMAND_INFO InfoStructure { get; set; }
		// access to the type of unit, this will only be valid after the setup request
		public char UnitType { get; private set; }
		// access to number of channels being used by the cashbox
		public int NumberOfChannels { get; set; }
		// access to number of notes stacked
		public int NumberOfNotesStacked { get; set; }
		// access to value multiplier
		public int Multiplier { get; set; }
		// get a channel value
		public int GetChannelValue(int channelNum)
		{
			if (channelNum >= 1 && channelNum <= NumberOfChannels)
			{
				foreach (var d in m_UnitDataList)
				{
                    if (d.Channel == channelNum)
                        return d.Value / 100;
				}
			}
			return -1;
		}

		// get a channel currency
		public string GetChannelCurrency(int channelNum)
		{
			if (channelNum >= 1 && channelNum <= NumberOfChannels)
			{
				foreach (var d in m_UnitDataList)
				{
					if (d.Channel == channelNum)
						return new string(d.Currency);
				}
			}
			return "";
		}

		//1打开串口
		/* Non-Command functions */

		// This function calls the open com port function of the SSP library.
		public bool OpenComPort()
		{
			Logger.Device.Info("Opening com port\r\n");
			if (!SSPComms.OpenSSPComPort(CommandStructure))
			{
				return false;
			}
			return true;
		}

		//设置允许识别哪几种纸币
		public void SetInhibits()
		{
			// set inhibits
			CommandStructure.CommandData[0] = Commands.SSP_CMD_SET_INHIBITS;
			CommandStructure.CommandData[1] = 0xFF;
			CommandStructure.CommandData[2] = 0xFF;
			CommandStructure.CommandDataLength = 3;

			if (!SendCommand()) return;
			if (CheckGenericResponses())
			{
				Logger.Device.Info("Inhibits set\r\n");
			}
		}

		// 为命令进行加密
		public bool NegotiateKeys()
		{
			byte i;

			// make sure encryption is off
			CommandStructure.EncryptionStatus = false;

			// send sync
			Logger.Device.Info("Syncing... ");
			CommandStructure.CommandData[0] = Commands.SSP_CMD_SYNC; //发送同步命令
			CommandStructure.CommandDataLength = 1;

			if (!SendCommand()) return false;
			Logger.Device.Info("Success");

			SSPComms.InitiateSSPHostKeys(keys, CommandStructure);

			// send generator
			CommandStructure.CommandData[0] = Commands.SSP_CMD_SET_GENERATOR;
			CommandStructure.CommandDataLength = 9;
			Logger.Device.Info("Setting generator... ");
			for (i = 0; i < 8; i++)
			{
				CommandStructure.CommandData[i + 1] = (byte) (keys.Generator >> (8*i));
			}

			if (!SendCommand()) return false;
			Logger.Device.Info("Success\r\n");

			// send modulus
			CommandStructure.CommandData[0] = Commands.SSP_CMD_SET_MODULUS;
			CommandStructure.CommandDataLength = 9;
			Logger.Device.Info("Sending modulus... ");
			for (i = 0; i < 8; i++)
			{
				CommandStructure.CommandData[i + 1] = (byte) (keys.Modulus >> (8*i));
			}

			if (!SendCommand()) return false;
			Logger.Device.Info("Success\r\n");

			// send key exchange
			CommandStructure.CommandData[0] = Commands.SSP_CMD_KEY_EXCHANGE;
			CommandStructure.CommandDataLength = 9;
			Logger.Device.Info("Exchanging keys... ");
			for (i = 0; i < 8; i++)
			{
				CommandStructure.CommandData[i + 1] = (byte) (keys.HostInter >> (8*i));
			}

			if (!SendCommand()) return false;
			Logger.Device.Info("Success\r\n");

			keys.SlaveInterKey = 0;
			for (i = 0; i < 8; i++)
			{
				keys.SlaveInterKey += (UInt64) CommandStructure.ResponseData[1 + i] << (8*i);
			}

			SSPComms.CreateSSPHostEncryptionKey(keys);

			// get full encryption key
			CommandStructure.Key.FixedKey = 0x0123456701234567;
			CommandStructure.Key.VariableKey = keys.KeyHost;

			Logger.Device.Info("Keys successfully negotiated\r\n");

			return true;
		}

		public void SetProtocolVersion(byte pVersion)
		{
			CommandStructure.CommandData[0] = Commands.SSP_CMD_HOST_PROTOCOL_VERSION;
			CommandStructure.CommandData[1] = pVersion;
			CommandStructure.CommandDataLength = 2;
			if (!SendCommand()) return;
		}

		public void SetupRequest()
		{
			// send setup request
			CommandStructure.CommandData[0] = Commands.SSP_CMD_SETUP_REQUEST;
			CommandStructure.CommandDataLength = 1;

			if (!SendCommand()) return;

			// display setup request
			var displayString = "Unit Type: ";
			var index = 1;

			// unit type (table 0-1)
			UnitType = (char) CommandStructure.ResponseData[index++];
			switch (UnitType)
			{
				case (char) 0x00:
					displayString += "Validator";
					break;
				case (char) 0x03:
					displayString += "SMART Hopper";
					break;
				case (char) 0x06:
					displayString += "SMART Payout";
					break;
				case (char) 0x07:
					displayString += "NV11";
					break;
				default:
					displayString += "Unknown Type";
					break;
			}

			displayString += "\r\nFirmware: ";

			// firmware (table 2-5)
			while (index <= 5)
			{
				displayString += (char) CommandStructure.ResponseData[index++];
				if (index == 4)
					displayString += ".";
			}

			// country code (table 6-8)
			// this is legacy code, in protocol version 6+ each channel has a seperate currency

			index = 9; // to skip country code

			// value multiplier (table 9-11)
			// also legacy code, a real value multiplier appears later in the response
			index = 12; // to skip value multiplier

			displayString += "\r\nNumber of Channels: ";
			int numChannels = CommandStructure.ResponseData[index++];
			NumberOfChannels = numChannels;

			displayString += numChannels + "\r\n";
			// channel values (table 13 to 13+n)
			// the channel values located here in the table are legacy, protocol 6+ provides a set of expanded
			// channel values.
			index = 13 + NumberOfChannels; // Skip channel values

			// channel security (table 13+n to 13+(n*2))
			// channel security values are also legacy code
			index = 13 + (NumberOfChannels*2); // Skip channel security

			displayString += "Real Value Multiplier: ";

			// real value multiplier (table 13+(n*2) to 15+(n*2))
			// (big endian)
			Multiplier = CommandStructure.ResponseData[index + 2];
			Multiplier += CommandStructure.ResponseData[index + 1] << 8;
			Multiplier += CommandStructure.ResponseData[index] << 16;
			displayString += Multiplier + "\r\nProtocol Version: ";
			index += 3;

			// protocol version (table 16+(n*2))
			index = 16 + (NumberOfChannels*2);
			int protocol = CommandStructure.ResponseData[index++];

			displayString += protocol + "\r\n";
			Console.WriteLine(protocol);

			// protocol 6+ only

			// channel currency country code (table 17+(n*2) to 17+(n*5))
			index = 17 + (NumberOfChannels*2);
			var sectionEnd = 17 + (NumberOfChannels*5);
			var count = 0;

			var channelCurrencyTemp = new byte[3*NumberOfChannels];
			for (var i = 0; i < 3*NumberOfChannels; i++)
			{
				Console.WriteLine(channelCurrencyTemp[i]);
			}
			while (index < sectionEnd)
			{
				displayString += "Channel " + ((count/3) + 1) + ", currency: ";
				channelCurrencyTemp[count] = CommandStructure.ResponseData[index++];
				displayString += (char) channelCurrencyTemp[count++];
				channelCurrencyTemp[count] = CommandStructure.ResponseData[index++];
				displayString += (char) channelCurrencyTemp[count++];
				channelCurrencyTemp[count] = CommandStructure.ResponseData[index++];
				displayString += (char) channelCurrencyTemp[count++];
				displayString += "\r\n";
			}

			// expanded channel values (table 17+(n*5) to 17+(n*9))
			index = sectionEnd;
			displayString += "Expanded channel values:\r\n";
			sectionEnd = 17 + (NumberOfChannels*9);
			var n = 0;
			count = 0;
			var channelValuesTemp = new int[NumberOfChannels];
			while (index < sectionEnd)
			{
				n = ChannelData.Helpers.ConvertBytesToInt32(CommandStructure.ResponseData, index);
				channelValuesTemp[count] = n;
				index += 4;
				displayString += "Channel " + ++count + ", value = " + n + "\r\n";
			}

			// Create list entry for each channel
			m_UnitDataList.Clear(); // clear old table
			for (byte i = 0; i < NumberOfChannels; i++)
			{
				var d = new ChannelData();
				d.Channel = i;
				d.Channel++; // Offset from array index by 1
				d.Value = channelValuesTemp[i]*Multiplier;
				d.Currency[0] = (char) channelCurrencyTemp[0 + (i*3)];
				d.Currency[1] = (char) channelCurrencyTemp[1 + (i*3)];
				d.Currency[2] = (char) channelCurrencyTemp[2 + (i*3)];
				d.Level = 0; // Can't store notes
				d.Recycling = false; // Can't recycle notes

				m_UnitDataList.Add(d);
			}

			// Sort the list of data by the value.
			m_UnitDataList.Sort(delegate(ChannelData d1, ChannelData d2) { return d1.Value.CompareTo(d2.Value); });

			Logger.Device.Info(displayString);
		}

		/* Command functions */

		// The enable command allows the cashbox to receive and act on commands sent to it.
		public void EnableValidator()
		{
			CommandStructure.CommandData[0] = Commands.SSP_CMD_ENABLE;
			CommandStructure.CommandDataLength = 1;

			if (!SendCommand()) return;
			// check response
			if (CheckGenericResponses())
				Logger.Device.Info("Unit enabled\r\n");
		}

		private bool SendCommand()
		{
			// Backup data and length in case we need to retry
			var backup = new byte[255];
			CommandStructure.CommandData.CopyTo(backup, 0);
			var length = CommandStructure.CommandDataLength;

			// attempt to send the command
			if (SSPComms.SSPSendCommand(CommandStructure, InfoStructure) == false)
			{
				SSPComms.CloseComPort();
				//m_log.UpdateLog(info, true); // update the log on fail as well
				Logger.Device.Info("Sending command failed\r\nPort status: " + CommandStructure.ResponseStatus + "\r\n");
				return false;
			}
			return true;
		}

		public bool CheckGenericResponses()
		{
			if (CommandStructure.ResponseData[0] == Commands.SSP_RESPONSE_CMD_OK)
				return true;
			switch (CommandStructure.ResponseData[0])
			{
				case Commands.SSP_RESPONSE_CMD_CANNOT_PROCESS:
					if (CommandStructure.ResponseData[1] == 0x03)
					{
						Logger.Device.Info(
							"Validator has responded with \"Busy\", command cannot be processed at this time\r\n");
					}
					else
					{
						Logger.Device.Info("Command response is CANNOT PROCESS COMMAND, error code - 0x"
										+ BitConverter.ToString(CommandStructure.ResponseData, 1, 1) + "\r\n");
					}
					return false;

				case Commands.SSP_RESPONSE_CMD_FAIL:
					Logger.Device.Info("Command response is FAIL\r\n");
					return false;

				case Commands.SSP_RESPONSE_CMD_KEY_NOT_SET:
					Logger.Device.Info("Command response is KEY NOT SET, Validator requires encryption on this command or there is"
									+ "a problem with the encryption on this request\r\n");
					return false;

				case Commands.SSP_RESPONSE_CMD_PARAM_OUT_OF_RANGE:
					Logger.Device.Info("Command response is PARAM OUT OF RANGE\r\n");
					return false;

				case Commands.SSP_RESPONSE_CMD_SOFTWARE_ERROR:
					Logger.Device.Info("Command response is SOFTWARE ERROR\r\n");
					return false;

				case Commands.SSP_RESPONSE_CMD_UNKNOWN:
					Logger.Device.Info("Command response is UNKNOWN\r\n");
					return false;

				case Commands.SSP_RESPONSE_CMD_WRONG_PARAMS:
					Logger.Device.Info("Command response is WRONG PARAMETERS\r\n");
					return false;

				default:
					return false;
			}
		}

		public static void setUpdateHandler(Action<int> updateDelegate)
		{
			updateCountDelegate = updateDelegate;
		}

		public void DisableValidator()
		{
			CommandStructure.CommandData[0] = Commands.SSP_CMD_DISABLE;
			CommandStructure.CommandDataLength = 1;

			if (!SendCommand()) return;
			// check response
			if (CheckGenericResponses())
				Logger.Device.Info("Unit disabled\r\n");
		}

		// The reset command instructs the Cashbox to restart (same effect as switching on and off)
		public void Reset()
		{
			CommandStructure.CommandData[0] = Commands.SSP_CMD_RESET;
			CommandStructure.CommandDataLength = 1;
			if (!SendCommand()) return;

			if (CheckGenericResponses())
				Logger.Device.Info("Resetting unit\r\n");
		}

		// This command just sends a sync command to the validator
		private bool SendSync()
		{
			CommandStructure.CommandData[0] = Commands.SSP_CMD_SYNC;
			CommandStructure.CommandDataLength = 1;
			if (!SendCommand()) return false;

			if (CheckGenericResponses())
				Logger.Device.Info("Successfully sent sync\r\n");
			return true;
		}

		public bool DoPoll()
		{
			byte i;

			//send poll
			CommandStructure.CommandData[0] = Commands.SSP_CMD_POLL;
			CommandStructure.CommandDataLength = 1;

			if (!SendCommand()) return false;

			//parse poll response
			var noteVal = 0;
			if (CommandStructure.ResponseDataLength == 1)
			{
                Nv200CashBox.CashState = false; //没有钱进入钱箱
                Nv200CashBox.CashRemoved = false;
                Nv200CashBox.CashFulled = false;
                if(!StopIdleLogger)
                    Logger.Device.Info("Idle...\r\n");
                StopIdleLogger = true;
			}

			for (i = 1; i < CommandStructure.ResponseDataLength; i++)
			{
                StopIdleLogger = false;
                Nv200CashBox.CashState = true; //有钱进入钱箱
                Nv200CashBox.CashRemoved = false;
                Nv200CashBox.CashFulled = false;
				switch (CommandStructure.ResponseData[i])
				{
					// This response indicates that the unit was reset and this is the first time a poll
					// has been called since the reset.
					case Commands.SSP_POLL_RESET:
                        Nv200CashBox.CashRemoved = false;
                        Nv200CashBox.CashFulled = false;
						Logger.Device.Info("Unit reset\r\n");
						break;
					// A note is currently being read by the cashbox sensors. The second byte of this response
					// is zero until the note's type has been determined, it then changes to the channel of the
					// scanned note.
					case Commands.SSP_POLL_NOTE_READ:
                        Nv200CashBox.CashRemoved = false;
                        Nv200CashBox.CashFulled = false;
                        if (CommandStructure.ResponseData[i + 1] > 0)
                        {
                            noteVal = GetChannelValue(CommandStructure.ResponseData[i + 1]);
                            Logger.Device.Info("Note in escrow, amount: " + noteVal + "\r\n");
                            d.Channel = CommandStructure.ResponseData[i + 1];
                        }
                        else
                            Logger.Device.Info("Reading note...\r\n");
						i++;
						break;
					// A credit event has been detected, this is when the Cashbox has accepted a note as legal currency.
					case Commands.SSP_POLL_CREDIT:
                        Nv200CashBox.CashRemoved = false;
                        Nv200CashBox.CashFulled = false;
						Logger.Device.Info("Credit .....\r\n");
						NumberOfNotesStacked++;
				        Logger.Device.Info($"[NumberOfNotesStacked]{NumberOfNotesStacked}");
						i++;
						break;
					// A note is being rejected from the cashbox, This will carry on polling while the note is in transit.
					case Commands.SSP_POLL_REJECTING:
                        Nv200CashBox.CashRemoved = false;
                        Nv200CashBox.CashFulled = false;
						Logger.Device.Info("Rejecting note...\r\n");
						break;
					// A note has been rejected from the cashbox, the note will be resting in the bezel. This response only
					// appears once.
					case Commands.SSP_POLL_REJECTED:
                        Nv200CashBox.CashRemoved = false;
                        Nv200CashBox.CashFulled = false;
						Logger.Device.Info("Note rejected\r\n");
						QueryRejection();
						break;
					// A note is in transit to the cashbox.
					case Commands.SSP_POLL_STACKING:
                        Nv200CashBox.CashRemoved = false;
                        Nv200CashBox.CashFulled = false;
						Logger.Device.Info("Stacking note...\r\n");
						break;
					// A note has reached the cashbox.
					case Commands.SSP_POLL_STACKED:
                        Nv200CashBox.CashRemoved = false;
                        Nv200CashBox.CashFulled = false;
						Logger.Device.Info($"Note stacked [NumberOfNotesStacked]{NumberOfNotesStacked} \r\n");
                    
				        if (NumberOfNotesStacked > 0)
				        {
                            switch (d.Channel)
                            {
                                case 1:
                                    updateCountDelegate(GetChannelValue(1));
                                    break;

                                case 2:
                                    updateCountDelegate(GetChannelValue(2));
                                    break;

                                case 3:
                                    updateCountDelegate(GetChannelValue(3));
                                    break;

                                case 4:
                                    updateCountDelegate(GetChannelValue(4));
                                    break;

                                case 5:
                                    updateCountDelegate(GetChannelValue(5));
                                    break;

                                case 6:
                                    updateCountDelegate(GetChannelValue(6));
                                    break;

                                case 7:
                                    updateCountDelegate(GetChannelValue(7));
                                    break;
                            }
                        }
				       
						break;
					// A safe jam has been detected. This is where the user has inserted a note and the note
					// is jammed somewhere that the user cannot reach.
					case Commands.SSP_POLL_SAFE_JAM:
                        Nv200CashBox.CashState = false; //没有钱进入钱箱
                        Nv200CashBox.CashRemoved = false;
                        Nv200CashBox.CashFulled = false;
						Logger.Device.Info("Safe jam\r\n");
                        NeedResetCash.Invoke(Commands.SSP_POLL_SAFE_JAM);   //弹出需要人工干预的事件
                        break;
					// An unsafe jam has been detected. This is where a user has inserted a note and the note
					// is jammed somewhere that the user can potentially recover the note from.
					case Commands.SSP_POLL_UNSAFE_JAM:
                        Nv200CashBox.CashState = false; //没有钱进入钱箱
                        Nv200CashBox.CashRemoved= false;
                        Nv200CashBox.CashFulled = false;
						Logger.Device.Info("Unsafe jam\r\n");
                        NeedResetCash.Invoke(Commands.SSP_POLL_SAFE_JAM);   //弹出需要人工干预的事件
                        break;
                    // The cashbox is disabled, it will not execute any commands or do any actions until enabled.
                    case Commands.SSP_POLL_DISABLED:
                        Nv200CashBox.CashState = false; //没有钱进入钱箱
                        Nv200CashBox.CashRemoved = false;
                        Nv200CashBox.CashFulled = false;
						break;
					// A fraud attempt has been detected. The second byte indicates the channel of the note that a fraud
					// has been attempted on.
					case Commands.SSP_POLL_FRAUD_ATTEMPT:
                        Nv200CashBox.CashState = false; //没有钱进入钱箱
                        Nv200CashBox.CashRemoved = false;
                        Nv200CashBox.CashFulled = false;
						Logger.Device.Info("Fraud attempt, note type: " + GetChannelValue(CommandStructure.ResponseData[i + 1]) + "\r\n");
						i++;
						break;
					// The stacker (cashbox) is full.
					case Commands.SSP_POLL_STACKER_FULL:
                        Nv200CashBox.CashState = false; //没有钱进入钱箱
                        Nv200CashBox.CashRemoved = false;
                        Nv200CashBox.CashFulled = true;
						Logger.Device.Info("Stacker full\r\n");
						break;
					// A note was detected somewhere inside the validator on startup and was rejected from the front of the
					// unit.
					case Commands.SSP_POLL_NOTE_CLEARED_FROM_FRONT:
						Logger.Device.Info(GetChannelValue(CommandStructure.ResponseData[i + 1]) + " note cleared from front at reset." +
										"\r\n");
						i++;
						break;
					// A note was detected somewhere inside the validator on startup and was cleared into the cashbox.
					case Commands.SSP_POLL_NOTE_CLEARED_TO_CASHBOX:
						Logger.Device.Info(GetChannelValue(CommandStructure.ResponseData[i + 1]) + " note cleared to stacker at reset." +
										"\r\n");
						i++;
						break;
					// The cashbox has been removed from the unit. This will continue to poll until the cashbox is replaced.
					case Commands.SSP_POLL_CASHBOX_REMOVED:
                        Nv200CashBox.CashState = false; //没有钱进入钱箱
                        Nv200CashBox.CashRemoved = true;
                        Nv200CashBox.CashFulled = false;
						Logger.Device.Info("Cashbox removed...\r\n");
						break;
					// The cashbox has been replaced, this will only display on a poll once.
					case Commands.SSP_POLL_CASHBOX_REPLACED:
                        Nv200CashBox.CashState = false; //没有钱进入钱箱
						Logger.Device.Info("Cashbox replaced\r\n");
						break;
					// A bar code ticket has been detected and validated. The ticket is in escrow, continuing to poll will accept
					// the ticket, sending a reject command will reject the ticket.
					case Commands.SSP_POLL_BAR_CODE_VALIDATED:
						Logger.Device.Info("Bar code ticket validated\r\n");
						break;
					// A bar code ticket has been accepted (equivalent to note credit).
					case Commands.SSP_POLL_BAR_CODE_ACK:
						Logger.Device.Info("Bar code ticket accepted\r\n");
						break;
					// The Cashbox has detected its note path is open. The unit is disabled while the note path is open.
					// Only available in protocol versions 6 and above.
					case Commands.SSP_POLL_NOTE_PATH_OPEN:
						Logger.Device.Info("Note path open\r\n");
						break;
					// All channels on the Cashbox have been inhibited so the Cashbox is disabled. Only available on protocol
					// versions 7 and above.
					case Commands.SSP_POLL_CHANNEL_DISABLE:
						Logger.Device.Info("All channels inhibited, unit disabled\r\n");
						break;

					default:
						Logger.Device.Info("Unrecognised poll response detected " + (int) CommandStructure.ResponseData[i] + "\r\n");
						break;
				}
			}
			return true;
		}

		// This function sends the command LAST REJECT CODE which gives info about why a note has been rejected. It then
		// outputs the info to a passed across textbox.
		private void QueryRejection()
		{
			CommandStructure.CommandData[0] = Commands.SSP_CMD_LAST_REJECT_CODE;
			CommandStructure.CommandDataLength = 1;
			if (!SendCommand()) return;

			if (CheckGenericResponses())
			{
				switch (CommandStructure.ResponseData[1])
				{
					case 0x00:
						Logger.Device.Info("Note accepted\r\n");
						break;
					case 0x01:
						Logger.Device.Info("Note length incorrect\r\n");
						break;
					case 0x02:
						Logger.Device.Info("Invalid note\r\n");
						break;
					case 0x03:
						Logger.Device.Info("Invalid note\r\n");
						break;
					case 0x04:
						Logger.Device.Info("Invalid note\r\n");
						break;
					case 0x05:
						Logger.Device.Info("Invalid note\r\n");
						break;
					case 0x06:
						Logger.Device.Info("Channel inhibited\r\n");
						break;
					case 0x07:
						Logger.Device.Info("Second note inserted during read\r\n");
						break;
					case 0x08:
						Logger.Device.Info("Host rejected note\r\n");
						break;
					case 0x09:
						Logger.Device.Info("Invalid note\r\n");
						break;
					case 0x0A:
						Logger.Device.Info("Invalid note read\r\n");
						break;
					case 0x0B:
						Logger.Device.Info("Note too long\r\n");
						break;
					case 0x0C:
						Logger.Device.Info("Validator disabled\r\n");
						break;
					case 0x0D:
						Logger.Device.Info("Mechanism slow/stalled\r\n");
						break;
					case 0x0E:
						Logger.Device.Info("Strim attempt\r\n");
						break;
					case 0x0F:
						Logger.Device.Info("Fraud channel reject\r\n");
						break;
					case 0x10:
						Logger.Device.Info("No notes inserted\r\n");
						break;
					case 0x11:
						Logger.Device.Info("Invalid note read\r\n");
						break;
					case 0x12:
						Logger.Device.Info("Twisted note detected\r\n");
						break;
					case 0x13:
						Logger.Device.Info("Escrow time-out\r\n");
						break;
					case 0x14:
						Logger.Device.Info("Bar code scan fail\r\n");
						break;
					case 0x15:
						Logger.Device.Info("Invalid note read\r\n");
						break;
					case 0x16:
						Logger.Device.Info("Invalid note read\r\n");
						break;
					case 0x17:
						Logger.Device.Info("Invalid note read\r\n");
						break;
					case 0x18:
						Logger.Device.Info("Invalid note read\r\n");
						break;
					case 0x19:
						Logger.Device.Info("Incorrect note width\r\n");
						break;
					case 0x1A:
						Logger.Device.Info("Note too short\r\n");
						break;
				}
			}
		}
	}
}