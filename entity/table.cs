namespace imarket.entity 
{
    public class User
    {
        public int id { get; set; }
        public string email { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string avatar { get; set; }
        public bool isAthorized { get; set; }
        public int icoin { get; set; }
        public string role { get; set; }
        public string name { get; set; }
        public string studentId { get; set; }
    }

    public class Post
    {
        public int id { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public int authorId { get; set; }
        public string authorName { get; set; }
        public string authorAvatar { get; set; }
        public string createdDate { get; set; }
    }

    public class Conment
    {
        public int id { get; set; }
        public string postId { get; set; }
        public string content { get; set; }
        public int authorId { get; set; }
        public string authorName { get; set; }
        public string authorAvatar { get; set; }
        public string createdDate { get; set; }
    }

    public class Reply
    {
        public int id { get; set; }
        public string commentId { get; set; }
        public string content { get; set; }
        public int authorId { get; set; }
        public string authorName { get; set; }
        public string authorAvatar { get; set; }
        public string createdDate { get; set; }
    }

    public class Notification
    {
        public int id { get; set; }
        public string content { get; set; }
        public string receiverId { get; set; }
        public int senderId { get; set; }
        public string senderName { get; set; }
        public string senderAvatar { get; set; }
        public string createdDate { get; set; }
    }

    public class Like_post
    {
        public int id { get; set; }
        public string postId { get; set; }
        public int authorId { get; set; }
        public string authorName { get; set; }
        public string authorAvatar { get; set; }
        public string createdDate { get; set; }
    }

    public class Like_coment
    {
        public int id { get; set; }
        public string commentId { get; set; }
        public int authorId { get; set; }
        public string authorName { get; set; }
        public string authorAvatar { get; set; }
        public string createdDate { get; set; }
    }

    public class Tag
    {
        public int postid { get; set; }
        public string name { get; set; }
    }

    public class Vote
    {
        public int id { get; set; }
        public string postId { get; set; }
        public int authorId { get; set; }
        public string authorName { get; set; }
        public string authorAvatar { get; set; }
        public string createdDate { get; set; }
    }
}
