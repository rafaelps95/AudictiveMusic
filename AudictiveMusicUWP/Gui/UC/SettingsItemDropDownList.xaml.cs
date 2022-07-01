using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AudictiveMusicUWP.Gui.UC
{
    public sealed partial class SettingsItemDropDownList : UserControl
    {
        public event RoutedEventHandler Toggled;
        public delegate void SettingsItemSelectionChangedEventArgs(object sender, SelectionChangedEventArgs e);

        public event SettingsItemSelectionChangedEventArgs SelectionChanged;

        public ItemCollection Items
        {
            get { return listBox.Items; }
            set { listBox.ItemsSource = value; }
        }

        //// Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ItemsProperty =
        //    DependencyProperty.Register("Items", typeof(ItemCollection), typeof(SettingsItemDropDownList), null);



        public bool IsToggleVisible
        {
            get { return (bool)GetValue(IsToggleVisibleProperty); }
            set { SetValue(IsToggleVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsToggleVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsToggleVisibleProperty =
            DependencyProperty.Register("IsToggleVisible", typeof(bool), typeof(SettingsItemDropDownList), new PropertyMetadata(false));

        private bool ignoreToggleEvent = false;

        public bool IsOn
        {
            get { return (bool)GetValue(IsOnProperty); }
            set
            {
                ignoreToggleEvent = true;
                toggle.IsOn = value;
                ignoreToggleEvent = false;

                SetValue(IsOnProperty, value);

                if (IsToggleVisible)
                {
                    if (!value)
                    {
                        CurrentState = State.Collapsed;
                        arrow.Opacity = 0.4;
                        listBox.SelectionChanged -= ListBox_SelectionChanged;
                        listBox.SelectedItem = null;
                        listBox.SelectionChanged += ListBox_SelectionChanged;
                    }
                    else
                        arrow.Opacity = 1;

                    button.IsEnabled = value;
                }
                else
                {
                    button.IsEnabled = true;
                    arrow.Opacity = 1;
                }

            }
        }

        // Using a DependencyProperty as the backing store for IsOn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOnProperty =
            DependencyProperty.Register("IsOn", typeof(bool), typeof(SettingsItemDropDownList), new PropertyMetadata(false));



        public int SelectedIndex
        {
            get
            {
                return listBox.SelectedIndex;
            }
            set
            {
                listBox.SelectionChanged -= ListBox_SelectionChanged;
                listBox.SelectedIndex = value;
                listBox.SelectionChanged += ListBox_SelectionChanged;
            }
        }


        private double PanelActualHeight
        {
            get;
            set;
        }


        public enum State
        {
            Collapsed,
            Expanded
        }

        private State fakeState;

        public State CurrentState
        {
            get
            {
                return fakeState;
            }
            set
            {
                fakeState = value;
                ChangeView(value);
            }
        }

        public void ChangeView(State state)
        {
            PanelActualHeight = ListContainer.Height;

            SineEase se = new SineEase()
            {
                EasingMode = EasingMode.EaseInOut,
            };

            if (CurrentState == State.Collapsed)
            {
                Panel.IsHitTestVisible = false;

                Storyboard sb = new Storyboard();
                DoubleAnimation da = new DoubleAnimation()
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EnableDependentAnimation = true,
                    EasingFunction = se
                };

                Storyboard.SetTarget(da, Panel);
                Storyboard.SetTargetProperty(da, "Opacity");

                DoubleAnimation da1 = new DoubleAnimation()
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EnableDependentAnimation = true,
                    EasingFunction = se
                };

                Storyboard.SetTarget(da1, ArrowIconRotation);
                Storyboard.SetTargetProperty(da1, "Angle");

                sb.Children.Add(da);
                sb.Children.Add(da1);

                sb.Completed += (snd, args) =>
                {
                    Panel.Visibility = Visibility.Collapsed;
                };

                sb.Begin();

                //Storyboard sb = this.Resources["ExpandAnimation"] as Storyboard;
                //sb.Begin();

            }
            else if (CurrentState == State.Expanded)
            {
                Panel.Visibility = Visibility.Visible;

                //Panel.Height = PanelActualHeight;
                Panel.IsHitTestVisible = true;
                Storyboard sb = new Storyboard();
                DoubleAnimation da = new DoubleAnimation()
                {
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EnableDependentAnimation = true,
                    EasingFunction = se
                };

                Storyboard.SetTarget(da, Panel);
                Storyboard.SetTargetProperty(da, "Opacity");

                DoubleAnimation da1 = new DoubleAnimation()
                {
                    To = 180,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EnableDependentAnimation = true,
                    EasingFunction = se
                };

                Storyboard.SetTarget(da1, ArrowIconRotation);
                Storyboard.SetTargetProperty(da1, "Angle");

                sb.Children.Add(da);
                sb.Children.Add(da1);

                sb.Begin();
                //}
            }
        }


        public SolidColorBrush BackgroundBrush
        {
            get { return (SolidColorBrush)GetValue(BackgroundBrushProperty); }
            set { SetValue(BackgroundBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundBrushProperty =
            DependencyProperty.Register("BackgroundBrush", typeof(SolidColorBrush), typeof(SettingsItemDropDownList), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));


        public SettingsItemDropDownList()
        {
            this.Loaded += SettingsItemDropDownList_Loaded;
            this.InitializeComponent();
            this.GotFocus += SettingsItemDropDownList_GotFocus;
            this.LostFocus += SettingsItemDropDownList_LostFocus;
            this.SizeChanged += SettingsItemDropDownList_SizeChanged;
            Panel.Height = 0;
            fakeState = new State();
        }

        private void SettingsItemDropDownList_LostFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void SettingsItemDropDownList_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.FocusState == FocusState.Keyboard || this.FocusState == FocusState.Programmatic)
                button.Focus(FocusState.Keyboard);
        }

        private void SettingsItemDropDownList_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateTextLayout();


            //listBox.SelectionChanged += ListBox_SelectionChanged;
        }

        private void UpdateTextLayout()
        {
            if (string.IsNullOrWhiteSpace(this.AdditionalInfo))
            {
                Grid.SetRowSpan(text, 2);
                additionalInfoTB.Visibility = Visibility.Collapsed;
            }
            else
            {
                Grid.SetRowSpan(text, 1);
                additionalInfoTB.Visibility = Visibility.Visible;
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentState = State.Collapsed;
            SelectionChanged?.Invoke(this, e);
        }

        private void SettingsItemDropDownList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (CurrentState == State.Expanded)
                Panel.Height = ListContainer.Height;
        }

        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(string), typeof(SettingsItemDropDownList), new PropertyMetadata(string.Empty));

        public int IconSize
        {
            get { return ((int)GetValue(IconSizeProperty)); }
            set
            {
                SetValue(IconSizeProperty, value);
            }
        }

        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register("IconSize", typeof(int), typeof(SettingsItemDropDownList), new PropertyMetadata(16));


        public static readonly DependencyProperty textProperty =
            DependencyProperty.Register("Text",
                typeof(string),
                typeof(SettingsItemDropDownList),
                new PropertyMetadata(string.Empty));

        public string Text
        {
            get { return (string)this.GetValue(textProperty); }
            set
            {
                this.SetValue(textProperty, value);

                NotifyPropertyChanged("Text");
            }
        }



        public string AdditionalInfo
        {
            get { return (string)GetValue(AdditionalInfoProperty); }
            set
            {
                SetValue(AdditionalInfoProperty, value);
                UpdateTextLayout();
            }
        }

        // Using a DependencyProperty as the backing store for AdditionalInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AdditionalInfoProperty =
            DependencyProperty.Register("AdditionalInfo", typeof(string), typeof(SettingsItemDropDownList), new PropertyMetadata(string.Empty));

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        private void ChangeState_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentState == State.Collapsed)
                CurrentState = State.Expanded;
            else
                CurrentState = State.Collapsed;
        }

        private void Toggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (ignoreToggleEvent)
                return;

            this.IsOn = toggle.IsOn;
            Toggled?.Invoke(this, e);
        }
    }
}
