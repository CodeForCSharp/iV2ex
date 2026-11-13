using System.Linq;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.Util;
using System.Collections.Generic;
using AngleSharp.Html.Parser;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace iV2EX.Views
{
    public sealed partial class PeopleNodeView
    {
        public PeopleNodeView()
        {
            InitializeComponent();
            async Task<List<NodeModel>> loadData()
            {
                var html = await ApiClient.GetFavoriteNodes();
                return new HtmlParser().ParseDocument(html).GetElementById("my-nodes").GetElementsByClassName("fav-node")
                    .Select(
                        a =>
                        {
                            var topicsText = a.QuerySelector("span.f12")?.TextContent ?? "0";
                            return new NodeModel
                            {
                                Id = int.Parse(a.Id.Replace("n_", "")),
                                Name = a.GetAttribute("href").Replace("/go/", ""),
                                Image = a.QuerySelector("img").GetAttribute("src"),
                                Title = a.QuerySelector("span.fav-node-name")?.TextContent ?? "",
                                Topics = int.Parse(topicsText.Trim())
                            };
                        }).ToList();
            }

            PeopleNodePage.Loaded += async (s, e) =>
            {
                LoadingRing.IsActive = true;
                LoadingRing.Visibility = Visibility.Visible;
                try
                {
                    PeopleNodeList.ItemsSource = await AsyncHelper.RetryAsync(() => loadData(), 5);
                }
                finally
                {
                    LoadingRing.IsActive = false;
                    LoadingRing.Visibility = Visibility.Collapsed;
                }
            };

            PeopleNodeList.ItemClick += (s, e) =>
            {
                if (e.ClickedItem is NodeModel item)
                    PageStack.Next("Right", "Right", typeof(OneNodeTopicsView), item);
            };
        }
    }
}
