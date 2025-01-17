namespace imarket.utils
{
    public class IPtool
    {
        public static string? GetClientIP(HttpContext context)
        {
            // 尝试从 X-Forwarded-For 头中获取 IP 地址
            var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                // X-Forwarded-For 可能包含多个 IP 地址（代理链），取第一个
                var ipAddress = forwardedHeader.Split(',').FirstOrDefault()?.Trim();
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    return ipAddress;
                }
            }
            // 尝试从 X-Real-IP 头中获取 IP 地址
            var realIpHeader = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIpHeader))
            {
                return realIpHeader;
            }
            // 如果以上头都不存在，则直接获取 RemoteIpAddress
            return context.Connection.RemoteIpAddress?.ToString();
        }
    }
}
