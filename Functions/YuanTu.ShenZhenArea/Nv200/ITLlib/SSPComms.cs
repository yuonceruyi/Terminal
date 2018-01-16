using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Security.Cryptography;
using System.Threading;

namespace YuanTu.ShenZhenArea.Nv200.ITLlib
{
    public class SSPComms
    {
        private Stopwatch sWatch = new Stopwatch();
        private SSP_TX_RX_PACKET ssp = new SSP_TX_RX_PACKET();
        private RandomNumber rand = new RandomNumber();
        private RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
        public const byte ssp_CMD_SYNC = 17;
        public const byte SSP_STX = 127;
        public const byte SSP_STEX = 126;
        public const ushort CRC_SSP_SEED = 65535;
        public const ushort CRC_SSP_POLY = 32773;
        public const ulong MAX_RANDOM_INTEGER = 2147483648;
        private Exception lastException;
        private bool crcRetry;
        private byte numCrcRetries;
        private SerialPort comPort;

        public bool OpenSSPComPort(SSP_COMMAND cmd)
        {
            try
            {
                this.comPort = new SerialPort();
                this.comPort.DataReceived += new SerialDataReceivedEventHandler(this.comPort_DataReceived);
                this.comPort.PortName = cmd.ComPort;
                this.comPort.BaudRate = cmd.BaudRate;
                this.comPort.Parity = Parity.None;
                this.comPort.StopBits = StopBits.Two;
                this.comPort.DataBits = 8;
                this.comPort.Handshake = Handshake.None;
                this.comPort.WriteTimeout = 500;
                this.comPort.ReadTimeout = 500;
                this.comPort.Open();
            }
            catch (Exception ex)
            {
                this.lastException = ex;
                return false;
            }
            return true;
        }

        public bool CloseComPort()
        {
            try
            {
                this.comPort.Close();
            }
            catch (Exception ex)
            {
                this.lastException = ex;
                return false;
            }
            return true;
        }

