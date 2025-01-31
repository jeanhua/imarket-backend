namespace imarket.models
{
    public class PostCategoryModels
    {
        public ulong PostId { get; set; } // 关联的帖子ID
        public ulong CategoryId { get; set; } // 关联的分类ID
    }
}
