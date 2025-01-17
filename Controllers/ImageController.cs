using imarket.service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace imarket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageService imageService;
        private readonly ILogger<ImageController> _logger;
        public ImageController(IImageService imageService, ILogger<ImageController> logger)
        {
            this.imageService = imageService;
            this._logger = logger;
        }

        [HttpPost("UploadImage")] // api/image/UploadImage
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> UploadImage([FromBody] ImageRequest request)
        {
            try
            {
                if (request.Base64 == null)
                {
                    return BadRequest("Image is required");
                }
                if(request.Base64.Length == 0)
                {
                    return BadRequest("Image is required");
                }
                // 图片大小限制3MB
                if (request.Base64.Length > 4 * 1024 * 1024)
                {
                    return BadRequest("Image size should be less than 3MB");
                }
                var path = await imageService.UploadImageAsync(request.Base64);
                return Ok(new { success = true, path = path }); 
            }
            catch (Exception e)
            {
                _logger.LogError("api/image/UploadImage:" + e.ToString());
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }

    public class ImageRequest
    {
        public string? Base64 { get; set; }
    }
}
