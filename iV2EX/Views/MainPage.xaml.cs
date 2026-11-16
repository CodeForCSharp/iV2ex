using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using iV2EX.Util;

namespace iV2EX.Views
{
    internal partial class MainPage
    {
        public const double WideBreakpoint = 860;
        private string _activeSectionTitle = "首页";

        public MainPage()
        {
            InitializeComponent();
            Instance = this;
            RightPart = RightFrame;
            PageStack.Next("Right", "Right", typeof(BlankPage), null);

            Loaded += (_, _) => SyncPaneVisibility();
            RootGrid.SizeChanged += (_, _) => SyncPaneVisibility();
        }

        public static MainPage Instance { get; private set; }
        public static Frame RightPart { get; private set; }

        public string ActiveSectionTitle => _activeSectionTitle;

        public void SyncPaneVisibility()
        {
            if (RootGrid == null || LeftPaneContainer == null || RightFrame == null) return;
            var width = RootGrid.ActualWidth > 0 ? RootGrid.ActualWidth : App.Window?.PageFrame?.ActualWidth ?? 0;
            var isWide = width >= WideBreakpoint;

            if (isWide)
            {
                LeftPaneContainer.Visibility = Visibility.Visible;
                RightFrame.Visibility = Visibility.Visible;
            }
            else if (PageStack.IsLeftToRightActive)
            {
                LeftPaneContainer.Visibility = Visibility.Collapsed;
                RightFrame.Visibility = Visibility.Visible;
            }
            else
            {
                LeftPaneContainer.Visibility = Visibility.Visible;
                RightFrame.Visibility = Visibility.Collapsed;
            }
        }

        private void NavRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (HomeView == null || NodesView == null || UserInfoView == null) return;
            if ((sender as FrameworkElement)?.Tag is not string tag) return;

            HomeView.Visibility = tag == "home" ? Visibility.Visible : Visibility.Collapsed;
            NodesView.Visibility = tag == "nodes" ? Visibility.Visible : Visibility.Collapsed;
            UserInfoView.Visibility = tag == "profile" ? Visibility.Visible : Visibility.Collapsed;

            _activeSectionTitle = tag switch
            {
                "nodes" => "节点",
                "profile" => "我的",
                _ => "首页"
            };

            if (tag == "profile")
                UserInfoView.RefreshIfStale();

            if (!PageStack.IsLeftToRightActive)
                App.Window?.UpdateTitle(_activeSectionTitle);
        }
    }
}
