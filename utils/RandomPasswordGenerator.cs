using System;
using System.Text;

public class RandomPasswordGenerator
{
    private static Random _random = new Random();

    // 生成随机密码
    public static string GenerateRandomPassword(int length = 12)
    {
        // 密码字符集：包括大写字母、小写字母、数字和特殊字符
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string specialChars = "!@#$%^&*()_+-=[]{}|;:',.<>?/";

        // 合并所有字符集
        string allChars = upperChars + lowerChars + digits + specialChars;
        StringBuilder password = new StringBuilder(length);

        // 保证密码中至少包含一个大写字母、小写字母、数字和特殊字符
        password.Append(upperChars[_random.Next(upperChars.Length)]);
        password.Append(lowerChars[_random.Next(lowerChars.Length)]);
        password.Append(digits[_random.Next(digits.Length)]);
        password.Append(specialChars[_random.Next(specialChars.Length)]);

        // 填充剩余的密码字符
        for (int i = password.Length; i < length; i++)
        {
            int index = _random.Next(allChars.Length);
            password.Append(allChars[index]);
        }

        // 打乱密码字符的顺序以增强随机性
        char[] passwordArray = password.ToString().ToCharArray();
        _random.Shuffle(passwordArray);

        return new string(passwordArray);
    }
}

public static class RandomExtensions
{
    // 扩展方法：打乱字符数组的顺序
    public static void Shuffle(this Random random, char[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            char value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
    }
}
