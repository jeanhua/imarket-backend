namespace imarket.models
{
    public class HotRankingModels
    {
        public class Post
        {
            public ulong Id { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public ulong UserId { get; set; }
            public DateTime CreatedAt { get; set; }
            public int LikeCount { get; set; }
        }

        public class Favorite
        {
            public ulong Id { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public ulong UserId { get; set; }
            public DateTime CreatedAt { get; set; }
            public int FavoriteCount { get; set; }
        }

    }
}
