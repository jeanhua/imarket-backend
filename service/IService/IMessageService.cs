using imarket.models;

namespace imarket.service.IService
{
    public interface IMessageService
    {
        Task<IEnumerable<MessageModels>> GetMessagesBySenderIdAsync(string senderId,int page,int pageSize);
        Task<IEnumerable<MessageModels>> GetMessagesByReceiverIdAsync(string receiverId,int page, int pageSize);
        Task<MessageModels?> GetMessageByIdAsync(string id);
        Task<int> CreateMessageAsync(MessageModels message);
        Task<int> DeleteMessageByIdAsync(string id);
        Task<int> DeleteMessageByReceiverIdAsync(string receiverId);
        Task<int> DeleteMessageBySenderIdAsync(string senderId);
        Task<int> DeleteMessageBySenderToReceiverIdAsync(string senderId,string receiverid);
    }
}
