namespace imarket.models
{
    public class ImageModels
    {
        public ulong Id { get; set; }
        public string Url { get; set; }
        public ulong PostId { get; set; } // 所属帖子ID
        public DateTime CreatedAt { get; set; }
    }
}
