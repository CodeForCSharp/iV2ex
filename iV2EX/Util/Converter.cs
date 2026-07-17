using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Color = Windows.UI.Color;

namespace iV2EX.Util
{
    public static class Converter
    {
        public static SolidColorBrush SymbolToColor(string s)
        {
            return s.StartsWith("-")
                ? new SolidColorBrush(Color.FromArgb(255, 229, 57, 53))
                : new SolidColorBrush(Color.FromArgb(255, 67, 160, 71));
        }

        public static SolidColorBrush SymbolToLightColor(string s)
        {
            return s.StartsWith("-")
                ? new SolidColorBrush(Color.FromArgb(36, 229, 57, 53))
                : new SolidColorBrush(Color.FromArgb(36, 67, 160, 71));
        }

        public static string TypeToGlyph(string s)
        {
            if (s.Contains("登录") || s.Contains("签到")) return "\uE787";
            if (s.Contains("活跃")) return "\uE945";
            if (s.Contains("谢意")) return "\uEB51";
            if (s.Contains("收益")) return "\uE8BD";
            if (s.Contains("回复")) return "\uE90A";
            if (s.Contains("主题")) return "\uE70F";
            return "\uE825";
        }

        public static string CheckedToString(bool check)
        {
            return check ? "未签到" : "已签到";
        }

        public static Visibility EmptyToVisibility(string s)
        {
            return string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}