using System;
using System.Threading;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.PanYu.House.CardReader.HuaDa;

namespace YuanTu.PanYu.House.CardReader
{
    public interface ICardService : IService
    {
        CardInfo CardInfo { get; set; }

        Result<CardInfo> ReadCard();

        Result<CardInfo> ReadCardExt();

        Result<CardInfo> ReadCardExt2();
    }

    public class CardService : ICardService
    {
        public CardInfo CardInfo { get; set; }
        private bool _running;

        //public CardService(string serviceName)
        //{
        //    ServiceName = serviceName;
        //}

        public Result<CardInfo> ReadCard()
        {
            try
            {
                if (!ACT_A6_V2.Init())
                {
                    ReportService.读卡器离线(null, ErrorSolution.读卡器离线);
                    return Result<CardInfo>.Fail("读卡器离线");
                }

                ACT_A6_V2.EnableCard(true);
                _running = true;
                while (_running)
                {
                    if (!ACT_A6_V2.HaveCard())
                    {
                        Thread.Sleep(200);
                        continue;
                    }
                    Thread.Sleep(200);
                    if (!ACT_A6_V2.HaveCard())
                    {
                        continue;
                    }
                    _running = false;
                }
                IACT_A6_V2.DetectIccType();
                string seqNo, cardNo, name, sex, idtype, idno, cardSeq;
                switch (IACT_A6_V2.cardType)
                {
                    case IACT_A6_V2.ICardType.M1:
                        var sw = new System.Diagnostics.Stopwatch();
                        sw.Start();
                        seqNo = IACT_A6_V2.GetSeqNo();
                        Logger.Main.Info("卡片序列号/" + seqNo);
                        sw.Stop();
                        Logger.Main.Info($"卡片序列号，时间：{sw.ElapsedMilliseconds}");
                        sw.Restart();
                        cardNo = IACT_A6_V2.ReadCardNo();
                        sw.Stop();
                        Logger.Main.Info("卡片卡号/" + cardNo);
                        Logger.Main.Info($"卡号，时间：{sw.ElapsedMilliseconds}");
                        sw.Restart();
                        IACT_A6_V2.ReadPatInfo(out name, out sex, out idtype, out idno);
                        sw.Stop();
                        cardSeq = "00";
                        Logger.Main.Info($"姓名/{name}性别/{sex}证件类型/{idtype}证件号/{idno}卡序号/{cardSeq}");
                        Logger.Main.Info($"读卡，时间：{sw.ElapsedMilliseconds}");
                        break;

                    case IACT_A6_V2.ICardType.CPU:
                        seqNo = "";
                        var res = IACT_A6_V2.ReadCPUCardNo_SZ();
                        if (res == "")
                        {
                            cardNo = IACT_A6_V2.ReadCPUCardNo_SH();
                            cardSeq = "FF";
                            IACT_A6_V2.GetCPUPatInfo(IACT_A6_V2.ReadCPUPatInfo_SH(), out name, out sex, out idtype, out idno);
                        }
                        else
                        {
                            cardNo = res.Substring(28, 19);
                            cardSeq = res.Substring(64, 2);
                            IACT_A6_V2.GetCPUPatInfo(IACT_A6_V2.ReadCPUPatInfo_SZ(), out name, out sex, out idtype, out idno);
                        }
                        Logger.Main.Info("卡片卡号/" + cardNo);
                        Logger.Main.Info("姓名/" + name + "性别/" + sex + "证件类型/" + idtype + "证件号/" + idno);
                        // todo
                        cardSeq = cardSeq == "FF" ? "00" : cardSeq;
                        break;

                    default:
                        return Result<CardInfo>.Fail("未检测到卡片类型，请插入正确的卡片");
                }
                CardInfo = new CardInfo
                {
                    SeqNo = seqNo,
                    CardNo = cardNo,
                    Name = name,
                    Sex = sex,
                    IdType = idtype,
                    IdNo = idno,
                    CardSeq = cardSeq
                };

                ACT_A6_V2.MoveOut();
                return Result<CardInfo>.Success(CardInfo);
            }
            catch (Exception ex)
            {
                ACT_A6_V2.MoveOut();
                Logger.Main.Error(ex.Message + "\n" + ex.StackTrace);
                return Result<CardInfo>.Fail("卡片信息读取失败");
            }
        }

