using System.Text.RegularExpressions;

namespace imarket.utils
{
    public class UsernameValidator
    {
        /// <summary>
        /// 验证用户名是否合法
        /// </summary>
        /// <param name ="username">需要验证的用户名</param>
        /// <returns>如果合法返回 true，否则返回 false</returns>
        public static bool IsValidUsername(string username)
        {
            // 检查是否为 null 或空字符串
            if (string.IsNullOrEmpty(username))
            {
                return false;
            }

            // 正则表达式：仅允许数字、英文和下划线，长度不超过 20 个字符
            string pattern = @"^[a-zA-Z0-9_]{1,20}$";
            return Regex.IsMatch(username, pattern);
        }
    }
}
