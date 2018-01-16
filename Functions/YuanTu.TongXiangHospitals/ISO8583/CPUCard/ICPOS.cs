using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;

using YuanTu.TongXiangHospitals.ISO8583.External;


namespace YuanTu.TongXiangHospitals.ISO8583.CPUCard
{
    public class ICPOS : IDisposable
    {
        public ICPOS(ICardDevice wrapper)
        {
            this.wrapper = wrapper;
        }

        private readonly ICardDevice wrapper;


        private AIP aip;

        private ICCard icCard;

        private string sdaData = string.Empty;
        private TSI tsi = 0;
        private TVR0 tvr0 = 0;
        private TVR1 tvr1 = 0;
        private TVR2 tvr2 = 0;
        private TVR3 tvr3 = 0;
        private TVR4 tvr4 = 0;

        public static Dictionary<string, Dictionary<int, byte[]>> PublicKeys { get; set; }

        public static Dictionary<string, Dictionary<int, byte[]>> Params { get; set; }

        private Dictionary<int, byte[]> publicKey;
        private Dictionary<int, byte[]> param;

        /// <summary>
        ///     卡内数据
        /// </summary>
        private Dictionary<int, CPUTlv> cardDic { get; set; }

        /// <summary>
        ///     设备数据
        /// </summary>
        private Dictionary<int, byte[]> devDic { get; set; } = new Dictionary<int, byte[]>();

        public static LoggerWrapper Log { get; set; } = Logger.POS;

        public void Dispose()
        {
            wrapper?.Uninit();
        }

        public bool Init()
        {
            var ret = wrapper.Init(3, 9600);
            if (!ret)
                ret = wrapper.Init(4, 9600);
            Log.Debug($"Init = {ret}");
            if (!ret)
                return false;
            ret = wrapper.EnterCard(true);
            var pos = CardPos.无卡;
            while (pos != CardPos.停卡位 && pos != CardPos.IC位)
            {
                Thread.Sleep(500);
                ret = wrapper.CheckCard(out pos);
                Log.Debug($"CheckCard = {pos}");
            }
            if (pos == CardPos.停卡位)
            {
                ret = wrapper.MoveCard();
                Log.Debug($"MoveCard = {ret}");
            }
            byte[] apdu;
            ret = wrapper.CPUCodeReset(out apdu);
            Log.Debug($"CPUCodeReset = {ret} {apdu.Bytes2Hex()}");

            return true;
        }

        public bool Uninit()
        {
            wrapper.EjectCard();
            return true;
        }

        public bool ReadCard(Input input)
        {
            icCard = new ICCard(wrapper);
            LoadDevice(input.Now, input.Amount);

            应用选择();

            // 读取参数
            var aid = devDic[0x84].Bytes2Hex();
            param = Params.FirstOrDefault(p => aid.StartsWith(p.Key)).Value; //Params[aid];            
            foreach (var kvp in param)
                devDic[kvp.Key] = kvp.Value;

            应用初始化();

            // 读取CA
            var id = aid.Substring(0,10) + "|" + cardDic[0x8F].Value.Bytes2Hex();
            publicKey = PublicKeys[id];

            input.BankNo = cardDic[0x5A].Value.Bytes2Hex().TrimEnd('F');
            input.Track2 = cardDic[0x57].Value.Bytes2Hex().TrimEnd('F').Replace('D', '=');

            devDic[0x5F34] = cardDic[0x5F34].Value;
            input.CardSNum = cardDic[0x5F34].Value[0];
            return true;
        }

        public List<CPUTlv> FirstHalf()
        {
            脱机数据认证();
            处理限制();
            持卡人验证();
            终端风险管理();
            终端行为分析();
            卡片行为分析();
            devDic[0x9B][0] = (byte) tsi;
            var list = MakeList(saleList);
            return list;
        }

        public bool SecondHalf(Output output)
        {
            try
            {
                var icDic = output.MessageBody.ICPackages;
                var eaSuccess = 联机处理(icDic);
                var notify1 = 发卡行脚本处理1(icDic);
                var success = 交易结束(eaSuccess);
                var notify2 = 发卡行脚本处理2(icDic);
                devDic[0x9B][0] = (byte) tsi;
                output.Notify = notify1 || notify2;
                MakePrintList(output);
                return success;
            }
            finally
            {
                Log.Debug($"{tvr0}\n{tvr1}\n{tvr2}\n{tvr3}\n{tvr4}\n");
            }
        }

