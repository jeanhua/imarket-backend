using imarket.models;

namespace imarket.service.IService
{
    public interface IMessageService
    {
        Task<IEnumerable<MessageModels>> GetMessagesBySenderIdAsync(ulong senderId,int page,int pageSize);
        Task<IEnumerable<MessageModels>> GetMessagesByReceiverIdAsync(ulong receiverId,int page, int pageSize);
        Task<MessageModels?> GetMessageByIdAsync(ulong id);
        Task<int> CreateMessageAsync(MessageModels message);
        Task<int> DeleteMessageByIdAsync(ulong id);
        Task<int> DeleteMessageByReceiverIdAsync(ulong receiverId);
        Task<int> DeleteMessageBySenderIdAsync(ulong senderId);
        Task<int> DeleteMessageBySenderToReceiverIdAsync(ulong senderId,ulong receiverid);
    }
}
