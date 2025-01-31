namespace imarket.models
{
    public class LikeModels
    {
        public ulong Id { get; set; }
        public ulong? PostId { get; set; }
        public ulong UserId { get; set; }
        public ulong? CommentId { get; set; }
        public DateTime CreatedAt { get; set; } //对于 `PostId` 和 `CommentId`，只有一个字段能有值，用于区分是给帖子点赞还是评论点赞。
    }
}