        public bool OnlineFail()
        {
            try
            {
                return 交易结束(false);
            }
            finally
            {
                Log.Debug($"{tvr0}\n{tvr1}\n{tvr2}\n{tvr3}\n{tvr4}\n");
            }
        }

        #region Steps

        private void 应用选择()
        {
            // 选择PSE
            var pseRes = icCard.Select(Encoding.Default.GetBytes("1PAY.SYS.DDF01"));
            // 建立候选应用列表
            var sf1 = pseRes.Value[0x88].Value[0];
            var list = new List<Dictionary<int, CPUTlv>>();
            byte n = 1;
            while (true)
            {
                var readRes = icCard.ReadRecord(sf1, n++);
                if (readRes.IsSuccess)
                    list.Add(readRes.Value);
                else
                    break;
            }
            // 标识并选出应用
            list.OrderBy(one => one[0x87].Value[0]);
            for (var i = 0; i < list.Count; i++)
            {
                var tlv = list[i];
                var aid = tlv[0x4F].Value.Bytes2Hex();
                var name = Encoding.Default.GetString(tlv[0x50].Value);
                var p = tlv[0x87].Value.Bytes2Hex();
                Log.Debug($"[{i}]\nAID:\t{aid}\n标签\t{name}\n优先权\t{p}");
            }

            //终端决定应用
            var AID = list[0];

            // AID
            devDic[0x84] = AID[0x4F].Value;
            // App Label
            devDic[0x50] = AID[0x50].Value;
        }

        private void 应用初始化()
        {
            // 选择AID
            var aidRes = icCard.Select(devDic[0x84]);
            var dicAID = aidRes.Value;

            // 卡片中提供PDOL
            byte[] _pdol;
            if (dicAID.ContainsKey(0x9F38))
            {
                var tagList = ParseDOL(dicAID[0x9F38].Value);
                var pdolData = MakeDOLData(tagList);
                _pdol = new byte[pdolData.Length + 2];
                _pdol[0] = 0x83;
                _pdol[1] = (byte) pdolData.Length;
                Array.Copy(pdolData, 0, _pdol, 2, pdolData.Length);
            }
            else
            {
                _pdol = new byte[] {0x83, 0x00};
            }
            var gpoRes = icCard.GetProcessOptions(_pdol);
            byte[] aip;
            byte[] afl;
            if (gpoRes.Value.ContainsKey(0x80))
            {
                var gpoData = gpoRes.Value[0x80].Value;
                aip = new[] {gpoData[0], gpoData[1]};
                afl = new byte[gpoData.Length - 2];
                Array.Copy(gpoData, 2, afl, 0, afl.Length);
            }
            else
            {
                aip = gpoRes.Value[0x82].Value;
                afl = gpoRes.Value[0x94].Value;
            }
            this.aip = (AIP) aip[0];
            Log.Debug("AIP0:" + this.aip);
            Log.Debug("AIP:" + aip.Bytes2Hex());
            // AIP
            devDic[0x82] = aip;

            // 读应用数据
            cardDic = new Dictionary<int, CPUTlv>();
            for (var i = 0; i < afl.Length/4; i++)
            {
                var sf1 = (byte) (afl[i*4] >> 3);
                int first = afl[i*4 + 1];
                int last = afl[i*4 + 2];
                int offlineCount = afl[i*4 + 3];
                Log.Debug($"[AFL {sf1}]{first} {last} {offlineCount}");
                for (var j = first; j <= last; j++)
                {
                    //Thread.Sleep(500);
                    icCard.ReadRecord(sf1, (byte) j, cardDic);
                    if (j > offlineCount)
                        continue;
                    var data = icCard.LastResult;
                    int pos = 0;
                    var tag = CPUDecoder.ParseTag(ref pos, ref data);
                    var len = CPUDecoder.ParseLength(ref pos, ref data);
                    sdaData += data.Bytes2Hex().Substring(pos*2);
                }
            }
            PrintDic(cardDic);
        }