        public Result<CardInfo> ReadCardExt()
        {
            try
            {
                var data = new byte[] {};
                if (!Common.ReaderOpen())
                {
                    ReportService.读卡器离线(null, ErrorSolution.读卡器离线);
                    return Result<CardInfo>.Fail("读卡器离线");
                }

                Common.ReaderPosBeep(25);
                _running = true;
                while (_running)
                {
                    if (!M1.PICCReaderRequest())
                    {
                        Thread.Sleep(200);
                        continue;
                    }
                   
                    if (!M1.PICCReaderAnticoll(out data))
                    {
                        continue;
                    }
                    _running = false;
                }
                Logger.Device.Debug($"检测到有卡片，开始检测卡片类型");
                if (!CPU.PICCReaderSelect(cardType.TypeA))
                {
                    return Result<CardInfo>.Fail("读卡失败");
                }
                string seqNo, cardNo, name, sex, idtype, idno, cardSeq;
                if (!CPU.PICCReaderPowerOnTypeA())
                {
                    Logger.Device.Debug("检测到M1卡，开始读");
                    var keyA = HuaDaReaderHelper.GetKeyA(data);

                    seqNo = HuaDaReaderHelper.Bytes2String(data, data.Length);

                    Logger.Device.Info($"[卡片序列号]{seqNo}[keyA]{HuaDaReaderHelper.Bytes2String(keyA, keyA.Length)}");

                    if (!M1.PICCReaderAuthenticationPass(mode2.keyA, 1, keyA))
                    {
                        return Result<CardInfo>.Fail("读卡失败");
                    }

                    if (!M1.PICCReaderRead(4, out data))
                    {
                        return Result<CardInfo>.Fail("读卡失败");
                    }
                    cardNo = HuaDaReaderHelper.GetCardNo(data);
                    Logger.Device.Info($"[卡号]{cardNo}");

                    if (!M1.PICCReaderAuthenticationPass(mode2.keyA, 2, keyA))
                    {
                        return Result<CardInfo>.Fail("读卡失败");
                    }
                    if (!M1.PICCReaderRead(8, out data))
                    {
                        return Result<CardInfo>.Fail("读卡失败");
                    }
                    var chunk1 = data;
                    if (!M1.PICCReaderRead(9, out data))
                    {
                        return Result<CardInfo>.Fail("读卡失败");
                    }
                    var chunk2 = data;
                    if (!M1.PICCReaderRead(10, out data))
                    {
                        return Result<CardInfo>.Fail("读卡失败");
                    }
                    var chunk3 = data;

                    name = HuaDaReaderHelper.GetName(chunk1);
                    sex = HuaDaReaderHelper.GetSex(chunk1);
                    idtype = HuaDaReaderHelper.GetIdType(chunk2);
                    idno = HuaDaReaderHelper.GetIdNo(chunk2, chunk3);
                    cardSeq = "00";
                    Logger.Device.Info($"[姓名]{name}[性别]{sex}[证件类型]{idtype}[证件号]{idno}[卡序号]{cardSeq}");

                }
                else
                {
                    Logger.Device.Debug("检测到CPU卡，开始读");
                    seqNo = HuaDaReaderHelper.Bytes2String(data, data.Length);

                    var res = HuaDaReaderHelper.ReadCPUCardNo_SZ();
                    if (res == "")
                    {
                        cardNo = HuaDaReaderHelper.ReadCPUCardNo_SH();
                        cardSeq = "FF";
                        HuaDaReaderHelper.GetCPUPatInfo(HuaDaReaderHelper.ReadCPUPatInfo_SH(), out name, out sex, out idtype, out idno);
                    }
                    else
                    {
                        cardNo = res.Substring(28, 19);
                        cardSeq = res.Substring(64, 2);
                        HuaDaReaderHelper.GetCPUPatInfo(HuaDaReaderHelper.ReadCPUPatInfo_SZ(), out name, out sex, out idtype, out idno);
                    }

                }
                cardSeq = cardSeq == "FF" ? "00" : cardSeq;
                CardInfo = new CardInfo
                {
                    SeqNo = seqNo,
                    CardNo = cardNo,
                    Name = name,
                    Sex = sex,
                    IdType = idtype,
                    IdNo = idno,
                    CardSeq = cardSeq
                };

             
                return Result<CardInfo>.Success(CardInfo);
            }
            catch (Exception ex)
            {
                Logger.Main.Error(ex.Message + "\n" + ex.StackTrace);
                return Result<CardInfo>.Fail("卡片信息读取失败");
            }
        }

