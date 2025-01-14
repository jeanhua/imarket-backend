namespace imarket.models
{
    public class PostModels
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string UserId { get; set; } // 关联的用户ID
        public DateTime CreatedAt { get; set; }
        public int Status { get; set; }
    }
}