        private void 脱机数据认证()
        {
            tsi |= TSI.脱机数据认证已进行;

            var aipCDA = false; //(aip & AIP.CDA) != 0;
            var aipDDA = (aip & AIP.DDA) != 0;
            var aipSDA = (aip & AIP.SDA) != 0;
            if (aipCDA)
            {
                var res = CDA();
                if (res.IsSuccess)
                    return;
                tvr0 |= TVR0.复合动态数据认证应用密码生成失败;
                Log.Warn("CDA Failed:" + res.Message);
            }
            else if (aipDDA)
            {
                var res = DDA();
                if (res.IsSuccess)
                    return;
                tvr0 |= TVR0.脱机动态数据认证失败;
                Log.Warn("DDA Failed:" + res.Message);
            }
            else if (aipSDA)
            {
                var res = SDA();
                if (res.IsSuccess)
                    return;
                tvr0 |= TVR0.脱机静态数据认证失败;
                Log.Warn("SDA Failed:" + res.Message);
            }
        }

        private Result<string> GetIssuerKey()
        {
            var iHasRemain = cardDic.ContainsKey(0x92);
            var iKeyRemain = iHasRemain ? cardDic[0x92].Value.Bytes2Hex() : string.Empty;

            var caKey = publicKey[0xDF02].Bytes2Hex();
            var caEx = publicKey[0xDF04].Bytes2Hex();

            var iDataEncrypted = cardDic[0x90].Value.Bytes2Hex();
            var iEx = cardDic[0x9F32].Value.Bytes2Hex();

            if (iDataEncrypted.Length != caKey.Length)
                return Result<string>.Fail("发卡行证书长度错误");

            var iData = RSA.Decode(iDataEncrypted, caEx, caKey);

            if (!iData.EndsWith("BC"))
                return Result<string>.Fail("发卡行数据尾部错误");

            if (!iData.StartsWith("6A02"))
                return Result<string>.Fail("发卡行数据头部错误");
            
            var i2Hash = iData.Substring(2, iData.Length - 44) + iKeyRemain + iEx;
            var iHash = RSA.Hash(i2Hash);
            if (string.CompareOrdinal(iData, iData.Length - 42, iHash, 0, 40) != 0)
                return Result<string>.Fail("发卡行数据Hash验证失败");
            
            var pan = cardDic[0x5A].Value.Bytes2Hex();
            var certPan = iData.Substring(4, 8).TrimEnd('F');
            if (!pan.StartsWith(certPan))
                return Result<string>.Fail("发卡行数据PAN验证失败");

            var year = iData.Substring(14, 2);
            var month = iData.Substring(12, 2);
            if (string.CompareOrdinal(DateTime.Today.ToString("yyMM"), year + month) > 0)
                return Result<string>.Fail("发卡行公钥过期");

            // 发卡行公钥
            var iKey = iData.Substring(30,iData.Length - 72) + iKeyRemain;
            int len = iKey.Length;
            while (iKey[len - 1] == 'B' && iKey[len - 2] == 'B')
                len -= 2;
            return Result<string>.Success(iKey.Substring(0, len));
        }

        private Result<string> GetCardKey(string iKey)
        {
            var iEx = cardDic[0x9F32].Value.Bytes2Hex();
            
            var cHasRemain = cardDic.ContainsKey(0x9F48);
            var cKeyRemain = cHasRemain ? cardDic[0x9F48].Value.Bytes2Hex() : string.Empty;
            var cEx = cardDic[0x9F47].Value.Bytes2Hex();

            var cDataEncrypted = cardDic[0x9F46].Value.Bytes2Hex();

            if (cDataEncrypted.Length != iKey.Length)
                return Result<string>.Fail("卡数据长度错误");

            var cData = RSA.Decode(cDataEncrypted, iEx, iKey);

            if (!cData.EndsWith("BC"))
                return Result<string>.Fail("卡数据尾部错误");

            if (!cData.StartsWith("6A04"))
                return Result<string>.Fail("卡数据头部错误");
            
            var c2Hash = new StringBuilder();
            c2Hash.Append(cData.Substring(2, cData.Length - 44));
            c2Hash.Append(cKeyRemain);
            c2Hash.Append(cEx);
            c2Hash.Append(sdaData);
            c2Hash.Append(devDic[0x82].Bytes2Hex());

            var cHash = RSA.Hash(c2Hash.ToString());
            if (string.CompareOrdinal(cData, cData.Length - 42, cHash, 0, 40) != 0)
                return Result<string>.Fail("卡数据Hash验证失败");

            var pan = cardDic[0x5A].Value.Bytes2Hex().TrimEnd('F');
            var certPan = cData.Substring(4, 20).TrimEnd('F');
            if (string.CompareOrdinal(pan, certPan) != 0)
                return Result<string>.Fail("发卡行数据PAN验证失败");
            
            var year = cData.Substring(26, 2);
            var month = cData.Substring(24, 2);
            if (string.CompareOrdinal(DateTime.Today.ToString("yyMM"), year + month) > 0)
                return Result<string>.Fail("发卡行公钥过期");

            var cKey = cData.Substring(42, cData.Length - 84) + cKeyRemain;
            int len = cKey.Length;
            while (cKey[len - 1] == 'B' && cKey[len - 2] == 'B')
                len -= 2;
            return Result<string>.Success(cKey.Substring(0, len));
        }

