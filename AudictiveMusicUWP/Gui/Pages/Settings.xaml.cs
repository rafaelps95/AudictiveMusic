using AudictiveMusicUWP.Gui.UC;
using AudictiveMusicUWP.Gui.Util;
using AudictiveMusicUWP.Purchase;
using BackgroundAudioShared.Messages;
using ClassLibrary;
using ClassLibrary.Control;
using ClassLibrary.Helpers;
using ClassLibrary.Themes;
using InAppNotificationLibrary;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.Foundation.Metadata;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using static AudictiveMusicUWP.Gui.Pages.ThemeSelector;
using static AudictiveMusicUWP.Gui.UC.SettingsSection;
using static ClassLibrary.Helpers.Enumerators;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace AudictiveMusicUWP.Gui.Pages
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class Settings : Page
    {
        private bool loadedColors = false;
        private string NavigationArguments;
        private double PageWidth;
        private bool handledNavigation = false;
        private string NavigationPath;
        private bool storageInfoLoaded = false;

        private string[] colors = new string[] { "#FFFFB900", "#FFFF8C00", "#FFF7630C", "#FFCA5010", "#FFDA3B01", "#FFEF6950", "#FFD13438", "#FFFF4343", "#FFE74856", "#FFE81123", "#FFEA005E", "#FFC30052", "#FFE3008C", "#FFBF0077", "#FFC239B3", "#FF9A0089", "#FF0078D7", "#FF0063B1", "#FF8E8CD8", "#FF6B69D6", "#FF8764B8", "#FF744DA9", "#FFB146C2", "#FF881798", "#FF0099BC", "#FF2D7D9A", "#FF00B7C3", "#FF038387", "#FF00B294", "#FF018574", "#FF00CC6A", "#FF10893E", "#FF7A7574", "#FF5D5A58", "#FF68768A", "#FF515C6B", "#FF567C73", "#FF486860", "#FF498205", "#FF107C10", "#FF767676", "#FF4C4A48", "#FF69797E", "#FF4A5459", "#FF647C64", "#FF525E54", "#FF847545", "#FF7E735F" };
        private ObservableCollection<ThemeColor> ColorsList = new ObservableCollection<ThemeColor>();

        private SettingsPageContent view;

        public SettingsPageContent CurrentView
        {
            get
            {
                return view;
            }
            set
            {
                //if (value != view)
                //    GoToView(value);

                view = value;
            }
        }


        private NavigationMode NavMode
        {
            get;
            set;
        }

        public Settings()
        {
            PageWidth = 0;
            view = SettingsPageContent.Menu;
            this.SizeChanged += Settings_SizeChanged;
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            this.InitializeComponent();
        }

        private void Settings_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PageWidth = e.NewSize.Width;

            content.Width = e.NewSize.Width;

            menuScroll.Margin = new Thickness(0, 0, 0, ApplicationInfo.Current.FooterHeight);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavMode = e.NavigationMode;

            string arguments = string.Empty;

            if (e.Parameter != null)
                arguments = e.Parameter.ToString();

            if (string.IsNullOrWhiteSpace(arguments) == false)
            {
                this.NavigationArguments = arguments;
                this.NavigationPath = NavigationHelper.GetParameter(this.NavigationArguments, "path");
            }
            else
            {
                this.CurrentView = SettingsPageContent.Menu;
            }

            OpenPage(NavMode == NavigationMode.Back);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            LockScreenToggleSwitch.Toggled -= LockScreenToggleSwitch_Toggled;
            LimitedConnectionToggleSwitch.Toggled -= CelullarDownloadToggleSwitch_Toggled;
            WhatsNextNotification.Toggled -= WhatsNextNotification_Toggled;
            TapToResumeSwitch.Toggled -= TapToResumeSwitch_Toggled;
            TransparencyToggleSwitch.Toggled -= TransparencyToggleSwitch_Toggled;

            Storyboard sb = this.Resources["ExitPageTransition"] as Storyboard;
            sb.Begin();
        }


        private void LoadSettings()
        {
            if (ApplicationInfo.Current.GetDeviceFormFactorType() != ApplicationInfo.DeviceFormFactorType.Phone)
            {
                LockScreenSettingsItem.Visibility = Visibility.Collapsed;
            }
            else
            {
                //TransparencySettingsItem.Visibility = Visibility.Collapsed;
            }

            if (ApiInformation.IsMethodPresent("Windows.Storage.StorageLibrary", "RequestAddFolderAsync")
                && ApplicationInfo.Current.GetDeviceFormFactorType() != ApplicationInfo.DeviceFormFactorType.Phone)
            {
                ChooseLibrarySettingsItem.Visibility = Visibility.Visible;
            }

            PageTheme theme = ApplicationSettings.AppTheme;

            // REPLACES THE LEGACY 'DEFAULT' (FOLLOW SYSTEM THEME) SETTING IN CASE THE USER IS UPDATING FROM AN OLD VERSION
            if ((int)theme == 2)
            {
                theme = ApplicationSettings.AppTheme = PageTheme.Dark;
            }

            AppThemeSettingsItem.SelectedIndex = (int)theme;
            UpdateDropDownItemAdditionalInfo(AppThemeSettingsItem);
            BackgroundPreferencesSettingsItem.SelectedIndex = ApplicationSettings.ThemeBackgroundPreference;
            UpdateDropDownItemAdditionalInfo(BackgroundPreferencesSettingsItem);
            ColorSettingsItem.SelectedIndex = ApplicationSettings.ThemeColorPreference;
            UpdateDropDownItemAdditionalInfo(ColorSettingsItem);
            CustomColorSettingsSection.Visibility = ApplicationSettings.ThemeColorPreference == 2 ? Visibility.Visible : Visibility.Collapsed;

            TransparencyToggleSwitch.IsOn = ApplicationSettings.TransparencyEnabled;

            SendInfoToggleSwitch.IsOn = ApplicationSettings.DownloadEnabled;

            LimitedConnectionToggleSwitch.IsEnabled = ApplicationSettings.DownloadEnabled;

            LimitedConnectionToggleSwitch.IsOn = ApplicationSettings.CellularDownloadEnabled;

            LockScreenToggleSwitch.IsOn = ApplicationSettings.LockscreenEnabled;
            WhatsNextNotification.IsOn = ApplicationSettings.NextSongInActionCenterEnabled;
            TapToResumeSwitch.IsOn = ApplicationSettings.TapToResumeNotificationEnabled;

            SendScrobbleToggleSwitch.IsEnabled = LastFm.Current.IsAuthenticated;
            SendScrobbleToggleSwitch.IsOn = ApplicationSettings.IsScrobbleEnabled;

            LoadTimerSettings();

            AppThemeSettingsItem.SelectionChanged += AppThemeSettingsItem_SelectionChanged;
            TransparencyToggleSwitch.Toggled += TransparencyToggleSwitch_Toggled;
            BackgroundPreferencesSettingsItem.SelectionChanged += BackgroundPreferencesSettingsItem_SelectionChanged;
            ColorSettingsItem.SelectionChanged += ColorSettingsItem_SelectionChanged;   
            LockScreenToggleSwitch.Toggled += LockScreenToggleSwitch_Toggled;
            SendInfoToggleSwitch.Toggled += SendInfoToggleSwitch_Toggled;
            LimitedConnectionToggleSwitch.Toggled += CelullarDownloadToggleSwitch_Toggled;
            WhatsNextNotification.Toggled += WhatsNextNotification_Toggled;
            TapToResumeSwitch.Toggled += TapToResumeSwitch_Toggled;
            SendScrobbleToggleSwitch.Toggled += SendScrobble_Toggled;
            TimerBox.TextChanged += TimerBox_TextChanged;
            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch
            {

            }

            try
            {
                Package package = Package.Current;
                PackageId packageId = package.Id;
                PackageVersion version = packageId.Version;
                appName.Text = package.DisplayName;
                appVersion.Text = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
            }
            catch
            {

            }
        }

        private void TransparencyToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.TransparencyEnabled = TransparencyToggleSwitch.IsOn;
        }

        private void ColorSettingsItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationSettings.ThemeColorPreference = ColorSettingsItem.SelectedIndex;
            UpdateDropDownItemAdditionalInfo(ColorSettingsItem);

            if (ApplicationSettings.ThemeColorPreference == (int)ThemeColorSource.AlbumColor)
            {
                ApplicationSettings.CurrentThemeColor = ImageHelper.GetColorFromHex(ApplicationSettings.CurrentSong.HexColor);
            }
            else if (ApplicationSettings.ThemeColorPreference == (int)ThemeColorSource.AccentColor)
            {
                ApplicationSettings.CurrentThemeColor = ApplicationInfo.Current.CurrentSystemAccentColor;
            }
            else if (ApplicationSettings.ThemeColorPreference == (int)ThemeColorSource.CustomColor)
            {
                ApplicationSettings.CurrentThemeColor = ApplicationSettings.CustomThemeColor;
                SetSelectedColor();
            }
            else if (ApplicationSettings.ThemeColorPreference == (int)ThemeColorSource.NoColor)
            {
                ApplicationSettings.CurrentThemeColor = ApplicationInfo.Current.CurrentAppThemeColor(ApplicationSettings.AppTheme == PageTheme.Dark);
            }

            CustomColorSettingsSection.Visibility = ApplicationSettings.ThemeColorPreference == 2 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetSelectedColor()
        {
            foreach (ThemeColor tc in ColorsList)
            {
                tc.IsSelected = tc.Color == ApplicationSettings.CurrentThemeColor;
            }
        }

        private async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            CurrentStateChangedMessage currentStateChangedMessage;
            if (MessageService.TryParseMessage(e.Data, out currentStateChangedMessage))
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    LoadTimerSettings();
                });
            }
        }


        private void LoadTimerSettings()
        {
            if (BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Closed)
                return;

            TimerCancelButton.IsEnabled = true;

            if (ApplicationSettings.IsPlaybackTimerEnabled)
            {
                TimerBox.IsEnabled = TimerButton.IsEnabled = false;
                TimerBox.Text = ApplicationSettings.PlaybackTimerDuration.ToString();
                TimerBoxCaption.Text = ApplicationInfo.Current.Resources.GetString("TimerBoxCaptionApproximately").Replace("#", Convert.ToString(ApplicationSettings.PlaybackTimerDuration / 4));
            }
            else
            {
                TimerBox.IsEnabled = true;
                TimerButton.IsEnabled = false;
            }
        }

        private void TimerBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TimerBox.Text.Length == 0)
            {
                if (ApplicationSettings.IsPlaybackTimerEnabled)
                {
                    TimerButton.IsEnabled = true;
                    TimerBoxCaption.Text = "";
                }
                else
                {
                    TimerButton.IsEnabled = false;
                    TimerBoxCaption.Text = "";
                }
                return;
            }

            int number = Convert.ToInt32(TimerBox.Text);
            if (number >= 4 && number <= 60)
            {
                TimerBoxCaption.Text = ApplicationInfo.Current.Resources.GetString("TimerBoxCaptionApproximately").Replace("#", Convert.ToString(number / 4));
                TimerButton.IsEnabled = true;
            }
            else
            {
                TimerBoxCaption.Text = ApplicationInfo.Current.Resources.GetString("TimerBoxCaptionError");
                TimerButton.IsEnabled = false;
            }

        }

        private void OpenPage(bool reload)
        {
            try
            {
                Storyboard sb = this.Resources["OpenPageTransition"] as Storyboard;

                if (reload)
                {
                    layoutRootScale.ScaleX = layoutRootScale.ScaleY = 1.1;
                }

                sb.Begin();
            }
            catch
            {

            }
        }

        #region EVENTS


        private void LightRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.AppTheme = PageTheme.Light;
        }

        private void DarkRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void LockScreenToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.LockscreenEnabled = LockScreenToggleSwitch.IsOn;
        }

        private void CelullarDownloadToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.CellularDownloadEnabled = LimitedConnectionToggleSwitch.IsOn;
        }

        private void SendInfoToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.DownloadEnabled = SendInfoToggleSwitch.IsOn;
            LimitedConnectionToggleSwitch.IsEnabled = SendInfoToggleSwitch.IsOn;
        }

        private void WhatsNextNotification_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.NextSongInActionCenterEnabled = WhatsNextNotification.IsOn;
        }

        private void TapToResumeSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.TapToResumeNotificationEnabled = TapToResumeSwitch.IsOn;
        }

        private void SendScrobble_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.IsScrobbleEnabled = SendScrobbleToggleSwitch.IsOn;
        }



        #endregion

        private async void WindowsColorSettings_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings:personalization-colors", UriKind.RelativeOrAbsolute), new LauncherOptions() { DesiredRemainingView = Windows.UI.ViewManagement.ViewSizePreference.UseHalf });
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(About));
        }

        private void SettingsGroup_Click(object sender, EventArgs e)
        {
            SettingsGroup sg = sender as SettingsGroup;
            if (sg.ReferencesTo == SettingsPageContent.AppInfo)
            {
                Frame.Navigate(typeof(About));
                return;
            }
            this.CurrentView = sg.ReferencesTo;
        }

        private void SettingsGroup_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            foreach (SettingsGroup sg in content.Children.OfType<SettingsGroup>())
            {
                if (sg != sender as SettingsGroup)
                {
                    Canvas.SetZIndex(sg, 3);
                }
                else
                {
                    Canvas.SetZIndex(sg, 1);
                }
            }
        }

        private void SettingsGroup_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            foreach (SettingsGroup sg in content.Children.OfType<SettingsGroup>())
            {
                Canvas.SetZIndex(sg, 1);
            }
        }

        private async void LoadStorageInfo()
        {
            await Task.Delay(200);
            storageInfoLoaded = true;
            storageProgressBar.IsActive = true;

            if (ApplicationSettings.IsCollectionLoaded)
            {
                songsFound.Text = Ctr_Song.Current.GetAllSongsPaths().Count.ToString();
            }

            artistsSize.Text = albumsSize.Text = otherSize.Text = allSize.Text = string.Empty;

            await Task.Run(async () =>
            {
                StorageFolder appFolder = ApplicationData.Current.LocalFolder;

                int installSize = await Package.Current.InstalledLocation.CalculateSize();

                StorageFolder artists = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Artists", CreationCollisionOption.OpenIfExists);
                int artSize = await artists.CalculateSize();

                StorageFolder covers = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Covers", CreationCollisionOption.OpenIfExists);
                int albSize = await covers.CalculateSize();

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    artistsSize.Text = artSize + " MB";
                    albumsSize.Text = albSize + " MB";
                    otherSize.Text = installSize + " MB";
                    allSize.Text = installSize + albSize + artSize + " MB";

                    storageProgressBar.IsActive = false;
                });
            });
        }


        private async void OpenPageTransition_Completed(object sender, object e)
        {
            LoadSettings();
            if (this.NavMode != NavigationMode.Back)
            {
                await Task.Delay(200);
                HandleNavigation();
            }
        }

        private void HandleNavigation()
        {
            if (this.NavigationPath == "scrobble")
            {
                PlaybackNotificationSection.CurrentState = SettingsSection.State.Expanded;
            }
            else if (this.NavigationPath == "timer")
            {
                PlaybackNotificationSection.CurrentState = SettingsSection.State.Expanded;
            }
        }

        private void TimerButton_Click(object sender, RoutedEventArgs e)
        {
            int number = Convert.ToInt32(TimerBox.Text);
            SetPlaybackTimer(number);
        }

        private void SetPlaybackTimer(int minutes)
        {
            long starterTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            ApplicationSettings.PlaybackTimerDuration = minutes;
            ApplicationSettings.PlaybackTimerStartTime = starterTime;
            TimerBox.IsEnabled = TimerButton.IsEnabled = false;
        }

        private void TimerCancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelPlaybackTimer();
        }

        private void CancelPlaybackTimer()
        {
            TimerBox.IsEnabled = true;
            ApplicationSettings.PlaybackTimerDuration = 0;
            ApplicationSettings.PlaybackTimerStartTime = 0;
            TimerBox.Text = "";
        }

        private void Themes_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, typeof(ThemeSelector));
        }

        private void MoreThemesSettingsItem_ItemClick(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, typeof(ThemeSelector));
        }

        private async void RateAppSettingsItem_ItemClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(
new Uri($"ms-windows-store://review/?PFN={Package.Current.Id.FamilyName}"));
        }

        private async void SupportSettingsItem_ItemClick(object sender, RoutedEventArgs e)
        {
            var mailto = new Uri("mailto:?to=audictivemusic@outlook.com&subject=Audictive Music 10 Support&body=\n\n\n\nAudictive Music: " + ApplicationInfo.Current.AppVersion);
            await Launcher.LaunchUriAsync(mailto);
        }

        private async void DonateSettingsItem_ItemClick(object sender, RoutedEventArgs e)
        {
            DonateSettingsItem.IsEnabled = false;

            if (ApplicationInfo.Current.HasInternetConnection)
            {
                try
                {
                    var iapList = await PurchaseHelper.GetDonationsIAP();

                    string title = ApplicationInfo.Current.Resources.GetString("AskForDonationTitle");
                    string message = ApplicationInfo.Current.Resources.GetString("AskForDonationMessage");
                    string icon = "\uECA7";
                    string primaryButtonContent = ApplicationInfo.Current.Resources.GetString("Proceed");
                    InAppNotification inAppNotification = new InAppNotification(title, message, icon, primaryButtonContent);
                    inAppNotification.PrimaryButtonEnabled = false;

                    ComboBox box = new ComboBox()
                    {
                        Margin = new Thickness(10),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                    };
                    box.SelectionChanged += (snd, args) =>
                    {
                        if (box.SelectedItem != null)
                            inAppNotification.PrimaryButtonEnabled = true;
                    };

                    foreach (ProductListing product in iapList)
                    {
                        ComboBoxItem cbi = new ComboBoxItem()
                        {
                            Tag = product,
                            Content = product.FormattedPrice,
                        };
                        box.Items.Add(cbi);
                    }

                    inAppNotification.PrimaryButtonClicked += async (snd, args) =>
                    {
                        ProductListing product = ((ComboBoxItem)box.SelectedItem).Tag as ProductListing;

                        ProductPurchaseStatus result = await PurchaseHelper.PurchaseAsync(product);

                        if (result == ProductPurchaseStatus.Succeeded)
                        {
                            PurchaseHelper.ShowDonationThanksMessage();
                        }
                        else
                        {
                            PurchaseHelper.ShowDonationErrorMessage();
                        }
                    };

                    inAppNotification.SetCustomContent(box);
                    InAppNotificationHelper.ShowNotification(inAppNotification);
                }
                catch (Exception ex)
                {

                }
            }

            DonateSettingsItem.IsEnabled = true;

        }

        private void ChooseLibrarySettingsItem_ItemClick(object sender, RoutedEventArgs e)
        {
            StorageHelper.RequestLibraryPicker(this);
        }

        private void FindMusicSettingsItem_ItemClick(object sender, RoutedEventArgs e)
        {
            InAppNotification inAppNotification = new InAppNotification();
            inAppNotification.Title = ApplicationInfo.Current.Resources.GetString("ReloadCollectionWarningTitle");
            inAppNotification.Message = ApplicationInfo.Current.Resources.GetString("ReloadCollectionWarningContent");
            inAppNotification.Icon = "\uE777";
            inAppNotification.PrimaryButtonContent = ApplicationInfo.Current.Resources.GetString("ProceedAnyway");

            inAppNotification.PrimaryButtonClicked += (s, a) =>
            {
                NavigationHelper.Navigate(this, typeof(PreparingCollection), null, true);
                NavigationHelper.ClearBackstack(this, true);
            };

            InAppNotificationHelper.ShowNotification(inAppNotification);
        }

        private void BackgroundPreferencesSettingsItem_ItemClick(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(BackgroundPreferencesSettingsItem);
        }

        private void BackgroundPreferencesSettingsItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationSettings.ThemeBackgroundPreference = BackgroundPreferencesSettingsItem.SelectedIndex;
            UpdateDropDownItemAdditionalInfo(BackgroundPreferencesSettingsItem);
        }

        private void UpdateDropDownItemAdditionalInfo(SettingsItemDropDownList dropDownList)
        {
            ListBoxItem item = dropDownList.Items[dropDownList.SelectedIndex] as ListBoxItem;
            if (item != null)
            {
                dropDownList.AdditionalInfo = item.Content.ToString();
            }
        }

        private void AppThemeSettingsItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationSettings.AppTheme = (PageTheme)AppThemeSettingsItem.SelectedIndex;

            UpdateDropDownItemAdditionalInfo(AppThemeSettingsItem);
        }

        private void PrepareColorsList()
        {
            if (loadedColors)
                return;

            colorsList.ItemsSource = ColorsList;

            foreach (string str in colors)
            {
                ThemeColor color = new ThemeColor();
                color.Color = ImageHelper.GetColorFromHex(str);
                color.IsSelected = false;
                ColorsList.Add(color);
            }

            loadedColors = true;
        }


        private void colorsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var color = e.ClickedItem as ThemeColor;
            color.IsSelected = true;

            ApplicationSettings.CurrentThemeColor = color.Color;

            SetSelectedColor();
        }

        private void CustomColorSettingsSection_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            PrepareColorsList();

            if (CustomColorSettingsSection.CurrentState == State.Expanded)
            {
                if (colorsList.ItemsSource == null)
                {
                    colorsList.ItemsSource = ColorsList;
                }
            }
        }

        private async void PlaybackNotificationSection_ExpandCompleted(object sender)
        {
            if (string.IsNullOrWhiteSpace(this.NavigationArguments) == false && handledNavigation == false && this.NavMode != NavigationMode.Back)
            {
                if (this.NavigationPath == "scrobble")
                {
                    await Task.Delay(200);
                    menuScroll.ScrollToElement(SendScrobbleSettingsItem);
                    SendScrobbleSettingsItem.Focus(FocusState.Keyboard);
                }
                else if (this.NavigationPath == "timer")
                {
                    await Task.Delay(200);
                    menuScroll.ScrollToElement(TimerBox);
                    TimerBox.Focus(FocusState.Keyboard);
                }
            }
            handledNavigation = true;
        }

        private void CustomColorSettingsSection_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            colorsList.MaxWidth = e.NewSize.Width - 30;
        }

        private void DataManagementSection_ExpandCompleted(object sender)
        {
            Debug.WriteLine("EXPAND ANIMATION COMPLETED! LET'S LOAD STORAGE INFO!!");
            if (storageInfoLoaded == false)
                LoadStorageInfo();
        }

        private void StorageProgressBar_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("PROGRESS BAR LOADED!!");
        }
    }
}
