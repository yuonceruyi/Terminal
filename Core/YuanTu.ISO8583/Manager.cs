using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Devices.CardReader;
using YuanTu.ISO8583.CPUCard;
using YuanTu.ISO8583.Interfaces;
using YuanTu.ISO8583.IO;
using YuanTu.ISO8583.Util;

namespace YuanTu.ISO8583
{
    public class Manager : IManager
    {
        protected List<CPUTlv> firstHalfList;

        protected bool icMode;

        public IMagCardReader MagCardReader { get; set; }

        public IIcCardReader IcCardReader { get; set; }

        public IPOS POS { get; set; }
        public IICPOS IcPos { get; set; }

        /// <summary>
        ///     CA 参数 下载
        /// </summary>
        /// <returns></returns>
        public virtual Result Initialize()
        {
            if (!File.Exists("PubKey.json"))
            {
                var downRes = POS.DownloadPubKey();
                if (!downRes.IsSuccess)
                    return Result.Fail("DownloadPubKey Failed!");
            }
            var listPubKeys = TLVStorage.Load("PubKey.json");
            ICPOS.PublicKeys = TLVStorage.LoadTLVDics(listPubKeys);

            if (!File.Exists("Params.json"))
            {
                var downRes = POS.DownloadParams();
                if (!downRes.IsSuccess)
                    return Result.Fail("DownloadParams Failed!");
            }
            var listParams = TLVStorage.Load("Params.json");
            ICPOS.Params = TLVStorage.LoadTLVDics(listParams);
            return Result.Success();
        }

        //public virtual Result ReadCard(Input input)
        //{
        //    CardPos pos;
        //    var ret = Wrapper.CheckCard(out pos);
        //    if (pos != CardPos.停卡位 && pos != CardPos.IC位)
        //        return Result.Fail("未检测到卡");
        //    if (pos == CardPos.停卡位)
        //    {
        //        ret = Wrapper.MoveCard();
        //        if (!ret)
        //            return Result.Fail("移动卡失败");
        //    }
        //    byte[] apdu;
        //    icMode = Wrapper.CPUCodeReset(out apdu);
        //    Console.WriteLine($"ICMode:{icMode}");
        //    if (!icMode)
        //    {
        //        string track2, track3;
        //        ret = Wrapper.ReadTracks(out track2, out track3);
        //        if (!ret)
        //            return Result.Fail("读磁轨失败");
        //        input.Track2 = track2;
        //        input.Track3 = track3;
        //        var n = track2.IndexOf('=');
        //        if (n < 0)
        //            return Result.Fail("读磁轨失败或该卡不是银行卡，请重试或更换卡");
        //        input.BankNo = track2.Substring(0, n);
        //        return Result.Success();
        //    }
        //    var res = IcPos.ReadCard(input, bytes =>
        //    {
        //        byte[] outBytes;
        //        if (Wrapper.CPUChipIO(true, bytes, out outBytes))
        //            return Result<byte[]>.Success(outBytes);
        //        return Result<byte[]>.Fail("");
        //    });
        //    if (!res.IsSuccess)
        //        return Result.Fail("读IC卡信息失败:" + res.Message);

        //    return Result.Success();
        //}
        public virtual Result ReadCard(Input input)
        {
            var posRes = MagCardReader.GetCardPosition();
            if (!posRes.IsSuccess)
                return posRes.Convert();
            var pos = posRes.Value;
            if ((pos != CardPos.停卡位) && (pos != CardPos.IC位))
                return Result.Fail("未检测到卡");
            if (pos == CardPos.停卡位)
            {
                var moveRes = MagCardReader.MoveCard(CardPos.IC位);
                if (!moveRes.IsSuccess)
                    return moveRes;
            }
            var resetRes = IcCardReader.CPUCodeReset();
            icMode = resetRes.IsSuccess;
            Console.WriteLine($"ICMode:{icMode}");
            if (!icMode)
            {
                var trackRes = MagCardReader.ReadTrackInfos(TrackRoad.Trace2, ReadType.ASCII);
                //var trackRes = MagCardReader.ReadTrackInfos(TrackRoad.Trace2 | TrackRoad.Trace3, ReadType.ASCII);
                if (!trackRes.IsSuccess)
                    return trackRes.Convert();

                var trackRes2 = MagCardReader.ReadTrackInfos(TrackRoad.Trace3, ReadType.ASCII);
                if (!trackRes2.IsSuccess)
                    return trackRes2.Convert();

                var dic = trackRes.Value;
                var dic2 = trackRes2.Value;
                string track2 = string.Empty;
                string track3 = string.Empty;

                if (dic.ContainsKey(TrackRoad.Trace2))
                    track2 = trackRes.Value[TrackRoad.Trace2];
                if (dic2.ContainsKey(TrackRoad.Trace3))
                    track3 = trackRes2.Value[TrackRoad.Trace3];

                input.Track2 = track2;
                input.Track3 = track3;
                var n = track2.IndexOf('=');
                if (n < 0)
                    return Result.Fail("读磁轨失败或该卡不是银行卡，请重试或更换卡");
                input.BankNo = track2.Substring(0, n);
                return Result.Success();
            }
            var res = IcPos.ReadCard(input, bytes => IcCardReader.CPUChipIO(true, bytes));
            if (!res.IsSuccess)
                return Result.Fail("读IC卡信息失败:" + res.Message);

            return Result.Success();
        }

        public virtual Result<byte[]> DoLogon()
        {
            return POS.DoLogon();
        }

        public virtual Result<Output> DoSale(Input input)
        {
            if (!icMode)
                return POS.DoSale(input);
            firstHalfList = IcPos.FirstHalf();
            if (firstHalfList == null)
                return Result<Output>.Fail("IC卡处理失败");

            var doRes = POS.DoSaleIC(input, firstHalfList);
            if (!doRes.IsSuccess)
            {
                IcPos.OnlineFail();
                return Result<Output>.Fail(doRes.Message);
            }
            var output = doRes.Value;
            if (!IcPos.SecondHalf(output))
            {
                var reRes = POS.DoReverseIC();
                if (reRes.IsSuccess)
                    return Result<Output>.Fail("IC卡确认失败 冲正成功");
                return Result<Output>.Fail("IC卡确认失败 冲正失败");
            }
            if (output.Notify)
            {
                var noRes = POS.DoNotifyIC(input, IcPos.MakeNotifyList());
                Console.WriteLine("DoNotifyIC " + (noRes.IsSuccess ? "Done" : "Failed"));
            }
            {
                var upRes = POS.DoUploadIC(input, IcPos.MakeUploadList());
                Console.WriteLine("DoUploadIC " + (upRes.IsSuccess ? "Done" : "Failed"));
            }
            return Result<Output>.Success(output);
        }

        public virtual Result<Output> DoReverse()
        {
            if (icMode)
                return POS.DoReverseIC();

            return POS.DoReverse();
        }

        public virtual Result<string> DoDownloadMasterKey(string sn)
        {
            return POS.DoDownloadMasterKey(sn);
        }

        public static Dictionary<int, string> LoadTagsDevice()
        {
            var dic = new Dictionary<int, string>();
            if (!File.Exists("Tags.csv"))
                return dic;
            using (var sr = new StreamReader("Tags.csv", Encoding.Default))
            {
                sr.ReadLine();
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var list = line.Split(',');
                    var tag = list[1];
                    var tagBytes = tag.Hex2Bytes();
                    int tagValue = tagBytes[0];
                    if (tagBytes.Length > 1)
                        tagValue = tagValue * 0x100 + tagBytes[1];
                    dic[tagValue] = list[0];
                }
            }
            return dic;
        }
    }
}