        private Result SDA()
        {
            var getiKeyRes = GetIssuerKey();
            if (!getiKeyRes.IsSuccess)
                return Result.Fail("获取发卡行公钥失败:"+getiKeyRes.Message);

            var iKey = getiKeyRes.Value;

            var iEx = cardDic[0x9F32].Value.Bytes2Hex();
            var cDataEncrypted = cardDic[0x93].Value.Bytes2Hex();

            if (cDataEncrypted.Length != iKey.Length)
                return Result.Fail("卡数据长度错误");

            var cData = RSA.Decode(cDataEncrypted, iEx, iKey);

            if (!cData.EndsWith("BC"))
                return Result.Fail("卡数据尾部错误");

            if (!cData.StartsWith("6A03"))
                return Result.Fail("卡数据头部错误");

            var c2Hash = new StringBuilder();
            c2Hash.Append(cData.Substring(2, cData.Length - 44));
            c2Hash.Append(sdaData);
            c2Hash.Append(devDic[0x82].Bytes2Hex());

            var cHash = RSA.Hash(c2Hash.ToString());
            if (string.CompareOrdinal(cData, cData.Length - 42, cHash, 0, 40) != 0)
                return Result.Fail("卡数据Hash验证失败");

            devDic[0x9F45] = cData.Substring(6, 4).Hex2Bytes();

            return Result.Success();
        }

        private Result DDA()
        {
            var getiKeyRes = GetIssuerKey();
            if (!getiKeyRes.IsSuccess)
                return Result.Fail("获取发卡行公钥失败:"+getiKeyRes.Message);

            var iKey = getiKeyRes.Value;

            var getcKeyRes = GetCardKey(iKey);
            if(!getcKeyRes.IsSuccess)
                return Result.Fail("获取卡公钥失败:"+getcKeyRes.Message);

            var cKey = getcKeyRes.Value;
            var cEx = cardDic[0x9F47].Value.Bytes2Hex();

            var ddol = cardDic.ContainsKey(0x9F49) ? cardDic[0x9F49].Value : devDic[0xDF14];

            var tagList = ParseDOL(ddol);
            if (tagList.FirstOrDefault(one => one.Item1 == 0x9F37) == null)
                return Result.Fail("DDOL中不包含不可预知数的标签");
            var ddolData = MakeDOLData(tagList);

            var inAuthRes = icCard.InternalAuth(ddolData);
            if (!inAuthRes.IsSuccess)
                return Result.Fail("内部认证失败:" + inAuthRes.Message);

            var cDataEncrypted = inAuthRes.Value[0x80].Value.Bytes2Hex();

            
            if (cDataEncrypted.Length != cKey.Length)
                return Result.Fail("卡数据长度错误");

            var cData = RSA.Decode(cDataEncrypted, cEx, cKey);
            
            if (!cData.EndsWith("BC"))
                return Result.Fail("卡数据尾部错误");

            if (!cData.StartsWith("6A05"))
                return Result.Fail("卡数据头部错误");
            
            var c2Hash = new StringBuilder();
            c2Hash.Append(cData.Substring(2, cData.Length - 44));
            c2Hash.Append(ddolData.Bytes2Hex());

            var cHash = RSA.Hash(c2Hash.ToString());
            if (string.CompareOrdinal(cData, cData.Length - 42, cHash, 0, 40) != 0)
                return Result.Fail("卡数据Hash验证失败");
            int len = 0;
            while (true)
            {
                if (cData[8 + len] == 'B' && cData[8 + len] == 'B')
                    len += 2;
                else
                    break;
            }
            devDic[0x9F4C] = cData.Substring(8, len).Hex2Bytes();

            return Result.Success();
        }

