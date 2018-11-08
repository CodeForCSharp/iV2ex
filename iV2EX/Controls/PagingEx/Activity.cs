using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace PagingEx
{
    public class Activity : ContentControl
    {
        public static readonly DependencyProperty TopAppBarProperty =
            DependencyProperty.Register(nameof(TopAppBar), typeof(AppBar), typeof(Activity),
                new PropertyMetadata(default(AppBar), (o, args) => ((Activity) o).OnUpdateTopAppBar()));

        public static readonly DependencyProperty BottomAppBarProperty =
            DependencyProperty.Register(nameof(BottomAppBar), typeof(AppBar), typeof(Activity),
                new PropertyMetadata(default(AppBar), (o, args) => ((Activity) o).OnUpdateBottomAppBar()));

        private Page _internalPage;
        private bool _isLoaded;

        public Activity()
        {
            NavigationCacheMode = NavigationCacheMode.Required;

            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            VerticalContentAlignment = VerticalAlignment.Stretch;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
        }

        protected ActivityContainer Container { get; private set; }

        public IActivityTransition ActivityTransition { get; set; }
        
        public Page InternalPage => _internalPage ?? (_internalPage = new Page {Content = this});

        public NavigationCacheMode NavigationCacheMode { get; set; }

        public AppBar TopAppBar
        {
            get => (AppBar) GetValue(TopAppBarProperty);
            set => SetValue(TopAppBarProperty, value);
        }

        public AppBar BottomAppBar
        {
            get => (AppBar) GetValue(BottomAppBarProperty);
            set => SetValue(BottomAppBarProperty, value);
        }

        protected internal virtual void SetContainer(ActivityContainer activityContainer)
        {
            Container = activityContainer;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        protected virtual void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _isLoaded = true;

            OnUpdateTopAppBar();
            OnUpdateBottomAppBar();
            OnDataContextChanged();
        }

        protected virtual void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (InternalPage.TopAppBar != null) InternalPage.TopAppBar = null;

            if (InternalPage.BottomAppBar != null) InternalPage.BottomAppBar = null;
        }

        private void OnDataContextChanged()
        {
            if (_isLoaded) InternalPage.DataContext = DataContext;
        }

        private void OnUpdateTopAppBar()
        {
            if (_isLoaded && TopAppBar != null)
            {
                InternalPage.TopAppBar = TopAppBar;
                foreach (var item in Resources.Where(item => !(item.Value is DependencyObject)))
                    InternalPage.TopAppBar.Resources[item.Key] = item.Value;
            }
        }

        private void OnUpdateBottomAppBar()
        {
            if (_isLoaded && BottomAppBar != null)
            {
                InternalPage.BottomAppBar = BottomAppBar;
                foreach (var item in Resources.Where(item => !(item.Value is DependencyObject)))
                    InternalPage.BottomAppBar.Resources[item.Key] = item.Value;
            }
        }

        protected void Finish()
        {
            if (Container.CanGoBack)
            {
                Container.GoBack();
            }
        }

        protected void StartActivity(Type type, object parameter = null)
        {
            Container.Navigate(type, parameter);
        }

        protected void StartActivity<T>(object parameter = null) where T : Activity
        {
            StartActivity(typeof(T), parameter);
        }

        protected internal virtual void OnCreate(object parameter)
        {
        }

        protected internal virtual void OnStart()
        {
        }

        protected internal virtual void OnRestart()
        {
        }

        protected internal virtual void OnStop()
        {
        }

        protected internal virtual void OnResume()
        {
        }

        protected internal virtual void OnPause()
        {
        }

        protected internal virtual void OnClose()
        {
        }

        protected internal virtual void OnDestroy()
        {
        }

        protected virtual void OnPrepareConnectedAnimation(ConnectedAnimationService service)
        {
        }

        protected virtual void OnUsingConnectedAnimation(ConnectedAnimationService service)
        {
        }

        internal void PrepareConnectedAnimation()
        {
            OnPrepareConnectedAnimation(ConnectedAnimationService.GetForCurrentView());
        }

        internal void UsingConnectedAnimation()
        {
            OnUsingConnectedAnimation(ConnectedAnimationService.GetForCurrentView());
        }
    }
}