namespace imarket.utils
{
    public class RandomChineseNicknameGenerator
    {
        private static Random _random = new Random();

        // 所有风格的网名（混合）
        private static string[] allNames = new string[]
        {
        // 生动活泼风格
        "阳光小猫", "飞翔的兔子", "甜蜜樱桃", "跳跃的鱼儿", "快乐小星", "风中的笑脸", "闪亮泡泡", "小幸运",

        // 优雅风格
        "清风徐来", "诗雨如梦", "月下柳影", "梅雪芳香", "梦中伊人", "静谧之心", "翩若惊鸿", "温柔一刀",

        // 俏皮风格
        "小调皮", "嘟嘟嘴", "捣蛋鬼", "萌萌哒", "爱笑的小仙女", "甜心小兔", "呆萌猫咪", "眨眼小精灵"
        };
        public static string GenerateRandomNickname()
        {
            int index = _random.Next(allNames.Length);
            return allNames[index];
        }
    }
}
