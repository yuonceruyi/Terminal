namespace YuanTu.Consts.Models.Payment
{
    public class PayInfoItem
    {
        public PayInfoItem()
        {
        }

        public PayInfoItem(string title, string content, bool special = false)
        {
            Title = title;
            Content = content;
            Special = special;
        }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool Special { get; set; }
    }
}