using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.Util;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;

namespace iV2EX.Views
{
    public sealed partial class PeopleNodeView
    {
        public PeopleNodeView()
        {
            InitializeComponent();
            async Task<IEnumerable<NodeModel>> loadData()
            {
                var html = await ApiClient.GetFavoriteNodes();
                return new HtmlParser().ParseDocument(html).GetElementById("my-nodes").GetElementsByClassName("grid_item")
                    .Select(
                        child =>
                        {
                            var strs = child.TextContent.Split(' ');
                            return new NodeModel
                            {
                                Id = int.Parse(child.Id.Replace("n_", "")),
                                Name = child.GetAttribute("href").Replace("/go/", ""),
                                Image = child.QuerySelector("img").GetAttribute("src"),
                                Title = string.Join("", strs.Take(strs.Length - 1)),
                                Topics = int.Parse(strs.Last())
                            };
                        });
            }

            PeopleNodePage.Loaded += async (s, e) =>
            {
                PeopleNodeList.ItemsSource = await AsyncHelper.RetryAsync(() => loadData(), 5);
            };

            PeopleNodeList.ItemClick += (s, e) =>
            {
                if (e.ClickedItem is NodeModel item)
                    PageStack.Next("Right", "Right", typeof(OneNodeTopicsView), item);
            };
        }
    }
}
