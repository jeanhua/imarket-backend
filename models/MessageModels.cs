namespace imarket.models
{
    public class MessageModels
    {
        public ulong Id { get; set; }
        public ulong SenderId { get; set; }
        public ulong ReceiverId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