        public bool SSPSendCommand(SSP_COMMAND cmd, SSP_COMMAND_INFO sspInfo)
        {
            byte[] p = new byte[(int)byte.MaxValue];
            byte[] numArray = (byte[])null;
            byte num1 = 0;
            if (this.crcRetry)
            {
                numArray = new byte[(int)byte.MaxValue];
                cmd.CommandData.CopyTo((Array)numArray, 0);
                num1 = cmd.CommandDataLength;
            }
            if (!this.CompileSSPCommand(ref cmd, ref sspInfo))
            {
                cmd.ResponseStatus = PORT_STATUS.SSP_PACKET_ERROR;
                return false;
            }
            byte retryLevel = cmd.RetryLevel;
            do
            {
                this.ssp.NewResponse = false;
                this.ssp.rxBufferLength = (byte)0;
                this.ssp.rxPtr = (byte)0;
                if (!this.WritePort())
                {
                    cmd.ResponseStatus = PORT_STATUS.PORT_ERROR;
                    return false;
                }
                cmd.ResponseStatus = PORT_STATUS.SSP_REPLY_OK;
                this.sWatch.Stop();
                this.sWatch.Reset();
                this.sWatch.Start();
                while (!this.ssp.NewResponse)
                {
                    Thread.Sleep(1);
                    if (this.sWatch.ElapsedMilliseconds > (long)cmd.Timeout)
                    {
                        this.sWatch.Stop();
                        this.sWatch.Reset();
                        cmd.ResponseStatus = PORT_STATUS.SSP_CMD_TIMEOUT;
                        break;
                    }
                }
                if (cmd.ResponseStatus != PORT_STATUS.SSP_REPLY_OK)
                    --retryLevel;
                else
                    break;
            }
            while ((int)retryLevel > 0);
            if (cmd.ResponseStatus == PORT_STATUS.SSP_CMD_TIMEOUT)
            {
                sspInfo.Receive.PacketLength = (byte)0;
                return false;
            }
            sspInfo.Receive.PacketLength = (byte)((uint)this.ssp.rxData[2] + 5U);
            for (int index = 0; index < (int)sspInfo.Receive.PacketLength; ++index)
                sspInfo.Receive.PacketData[index] = this.ssp.rxData[index];
            if ((int)this.ssp.rxData[3] == 126)
            {
                byte length = (byte)((uint)this.ssp.rxData[2] - 1U);
                this.aes_decrypt(ref cmd.Key, ref this.ssp.rxData, ref length, (byte)4);
                ushort num2 = this.cal_crc_loop_CCITT_A((ushort)((uint)length - 2U), (ushort)4, ref this.ssp.rxData, ushort.MaxValue, (ushort)32773);
                if ((int)(byte)((uint)num2 & (uint)byte.MaxValue) != (int)this.ssp.rxData[(int)this.ssp.rxData[2] + 1] || (int)(byte)((int)num2 >> 8 & (int)byte.MaxValue) != (int)this.ssp.rxData[(int)this.ssp.rxData[2] + 2])
                {
                    if (this.crcRetry && (int)this.numCrcRetries < 3)
                    {
                        numArray.CopyTo((Array)cmd.CommandData, 0);
                        cmd.CommandDataLength = num1;
                        ++this.numCrcRetries;
                        --cmd.encPktCount;
                        if (this.SSPSendCommand(cmd, sspInfo))
                        {
                            this.numCrcRetries = (byte)0;
                            return true;
                        }
                        cmd.ResponseStatus = PORT_STATUS.SSP_PACKET_ERROR_CRC_FAIL;
                        sspInfo.Receive.PacketLength = (byte)0;
                        this.numCrcRetries = (byte)0;
                        return false;
                    }
                    cmd.ResponseStatus = PORT_STATUS.SSP_PACKET_ERROR_CRC_FAIL;
                    sspInfo.Receive.PacketLength = (byte)0;
                    this.numCrcRetries = (byte)0;
                    return false;
                }
                uint num3 = 0;
                for (int index = 0; index < 4; ++index)
                    num3 += (uint)this.ssp.rxData[5 + index] << index * 8;
                if ((int)num3 != (int)cmd.encPktCount)
                {
                    cmd.ResponseStatus = PORT_STATUS.SSP_PACKET_ERROR_ENC_COUNT;
                    sspInfo.Receive.PacketLength = (byte)0;
                    return false;
                }
                this.ssp.rxBufferLength = (byte)((uint)this.ssp.rxData[4] + 5U);
                p[0] = this.ssp.rxData[0];
                p[1] = this.ssp.rxData[1];
                p[2] = this.ssp.rxData[4];
                for (int index = 0; index < (int)this.ssp.rxData[4]; ++index)
                    p[3 + index] = this.ssp.rxData[9 + index];
                ushort num4 = this.cal_crc_loop_CCITT_A((ushort)((uint)this.ssp.rxBufferLength - 3U), (ushort)1, ref p, ushort.MaxValue, (ushort)32773);
                p[3 + (int)this.ssp.rxData[4]] = (byte)((uint)num4 & (uint)byte.MaxValue);
                p[4 + (int)this.ssp.rxData[4]] = (byte)((int)num4 >> 8 & (int)byte.MaxValue);
                for (int index = 0; index < (int)this.ssp.rxBufferLength; ++index)
                    this.ssp.rxData[index] = p[index];
            }
            cmd.ResponseDataLength = this.ssp.rxData[2];
            for (int index = 0; index < (int)cmd.ResponseDataLength; ++index)
                cmd.ResponseData[index] = this.ssp.rxData[index + 3];
            sspInfo.PreEncryptedRecieve.PacketLength = this.ssp.rxBufferLength;
            for (int index = 0; index < (int)this.ssp.rxBufferLength; ++index)
                sspInfo.PreEncryptedRecieve.PacketData[index] = this.ssp.rxData[index];
            cmd.sspSeq = (int)cmd.sspSeq != 128 ? (byte)128 : (byte)0;
            cmd.ResponseStatus = PORT_STATUS.SSP_REPLY_OK;
            return true;
        }

