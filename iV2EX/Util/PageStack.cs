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
            /*
                TODO WinUI3 应用中不存在标题栏中的默认后退按钮。
               该工具已在 MainWindow.xaml.cs 文件中生成自定义后退按钮。
               你可以随意编辑其位置、行为并改用自定义后退按钮。
               读取: https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/case-study-1#restoring-back-button-functionality
            */

            //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = CanGoBack
            //    ? AppViewBackButtonVisibility.Visible
            //    : AppViewBackButtonVisibility.Collapsed;
            MainWindow window = App.Window;
            window.BackButton.Visibility = CanGoBack ? Visibility.Visible : Visibility.Collapsed;
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
            window.BackButton.Visibility = CanGoBack ? Visibility.Visible : Visibility.Collapsed;
        }

        public static void Clear()
        {
            PageContainer.Clear();
            MainWindow window = App.Window;
            window.BackButton.Visibility = CanGoBack ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}