        private Result CDA()
        {
            var getiKeyRes = GetIssuerKey();
            if (!getiKeyRes.IsSuccess)
                return Result.Fail("获取发卡行公钥失败:"+getiKeyRes.Message);

            var iKey = getiKeyRes.Value;

            var getcKeyRes = GetCardKey(iKey);
            if(!getcKeyRes.IsSuccess)
                return Result.Fail("获取卡公钥失败:"+getcKeyRes.Message);

            var cKey = getcKeyRes.Value;
            var cEx = cardDic[0x9F47].Value.Bytes2Hex();

            var ddol = cardDic.ContainsKey(0x9F49) ? cardDic[0x9F49].Value : devDic[0xDF14];

            var tagList = ParseDOL(ddol);
            if (tagList.FirstOrDefault(one => one.Item1 == 0x9F37) == null)
                return Result.Fail("DDOL中不包含不可预知数的标签");
            var ddolData = MakeDOLData(tagList);
            // TODO CDA
            return Result.Fail("");
        }

        private void 处理限制()
        {
            // 应用版本号检查
            var version = cardDic[0x9F08].Value.Bytes2Hex();
            var devVersion = devDic[0x9F08].Bytes2Hex();
            if(string.CompareOrdinal(version, devVersion) != 0)
                tvr1 |= TVR1.IC卡和终端应用版本不一致;

            // 应用用途控制检查
            var country = cardDic[0x5F28].Value.Bytes2Hex();
            if (country != "0156")
                Log.Debug("非国内卡:" + country);
            var auc = cardDic[0x9F07].Value[0];
            Log.Debug("[应用用途控制检查]" + (AUC) auc);
            if ((auc & 0x89) != 0x89)
            {
                Log.Debug("应用用途控制检查失败");
            }
            var today = Convert.ToInt32(DateTime.Today.ToString("yyMMdd"));

            //tvr1 |= TVR1.卡片不允许所请求的服务;

            // 应用生效期检查
            var beginTime = cardDic[0x5F25].Value.Bytes2Hex();
            if (Convert.ToInt32(beginTime) > today)
            {
                tvr1 |= TVR1.应用尚未生效;
                Log.Debug("应用生效期检查失败");
            }
            //应用失效期检查
            var endTime = cardDic[0x5F24].Value.Bytes2Hex();
            if (Convert.ToInt32(endTime) < today)
            {
                tvr1 |= TVR1.应用已过期;
                Log.Debug("应用失效期检查失败");
            }
        }

        private void 持卡人验证()
        {
            tsi |= TSI.持卡人验证已进行;
            var pass = false;
            var CVMR = new byte[] {0x00, 0x00, 0x01};
            var CVM = cardDic[0x8E].Value;

            var x = 0;
            var y = 0;

            for (var i = 0; i < 4; i++)
                x = x*0x100 + CVM[i];
            for (var i = 0; i < 4; i++)
                y = y*0x100 + CVM[i + 4];

            for (var i = 8; i < CVM.Length; i += 2)
            {
                var pboc = (CVM[i] & 0x80) != 0x80;
                if (!pboc)
                    continue;
                var type = CVM[i] & 0x3F;
                if (type == 0x02) // 输入联机PIN
                {
                    if (CVM[i + 1] == 0x03) // 如果终端支持
                    {
                        tvr2 |= TVR2.输入联机PIN;
                        CVMR[0] = CVM[i];
                        CVMR[1] = CVM[i + 1];
                        pass = true;
                    }
                    break;
                }
                if (type == 0x1F)
                {
                    CVMR[0] = CVM[i];
                    CVMR[1] = CVM[i + 1];
                    pass = true;
                    break;
                }
                var fallback = (CVM[i] & 0x40) == 0x40;
                if (!fallback)
                    break;
            }
            if (!pass)
                tvr2 |= TVR2.持卡人验证失败;
            else
                CVMR[2] = 0x00;

            Log.Debug("持卡人验证:" + pass);
            devDic[0x9F34] = CVMR;
        }

