using System;
using System.Collections.Generic;
using iV2EX.Views;

namespace iV2EX.Util
{
    public static class PageStack
    {
        private static readonly Stack<PageInformation> PageContainer = new Stack<PageInformation>();

        public static bool CanGoBack => PageContainer.Count > 1;

        public static bool IsLeftToRightActive { get; private set; }

        public static void Next(string from, string to, Type page, object param)
        {
            if (from == "Left" && to == "Right")
            {
                IsLeftToRightActive = true;
                MainPage.Instance?.SyncPaneVisibility();

                PageContainer.Clear();
                while (MainPage.RightPart.CanGoBack)
                {
                    MainPage.RightPart.GoBack();
                }
                PageContainer.Push(new PageInformation { From = "Left", To = "Right", PageType = typeof(BlankPage) });
                MainPage.RightPart.Navigate(page, param);
                PageContainer.Push(new PageInformation { From = "Left", To = "Right", PageType = page });
            }
            else if (from == "Right" && to == "Right")
            {
                MainPage.RightPart.Navigate(page, param);
                PageContainer.Push(new PageInformation { From = from, To = to, PageType = page });
            }

            UpdateWindowChrome();
        }

        public static void Back()
        {
            if (PageContainer.Count == 0) return;

            var page = PageContainer.Peek();
            if (page.From == "Left" && page.To == "Right")
            {
                IsLeftToRightActive = false;
            }

            MainPage.RightPart.GoBack();
            PageContainer.Pop();
            MainPage.Instance?.SyncPaneVisibility();
            UpdateWindowChrome();
        }

        public static void Clear()
        {
            PageContainer.Clear();
            IsLeftToRightActive = false;
            UpdateWindowChrome();
        }

        private static void UpdateWindowChrome()
        {
            var window = App.Window;
            if (window == null) return;
            window.UpdateBackButton();
            var currentType = PageContainer.Count > 0 ? PageContainer.Peek().PageType : null;
            window.UpdateTitle(GetSubtitleForPage(currentType));
        }

        private static string GetSubtitleForPage(Type pageType)
        {
            if (pageType == null || pageType == typeof(BlankPage))
                return MainPage.Instance?.ActiveSectionTitle;
            if (pageType == typeof(RepliesAndTopicView)) return "主题详情";
            if (pageType == typeof(OneNodeTopicsView)) return "节点话题";
            if (pageType == typeof(MemberView)) return "用户主页";
            if (pageType == typeof(WriteTopicView)) return "写新主题";
            if (pageType == typeof(BrowseHistoryView)) return "浏览历史";
            if (pageType == typeof(MoneyDetailView)) return "账户余额";
            if (pageType == typeof(PeopleNotificationView)) return "我的消息";
            if (pageType == typeof(PeopleTopicView)) return "收藏主题";
            if (pageType == typeof(PeopleFollowerView)) return "特别关注";
            if (pageType == typeof(PeopleNodeView)) return "收藏节点";
            if (pageType == typeof(ImageViewerPage)) return "图片查看";
            return null;
        }
    }
}