        public Result<CardInfo> ReadCardExt2()
        {
            try
            {
                byte[] data;
                if (!M1.PICCReaderAnticoll(out data))
                    {
                    return Result<CardInfo>.Fail("读卡失败");
                }
                  
               
                Logger.Device.Debug($"检测到有卡片，开始检测卡片类型");
                if (!CPU.PICCReaderSelect(cardType.TypeA))
                {
                    return Result<CardInfo>.Fail("读卡失败");
                }
                string seqNo, cardNo, name, sex, idtype, idno, cardSeq;
                if (!CPU.PICCReaderPowerOnTypeA())
                {
                    Logger.Device.Debug("检测到M1卡，开始读");
                    var keyA = HuaDaReaderHelper.GetKeyA(data);

                    seqNo = HuaDaReaderHelper.Bytes2String(data, data.Length);

                    Logger.Device.Info($"[卡片序列号]{seqNo}[keyA]{HuaDaReaderHelper.Bytes2String(keyA, keyA.Length)}");

                    if (!M1.PICCReaderAuthenticationPass(mode2.keyA, 1, keyA))
                    {
                        return Result<CardInfo>.Fail("读卡失败");
                    }

                    if (!M1.PICCReaderRead(4, out data))
                    {
                        return Result<CardInfo>.Fail("读卡失败");
                    }
                    cardNo = HuaDaReaderHelper.GetCardNo(data);
                    Logger.Device.Info($"[卡号]{cardNo}");

                    if (!M1.PICCReaderAuthenticationPass(mode2.keyA, 2, keyA))
                    {
                        return Result<CardInfo>.Fail("读卡失败");
                    }
                    if (!M1.PICCReaderRead(8, out data))
                    {
                        return Result<CardInfo>.Fail("读卡失败");
                    }
                    var chunk1 = data;
                    if (!M1.PICCReaderRead(9, out data))
                    {
                        return Result<CardInfo>.Fail("读卡失败");
                    }
                    var chunk2 = data;
                    if (!M1.PICCReaderRead(10, out data))
                    {
                        return Result<CardInfo>.Fail("读卡失败");
                    }
                    var chunk3 = data;

                    name = HuaDaReaderHelper.GetName(chunk1);
                    sex = HuaDaReaderHelper.GetSex(chunk1);
                    idtype = HuaDaReaderHelper.GetIdType(chunk2);
                    idno = HuaDaReaderHelper.GetIdNo(chunk2, chunk3);
                    cardSeq = "00";
                    Logger.Device.Info($"[姓名]{name}[性别]{sex}[证件类型]{idtype}[证件号]{idno}[卡序号]{cardSeq}");

                }
                else
                {
                    Logger.Device.Debug("检测到CPU卡，开始读");
                    seqNo = HuaDaReaderHelper.Bytes2String(data, data.Length);

                    var res = HuaDaReaderHelper.ReadCPUCardNo_SZ();
                    if (res == "")
                    {
                        cardNo = HuaDaReaderHelper.ReadCPUCardNo_SH();
                        cardSeq = "FF";
                        HuaDaReaderHelper.GetCPUPatInfo(HuaDaReaderHelper.ReadCPUPatInfo_SH(), out name, out sex, out idtype, out idno);
                    }
                    else
                    {
                        cardNo = res.Substring(28, 19);
                        cardSeq = res.Substring(64, 2);
                        HuaDaReaderHelper.GetCPUPatInfo(HuaDaReaderHelper.ReadCPUPatInfo_SZ(), out name, out sex, out idtype, out idno);
                    }

                }
                cardSeq = cardSeq == "FF" ? "00" : cardSeq;
                CardInfo = new CardInfo
                {
                    SeqNo = seqNo,
                    CardNo = cardNo,
                    Name = name,
                    Sex = sex,
                    IdType = idtype,
                    IdNo = idno,
                    CardSeq = cardSeq
                };


                return Result<CardInfo>.Success(CardInfo);
            }
            catch (Exception ex)
            {
                Logger.Main.Error(ex.Message + "\n" + ex.StackTrace);
                return Result<CardInfo>.Fail("卡片信息读取失败");
            }
        }
        public string ServiceName { get; }
    }

    public class CardInfo
    {
        public string CardNo { get; set; }

        public string SeqNo { get; set; }
        public string Name { get; set; }
        public string Sex { get; set; }
        public string IdType { get; set; }
        public string IdNo { get; set; }
        public string CardSeq { get; set; }
    }
}