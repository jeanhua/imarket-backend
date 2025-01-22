namespace imarket.models
{
    public class HotRankingModels
    {
        public class Post
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public string UserId { get; set; }
            public DateTime CreatedAt { get; set; }
            public int LikeCount { get; set; }
        }

        public class Favorite
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public string UserId { get; set; }
            public DateTime CreatedAt { get; set; }
            public int FavoriteCount { get; set; }
        }

    }
}
