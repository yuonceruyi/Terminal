namespace YuanTu.ZheJiangZhongLiuHospital.ICBC
{
    public interface ITable
    {
        string ClassName { get; }
    }

    public interface IReq : ITable
    {
    }

    public interface IRes : ITable
    {
        string ResultFlag { get; set; }
        string ResultMark { get; set; }
    }
}
