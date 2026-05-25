using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Input;
using Windows.UI;

namespace iV2EX.Controls
{
    public sealed partial class RefreshButton
    {
        private static readonly SolidColorBrush NormalBrush = new(Color.FromArgb(0xFF, 0xFF, 0xC1, 0x07));
        private static readonly SolidColorBrush PressedBrush = new(Color.FromArgb(0xFF, 0xFF, 0xA0, 0x00));

        public RefreshButton()
        {
            InitializeComponent();

            RootGrid.PointerPressed += (_, _) =>
            {
                RootGrid.Background = PressedBrush;
                Scale.ScaleX = 0.9;
                Scale.ScaleY = 0.9;
            };

            RootGrid.PointerReleased += (_, _) =>
            {
                Scale.ScaleX = 1.0;
                Scale.ScaleY = 1.0;
                RootGrid.Background = NormalBrush;
            };
        }
    }
}