        private void 终端风险管理()
        {
            tsi |= TSI.终端风险管理已进行;
            // TODO 终端异常文件检查

            // 商户强制交易联机
            tvr3 |= TVR3.商户要求联机交易;

            // 最低限额检查
            tvr3 |= TVR3.交易超过最低限额;

            var regRes = icCard.GetData(new byte[] {0x9F, 0x13}, cardDic);
            var atcRes = icCard.GetData(new byte[] {0x9F, 0x36}, cardDic);

            var reg = cardDic[0x9F13].Value;
            var atc = cardDic[0x9F36].Value;
            if (atc[0] == 0x00 & atc[1] == 0x00)
            {
                tvr1 |= TVR1.新卡;
                Log.Debug("New Card");
            }
        }

        private void 终端行为分析()
        {
            devDic[0x95] = new[]
            {
                (byte) tvr0, (byte) tvr1, (byte) tvr2, (byte) tvr3, (byte) tvr4
            };
            // TODO TAC IAC 比较

            var iacDefault = cardDic.ContainsKey(0x9F0D) ? cardDic[0x9F0D].Value : new byte[5];
            var iacDeny = cardDic.ContainsKey(0x9F0E) ? cardDic[0x9F0E].Value : new byte[5];
            var iacOnline = cardDic.ContainsKey(0x9F0F) ? cardDic[0x9F0F].Value : new byte[5];

            var tacDefault = devDic.ContainsKey(0xDF11) ? devDic[0xDF11] : new byte[5];
            var tacDeny = devDic.ContainsKey(0xDF13) ? devDic[0xDF11] : new byte[5];
            var tacOnline = devDic.ContainsKey(0xDF12) ? devDic[0xDF11] : new byte[5];

            tsi |= TSI.卡片风险管理已进行;
            var tagList = ParseDOL(cardDic[0x8C].Value);
            var cdol1Data = MakeDOLData(tagList);
            var gacRes = icCard.GenerateAC(ACType.ARQC, false, cdol1Data);
            if (!gacRes.IsSuccess)
            {
                Log.Debug("GAC1 Failed!");
            }
            var dic = gacRes.Value;
            var data = dic[0x80].Value;

            var CID = new[] {data[0]};
            devDic[0x9F27] = CID;
            var ATC = new[] {data[1], data[2]};
            devDic[0x9F36] = ATC;
            var AC = new byte[8];
            Array.Copy(data, 3, AC, 0, 8);
            devDic[0x9F26] = AC;
            var IAD = new byte[8];
            Array.Copy(data, 11, IAD, 0, 8);
            devDic[0x9F10] = IAD;
        }

        private void 卡片行为分析()
        {
        }

        private bool 联机处理(Dictionary<int, CPUTlv> icDic)
        {
            var aipAuth = (aip & AIP.发卡行认证) != 0;
            if (!aipAuth || !icDic.ContainsKey(0x91))
                return true;
            tsi |= TSI.发卡行认证已进行;
            var auth = icDic[0x91].Value;
            var eaRes = icCard.ExternalAuth(auth);
            if (!eaRes.IsSuccess)
            {
                tvr4 |= TVR4.发卡行认证失败;
                Log.Debug("ExternalAuth Failed!");
            }
            return eaRes.IsSuccess;
        }

        private bool 发卡行脚本处理1(Dictionary<int, CPUTlv> icDic)
        {
            if (!icDic.ContainsKey(0x71))
                return false;
            Log.Warn("发卡行脚本处理 出现 Tag为71的脚本");
            tvr4 |= TVR4.最后一次GENERATE_AC命令之前脚本处理失败;
            return true;
        }

        private bool 发卡行脚本处理2(Dictionary<int, CPUTlv> icDic)
        {
            if (!icDic.ContainsKey(0x72))
                return false;
            tsi |= TSI.脚本处理已进行;
            var scriptRes = new byte[] {0x20, 0x00, 0x00, 0x00, 0x00};

            var scripts = icDic[0x72].InnerTlvs.Where(one => one.Tag == 0x86).Select(one => one.Value).ToList();
            for (var i = 0; i < scripts.Count; i++)
            {
                var res = icCard.Run(scripts[i]);
                if (res.IsSuccess)
                    continue;
                if (res.Message.StartsWith("62") || res.Message.StartsWith("63"))
                    continue;
                tvr4 |= TVR4.最后一次GENERATE_AC命令之后脚本处理失败;
                scriptRes[0] = (byte) (0x10 + i + 1);
                break;
            }
            devDic[0xDF31] = scriptRes;
            return true;
        }

