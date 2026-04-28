using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using iV2EX.Views;
using Microsoft.UI.Xaml.Controls;

namespace iV2EX.Util
{
    public static class PageStack
    {
        private static readonly Stack<PageInformation> PageContainer = new Stack<PageInformation>();

        public static bool CanGoBack => PageContainer.Count > 1;

        public static void Next(string from, string to, Type page, object param)
        {
            if (from == "Left" && to == "Left")
            {
            }
            else if (from == "Left" && to == "Right")
            {
                if (App.Window.PageFrame.ActualWidth < 600)
                {
                    MainPage.LeftPart.Visibility = Visibility.Collapsed;
                    MainPage.RightPart.Visibility = Visibility.Visible;
                }

                PageContainer.Clear();
                while (MainPage.RightPart.CanGoBack)
                {
                    MainPage.RightPart.GoBack();
                }
                PageContainer.Push(new PageInformation {From = "Left", To = "Right", PageType = typeof(BlankPage)});
                MainPage.RightPart.Navigate(page, param);
                PageContainer.Push(new PageInformation {From = "Left", To = "Right", PageType = page});
            }
            else if (from == "Right" && to == "Right")
            {
                MainPage.RightPart.Navigate(page, param);
                PageContainer.Push(new PageInformation {From = from, To = to, PageType = page});
            }

            MainWindow window = App.Window;
            window.UpdateBackButton();
        }

        public static void Back()
        {
            var page = PageContainer.Peek();
            if (App.Window.PageFrame.ActualWidth < 600 && page.From == "Left" &&
                page.To == "Right")
            {
                MainPage.LeftPart.Visibility = Visibility.Visible;
                MainPage.RightPart.Visibility = Visibility.Collapsed;
            }

            MainPage.RightPart.GoBack();
            PageContainer.Pop();
            MainWindow window = App.Window;
            window.UpdateBackButton();
        }

        public static void Clear()
        {
            PageContainer.Clear();
            MainWindow window = App.Window;
            window.UpdateBackButton();
        }
    }
}