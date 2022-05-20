using ClassLibrary.Helpers;
using Microsoft.Graphics.Canvas.Effects;
using RPSToolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AudictiveMusicUWP.Gui.UC
{
    public sealed partial class SelectedItemsBar : UserControl
    {
        public enum BarMode
        {
            Enabled,
            Disabled
        }

        public enum PlayMode
        {
            Play,
            PlayNext
        }

        public enum AddMode
        {
            AddToQueue,
            AddToPlaylist
        }

        public delegate void PlaySelectedEventArgs(object sender, PlayMode playMode);
        public event PlaySelectedEventArgs PlaySelected;
        public delegate void AddSelectedEventArgs(object sender, AddMode addMode);
        public event AddSelectedEventArgs AddSelected;
        public event RoutedEventHandler ShareSelected;
        public delegate void SelectionModeChangedEventArgs(object sender, BarMode barMode);
        public event SelectionModeChangedEventArgs SelectionModeChanged;
        public event RoutedEventHandler ClearRequest;
        public event RoutedEventHandler SelectAllRequest;

        Compositor _compositor;
        SpriteVisual _sprite;
        CompositionEffectBrush _brush;

        public SelectedItemsBar()
        {
            this.Loaded += SelectedItemsBar_Loaded;
            this.InitializeComponent();
        }

        private void SelectedItemsBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_sprite != null)
                _sprite.Size = e.NewSize.ToVector2();
        }

        private void SelectedItemsBar_Loaded(object sender, RoutedEventArgs e)
        {
            playMenuFlyoutItem.Text = playButton.Text = ApplicationInfo.Current.Resources.GetString("Play");
            playNextMenuFlyoutItem.Text = addNextButton.Text = ApplicationInfo.Current.Resources.GetString("PlayNext");
            addMenuFlyoutItem.Text = ApplicationInfo.Current.Resources.GetString("AddTo");
            addToQueueMenuFlyoutItem.Text = ApplicationInfo.Current.Resources.GetString("AddToPlaylist");
            addToPlaylistMenuFlyoutItem.Text = ApplicationInfo.Current.Resources.GetString("AddToPlaylistFile");
            shareMenuFlyoutItem.Text = ApplicationInfo.Current.Resources.GetString("Share");

            UpdateView();

            shadowGrid.ApplyShadow(new Vector3(0, 5, 2));

            ApplicationAccentColor aac = App.Current.Resources["ApplicationAccentColor"] as ApplicationAccentColor;
            aac.PropertyChanged += Aac_PropertyChanged;
        }

        private void Aac_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            playButton.IsMainAction = false;
            playButton.IsMainAction = true;
        }

        public BarMode SelectionMode
        {
            get { return (BarMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value);
                if (value == BarMode.Disabled)
                    DisableSelectionMode();
                else
                    EnableSelectionMode();

                SelectionModeChanged?.Invoke(this, value);
            }
        }

        // Using a DependencyProperty as the backing store for SelectionMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionModeProperty =
            DependencyProperty.Register("SelectionMode", typeof(BarMode), typeof(SelectedItemsBar), new PropertyMetadata(BarMode.Disabled));




        public int SelectedItemsCount
        {
            get { return (int)GetValue(SelectedItemsCountProperty); }
            set
            {
                SetValue(SelectedItemsCountProperty, value);
                if (value > 0)
                {
                    SelectedItemsCountText = value + " " + ApplicationInfo.Current.GetSingularPlural(value, "ItemSelected");
                    playButton.IsEnabled = addNextButton.IsEnabled = addButton.IsEnabled = moreButton.IsEnabled = true;
                    UpdateView();
                }
                else
                {
                    SelectedItemsCountText = "";
                    playButton.IsEnabled = addNextButton.IsEnabled = addButton.IsEnabled = moreButton.IsEnabled = false;
                    DisableSelectionMode();
                }
            }
        }

        // Using a DependencyProperty as the backing store for SelectedItemsCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsCountProperty =
            DependencyProperty.Register("SelectedItemsCount", typeof(int), typeof(SelectedItemsBar), new PropertyMetadata(0));



        private string SelectedItemsCountText
        {
            get { return (string)GetValue(SelectedItemsCountTextProperty); }
            set { SetValue(SelectedItemsCountTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItemsCountText.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty SelectedItemsCountTextProperty =
            DependencyProperty.Register("SelectedItemsCountText", typeof(string), typeof(SelectedItemsBar), new PropertyMetadata(""));

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlaySelected?.Invoke(this, PlayMode.Play);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void AddToQueueButton_Click(object sender, RoutedEventArgs e)
        {
            AddSelected?.Invoke(this, AddMode.AddToQueue);
        }

        private void AddToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            AddSelected?.Invoke(this, AddMode.AddToPlaylist);
        }

        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearRequest?.Invoke(this, new RoutedEventArgs());
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SelectAllRequest?.Invoke(this, new RoutedEventArgs());
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ClearRequest?.Invoke(this, new RoutedEventArgs());
            DisableSelectionMode();
        }

        private void AddNextButton_Click(object sender, RoutedEventArgs e)
        {
            PlaySelected?.Invoke(this, PlayMode.PlayNext);
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            ShareSelected?.Invoke(this, new RoutedEventArgs());
        }

        private void ButtonsArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateView();
            //tempTB.Text = e.NewSize.Width.ToString();
        }

        private void UpdateView()
        {
            int availableWidth = Convert.ToInt32(buttonsArea.ActualWidth);
            double visibleButtonsWidth = 0;
            double totalButtonsWidth = addButton.ActualWidth + addNextButton.ActualWidth + playButton.ActualWidth;

            if (SelectedItemsCount == 0)
                return;

            if (addButton.Visibility == Visibility.Visible)
                visibleButtonsWidth += addButton.ActualWidth;

            if (addNextButton.Visibility == Visibility.Visible)
                visibleButtonsWidth += addNextButton.ActualWidth;

            if (playButton.Visibility == Visibility.Visible)
                visibleButtonsWidth += playButton.ActualWidth;

            if (availableWidth <= Convert.ToInt32(visibleButtonsWidth))
            {
                addButton.Visibility = Visibility.Collapsed;
                addMenuFlyoutItem.Visibility = Visibility.Visible;
            }

            int d = Convert.ToInt32(playButton.ActualWidth + addNextButton.ActualWidth);

            if (availableWidth <= d)
            {
                addNextButton.Visibility = Visibility.Collapsed;
                playNextMenuFlyoutItem.Visibility = Visibility.Visible;
            }
            else
            {
                addNextButton.Visibility = Visibility.Visible;
                playNextMenuFlyoutItem.Visibility = Visibility.Collapsed;
            }

            if (availableWidth < Convert.ToInt32(playButton.ActualWidth))
            {
                playButton.Visibility = Visibility.Collapsed;
                playMenuFlyoutItem.Visibility = Visibility.Visible;
            }
            else
            {
                playButton.Visibility = Visibility.Visible;
                playMenuFlyoutItem.Visibility = Visibility.Collapsed;
            }

            if (availableWidth >= Convert.ToInt32(playButton.ActualWidth + addNextButton.ActualWidth + addButton.ActualWidth) && addButton.Visibility == Visibility.Collapsed)
            {
                addButton.Visibility = Visibility.Visible;
                addMenuFlyoutItem.Visibility = Visibility.Collapsed;
            }

        }

        private void selectButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectionMode == BarMode.Disabled)
                SelectionMode = BarMode.Enabled;
            else
                SelectionMode = BarMode.Disabled;
        }

        private void EnableSelectionMode()
        {
            checkBox.Checked += CheckBox_Checked;
            checkBox.Unchecked += CheckBox_Unchecked;
            shadowGrid.Visibility = checkBox.Visibility = buttonsArea.Visibility = moreButton.Visibility = Visibility.Visible;
            selectButton.Icon = "\uE624";
            UpdateView();
        }

        private void DisableSelectionMode()
        {
            checkBox.Checked -= CheckBox_Checked;
            checkBox.Unchecked -= CheckBox_Unchecked;
            checkBox.IsChecked = false;
            shadowGrid.Visibility = checkBox.Visibility = buttonsArea.Visibility = moreButton.Visibility = Visibility.Collapsed;
            selectButton.Icon = "\uE1EF";
        }
    }
}
