namespace imarket.models
{
    public class FavoriteModels
    {
        public ulong Id { get; set; }
        public ulong PostId { get; set; }
        public ulong UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
