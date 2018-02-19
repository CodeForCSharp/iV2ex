using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace iV2EX.Util
{
    public static class Converter
    {
        public static SolidColorBrush SymbolToColor(string s)
        {
            return s.StartsWith("-")
                ? new SolidColorBrush(Color.FromArgb(255, 255, 60, 112))
                : new SolidColorBrush(Color.FromArgb(255, 10, 163, 77));
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