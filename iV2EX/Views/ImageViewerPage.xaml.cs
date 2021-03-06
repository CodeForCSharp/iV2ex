﻿using System;
using System.IO;
using System.Reactive.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Microsoft.Toolkit.Uwp.UI;
using System.Collections.Generic;

namespace iV2EX.Views
{
    public sealed partial class ImageViewerPage
    {
        private StorageFile _file;
        private string _imageUrl;
        private List<IDisposable> _events;

        public ImageViewerPage()
        {
            InitializeComponent();
            DataTransferManager.GetForCurrentView().DataRequested += (s, e) =>
            {
                e.Request.Data.Properties.Title = "分享自iV2EX";
                e.Request.Data.SetText(_imageUrl);
                e.Request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(_file));
            };
            var share = Observable.FromEventPattern<RoutedEventArgs>(ShareImage, nameof(ShareImage.Click))
                .ObserveOnCoreDispatcher()
                .Subscribe(x =>
                {
                    if (DataTransferManager.IsSupported()) DataTransferManager.ShowShareUI();
                });
            var save = Observable.FromEventPattern<RoutedEventArgs>(SaveImage, nameof(SaveImage.Click))
                .ObserveOnCoreDispatcher()
                .Subscribe(async x =>
                {
                    try
                    {
                        var library = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
                        var path = await library.SaveFolder.CreateFolderAsync("iV2EX",
                            CreationCollisionOption.OpenIfExists);
                        await _file.CopyAsync(path, _file.Name + Path.GetExtension(_imageUrl),
                            NameCollisionOption.ReplaceExisting);
                        ToastTips.ShowTips("已经保存到图片库");
                    }
                    catch
                    {
                        ToastTips.ShowTips("保存失败");
                    }
                });
            var menu = Observable.FromEventPattern<TappedRoutedEventArgs>(MenuItemPanel, nameof(MenuItemPanel.Tapped))
                .ObserveOnCoreDispatcher()
                .Subscribe(x => MenuItemPanel.ContextFlyout.ShowAt(MenuItemPanel));

            _events = new List<IDisposable> { share, save, menu };
        }

        protected internal override void OnDestroy()
        {
            base.OnDestroy();
            _events.ForEach(x => x.Dispose());
        }

        protected internal override async void OnCreate(object parameter)
        {
            _imageUrl = parameter as string;
            ImagePanel.Source = _imageUrl;
            await ImageCache.Instance.PreCacheAsync(new Uri(_imageUrl));
            _file = await ImageCache.Instance.GetFileFromCacheAsync(new Uri(_imageUrl));
            base.OnCreate(parameter);
        }
    }
}