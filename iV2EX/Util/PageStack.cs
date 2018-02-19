using System;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml;
using iV2EX.Views;
using MyToolkit.Paging;

namespace iV2EX.Util
{
    public static class PageStack
    {
        private static readonly Stack<PageInformation> PageContainer = new Stack<PageInformation>();

        public static bool CanGoBack => PageContainer.Count > 1;

        public static async void Next(string from, string to, Type page, object param)
        {
            if (from == "Left" && to == "Left")
            {
            }
            else if (from == "Left" && to == "Right")
            {
                if (Window.Current.Content is MtFrame mtFrame && mtFrame.ActualWidth < 600)
                {
                    MainPage.LeftPart.Visibility = Visibility.Collapsed;
                    MainPage.RightPart.Visibility = Visibility.Visible;
                }

                PageContainer.Clear();
                await MainPage.RightPart.GoHomeAsync();
                PageContainer.Push(new PageInformation {From = "Left", To = "Right", PageType = typeof(BlankPage)});
                await MainPage.RightPart.NavigateAsync(page, param);
                PageContainer.Push(new PageInformation {From = "Left", To = "Right", PageType = page});
            }
            else if (from == "Right" && to == "Right")
            {
                await MainPage.RightPart.NavigateAsync(page, param);
                PageContainer.Push(new PageInformation {From = from, To = to, PageType = page});
            }

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = CanGoBack
                ? AppViewBackButtonVisibility.Visible
                : AppViewBackButtonVisibility.Collapsed;
        }

        public static void Back()
        {
            var page = PageContainer.Peek();
            if (Window.Current.Content is MtFrame mtFrame && mtFrame.ActualWidth < 600 && page.From == "Left" &&
                page.To == "Right")
            {
                MainPage.LeftPart.Visibility = Visibility.Visible;
                MainPage.RightPart.Visibility = Visibility.Collapsed;
            }

            MainPage.RightPart.GoBackAsync();
            PageContainer.Pop();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = CanGoBack
                ? AppViewBackButtonVisibility.Visible
                : AppViewBackButtonVisibility.Collapsed;
        }

        public static void Clear()
        {
            PageContainer.Clear();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = CanGoBack
                ? AppViewBackButtonVisibility.Visible
                : AppViewBackButtonVisibility.Collapsed;
        }
    }
}