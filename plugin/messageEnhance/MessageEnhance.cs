using imarket.models;
using imarket.service.IService;

namespace imarket.plugin.messageEnhance
{
    /// <summary>
    /// 该插件用于增强消息获取接口，添加了用户名和昵称
    /// </summary>
    [PluginTag(Name = "messageEnhance plugin", Description = "该插件用于增强消息获取接口，添加了用户名和昵称", Author = "jeanhua", Enable = true)]
    public class MessageEnhance:IPluginInterceptor
    {
        private readonly IUserService userService;
        public MessageEnhance(IUserService userService)
        {
            this.userService = userService;
        }
        public async Task<(bool op, object? result)> OnBeforeExecutionAsync(string route, object?[] args, string? username = null)
        {
            return (false, null);
        }
        public async Task<(bool op, object? result)> OnAfterExecutionAsync(string route, object? result, string? username = null)
        {
            if(route== "/api/Message/List")
            {
                var messages = (result as dynamic).messages as List<MessageModels>;
                var newMessages = new List<MessageModelsEnhance>();
                foreach (var message in messages)
                {
                    var sender = await userService.GetUserByIdAsync(message.SenderId);
                    var receiver = await userService.GetUserByIdAsync(message.ReceiverId);
                    newMessages.Add(new MessageModelsEnhance
                    {
                        Id = message.Id,
                        SenderId = message.SenderId,
                        ReceiverId = message.ReceiverId,
                        Content = message.Content,
                        CreatedAt = message.CreatedAt,
                        SenderUserName = sender.Username,
                        SenderNickname = sender.Nickname,
                        ReceiverUserName = receiver.Username,
                        ReceiverNickname = receiver.Nickname
                    });
                }
                return (true, new { success = true,messages = newMessages });
            }
            return (false, null);
        }

        public void RegisterRoutes(IEndpointRouteBuilder endpoints)
        {
        }
    }
    class MessageModelsEnhance : MessageModels
    {
        public string SenderUserName { get; set; }
        public string SenderNickname { get; set; }
        public string ReceiverUserName { get; set; }
        public string ReceiverNickname { get; set; }
    }
}
