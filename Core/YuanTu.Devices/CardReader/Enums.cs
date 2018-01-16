using System;

namespace YuanTu.Devices.CardReader
{
    [Flags]
    public enum TrackRoad : uint
    {
        Trace1 = 0x10,
        Trace2 = 0x20,
        Trace3 = 0x40
    }

    public enum CardPos : byte
    {
        未知 = 0x00,
        长卡 = 0x46,
        短卡 = 0x47,
        不持卡位 = 0x48,
        持卡位 = 0x49,
        停卡位 = 0x4A,
        IC位 = 0x4B,
        后端持卡位 = 0x4C,
        后端不持卡位 = 0x4D,
        无卡 = 0x4E
    }

    public enum CardMovePos : byte
    {
        非接 = 0x2E,
        IC = 0x2F,
        前端不持卡 = 0x30,
        前端持卡 = 0x31,
        后端持卡 = 0x32,
        后端不持卡 = 0x33
    }

    public enum ICCardProtocol : byte
    {
        T0 = 0x31,
        T1 = 0x32
    }

    public enum ReadType : byte
    {
        ASCII = 0x30,
        BINARY = 0x31
    }

    public enum SlotNo : byte
    {
        大卡座 = 0x01,
        副卡座 = 0x02,
        SAM1卡座 = 0x11,
        SAM2卡座 = 0x12,
        SAM3卡座 = 0x13,
        SAM4卡座 = 0x14
    }

    public enum CardPosF6 : byte
    {
        移到读卡器内部 = 0x30,
        移到IC卡位置 = 0x31,
        移到前端夹卡位置 = 0x32,
        移到后端夹卡位置 = 0x33,
        从前端弹出 = 0x34,
        吞入 = 0x35,
        移动到重入卡位置 = 0x36
    }
}