        private bool CompileSSPCommand(ref SSP_COMMAND cmd, ref SSP_COMMAND_INFO sspInfo)
        {
            byte[] numArray1 = new byte[(int)byte.MaxValue];
            this.ssp.rxPtr = (byte)0;
            for (uint index = 0; index < (uint)byte.MaxValue; ++index)
                this.ssp.rxData[index] = (byte)0;
            if ((int)cmd.CommandData[0] == 17)
                cmd.sspSeq = (byte)128;
            sspInfo.Encrypted = cmd.EncryptionStatus;
            sspInfo.PreEncryptedTransmit.PacketLength = (byte)((uint)cmd.CommandDataLength + 5U);
            sspInfo.PreEncryptedTransmit.PacketData[0] = (byte)127;
            sspInfo.PreEncryptedTransmit.PacketData[1] = (byte)((uint)cmd.SSPAddress | (uint)cmd.sspSeq);
            sspInfo.PreEncryptedTransmit.PacketData[2] = cmd.CommandDataLength;
            for (uint index = 0; index < (uint)cmd.CommandDataLength; ++index)
                sspInfo.PreEncryptedTransmit.PacketData[(3U + index)] = cmd.CommandData[index];
            ushort num1 = this.cal_crc_loop_CCITT_A((ushort)((uint)cmd.CommandDataLength + 2U), (ushort)1, ref sspInfo.PreEncryptedTransmit.PacketData, ushort.MaxValue, (ushort)32773);
            sspInfo.PreEncryptedTransmit.PacketData[3 + (int)cmd.CommandDataLength] = (byte)((uint)num1 & (uint)byte.MaxValue);
            sspInfo.PreEncryptedTransmit.PacketData[4 + (int)cmd.CommandDataLength] = (byte)((int)num1 >> 8 & (int)byte.MaxValue);
            if (cmd.EncryptionStatus && !this.EncryptSSPPacket(ref cmd.encPktCount, ref cmd.CommandData, ref cmd.CommandData, ref cmd.CommandDataLength, ref cmd.CommandDataLength, ref cmd.Key))
                return false;
            this.ssp.CheckStuff = (byte)0;
            this.ssp.SSPAddress = cmd.SSPAddress;
            this.ssp.rxPtr = (byte)0;
            this.ssp.txPtr = (byte)0;
            this.ssp.txBufferLength = (byte)((uint)cmd.CommandDataLength + 5U);
            this.ssp.txData[0] = (byte)127;
            this.ssp.txData[1] = (byte)((uint)cmd.SSPAddress | (uint)cmd.sspSeq);
            this.ssp.txData[2] = cmd.CommandDataLength;
            for (uint index = 0; index < (uint)cmd.CommandDataLength; ++index)
                this.ssp.txData[(3U + index)] = cmd.CommandData[index];
            ushort num2 = this.cal_crc_loop_CCITT_A((ushort)((uint)this.ssp.txBufferLength - 3U), (ushort)1, ref this.ssp.txData, ushort.MaxValue, (ushort)32773);
            this.ssp.txData[3 + (int)cmd.CommandDataLength] = (byte)((uint)num2 & (uint)byte.MaxValue);
            this.ssp.txData[4 + (int)cmd.CommandDataLength] = (byte)((int)num2 >> 8 & (int)byte.MaxValue);
            for (uint index = 0; index < (uint)this.ssp.txBufferLength; ++index)
                sspInfo.Transmit.PacketData[index] = this.ssp.txData[index];
            sspInfo.Transmit.PacketLength = this.ssp.txBufferLength;
            uint num3 = 0;
            byte[] numArray2 = numArray1;
            int num4 = (int)num3;
            int num5 = 1;
            uint num6 = (uint)(num4 + num5);
            uint index1 = (uint)num4;
            int num7 = (int)this.ssp.txData[0];
            numArray2[index1] = (byte)num7;
            for (uint index2 = 1; index2 < (uint)this.ssp.txBufferLength; ++index2)
            {
                numArray1[num6] = this.ssp.txData[index2];
                if ((int)this.ssp.txData[index2] == (int)sbyte.MaxValue)
                    numArray1[++num6] = (byte)127;
                ++num6;
            }
            for (uint index2 = 0; index2 < num6; ++index2)
                this.ssp.txData[index2] = numArray1[index2];
            this.ssp.txBufferLength = (byte)num6;
            return true;
        }

