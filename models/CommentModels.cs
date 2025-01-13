namespace imarket.models
{
    public class CommentModels
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int UserId { get; set; } // 关联的用户ID
        public int PostId { get; set; } // 关联的帖子ID
        public DateTime CreatedAt { get; set; }
    }
}
