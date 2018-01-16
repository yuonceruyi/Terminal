namespace YuanTu.Consts.Models.Configs
{
    public class Section
    {
        public Section(string key, string path, string value)
        {
            Key = key;
            Path = path;
            Value = value;
        }
        //
        // 摘要:
        //     Gets the key this section occupies in its parent.
        public string Key { get; }
        //
        // 摘要:
        //     Gets the full path to this section within the Microsoft.Extensions.Configuration.IConfiguration.
        public string Path { get;}
        //
        // 摘要:
        //     Gets or sets the section value.
        public string Value { get; set; }
    }
}
