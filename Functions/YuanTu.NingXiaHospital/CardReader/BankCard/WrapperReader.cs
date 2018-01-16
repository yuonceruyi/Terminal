using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Devices;
using YuanTu.Devices.CardReader;
using YuanTu.NingXiaHospital.CardReader.BankCard.CPUCard;
using YuanTu.NingXiaHospital.CardReader.BankCard.IO;

namespace YuanTu.NingXiaHospital.CardReader.BankCard
{
    public class WrapperReader
    {
        public static IMagCardReader MagCardReader { get; set; }

        public static IIcCardReader IcCardReader { get; set; }

        public static Result<string> IcRead()
        {
            var pos = IcCardReader.GetCardPosition();
            if (!(pos.IsSuccess && (pos.Value == CardPos.停卡位 || pos.Value == CardPos.IC位)))
            {
                var connResult = IcCardReader.Connect();
                if (!connResult)
                    return Result<string>.Fail(connResult.Message);
                var initResult = IcCardReader.Initialize();
                if (!initResult)
                    return Result<string>.Fail(initResult.Message);
            }
            var moveResult = IcCardReader.MoveCard(CardPos.IC位);
            if (!moveResult)
                return Result<string>.Fail(moveResult.Message);
            var powerOnResult = IcCardReader.PowerOn(SlotNo.大卡座);
            if (!powerOnResult)
                return Result<string>.Fail(powerOnResult.Message);
            var _icpos = new ICPOS();
            var input = new Input
            {
                Amount = 0,
                Now = DateTime.Now
            };
            var resultReadCard = _icpos.ReadCard(input, IoFunc);
            if (resultReadCard)
            {
                Logger.Device.Info($"[银行卡读卡]IC读卡内容:{input.ToJsonString()}");
                return Result<string>.Success(input.Track2);
            }
            return Result<string>.Fail(resultReadCard.Message);
        }

        public static Result<string> MagRead()
        {
            var connResult = MagCardReader.Connect();
            if (!connResult)
                return Result<string>.Fail(connResult.Message);
            var initResult = MagCardReader.Initialize();
            if (!initResult)
                return Result<string>.Fail(initResult.Message);
            var ret = MagCardReader.ReadTrackInfos(2);
            if (!ret)
                return Result<string>.Fail($"[银行卡读卡]Rcard Failed: {ret.Message}");
            Logger.Device.Info($"[银行卡读卡]磁条读卡内容:{ret.Value}");
            return Result<string>.Success(ret.Value);
        }

        private static Result<byte[]> IoFunc(byte[] bytes)
        {
            var ret = IcCardReader.CPUChipIO(true, bytes);
            if (!ret)
                return Result<byte[]>.Fail($"Application Failed: {ret.Message}");
            // GetResponse

            if (ret.Value[0]== 0x61)
            {
                var command = new byte[]
                {
                    0x00, 0xC0, 0x00, 0x00, ret.Value[1]
                };
                ret = IcCardReader.CPUChipIO(true, command);
                if (!ret)
                    return Result<byte[]>.Fail($"Application Failed: {ret.Message}");
            }
            return Result<byte[]>.Success(ret.Value);
        }

    }
}
