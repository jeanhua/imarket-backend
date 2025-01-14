namespace imarket.models
{
    public class ImageModels
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string PostId { get; set; } // 所属帖子ID
        public DateTime CreatedAt { get; set; }
    }
}
