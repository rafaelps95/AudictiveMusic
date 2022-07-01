using AudictiveMusicUWP.Gui.Util;
using RPSToolkit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace AudictiveMusicUWP.Gui.UC
{
    [ContentProperty(Name = "Children")]
    public sealed partial class SettingsExpandableToggleItem : UserControl, INotifyPropertyChanged
    {
        public event RoutedEventHandler Toggled;
        public delegate void StateChangedEventArgs(object sender, RoutedEventArgs e);
        public event StateChangedEventArgs CurrentStateChanged;
        public delegate void AnimationCompletedEventArgs(object sender);
        public event AnimationCompletedEventArgs ExpandCompleted;
        public event AnimationCompletedEventArgs CollapseCompleted;


        private bool ignoreToggleEvent = false;

        public bool IsOn
        {
            get { return (bool)GetValue(IsOnProperty); }
            set
            {
                ignoreToggleEvent = true;
                SetValue(IsOnProperty, value);
                if (!value)
                {
                    CurrentState = State.Collapsed;
                    arrow.Opacity = 0.4;
                }
                else
                    arrow.Opacity = 1;

                button.IsEnabled = value;
                toggle.IsOn = value;
                ignoreToggleEvent = false;
            }
        }

        public bool IsToggleVisible
        {
            get { return (bool)GetValue(IsToggleVisibleProperty); }
            set { SetValue(IsToggleVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsToggleVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsToggleVisibleProperty =
            DependencyProperty.Register("IsToggleVisible", typeof(bool), typeof(SettingsExpandableToggleItem), new PropertyMetadata(true));

        // Using a DependencyProperty as the backing store for IsOn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOnProperty =
            DependencyProperty.Register("IsOn", typeof(bool), typeof(SettingsExpandableToggleItem), new PropertyMetadata(false));

        public SolidColorBrush ExpandedBackgroundBrush
        {
            get { return (SolidColorBrush)GetValue(ExpandedBackgroundBrushProperty); }
            set { SetValue(ExpandedBackgroundBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExpandedBackgroundBrushProperty =
            DependencyProperty.Register("ExpandedBackgroundBrush", typeof(SolidColorBrush), typeof(SettingsExpandableToggleItem), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));



        public SolidColorBrush BackgroundBrush
        {
            get { return (SolidColorBrush)GetValue(BackgroundBrushProperty); }
            set { SetValue(BackgroundBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundBrushProperty =
            DependencyProperty.Register("BackgroundBrush", typeof(SolidColorBrush), typeof(SettingsExpandableToggleItem), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));




        public enum PanelOrientation
        {
            Horizontal,
            Vertical
        }

        private PanelOrientation fakeOrientation;

        public PanelOrientation Orientation
        {
            get
            {
                return fakeOrientation;
            }
            set
            {
                fakeOrientation = value;
                if (value == PanelOrientation.Horizontal)
                    Items.Orientation = Windows.UI.Xaml.Controls.Orientation.Horizontal;
                else
                    Items.Orientation = Windows.UI.Xaml.Controls.Orientation.Vertical;
            }
        }


        public static readonly DependencyProperty orientationProperty =
            DependencyProperty.Register("Orientation",
                typeof(PanelOrientation),
                typeof(SettingsExpandableToggleItem),
                new PropertyMetadata(PanelOrientation.Vertical));


        public static readonly DependencyProperty appsettingsproperty =
            DependencyProperty.Register("AppSettings",
                typeof(string),
                typeof(SettingsExpandableToggleItem),
                new PropertyMetadata(""));

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

        private double PanelActualHeight
        {
            get;
            set;
        }

        public SettingsExpandableToggleItem()
        {
            this.Loaded += SettingsExpandableItem_Loaded;
            this.InitializeComponent();
            this.SizeChanged += SettingsExpandableItem_SizeChanged;
            this.GotFocus += SettingsExpandableToggleItem_GotFocus;
            this.LostFocus += SettingsExpandableToggleItem_LostFocus;
            //Panel.Height = 0;
            CurrentState = new State();
            Children = Items.Children;
            Orientation = new PanelOrientation();
            fakeOrientation = new PanelOrientation();
            fakeState = new State();
            //CurrentState = State.Collapsed;
        }

        private void SettingsExpandableToggleItem_LostFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void SettingsExpandableToggleItem_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.FocusState == FocusState.Keyboard || this.FocusState == FocusState.Programmatic)
                button.Focus(FocusState.Keyboard);
        }

        private void SettingsExpandableItem_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void SettingsExpandableItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.Subtitle))
            {
                subtitle.Visibility = Visibility.Collapsed;
                Grid.SetRowSpan(title, 2);
                title.Margin = new Thickness(10, 15, 10, 15);
                title.VerticalAlignment = VerticalAlignment.Center;
            }
            else
            {
                subtitle.Visibility = Visibility.Visible;
                Grid.SetRowSpan(title, 1);
                title.Margin = new Thickness(10, 15, 10, 0);
                title.VerticalAlignment = VerticalAlignment.Bottom;
            }
        }

        private void Toggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (ignoreToggleEvent)
                return;

            this.IsOn = toggle.IsOn;
            Toggled?.Invoke(this, e);
        }

        public void ChangeView(State state)
        {
            PanelActualHeight = Items.ActualHeight;

            SineEase se = new SineEase()
            {
                EasingMode = EasingMode.EaseInOut,
            };

            if (CurrentState == State.Collapsed)
            {
                Panel.IsHitTestVisible = false;

                Animation animation = new Animation();
                animation.AddFadeOutAnimation(Panel, 200);
                //animation.AddDoubleAnimation(0, 200, Panel, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.SineEase, EasingMode.EaseInOut), true);
                animation.AddDoubleAnimation(0, 200, ArrowIconRotation, "Angle", Animation.GenerateEasingFunction(EasingFunctionType.SineEase, EasingMode.EaseInOut), true);
                //animation.AddDoubleAnimation(0, 200, shadowBorder, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.SineEase, EasingMode.EaseInOut), true);
                animation.Completed += (snd, args) =>
                {
                    Panel.Visibility = Visibility.Collapsed;
                    CollapseCompleted?.Invoke(this);
                };

                animation.Begin();
            }
            else if (CurrentState == State.Expanded)
            {
                Panel.Visibility = Visibility.Visible;
                Panel.IsHitTestVisible = true;

                Animation animation = new Animation();
                animation.AddFadeInAnimation(Panel, 200);
                //animation.AddDoubleAnimation(1, 200, Panel, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.SineEase, EasingMode.EaseInOut), true);
                animation.AddDoubleAnimation(180, 200, ArrowIconRotation, "Angle", Animation.GenerateEasingFunction(EasingFunctionType.SineEase, EasingMode.EaseInOut), true);
                //animation.AddDoubleAnimation(1, 200, shadowBorder, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.SineEase, EasingMode.EaseInOut), true);
                animation.Completed += (snd, args) =>
                {
                    ExpandCompleted?.Invoke(this);
                };

                animation.Begin();
            }

            CurrentStateChanged?.Invoke(this, new RoutedEventArgs());
        }

        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(string), typeof(SettingsExpandableToggleItem), new PropertyMetadata(string.Empty));

        public int IconSize
        {
            get { return ((int)GetValue(IconSizeProperty)); }
            set
            {
                SetValue(IconSizeProperty, value);
            }
        }

        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register("IconSize", typeof(int), typeof(SettingsExpandableToggleItem), new PropertyMetadata(30));


        public static readonly DependencyProperty titleProperty =
            DependencyProperty.Register("Title",
                typeof(string),
                typeof(SettingsExpandableToggleItem),
                new PropertyMetadata(string.Empty));

        public string Title
        {
            get { return (string)this.GetValue(titleProperty); }
            set
            {
                this.SetValue(titleProperty, value);

                NotifyPropertyChanged("Title");
            }
        }

        public string Subtitle
        {
            get { return (string)GetValue(SubtitleProperty); }
            set { SetValue(SubtitleProperty, value); }
        }

        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register("Subtitle", typeof(string), typeof(SettingsExpandableToggleItem), new PropertyMetadata(string.Empty));


        public static readonly DependencyProperty ChildrenProperty = DependencyProperty.Register(
            "Children",
            typeof(UIElementCollection),
            typeof(SettingsExpandableToggleItem),
            null);

        public UIElementCollection Children
        {
            get { return (UIElementCollection)GetValue(ChildrenProperty); }
            private set
            {
                SetValue(ChildrenProperty, value);
                foreach (FrameworkElement elem in value)
                    Items.Children.Add(elem);

                PanelActualHeight = Items.ActualHeight;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        public void Add(FrameworkElement elem)
        {
            Items.Children.Add(elem);
        }

        private void ChangeState_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentState == State.Collapsed)
                CurrentState = State.Expanded;
            else
                CurrentState = State.Collapsed;
        }

        private void Storyboard_Completed(object sender, object e)
        {
            this.InvalidateMeasure();
        }
    }
}