        private bool 交易结束(bool eaSuccess)
        {
            devDic[0x95] = new[]
            {
                (byte) tvr0, (byte) tvr1, (byte) tvr2, (byte) tvr3, (byte) tvr4
            };
            var tagList = ParseDOL(cardDic[0x8D].Value);
            var cdol2Data = MakeDOLData(tagList);
            var gacRes = icCard.GenerateAC(eaSuccess ? ACType.TC : ACType.AAC, false, cdol2Data);
            if (!gacRes.IsSuccess)
            {
                Log.Warn("GAC2 Failed!");
                return false;
            }
            var dic = gacRes.Value;
            var data = dic[0x80].Value;

            var CID = new[] {data[0]};
            devDic[0x9F27] = CID;
            var ATC = new[] {data[1], data[2]};
            devDic[0x9F36] = ATC;
            var AC = new byte[8];
            Array.Copy(data, 3, AC, 0, 8);
            devDic[0x9F26] = AC;
            var IAD = new byte[8];
            Array.Copy(data, 11, IAD, 0, 8);
            devDic[0x9F10] = IAD;

            if (CID[0] >> 6 == 0)
            {
                Log.Warn("GAC2 Failed! AAC Received");
                return false;
            }
            return true;
        }

        #endregion

        #region Lists

        private readonly int[] saleList =
        {
            0x9F26,
            0x9F27,
            0x9F10,
            0x9F37,
            0x9F36,
            0x95,
            0x9A,
            0x9C,
            0x9F02,
            0x5F2A,
            0x82,
            0x9F1A,
            0x9F03,
            0x9F33,
            0x9F34,
            0x9F35,
        };

        private readonly int[] notifyList =
        {
            0x9F33,
            0x95,
            0x9F37,
            //0x9F1E,
            0x9F10,
            0x9F26,
            0x9F36,
            0x82,
            0xDF31, //发卡行脚本结果
            0x9F1A,
            0x9A
        };

        private readonly int[] printList =
        {
            0x9F26, // TC
            0x95,   // TVR
            0x5F34, // CSN
            0x84,   // AID
            0x9F36, // ATC
            0x9B,   // TSI
            0x50,   // APP LABEL
            0x9F37, // UNPR NUM
            0x82,   // AIP
            0x9F34, // CVMR
            0x9F10, // IAD
            0x9F33, // Term Capa
        };

        public List<CPUTlv> MakeNotifyList()
        {
            var list = MakeList(notifyList);
            return list;
        }

        public List<CPUTlv> MakeUploadList()
        {
            var list = MakeList(saleList);
            return list;
        }

        public void MakePrintList(Output output)
        {
            output.TC = devDic[0x9F26].Bytes2Hex();
            output.TVR = devDic[0x95].Bytes2Hex();
            output.CSN = devDic[0x5F34].Bytes2Hex();
            output.AID = devDic[0x84].Bytes2Hex();
            output.ATC = devDic[0x9F36].Bytes2Hex();
            output.TSI = devDic[0x9B].Bytes2Hex();
            output.APP_LABEL = Encoding.Default.GetString(devDic[0x50]);
            output.UNPR_NUM = devDic[0x9F37].Bytes2Hex();
            output.AIP = devDic[0x82].Bytes2Hex();
            output.CVMR = devDic[0x9F34].Bytes2Hex();
            output.IAD = devDic[0x9F10].Bytes2Hex();
            output.Term_Capa = devDic[0x9F33].Bytes2Hex();
        }

        #endregion Lists

        #region Tools

        public byte[] Int2BCD(int value, int len)
        {
            var bytes = new byte[len];
            for (var i = 0; i < len; i++)
            {
                var r = value%100;
                bytes[i] = (byte) (r/10*0x10 + r%10);
                value = value/100;
                if (value == 0)
                    break;
            }
            Array.Reverse(bytes);
            return bytes;
        }

