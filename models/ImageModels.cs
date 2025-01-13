namespace imarket.models
{
    public class ImageModels
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public int PostId { get; set; } // 所属帖子ID
        public DateTime CreatedAt { get; set; }
    }
}
