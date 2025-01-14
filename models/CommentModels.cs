namespace imarket.models
{
    public class CommentModels
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public string UserId { get; set; } // 关联的用户ID
        public string PostId { get; set; } // 关联的帖子ID
        public DateTime CreatedAt { get; set; }
    }
}
