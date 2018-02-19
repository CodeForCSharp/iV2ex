﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Windows.UI.Xaml.Controls;
using AngleSharp.Parser.Html;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.TupleModel;
using iV2EX.Util;

namespace iV2EX.Views
{
    public partial class PeopleTopicView
    {
        public PeopleTopicView()
        {
            InitializeComponent();
            var click = Observable
                .FromEventPattern<ItemClickEventArgs>(PeopleTopicsList, nameof(PeopleTopicsList.ItemClick))
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    var item = x.EventArgs.ClickedItem as TopicModel;
                    PageStack.Next("Right", "Right", typeof(RepliesAndTopicView), item.Id);
                });
            NotifyData.LoadDataTask = async count =>
            {
                var html = await ApiClient.GetFavoriteTopics(NotifyData.CurrentPage);
                var dom = new HtmlParser().Parse(html);
                return new PagesBaseModel<TopicModel>
                {
                    Pages = DomParse.ParseMaxPage(dom),
                    Entity = DomParse.ParseTopics(dom) ?? new List<TopicModel>()
                };
            };
        }

        private IncrementalLoadingCollection<TopicModel> NotifyData { get; } =
            new IncrementalLoadingCollection<TopicModel>();
    }
}