        public void LoadDevice(DateTime now, int amount)
        {
            devDic[0x9F02] = Int2BCD(amount, 6); //授权金额（数值型）
            devDic[0x9F03] = new byte[6]; //其它金额（数值型）

            devDic[0x9A  ] = Int2BCD(Convert.ToInt32(now.ToString("yyMMdd")), 3);
            devDic[0x9F21] = Int2BCD(Convert.ToInt32(now.ToString("HHmmss")), 3);

            devDic[0x9F1A] = new byte[] {0x01, 0x56}; //终端国家代码
            devDic[0x5F2A] = new byte[] {0x01, 0x56}; //交易货币代码

            devDic[0x9C  ] = new byte[] {0x00}; //交易类型
            devDic[0x9F7A] = new byte[] {0x00}; //电子现金终端指示器
            
            devDic[0x9F35] = new byte[] {0x24}; //终端类型

            devDic[0x9F4E] = new byte[20]; //商户名称
            devDic[0x8A  ] = new byte[] {0x30, 0x30}; //授权响应代码
            devDic[0x9B  ] = new byte[1];
            
            var r = new byte[4];
            new Random(now.Millisecond).NextBytes(r);
            devDic[0x9F37] = r; //不可预知数

            var devCap0 = DevCap0.手工键盘输入 | DevCap0.接触式IC卡 | DevCap0.磁条;
            var devCap1 = DevCap1.加密PIN联机验证 | DevCap1.无需CVM;
            // TODO CDA
            var devCap2 = DevCap2.静态数据认证_SDA | DevCap2.动态数据认证_DDA; // | DevCap2.复合动态数据认证_应用密码生成_CDA;
            devDic[0x9F33] = new byte[]
            {
                (byte)devCap0,
                (byte)devCap1,
                (byte)devCap2,
            }; //终端性能
        }

        public void PrintDic(Dictionary<int, CPUTlv> dic)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            foreach (var kvp in dic)
            {
                var package = kvp.Value;

                var name = string.Empty;
                var tag = package.Tag;
                if (CPUDecoder.TagNames.ContainsKey(tag))
                    name = CPUDecoder.TagNames[tag];

                sb.AppendLine(
                    $"\t[{tag.ToString("X").PadRight(4)}]=[{package.Length.ToString().PadLeft(4)}][{(package.Constructed ? "+" : package.Value.Bytes2Hex())}][{name}]");
            }
            Log.Debug(sb.ToString());
        }

        public List<Tuple<int, int>> ParseDOL(byte[] dol)
        {
            var sb = new StringBuilder();
            var data = dol;
            var pos = 0;
            var tagList = new List<Tuple<int, int>>();
            while (pos < data.Length)
            {
                var tag = CPUDecoder.ParseTag(ref pos, ref data);
                var len = CPUDecoder.ParseLength(ref pos, ref data);
                tagList.Add(new Tuple<int, int>(tag, len));
                var name = string.Empty;
                if (ICCard.TagNames.ContainsKey(tag))
                    name = ICCard.TagNames[tag];
                sb.AppendLine($"\t[{tag.ToString("X").PadRight(4)}]=[{len.ToString().PadLeft(4)}][{name}]");
            }
            Log.Debug(sb.ToString());
            return tagList;
        }

        public byte[] MakeDOLData(List<Tuple<int, int>> list)
        {
            var len = list.Sum(one => one.Item2);
            var bytes = new byte[len];
            var pos = 0;
            for (var i = 0; i < list.Count; i++)
            {
                var dolTag = list[i].Item1;
                var dolLen = list[i].Item2;
                var value = devDic[dolTag];
                if (dolLen != value.Length)
                    Log.Debug($"!!! [Tag]{dolTag:X} [Length]{dolLen} != [Value.Length]{value.Length}");
                Array.Copy(value, 0, bytes, pos, value.Length);
                pos += dolLen;
            }

            return bytes;
        }

        public bool CompareTVR2IAC(byte[] tvr, byte[] iac)
        {
            for (var i = 0; i < tvr.Length; i++)
            {
                if ((tvr[i] & iac[i]) == iac[i])
                    return false;
            }
            return true;
        }

        public List<CPUTlv> MakeList(int[] indexes)
        {
            return indexes.Select(i => new CPUTlv
            {
                Tag = i,
                Value = devDic[i]
            }).ToList();
        }

        #endregion Tools
    }
}