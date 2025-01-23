using imarket.service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace imarket.Controllers.open
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageService imageService;
        private readonly ILogger<ImageController> logger;
        public ImageController(IImageService imageService, ILogger<ImageController> logger)
        {
            this.imageService = imageService;
            this.logger = logger;
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("UploadImage")] // api/Image/UploadImage
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> UploadImage([FromBody] ImageRequest request)
        {
            if (request.Base64 == null)
            {
                return BadRequest("Image is required");
            }
            if (request.Base64.Length == 0)
            {
                return BadRequest("Image is required");
            }
            // 图片大小限制3MB
            if (request.Base64.Length > 4 * 1024 * 1024)
            {
                return BadRequest("Image size should be less than 3MB");
            }
            if (!request.Base64.StartsWith("data:image/"))
            {
                return BadRequest("Invalid base64 format");
            }
            var path = await imageService.UploadImageAsync(request.Base64);
            return Ok(new { success = true, path });
        }
    }

    public class ImageRequest
    {
        [Required]
        public string? Base64 { get; set; }
    }
}
