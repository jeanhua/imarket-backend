namespace imarket.models
{
    public class CommentModels
    {
        public ulong Id { get; set; }
        public string Content { get; set; }
        public ulong UserId { get; set; } // 关联的用户ID
        public ulong PostId { get; set; } // 关联的帖子ID
        public DateTime CreatedAt { get; set; }
    }
}