        private ushort cal_crc_loop_CCITT_A(ushort l, ushort offset, ref byte[] p, ushort seed, ushort cd)
        {
            ushort num = seed;
            for (ushort index1 = 0; (int)index1 < (int)l; ++index1)
            {
                num ^= (ushort)((uint)p[(int)index1 + (int)offset] << 8);
                for (ushort index2 = 0; (int)index2 < 8; ++index2)
                {
                    if (((int)num & 32768) != 0)
                        num = (ushort)((uint)num << 1 ^ (uint)cd);
                    else
                        num <<= 1;
                }
            }
            return num;
        }

        private bool WritePort()
        {
            try
            {
                this.comPort.Write(this.ssp.txData, 0, (int)this.ssp.txBufferLength);
            }
            catch (Exception ex)
            {
                this.lastException = ex;
                return false;
            }
            return true;
        }

        private void comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = (SerialPort)sender;
            if (!serialPort.IsOpen)
                return;
            try
            {
                while (serialPort.BytesToRead > 0)
                    this.SSPDataIn((byte)serialPort.ReadByte());
            }
            catch (Exception ex)
            {
                this.lastException = ex;
            }
        }

        private void SSPDataIn(byte RxChar)
        {
            if ((int)RxChar == (int)sbyte.MaxValue && (int)this.ssp.rxPtr == 0)
            {
                this.ssp.rxData[(int)this.ssp.rxPtr++] = RxChar;
            }
            else
            {
                if ((int)this.ssp.CheckStuff == 1)
                {
                    if ((int)RxChar != (int)sbyte.MaxValue)
                    {
                        this.ssp.rxData[0] = (byte)127;
                        this.ssp.rxData[1] = RxChar;
                        this.ssp.rxPtr = (byte)2;
                    }
                    else
                        this.ssp.rxData[(int)this.ssp.rxPtr++] = RxChar;
                    this.ssp.CheckStuff = (byte)0;
                }
                else if ((int)RxChar == (int)sbyte.MaxValue)
                {
                    this.ssp.CheckStuff = (byte)1;
                }
                else
                {
                    this.ssp.rxData[(int)this.ssp.rxPtr++] = RxChar;
                    if ((int)this.ssp.rxPtr == 3)
                        this.ssp.rxBufferLength = (byte)((uint)this.ssp.rxData[2] + 5U);
                }
                if ((int)this.ssp.rxPtr != (int)this.ssp.rxBufferLength)
                    return;
                if (((int)this.ssp.rxData[1] & (int)sbyte.MaxValue) == (int)this.ssp.SSPAddress)
                {
                    ushort num = this.cal_crc_loop_CCITT_A((ushort)(byte)((uint)this.ssp.rxBufferLength - 3U), (ushort)1, ref this.ssp.rxData, ushort.MaxValue, (ushort)32773);
                    if ((int)(byte)((uint)num & (uint)byte.MaxValue) == (int)this.ssp.rxData[(int)this.ssp.rxBufferLength - 2] && (int)(byte)((int)num >> 8 & (int)byte.MaxValue) == (int)this.ssp.rxData[(int)this.ssp.rxBufferLength - 1])
                        this.ssp.NewResponse = true;
                }
                this.ssp.rxPtr = (byte)0;
                this.ssp.CheckStuff = (byte)0;
            }
        }

