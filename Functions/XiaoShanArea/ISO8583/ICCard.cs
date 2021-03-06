﻿using System;
using System.Collections.Generic;
using System.Threading;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.YuHangArea.ISO8583.CPUCard;
using YuanTu.YuHangArea.ISO8583.External;
using DiTlv = System.Collections.Generic.Dictionary<int, YuanTu.YuHangArea.ISO8583.CPUCard.CPUTlv>;

namespace YuanTu.YuHangArea.ISO8583
{
    public class ICCard
    {
        private readonly ICardDevice _device;

        public byte[] LastResult { get; private set; }

        public ICCard(ICardDevice device)
        {
            _device = device;
        }

        public static Dictionary<int, string> TagNames { get; set; }

        public static LoggerWrapper Log { get; set; } = Logger.POS;

        private Result<byte[]> CPUChipIO(byte[] input)
        {
            Log.Debug("<= " + input.Bytes2Hex());
            byte[] output;
            var retry = 15;
            var total = retry;
            while (!_device.CPUChipIO(true, input, out output)&& --retry>0)
            {
                Log.Error("=> 通信失败");
               Thread.Sleep(100*(total+3 - retry));
            }
            if (retry<=0)
            {
                Log.Error("=> 多次尝试均失败，通信失败");
                return Result<byte[]>.Fail("通信失败");
            }
            Log.Debug("=> " + output.Bytes2Hex());
            return Result<byte[]>.Success(output);
        }

        public Result<DiTlv> Run(byte[] input, DiTlv dictionary = null)
        {
            var ioRes = CPUChipIO(input);
            if (!ioRes.IsSuccess)
                return ioRes.Convert().Convert<DiTlv>(); 

            var parseRes = ParseResult(ioRes.Value);
            if (!parseRes.IsSuccess)
                return parseRes.Convert().Convert<DiTlv>(); 

            if (parseRes.Value == null)
                return Result<DiTlv>.Success(dictionary);

            LastResult = parseRes.Value;

            var decoder = new CPUDecoder().Decode(parseRes.Value, dictionary);

            return Result<DiTlv>.Success(decoder.Dictionary);
        }

        public Result<byte[]> ParseResult(byte[] message)
        {
            var len = message.Length;
            var sw1 = message[len - 2];
            var sw2 = message[len - 1];
            switch (sw1)
            {
                case 0x90:
                    if (len == 2)
                        return Result<byte[]>.Success(null);
                    var data = new byte[len - 2];
                    Array.Copy(message, data, len - 2);
                    return Result<byte[]>.Success(data);

                default:
                    return Result<byte[]>.Fail(message.Bytes2Hex());
            }
        }

        public Result<DiTlv> Select(byte[] name)
        {
            Log.Info($"[Select] {name.Bytes2Hex()}");
            var command = new SELECT
            {
                Name = name
            };
            return Run(command.Make());
        }

        public Result<DiTlv> ReadRecord(byte sf1, byte id, DiTlv dictionary = null)
        {
            Log.Info($"[ReadRecord] {sf1} {id}");
            var command = new READ_RECORD
            {
                SF1 = sf1,
                Id = id
            };
            return Run(command.Make(), dictionary);
        }

        public Result<DiTlv> GetProcessOptions(byte[] data)
        {
            Log.Info($"[GetProcessOptions] {data.Bytes2Hex()}");
            var command = new GET_PROCESS_OPTIONS
            {
                PDOL = data
            };
            return Run(command.Make());
        }

        public Result<DiTlv> GetData(byte[] tag, DiTlv dictionary = null)
        {
            Log.Info($"[GetData] {tag.Bytes2Hex()}");
            var command = new GET_DATA
            {
                Tag = tag
            };
            return Run(command.Make(), dictionary);
        }

        public Result<DiTlv> GenerateAC(ACType acType, bool req, byte[] data)
        {
            Log.Info($"[GenerateAC] {acType} Req:{req}");
            var command = new GENERATE_AC
            {
                AcType = acType,
                Req = req,
                Bytes = data
            };
            return Run(command.Make());
        }

        public Result<DiTlv> ExternalAuth(byte[] data)
        {
            Log.Info($"[ExternalAuth] Auth:{data.Bytes2Hex()}");
            var command = new EXTERNAL_AUTHENTICATE
            {
                Auth = data
            };
            return Run(command.Make());
        }

        public Result<DiTlv> InternalAuth(byte[] data)
        {
            Log.Info($"[InternalAuth] Auth:{data.Bytes2Hex()}");
            var command = new INTERNAL_AUTHENTICATE
            {
                Auth = data
            };
            return Run(command.Make());
        }
    }
}