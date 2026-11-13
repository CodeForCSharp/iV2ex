using System;
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

        public static string PercentToString(double percent)
        {
            return $"{percent:0}%";
        }

        public static string TimestampToString(long unixTimeMs)
        {
            return unixTimeMs <= 0
                ? ""
                : DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMs).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static Visibility EmptyToVisibility(string s)
        {
            return string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible;
        }

        public static Visibility GreaterThanZeroToVisibility(int n)
        {
            return n > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public static string CollectToGlyph(bool isCollected)
        {
            return isCollected ? "\uE735" : "\uE734";
        }

        public static string CollectToText(bool isCollected)
        {
            return isCollected ? "已收藏" : "收藏";
        }

        public static SolidColorBrush ThanksToBackground(bool isThanked)
        {
            return isThanked
                ? new SolidColorBrush(Color.FromArgb(255, 224, 224, 224))
                : new SolidColorBrush(Color.FromArgb(24, 63, 81, 181));
        }

        public static SolidColorBrush ThanksToForeground(bool isThanked)
        {
            return isThanked
                ? new SolidColorBrush(Color.FromArgb(255, 117, 117, 117))
                : new SolidColorBrush(Color.FromArgb(255, 63, 81, 181));
        }

        public static string ThanksToText(int thanks)
        {
            return thanks > 0 ? thanks.ToString() : "感谢";
        }

        public static SolidColorBrush TabToBackground(bool isSelected)
        {
            return isSelected
                ? new SolidColorBrush(Color.FromArgb(255, 63, 81, 181))
                : new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }

        public static SolidColorBrush TabToForeground(bool isSelected)
        {
            return isSelected
                ? new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                : new SolidColorBrush(Color.FromArgb(255, 96, 96, 96));
        }
    }
}