﻿using System;
using System.Linq;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
        private List<IDisposable> _events;
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
            var load = Observable.FromEventPattern<RoutedEventArgs>(PeopleNodePage, nameof(PeopleNodePage.Loaded))
                .SelectMany(x => loadData())
                .Retry(10)
                .ObserveOnCoreDispatcher()
                .Subscribe(x => PeopleNodeList.ItemsSource = x);
            var click = Observable
                .FromEventPattern<ItemClickEventArgs>(PeopleNodeList, nameof(PeopleNodeList.ItemClick))
                .Select(x => x.EventArgs.ClickedItem as NodeModel)
                .ObserveOnCoreDispatcher()
                .Subscribe(x => PageStack.Next("Right", "Right", typeof(OneNodeTopicsView), x));

            _events = new List<IDisposable> { load, click };
        }

        protected internal override void OnDestroy()
        {
            base.OnDestroy();
            _events.ForEach(x => x.Dispose());
        }
    }
}