using imarket.models;
using imarket.service.IService;
using imarket.service.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;

namespace imarket.Controllers.open
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "user,unverified,banned,admin")]
    public class MessageController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IMessageService messageService;
        private readonly ILogger _logger;
        private readonly IMemoryCache _memoryCache;
        public MessageController(IUserService userService, IMessageService messageService, ILogger logger,IMemoryCache memoryCache)
        {
            this.userService = userService;
            this.messageService = messageService;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// 获取消息列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("List")] // api/Message/List
        public async Task<IActionResult> GetMessage([FromQuery]int page, [FromQuery]int pageSize)
        {
            var user = await userService.GetUserByUsernameAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }
            var messages_send = await messageService.GetMessagesBySenderIdAsync(user.Id, page, pageSize);
            var messages_receive = await messageService.GetMessagesByReceiverIdAsync(user.Id, page, pageSize);
            return Ok(new
            {
                success = true,
                messages = new
                {
                    send = messages_send,
                    receive = messages_receive
                }
            });
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("Send")] // api/Message/Send
        public async Task<IActionResult> SendMessage([FromBody][Required]SendMessageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var sender = await userService.GetUserByUsernameAsync(User.Identity?.Name);
            if (sender == null)
            {
                return NotFound("user not found");
            }
            var receiver = await userService.GetUserByUsernameAsync(request.ReceiveId);
            if (receiver == null)
            {
                return NotFound("receiver not found");
            }
            if(_memoryCache.TryGetValue(sender.Id, out var sendNumsCache))
            {
                var sendNums = sendNumsCache as int? ?? 0;
                if (sendNums as int? >= 2)
                {
                    return BadRequest("send too many messages");
                }
                else {
                    _memoryCache.Set(sender.Id, sendNums + 1, TimeSpan.FromSeconds(1));
                }
            }
            await messageService.CreateMessageAsync(new MessageModels
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Content = request.Content,
                CreatedAt = DateTime.Now,
                Id = Guid.NewGuid().ToString(),
            });
            return Ok(new
            {
                success = true,
            });
        }

        /// <summary>
        /// 删除消息
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        [HttpGet("Delete")] // api/Message/Delete?messageId={messageId}
        public async Task<IActionResult> DeleteMessage([FromQuery][Required] string messageId)
        {
            var user = await userService.GetUserByUsernameAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound("user not found");
            }
            var message = await messageService.GetMessageByIdAsync(messageId);
            if (message == null)
            {
                return NotFound("message not found");
            }
            if (message.SenderId != user.Id || message.ReceiverId != user.Id)
            {
                return BadRequest("permission denied");
            }
            await messageService.DeleteMessageByIdAsync(messageId);
            return Ok(new
            {
                success = true,
            });
        }
    }

    public class SendMessageRequest
    {
        [Required]
        public string? ReceiveId { get; set; }
        [Required]
        public string? Content { get; set; }
    }
}
