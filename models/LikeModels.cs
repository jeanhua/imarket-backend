namespace imarket.models
{
    public class LikeModels
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public int CommentId { get; set; }
        public DateTime CreatedAt { get; set; } //对于 `PostId` 和 `CommentId`，只有一个字段能有值，用于区分是给帖子点赞还是评论点赞。
    }
}
