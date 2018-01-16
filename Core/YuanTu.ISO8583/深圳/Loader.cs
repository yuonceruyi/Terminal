using System.Collections.Generic;
using System.Linq;
using Autofac;
using YuanTu.ISO8583.CPUCard;
using YuanTu.ISO8583.Data;
using YuanTu.ISO8583.Enums;
using YuanTu.ISO8583.Interfaces;
using YuanTu.ISO8583.IO;

namespace YuanTu.ISO8583.深圳
{
    public class Loader : ILoader
    {
        public string ServiceName => "";

        public IManager Initialize(IConfig config)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(config).As<IConfig>();

            builder.RegisterInstance(BuildConfig()).As<IBuildConfig>();

            var containerWrapper = new ContainerWrapper();
            builder.RegisterInstance(containerWrapper).As<IContainerWrapper>();

            builder.RegisterType<Connection>().PropertiesAutowired().As<IConnection>();
            builder.RegisterType<Decoder>().PropertiesAutowired().As<IDecoder>();
            builder.RegisterType<Encoder>().PropertiesAutowired().As<IEncoder>();
            builder.RegisterType<POS>().PropertiesAutowired().As<IPOS>();
            builder.RegisterType<ICPOS>().PropertiesAutowired().As<IICPOS>();
            builder.RegisterType<Manager>().PropertiesAutowired().As<IManager>();

            var container = builder.Build();

            containerWrapper.Container = container;

            return container.Resolve<IManager>();
        }

        private BuildConfig BuildConfig()
        {
            var fields = new List<Field>
            {
                new Field {Id = 1, Name = "Bitmap", Length = 0, Format = Format.ASCII, VarType = VarType.Fixed},
                new Field {Id = 2, Name = "PAN", Length = 19, Format = Format.BCD, VarType = VarType.LLVar},
                new Field {Id = 3, Name = "ProcCode", Length = 6, Format = Format.BCD, VarType = VarType.Fixed},
                new Field {Id = 4, Name = "TranAmt", Length = 12, Format = Format.BCD, VarType = VarType.Fixed},
                new Field {Id = 11, Name = "AcqSsn", Length = 6, Format = Format.BCD, VarType = VarType.Fixed},
                new Field {Id = 12, Name = "LTime", Length = 6, Format = Format.BCD, VarType = VarType.Fixed},
                new Field {Id = 13, Name = "LDate", Length = 4, Format = Format.BCD, VarType = VarType.Fixed},
                new Field {Id = 14, Name = "ExpDate", Length = 4, Format = Format.BCD, VarType = VarType.Fixed},
                new Field {Id = 15, Name = "SettDate", Length = 4, Format = Format.BCD, VarType = VarType.Fixed},
                new Field {Id = 22, Name = "EntrMode", Length = 3, Format = Format.BCD, VarType = VarType.Fixed},
                new Field {Id = 23, Name = "CardSNum", Length = 4, Format = Format.BCD, VarType = VarType.Fixed},
                new Field {Id = 25, Name = "CondMode", Length = 2, Format = Format.BCD, VarType = VarType.Fixed},
                new Field {Id = 26, Name = "PinCode", Length = 2, Format = Format.BCD, VarType = VarType.Fixed},
                new Field {Id = 32, Name = "AcqInst", Length = 11, Format = Format.BCD, VarType = VarType.LLVar},
                new Field {Id = 35, Name = "Trck2Dat", Length = 37, Format = Format.BCD, VarType = VarType.LLVar},
                new Field {Id = 36, Name = "Trck3Dat", Length = 104, Format = Format.BCD, VarType = VarType.LLLVar},
                new Field {Id = 37, Name = "IndexNum", Length = 12, Format = Format.ASCII, VarType = VarType.Fixed},
                new Field {Id = 38, Name = "AuthCode", Length = 6, Format = Format.ASCII, VarType = VarType.Fixed},
                new Field {Id = 39, Name = "RespCode", Length = 2, Format = Format.ASCII, VarType = VarType.Fixed},
                new Field {Id = 41, Name = "TermCode", Length = 8, Format = Format.ASCII, VarType = VarType.Fixed},
                new Field {Id = 42, Name = "MercCode", Length = 15, Format = Format.ASCII, VarType = VarType.Fixed},
                new Field {Id = 44, Name = "AddiResp", Length = 20, Format = Format.ASCII, VarType = VarType.LLVar},
                new Field {Id = 48, Name = "AddiData", Length = 600, Format = Format.ASCII, VarType = VarType.LLLVar},
                new Field {Id = 49, Name = "TranCurr", Length = 3, Format = Format.ASCII, VarType = VarType.Fixed},
                new Field {Id = 52, Name = "PinData", Length = 64, Format = Format.Binary, VarType = VarType.Fixed},
                new Field {Id = 53, Name = "CtrlInfo", Length = 16, Format = Format.BCD, VarType = VarType.Fixed},
                new Field {Id = 54, Name = "AddiAmts", Length = 16, Format = Format.BCD, VarType = VarType.Fixed},
                new Field {Id = 55, Name = "ICCData", Length = 40, Format = Format.Binary, VarType = VarType.LLLVar},
                new Field {Id = 59, Name = "Detail", Length = 600, Format = Format.ASCII, VarType = VarType.LLLVar},
                new Field {Id = 60, Name = "ReveCode", Length = 100, Format = Format.BCD, VarType = VarType.LLLVar},
                new Field {Id = 61, Name = "IdentNum", Length = 255, Format = Format.ASCII, VarType = VarType.LLLVar},
                new Field {Id = 62, Name = "BCSCData", Length = 80, Format = Format.Binary, VarType = VarType.LLLVar},
                new Field {Id = 63, Name = "ResvPri", Length = 512, Format = Format.ASCII, VarType = VarType.LLLVar},
                new Field {Id = 64, Name = "MAC", Length = 64, Format = Format.Binary, VarType = VarType.Fixed}
            };

            return new BuildConfig
            {
                LengthBytesLength = 2,
                LengthBytesFormat = Format.Binary,
                BitmapLength = 64,
                MACFieldId = 64,
                VarFormat = VarFormat.BCD,
                LengthFormat = VarFormat.BCD,
                MTIFormat = VarFormat.BCD,
                OmitHead = false,
                OmitTPDU = false,
                Fields = fields.ToDictionary(one => one.Id),
                Packages = new Dictionary<int, TlvPackage>()
            };
        }
    }
}