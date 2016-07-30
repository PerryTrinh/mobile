﻿using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Bit.App.Abstractions;
using Bit.App.Controls;
using Bit.App.Resources;
using Plugin.Connectivity.Abstractions;
using Xamarin.Forms;
using XLabs.Ioc;
using Plugin.Settings.Abstractions;

namespace Bit.App.Pages
{
    public class SettingsSyncPage : ExtendedContentPage
    {
        private readonly ISyncService _syncService;
        private readonly IUserDialogs _userDialogs;
        private readonly IConnectivity _connectivity;
        private readonly ISettings _settings;

        public SettingsSyncPage()
        {
            _syncService = Resolver.Resolve<ISyncService>();
            _userDialogs = Resolver.Resolve<IUserDialogs>();
            _connectivity = Resolver.Resolve<IConnectivity>();
            _settings = Resolver.Resolve<ISettings>();

            Init();
        }

        public Label LastSyncLabel { get; set; }

        public void Init()
        {
            var syncButton = new Button
            {
                Text = "Sync Vault Now",
                Command = new Command(async () => await SyncAsync()),
                Style = (Style)Application.Current.Resources["btn-primaryAccent"]
            };

            LastSyncLabel = new Label
            {
                Style = (Style)Application.Current.Resources["text-muted"],
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                HorizontalTextAlignment = TextAlignment.Center
            };

            SetLastSync();

            var stackLayout = new StackLayout
            {
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Children = { syncButton, LastSyncLabel },
                Padding = new Thickness(15, 0)
            };

            Title = "Sync";
            Content = stackLayout;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if(!_connectivity.IsConnected)
            {
                AlertNoConnection();
            }
        }

        private void SetLastSync()
        {
            var lastSyncDate = _settings.GetValueOrDefault<DateTime?>(Constants.SettingLastSync);
            LastSyncLabel.Text = "Last Sync: " + lastSyncDate?.ToString() ?? "Never";
        }

        public async Task SyncAsync()
        {
            if(!_connectivity.IsConnected)
            {
                AlertNoConnection();
                return;
            }

            _userDialogs.ShowLoading("Syncing...", MaskType.Black);
            var succeeded = await _syncService.FullSyncAsync();
            _userDialogs.HideLoading();
            if(succeeded)
            {
                _userDialogs.Toast("Syncing complete.");
            }
            else
            {
                _userDialogs.Toast("Syncing failed.");
            }

            SetLastSync();
        }

        public void AlertNoConnection()
        {
            DisplayAlert(AppResources.InternetConnectionRequiredTitle, AppResources.InternetConnectionRequiredMessage, AppResources.Ok);
        }
    }
}
