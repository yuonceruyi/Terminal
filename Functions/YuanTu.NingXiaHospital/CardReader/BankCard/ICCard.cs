using System;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.NingXiaHospital.CardReader.BankCard.CPUCard;
using YuanTu.NingXiaHospital.CardReader.BankCard.Util;
using DiTlv = System.Collections.Generic.Dictionary<int, YuanTu.NingXiaHospital.CardReader.BankCard.CPUCard.CPUTlv>;

namespace YuanTu.NingXiaHospital.CardReader.BankCard
{
    public class ICCard
    {
        private readonly Func<byte[], Result<byte[]>> _ioFunc;

        public ICCard(Func<byte[], Result<byte[]>> ioFunc)
        {
            _ioFunc = ioFunc;
        }

        public byte[] LastResult { get; private set; }


        private Result<byte[]> CPUChipIO(byte[] input)
        {
            Logger.Device.Info("<= " + input.Bytes2Hex());
            var res = _ioFunc(input);
            if (!res.IsSuccess)
            {
                Logger.Device.Info("=> 通信失败");
                return Result<byte[]>.Fail("通信失败");
            }
            var output = res.Value;
            Logger.Device.Info("=> " + output.Bytes2Hex());
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
            Logger.Device.Info($"[Select] {name.Bytes2Hex()}");
            var command = new SELECT
            {
                Name = name
            };
            return Run(command.Make());
        }

        public Result<DiTlv> ReadRecord(byte sf1, byte id, DiTlv dictionary = null)
        {
            Logger.Device.Info($"[ReadRecord] {sf1} {id}");
            var command = new READ_RECORD
            {
                SF1 = sf1,
                Id = id
            };
            return Run(command.Make(), dictionary);
        }

        public Result<DiTlv> GetProcessOptions(byte[] data)
        {
            Logger.Device.Info($"[GetProcessOptions] {data.Bytes2Hex()}");
            var command = new GET_PROCESS_OPTIONS
            {
                PDOL = data
            };
            return Run(command.Make());
        }

        public Result<DiTlv> GetData(byte[] tag, DiTlv dictionary = null)
        {
            Logger.Device.Info($"[GetData] {tag.Bytes2Hex()}");
            var command = new GET_DATA
            {
                Tag = tag
            };
            return Run(command.Make(), dictionary);
        }

        public Result<DiTlv> GenerateAC(ACType acType, bool req, byte[] data)
        {
            Logger.Device.Info($"[GenerateAC] {acType} Req:{req}");
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
            Logger.Device.Info($"[ExternalAuth] Auth:{data.Bytes2Hex()}");
            var command = new EXTERNAL_AUTHENTICATE
            {
                Auth = data
            };
            return Run(command.Make());
        }

        public Result<DiTlv> InternalAuth(byte[] data)
        {
            Logger.Device.Info($"[InternalAuth] Auth:{data.Bytes2Hex()}");
            var command = new INTERNAL_AUTHENTICATE
            {
                Auth = data
            };
            return Run(command.Make());
        }
    }
}
