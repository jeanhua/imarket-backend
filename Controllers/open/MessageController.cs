using imarket.models;
using imarket.plugin;
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
        private readonly ILogger<MessageController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly PluginManager pluginManager;
        public MessageController(IUserService userService, IMessageService messageService, ILogger<MessageController> logger,IMemoryCache memoryCache,PluginManager pluginManager)
        {
            this.userService = userService;
            this.messageService = messageService;
            _logger = logger;
            _memoryCache = memoryCache;
            this.pluginManager = pluginManager;
        }

        /// <summary>
        /// 获取消息列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("List")] // api/Message/List
        public async Task<IActionResult> GetMessage([FromQuery]bool isSender = false,[FromQuery]int page=1, [FromQuery]int pageSize=10)
        {
            var args = new object[] { page, pageSize };
            var result_before = await pluginManager.ExecuteBeforeAsync(HttpContext.Request.Path.Value!, args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
            var user = await userService.GetUserByUsernameAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }
            var messages = new List<MessageModels>();
            if (isSender)
            {
                messages = (await messageService.GetMessagesBySenderIdAsync(user.Id, page, pageSize)).ToList();
            }
            else
            {
                messages = (await messageService.GetMessagesByReceiverIdAsync(user.Id, page, pageSize)).ToList();
            }
            var response = new
            {
                success = true,
                messages
            };
            var result_after = await pluginManager.ExecuteAfterAsync(HttpContext.Request.Path.Value!, response, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(response);
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
            if(User.IsInRole("banned") || User.IsInRole("unverified"))
            {
                return BadRequest("permission denied");
            }
            var args = new object[] { request };
            var result_before = await pluginManager.ExecuteBeforeAsync(HttpContext.Request.Path.Value!, args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
            var sender = await userService.GetUserByUsernameAsync(User.Identity?.Name);
            if (sender == null)
            {
                return NotFound("user not found");
            }
            var receiver = await userService.GetUserByUsernameAsync(request.Username);
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
                Content = request.Content??"",
                CreatedAt = DateTime.Now,
            });
            var response = new
            {
                success = true
            };
            var result_after = await pluginManager.ExecuteAfterAsync(HttpContext.Request.Path.Value!, response, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(response);
        }

        /// <summary>
        /// 删除消息
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        [HttpPost("Delete")] // api/Message/Delete?messageId={messageId}
        public async Task<IActionResult> DeleteMessage([FromBody][Required] ulong[] messageId)
        {
            if (User.IsInRole("banned") || User.IsInRole("unverified"))
            {
                return BadRequest("permission denied");
            }
            var args = new object[] { messageId };
            var result_before = await pluginManager.ExecuteBeforeAsync(HttpContext.Request.Path.Value!, args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
            var user = await userService.GetUserByUsernameAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound("user not found");
            }
            foreach(var msgId in messageId)
            {
                var message = await messageService.GetMessageByIdAsync(msgId);
                if (message == null)
                {
                    return NotFound("message not found");
                }
                if (message.SenderId != user.Id && message.ReceiverId != user.Id)
                {
                    return BadRequest("permission denied");
                }
                await messageService.DeleteMessageByIdAsync(msgId);
            }
            var response = new
            {
                success = true
            };
            var result_after = await pluginManager.ExecuteAfterAsync(HttpContext.Request.Path.Value!, response, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(response);
        }
    }

    public class SendMessageRequest
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Content { get; set; }
    }
}
