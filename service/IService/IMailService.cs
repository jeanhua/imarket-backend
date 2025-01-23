namespace imarket.service.IService
{
    public interface IMailService
    {
        Task<bool> SendMail(string receiver,string title,string content);
    }
}
