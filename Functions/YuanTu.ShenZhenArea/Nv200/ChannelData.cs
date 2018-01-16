﻿using System;

namespace YuanTu.ShenZhenArea.Nv200
{
	internal class ChannelData
	{
		public byte Channel;
		public char[] Currency;
		public int Level;
		public bool Recycling;
		public int Value;

		public ChannelData()
		{
			Value = 0;
			Channel = 0;
			Currency = new char[3];
			Level = 0;
			Recycling = false;
		}

		public class Helpers
		{
			// Helper function to convert 4 bytes to an int 32 from a specified array and index.
			public static int ConvertBytesToInt32(byte[] b, int index)
			{
				return BitConverter.ToInt32(b, index);
			}

			// 2 bytes to int 16
			public static int ConvertBytesToInt16(byte[] b, int index)
			{
				return BitConverter.ToInt16(b, index);
			}

			// Convert int32 to byte array
			public static byte[] ConvertIntToBytes(int n)
			{
				return BitConverter.GetBytes(n);
			}

			// Convert int16 to byte array
			public static byte[] ConvertIntToBytes(short n)
			{
				return BitConverter.GetBytes(n);
			}

			// Convert int8 to byte
			public static byte[] ConvertIntToBytes(char n)
			{
				return BitConverter.GetBytes(n);
			}

			// Helper uses value of channel and adds a decimal point and two zeroes, also adds current
			// currency.
			public static string FormatToCurrency(int unformattedNumber)
			{
				var f = unformattedNumber*0.01f;
				return f.ToString("0.00");
			}

			// This helper takes a byte and returns the command/response name as a string.
			public static string ConvertByteToName(byte b)
			{
				switch (b)
				{
					case 0x01:
						return "RESET COMMAND";
					case 0x11:
						return "SYNC COMMAND";
					case 0x4A:
						return "SET GENERATOR COMMAND";
					case 0x4B:
						return "SET MODULUS COMMAND";
					case 0x4C:
						return "KEY EXCHANGE COMMAND";
					case 0x2:
						return "SET INHIBITS COMMAND";
					case 0xA:
						return "ENABLE COMMAND";
					case 0x09:
						return "DISABLE COMMAND";
					case 0x7:
						return "POLL COMMAND";
					case 0x05:
						return "SETUP REQUEST COMMAND";
					case 0x03:
						return "DISPLAY ON COMMAND";
					case 0x04:
						return "DISPLAY OFF COMMAND";
					case 0x5C:
						return "ENABLE PAYOUT COMMAND";
					case 0x5B:
						return "DISABLE PAYOUT COMMAND";
					case 0x3B:
						return "SET ROUTING COMMAND";
					case 0x45:
						return "SET VALUE REPORTING TYPE COMMAND";
					case 0X42:
						return "PAYOUT LAST NOTE COMMAND";
					case 0x3F:
						return "EMPTY COMMAND";
					case 0x41:
						return "GET NOTE POSITIONS COMMAND";
					case 0x43:
						return "STACK LAST NOTE COMMAND";
					case 0xF1:
						return "RESET RESPONSE";
					case 0xEF:
						return "NOTE READ RESPONSE";
					case 0xEE:
						return "CREDIT RESPONSE";
					case 0xED:
						return "REJECTING RESPONSE";
					case 0xEC:
						return "REJECTED RESPONSE";
					case 0xCC:
						return "STACKING RESPONSE";
					case 0xEB:
						return "STACKED RESPONSE";
					case 0xEA:
						return "SAFE JAM RESPONSE";
					case 0xE9:
						return "UNSAFE JAM RESPONSE";
					case 0xE8:
						return "DISABLED RESPONSE";
					case 0xE6:
						return "FRAUD ATTEMPT RESPONSE";
					case 0xE7:
						return "STACKER FULL RESPONSE";
					case 0xE1:
						return "NOTE CLEARED FROM FRONT RESPONSE";
					case 0xE2:
						return "NOTE CLEARED TO CASHBOX RESPONSE";
					case 0xE3:
						return "CASHBOX REMOVED RESPONSE";
					case 0xE4:
						return "CASHBOX REPLACED RESPONSE";
					case 0xDB:
						return "NOTE STORED RESPONSE";
					case 0xDA:
						return "NOTE DISPENSING RESPONSE";
					case 0xD2:
						return "NOTE DISPENSED RESPONSE";
					case 0xC9:
						return "NOTE TRANSFERRED TO STACKER RESPONSE";
					case 0xF0:
						return "OK RESPONSE";
					case 0xF2:
						return "UNKNOWN RESPONSE";
					case 0xF3:
						return "WRONG PARAMS RESPONSE";
					case 0xF4:
						return "PARAM OUT OF RANGE RESPONSE";
					case 0xF5:
						return "CANNOT PROCESS RESPONSE";
					case 0xF6:
						return "SOFTWARE ERROR RESPONSE";
					case 0xF8:
						return "FAIL RESPONSE";
					case 0xFA:
						return "KEY NOT SET RESPONSE";
					default:
						return "Byte command name unsupported";
				}
			}
		}
	}
}