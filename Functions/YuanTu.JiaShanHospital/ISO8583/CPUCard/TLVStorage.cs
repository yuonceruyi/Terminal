﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.JiaShanHospital.ISO8583.CPUCard
{
    public class TLVStorage
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public static void Save(string fileName, List<TLVStorage> list)
        {
            Logger.Main.Info($"[农行参数地址保存]{fileName}");
            using (var sw = new StreamWriter(fileName))
                sw.Write(list.ToJsonString());
        }

        public static List<TLVStorage> Load(string fileName)
        {
            Logger.Main.Info($"[农行参数地址读取]{fileName}");

            using (var sr = new StreamReader(fileName))
                return sr.ReadToEnd().ToJsonObject<List<TLVStorage>>();

        }

        public static Dictionary<string, Dictionary<int, byte[]>> LoadTLVDics(List<TLVStorage> list)
        {
            var tlvDics = new Dictionary<string, Dictionary<int, byte[]>>();
            foreach (var storage in list)
            {
                var data = storage.Value.Hex2Bytes();
                var pos = 0;
                var dic = new Dictionary<int, byte[]>();
                while (pos < data.Length)
                {
                    var tag = CPUDecoder.ParseTag(ref pos, ref data);
                    var len = CPUDecoder.ParseLength(ref pos, ref data);
                    var bytes = new byte[len];
                    Array.Copy(data, pos, bytes, 0, len);
                    pos += len;
                    dic[tag] = bytes;
                }
                tlvDics[storage.Name] = dic;
            }
            return tlvDics;
        }
    }
}