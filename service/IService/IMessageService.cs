using imarket.models;

namespace imarket.service.IService
{
    public interface IMessageService
    {
        Task<IEnumerable<MessageModels>> GetMessagesBySenderIdAsync(string senderId);
        Task<IEnumerable<MessageModels>> GetMessagesByReceiverIdAsync(string receiverId);
        Task<MessageModels> GetMessageByIdAsync(string id);
        Task<int> CreateMessageAsync(MessageModels message);
        Task<int> DeleteMessageByIdAsync(string id);
        Task<int> DeleteMessageByReceiverIdAsync(string receiverId);
        Task<int> DeleteMessageBySenderToReceiverIdAsync(string senderId,string receiverid);
    }
}
