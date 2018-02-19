using System;
using Windows.UI.Xaml;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace iV2EX.Controls
{
    public sealed partial class ToastTips
    {
        public ToastTips()
        {
            InitializeComponent();
        }

        public void ShowTips(string content)
        {
            Container.Text = content;
            Small.ContextFlyout.ShowAt(Small);
            var timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(1500)};
            timer.Tick += (sender, e) =>
            {
                Small.ContextFlyout.Hide();
                timer.Stop();
            };
            timer.Start();
        }
    }
}