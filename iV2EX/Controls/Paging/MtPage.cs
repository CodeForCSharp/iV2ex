//-----------------------------------------------------------------------
// <copyright file="MtPage.cs" company="MyToolkit">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/MyToolkit/MyToolkit/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MyToolkit.Mvvm;
using MyToolkit.Paging.Animations;
using MyToolkit.Paging.Handlers;
using MyToolkit.Utilities;

namespace MyToolkit.Paging
{
    /// <summary>The customized page base class.</summary>
    public class MtPage : ContentControl
    {
        private ContentControl _internalPage;
        private bool _isLoaded;

        public MtPage()
        {
            UseBackKeyToNavigate = true;
            IsSuspendable = true;
            UseAltLeftOrRightToNavigate = true;

            NavigationCacheMode = NavigationCacheMode.Required;

            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            VerticalContentAlignment = VerticalAlignment.Stretch;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;

            NavigationKeyHandler = new NavigationKeyHandler(this);
            PageStateHandler = new PageStateHandler(this, Guid.NewGuid().ToString());

            DependencyPropertyChangedEvent.Register(this, DataContextProperty, delegate { OnDataContextChanged(); });
        }

        internal PageStateHandler PageStateHandler { get; }

        internal NavigationKeyHandler NavigationKeyHandler { get; }

        /// <summary>
        ///     Gets the current page.
        /// </summary>
        public static MtPage Current => MtFrame.Current != null && MtFrame.Current.CurrentPage != null
            ? MtFrame.Current.CurrentPage.Page
            : null;

        /// <summary>
        ///     Gets the current page animation.
        ///     Only available when the <see cref="MtFrame" />'s ContentTransitions  is null.
        ///     Overrides the <see cref="MtFrame" />'s PageAnimation property.
        /// </summary>
        public IPageAnimation PageAnimation { get; set; }

        /// <summary>
        ///     Gets the <see cref="MtFrame" /> instance which is hosting the page.
        /// </summary>
        public MtFrame Frame { get; private set; }

        /// <summary>
        ///     Gets or sets the control which is used for page animations.
        ///     If set to null, the root control of the page is used.
        /// </summary>
        public FrameworkElement AnimationContext { get; set; }

        /// <summary>Gets the current animation context based on the <see cref="AnimationContext" /> property. </summary>
        public FrameworkElement ActualAnimationContext => AnimationContext ?? this;

        /// <summary>
        ///     Gets the underlying WinRT page object.
        /// </summary>
        public ContentControl InternalPage => _internalPage ?? (_internalPage = new ContentControl
        {
            Content = this,
            VerticalContentAlignment = VerticalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch
        });

        /// <summary>
        ///     Gets or sets the navigation cache mode (default: required).
        /// </summary>
        public NavigationCacheMode NavigationCacheMode { get; set; }

        //public static readonly DependencyProperty TopAppBarProperty =
        //    DependencyProperty.Register("TopAppBar", typeof(AppBar), typeof(MtPage), 
        //        new PropertyMetadata(default(AppBar), (o, args) => ((MtPage)o).OnUpdateTopAppBar()));

        /// <summary>Gets or sets the top app bar.</summary>
        /// <summary>Gets or sets the bottom app bar.</summary>
        /// <summary>
        ///     Gets or sets a value indicating whether the page can save and load its state (default: true).
        ///     If false, then the page and all following pages are removed from the page stack when the app gets suspended.
        /// </summary>
        public bool IsSuspendable { get; protected set; }

