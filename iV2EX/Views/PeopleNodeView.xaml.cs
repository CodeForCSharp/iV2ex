using System;
using System.Linq;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using AngleSharp.Parser.Html;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.Util;
using MyToolkit.Paging;

namespace iV2EX.Views
{
    public sealed partial class PeopleNodeView : MtPage
    {
        public PeopleNodeView()
        {
            InitializeComponent();
            var loadData = Observable.FromAsync(async x =>
            {
                var html = await ApiClient.GetFavoriteNodes();
                return new HtmlParser().Parse(html).GetElementById("MyNodes").GetElementsByClassName("grid_item")
                    .Select(
                        child =>
                        {
                            var strs = child.TextContent.Split(' ');
                            return new NodeModel
                            {
                                Id = int.Parse(child.Id.Replace("n_", "")),
                                Name = child.GetAttribute("href").Replace("/go/", ""),
                                Image = $"http:{child.QuerySelector("img").GetAttribute("src")}",
                                Title = string.Join("", strs.Take(strs.Length - 1)),
                                Topics = int.Parse(strs.Last())
                            };
                        });
            }).Retry();
            var load = Observable.FromEventPattern<RoutedEventArgs>(PeopleNodePage, nameof(PeopleNodePage.Loaded))
                .Publish(x => loadData)
                .ObserveOnDispatcher()
                .Subscribe(x => PeopleNodeList.ItemsSource = x);
            var click = Observable
                .FromEventPattern<ItemClickEventArgs>(PeopleNodeList, nameof(PeopleNodeList.ItemClick))
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    var item = x.EventArgs.ClickedItem as NodeModel;
                    PageStack.Next("Right", "Right", typeof(OneNodeTopicsView), item);
                });
        }
    }
}