        private bool EncryptSSPPacket(ref uint ePktCount, ref byte[] dataIn, ref byte[] dataOut, ref byte lengthIn, ref byte lengthOut, ref SSP_FULL_KEY key)
        {
            byte num1 = 0;
            byte[] numArray = new byte[(int)byte.MaxValue];
            byte num2 = (byte)((uint)lengthIn + 7U);
            if ((int)num2 % 16 != 0)
                num1 = (byte)(16 - (int)num2 % 16);
            byte length = (byte)((uint)num2 + (uint)num1);
            numArray[0] = lengthIn;
            for (byte index = 0; (int)index < 4; ++index)
                numArray[1 + (int)index] = (byte)(ePktCount >> 8 * (int)index & (uint)byte.MaxValue);
            for (byte index = 0; (int)index < (int)lengthIn; ++index)
                numArray[(int)index + 5] = dataIn[(int)index];
            for (byte index = 0; (int)index < (int)num1; ++index)
                numArray[5 + (int)lengthIn + (int)index] = (byte)this.rand.GenerateRandomNumber();
            ushort num3 = this.cal_crc_loop_CCITT_A((ushort)((uint)length - 2U), (ushort)0, ref numArray, ushort.MaxValue, (ushort)32773);
            numArray[(int)length - 2] = (byte)((uint)num3 & (uint)byte.MaxValue);
            numArray[(int)length - 1] = (byte)((int)num3 >> 8 & (int)byte.MaxValue);
            this.aes_encrypt(ref key, ref numArray, ref length, (byte)0);
            ++length;
            lengthOut = length;
            dataOut[0] = (byte)126;
            for (byte index = 0; (int)index < (int)length - 1; ++index)
                dataOut[1 + (int)index] = numArray[(int)index];
            ++ePktCount;
            return true;
        }

        private void aes_encrypt(ref SSP_FULL_KEY sspKey, ref byte[] data, ref byte length, byte offset)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (Aes aes = (Aes)new AesManaged())
                {
                    byte[] numArray = new byte[16];
                    for (byte index = 0; (int)index < 8; ++index)
                    {
                        numArray[(int)index] = (byte)(sspKey.FixedKey >> 8 * (int)index);
                        numArray[(int)index + 8] = (byte)(sspKey.VariableKey >> 8 * (int)index);
                    }
                    aes.BlockSize = 128;
                    aes.KeySize = 128;
                    aes.Key = numArray;
                    aes.Mode = CipherMode.ECB;
                    aes.Padding = PaddingMode.None;
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        cryptoStream.Write(data, (int)offset, (int)length);
                    data = memoryStream.ToArray();
                }
            }
        }

        private void aes_decrypt(ref SSP_FULL_KEY sspKey, ref byte[] data, ref byte length, byte offset)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (Aes aes = (Aes)new AesManaged())
                {
                    byte[] numArray1 = new byte[(int)length];
                    byte[] numArray2 = new byte[16];
                    for (byte index = 0; (int)index < 8; ++index)
                    {
                        numArray2[(int)index] = (byte)(sspKey.FixedKey >> 8 * (int)index);
                        numArray2[(int)index + 8] = (byte)(sspKey.VariableKey >> 8 * (int)index);
                    }
                    aes.BlockSize = 128;
                    aes.KeySize = 128;
                    aes.Key = numArray2;
                    aes.Mode = CipherMode.ECB;
                    aes.Padding = PaddingMode.None;
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        cryptoStream.Write(data, (int)offset, (int)length);
                    byte[] array = memoryStream.ToArray();
                    for (byte index = 0; (int)index < (int)length; ++index)
                        data[(int)index + (int)offset] = array[(int)index];
                }
            }
        }

        public bool InitiateSSPHostKeys(SSP_KEYS keys, SSP_COMMAND cmd)
        {
            keys.Generator = this.rand.GeneratePrime();
            keys.Modulus = this.rand.GeneratePrime();
            if (keys.Generator < keys.Modulus)
            {
                ulong generator = keys.Generator;
                keys.Generator = keys.Modulus;
                keys.Modulus = generator;
            }
            if (!this.CreateHostInterKey(keys))
                return false;
            cmd.encPktCount = 0U;
            return true;
        }

        public bool CreateHostInterKey(SSP_KEYS keys)
        {
            if ((long)keys.Generator == 0L || (long)keys.Modulus == 0L)
                return false;
            keys.HostRandom = this.rand.GenerateRandomNumber() % 2147483648UL;
            keys.HostInter = this.rand.XpowYmodN(keys.Generator, keys.HostRandom, keys.Modulus);
            return true;
        }

        public bool CreateSSPHostEncryptionKey(SSP_KEYS keys)
        {
            keys.KeyHost = this.rand.XpowYmodN(keys.SlaveInterKey, keys.HostRandom, keys.Modulus);
            return true;
        }

        public Exception GetLastException()
        {
            return this.lastException;
        }

        public void RetryOnFailedCRC(bool doRetry)
        {
            this.crcRetry = doRetry;
        }
    }
}