        /// <summary>Gets or sets a value indicating whether to use the special pointer buttons to navigate (default: true).</summary>
        public bool UsePointerButtonsToNavigate { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to use alt-left or alt-right to
        ///     navigate back or forward (default: true).
        /// </summary>
        public bool UseAltLeftOrRightToNavigate { get; set; }

        /// <summary>Gets or sets a value indicating whether the back key is used to navigate back (default: true).</summary>
        public bool UseBackKeyToNavigate { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the back key is used to navigate back
        ///     even if the focus is in a web view (default: false).
        /// </summary>
        public bool UseBackKeyToNavigateInWebView { get; set; }

        internal void SetFrame(MtFrame frame, string pageKey)
        {
            Frame = frame;
            PageStateHandler.PageKey = pageKey;
        }

        /// <summary>
        ///     Initializes the view model and registers events so that the OnLoaded and OnUnloaded methods are called.
        ///     This method must be called in the constructor after the <see cref="InitializeComponent" /> method call.
        /// </summary>
        /// <param name="viewModel">The view model. </param>
        /// <param name="registerForStateHandling">
        ///     Registers the view model also for state handling
        ///     The view model has to implement <see cref="IStateHandlingViewModel" /> and the view must be a <see cref="MtPage" />
        ///     .
        /// </param>
        public void RegisterViewModel(ViewModelBase viewModel, bool registerForStateHandling = true)
        {
            ViewModelHelper.RegisterViewModel(viewModel, this, registerForStateHandling);
        }

        /// <summary>Registers the view model for state handling. </summary>
        /// <param name="viewModel">The view model. </param>
        public void RegisterViewModelForStateHandling(IStateHandlingViewModel viewModel)
        {
            ViewModelHelper.RegisterViewModelForStateHandling(viewModel, this);
        }

        /// <summary>
        ///     Binds the <see cref="ViewModelBase.IsLoading" /> property of the view model to
        ///     the progress bar visibility of the status bar (Windows Phone only).
        /// </summary>
        /// <param name="viewModel">The view model. </param>
        public void BindViewModelToStatusBarProgress(ViewModelBase viewModel)
        {
            ViewModelHelper.BindViewModelToStatusBarProgress(viewModel);
        }

        /// <summary>
        ///     Adds a go back handler at the end of the handler queue.
        ///     For example called when back key has been pressed on Windows Phone or backspace key or alt-left has been pressed
        ///     Windows.
        /// </summary>
        /// <param name="handler">The handler. </param>
        /// <returns>Returns the created async go back handler which is used for deregistration (RemoveGoBackAsyncHandler). </returns>
        public Func<CancelEventArgs, Task> AddGoBackHandler(Action<CancelEventArgs> handler)
        {
            return NavigationKeyHandler.AddGoBackHandler(handler);
        }

        /// <summary>
        ///     Adds an async go back handler at the end of the handler queue.
        ///     For example called when back key has been pressed on Windows Phone or backspace key or alt-left has been pressed
        ///     Windows.
        /// </summary>
        /// <param name="handler">The handler. </param>
        public void AddGoBackAsyncHandler(Func<CancelEventArgs, Task> handler)
        {
            NavigationKeyHandler.AddGoBackAsyncHandler(handler);
        }

        /// <summary>
        ///     Removes an async go back handler.
        /// </summary>
        /// <param name="handler">The handler. </param>
        public void RemoveGoBackAsyncHandler(Func<CancelEventArgs, Task> handler)
        {
            NavigationKeyHandler.RemoveGoBackAsyncHandler(handler);
        }

        /// <summary>Used to load the saved state when the page has been reactivated. </summary>
        /// <param name="parameter">The initial page parameter. </param>
        /// <param name="pageState">The saved page state. </param>
        [Obsolete("Use OnLoadState instead. 7/28/2014")]
        protected internal virtual void LoadState(object parameter, Dictionary<string, object> pageState)
        {
            // Must be empty
        }

        /// <summary>Used to save the state when the page gets suspended. </summary>
        /// <param name="pageState">The dictionary to save the page state into. </param>
        [Obsolete("Use OnSaveState instead. 7/28/2014")]
        protected internal virtual void SaveState(Dictionary<string, object> pageState)
        {
            // Must be empty
        }

        /// <summary>Used to load the saved state when the page has been reactivated. </summary>
        /// <param name="pageState">The saved page state. </param>
        protected internal virtual void OnLoadState(MtLoadStateEventArgs pageState)
        {
            // Must be empty
        }

        /// <summary>Used to save the state when the page gets suspended. </summary>
        /// <param name="pageState">The dictionary to save the page state into. </param>
        protected internal virtual void OnSaveState(MtSaveStateEventArgs pageState)
        {
            // Must be empty
        }

        /// <summary>
        ///     Called when an accelerator key has been activated (not supported on WP).
        /// </summary>
        /// <param name="args"></param>
        protected internal virtual void OnKeyActivated(AcceleratorKeyEventArgs args)
        {
            // Must be empty
        }

        /// <summary>Called when a key up event has occured (not supported on WP). </summary>
        /// <param name="args">The event arguments. </param>
        protected internal virtual void OnKeyUp(AcceleratorKeyEventArgs args)
        {
            // Must be empty
        }

        /// <summary>Called when navigating from this page. </summary>
        /// <param name="args">The event arguments. </param>
        protected internal virtual void OnNavigatingFrom(MtNavigatingCancelEventArgs args)
        {
            // Must be empty
        }

        /// <summary>
        ///     Called when navigating from this page.
        ///     The navigation does no happen until the returned task has completed.
        ///     Return null or empty task to run the method synchronously.
        /// </summary>
        /// <param name="args">The event arguments. </param>
        /// <returns>The task. </returns>
        protected internal virtual Task OnNavigatingFromAsync(MtNavigatingCancelEventArgs args)
        {
            // Must be empty
            return null;
        }

        /// <summary>Called when navigated from this page. </summary>
        /// <param name="args">The event arguments. </param>
        protected internal virtual void OnNavigatedFrom(MtNavigationEventArgs args)
        {
            // Must be empty
        }

        /// <summary>Called when navigated to this page. </summary>
        /// <param name="args">The event arguments. </param>
        protected internal virtual void OnNavigatedTo(MtNavigationEventArgs args)
        {
            // Leave empty!
        }

        /// <summary>
        ///     Called when the page visibility has changed (e.g. the app has been suspended and it is no longer visible to
        ///     the user).
        /// </summary>
        /// <param name="args">The event arguments. </param>
        protected internal virtual void OnVisibilityChanged(VisibilityChangedEventArgs args)
        {
            // Must be empty
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Loaded += OnLoaded;
            //Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _isLoaded = true;

            //OnUpdateTopAppBar();
            //OnUpdateBottomAppBar();
            OnDataContextChanged();
        }

        private void OnDataContextChanged()
        {
            if (_isLoaded)
                InternalPage.DataContext = DataContext;
        }

        //private void OnUpdateTopAppBar()
        //{
        //    if (_isLoaded && TopAppBar != null)
        //    {
        //        InternalPage.TopAppBar = TopAppBar;
        //        foreach (var item in Resources.Where(item => !(item.Value is DependencyObject)))
        //            InternalPage.TopAppBar.Resources[item.Key] = item.Value;
        //    }
        //}

        //private void OnUpdateBottomAppBar()
        //{
        //    if (_isLoaded && BottomAppBar != null)
        //    {
        //        InternalPage.BottomAppBar = BottomAppBar;
        //        foreach (var item in Resources.Where(item => !(item.Value is DependencyObject)))
        //            InternalPage.BottomAppBar.Resources[item.Key] = item.Value;
        //    }
        //}

        //private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        //{
        //    if (InternalPage.TopAppBar != null)
        //        InternalPage.TopAppBar = null;

        //    if (InternalPage.BottomAppBar != null)
        //        InternalPage.BottomAppBar = null;
        //}

        protected internal virtual async Task OnNavigatingFromCoreAsync(MtNavigatingCancelEventArgs e)
        {
#if DEBUG
            Debug.WriteLine("1. OnNavigatingFrom:" + GetType().FullName);
#endif

            OnNavigatingFrom(e);

            var task = OnNavigatingFromAsync(e);
            if (task != null)
                await task;
        }

        protected internal void OnNavigatedFromCore(MtNavigationEventArgs e)
        {
#if DEBUG
            Debug.WriteLine("2. OnNavigatedFrom:" + GetType().FullName);
#endif

            OnNavigatedFrom(e);
            PageStateHandler.OnNavigatedFrom(e);
        }

        // internal methods ensure that base implementations of InternalOn* is always called
        // even if user does not call base.InternalOn* in the overridden On* method. 

        protected internal virtual void OnNavigatedToCore(MtNavigationEventArgs e)
        {
#if DEBUG
            Debug.WriteLine("3. OnNavigatedTo:" + GetType().FullName);
#endif

            OnNavigatedTo(e);
            PageStateHandler.OnNavigatedTo(e);
        }

        #region Default callback implementations

        /// <summary>
        ///     Navigates to the first page in the page stack.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void GoHome(object sender, RoutedEventArgs e)
        {
            Frame?.GoHomeAsync();
        }

        /// <summary>
        ///     Navigates to the previous page in the page stack.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void GoBack(object sender, RoutedEventArgs e)
        {
            if (Frame != null && Frame.CanGoBack)
                Frame.GoBackAsync();
        }

        /// <summary>
        ///     Navigates forward to the next page in the page stack.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void GoForward(object sender, RoutedEventArgs e)
        {
            if (Frame != null && Frame.CanGoForward)
                Frame.GoForwardAsync();
        }

        #endregion
    }
}