using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Color = Windows.UI.Color;

namespace iV2EX.Util
{
    public static class Converter
    {
        private static bool IsDarkTheme =>
            Application.Current?.RequestedTheme == ApplicationTheme.Dark;

        public static SolidColorBrush SymbolToColor(string s)
        {
            var isNegative = s != null && s.StartsWith("-");
            if (IsDarkTheme)
            {
                return isNegative
                    ? new SolidColorBrush(Color.FromArgb(255, 239, 83, 80))
                    : new SolidColorBrush(Color.FromArgb(255, 102, 187, 106));
            }
            return isNegative
                ? new SolidColorBrush(Color.FromArgb(255, 229, 57, 53))
                : new SolidColorBrush(Color.FromArgb(255, 67, 160, 71));
        }

        public static SolidColorBrush SymbolToLightColor(string s)
        {
            var isNegative = s != null && s.StartsWith("-");
            return isNegative
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
            return check ? "每日签到" : "今日已签";
        }

        public static string CheckedToGlyph(bool check)
        {
            return check ? "\uE787" : "\uE73E";
        }

        public static string CountOrDash(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? "-" : s.Trim();
        }

        public static Visibility UnreadBadgeVisibility(string notifications)
        {
            return int.TryParse(notifications, out var count) && count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public static Visibility NoUnreadVisibility(string notifications)
        {
            return int.TryParse(notifications, out var count) && count > 0
                ? Visibility.Collapsed
                : Visibility.Visible;
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
            return string.IsNullOrWhiteSpace(s) ? Visibility.Collapsed : Visibility.Visible;
        }

        public static Visibility HasItemsToVisibility(System.Collections.ICollection items)
        {
            return items is { Count: > 0 } ? Visibility.Visible : Visibility.Collapsed;
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
            if (IsDarkTheme)
            {
                return isThanked
                    ? new SolidColorBrush(Color.FromArgb(36, 255, 255, 255))
                    : new SolidColorBrush(Color.FromArgb(36, 140, 158, 255));
            }
            return isThanked
                ? new SolidColorBrush(Color.FromArgb(255, 224, 224, 224))
                : new SolidColorBrush(Color.FromArgb(24, 63, 81, 181));
        }

        public static SolidColorBrush ThanksToForeground(bool isThanked)
        {
            if (IsDarkTheme)
            {
                return isThanked
                    ? new SolidColorBrush(Color.FromArgb(255, 160, 160, 160))
                    : new SolidColorBrush(Color.FromArgb(255, 140, 158, 255));
            }
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
            if (IsDarkTheme)
            {
                return isSelected
                    ? new SolidColorBrush(Color.FromArgb(255, 92, 107, 192))
                    : new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
            return isSelected
                ? new SolidColorBrush(Color.FromArgb(255, 63, 81, 181))
                : new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }

        public static SolidColorBrush TabToForeground(bool isSelected)
        {
            if (IsDarkTheme)
            {
                return isSelected
                    ? new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                    : new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
            }
            return isSelected
                ? new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                : new SolidColorBrush(Color.FromArgb(255, 96, 96, 96));
        }
    }
}
