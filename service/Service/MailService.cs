using System.Net.Mail;
using System.Net;
using imarket.service.IService;

namespace imarket.service.Service
{
    public class MailService:IMailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MailService> _logger;
        public MailService(IConfiguration configuration, ILogger<MailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendMail(string receiver, string title, string content)
        {
            try
            {
                if(Convert.ToBoolean(_configuration["MailSetting:Enable"])==false)
                {
                    return false;
                }
                MailAddress from = new MailAddress(_configuration["MailSetting:EmailAddress"], _configuration["MailSetting:Name"]);
                MailAddress to = new MailAddress(receiver);
                MailMessage message = new MailMessage(from, to);
                message.Subject = title;
                message.Body = content;
                message.IsBodyHtml = Convert.ToBoolean(_configuration["MailSetting:HtmlEnable"]);
                SmtpClient client = new SmtpClient(_configuration["MailSetting:SmtpHost"], Convert.ToInt32(_configuration["MailSetting:SmtpPort"])); // SMTP服务器地址和端口
                client.EnableSsl = Convert.ToBoolean(_configuration["MailSetting:SslEnable"]); // 启用SSL加密
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_configuration["MailSetting:EmailAddress"], _configuration["MailSetting:EmailPassword"]);
                await client.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error sending email: " + ex.Message);
                return false;
            }
        }
    }
}
