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
    public sealed partial class SettingsSection : UserControl, INotifyPropertyChanged
    {
        public delegate void StateChangedEventArgs(object sender, RoutedEventArgs e);
        public event StateChangedEventArgs CurrentStateChanged;
        public delegate void AnimationCompletedEventArgs(object sender);
        public event AnimationCompletedEventArgs ExpandCompleted;
        public event AnimationCompletedEventArgs CollapseCompleted;
        public event RoutedEventHandler Click;

        public SolidColorBrush ExpandedBackgroundBrush
        {
            get { return (SolidColorBrush)GetValue(ExpandedBackgroundBrushProperty); }
            set { SetValue(ExpandedBackgroundBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExpandedBackgroundBrushProperty =
            DependencyProperty.Register("ExpandedBackgroundBrush", typeof(SolidColorBrush), typeof(SettingsSection), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));



        public SolidColorBrush BackgroundBrush
        {
            get { return (SolidColorBrush)GetValue(BackgroundBrushProperty); }
            set { SetValue(BackgroundBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundBrushProperty =
            DependencyProperty.Register("BackgroundBrush", typeof(SolidColorBrush), typeof(SettingsSection), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));




        public bool IsClickable
        {
            get { return (bool)GetValue(IsClickableProperty); }
            set { SetValue(IsClickableProperty, value);
                if (value)
                {
                    ArrowIconRotation.Angle = -90;
                }
                else
                {
                    ArrowIconRotation.Angle = 0;
                }
            }
        }

        // Using a DependencyProperty as the backing store for IsClickable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsClickableProperty =
            DependencyProperty.Register("IsClickable", typeof(bool), typeof(SettingsSection), new PropertyMetadata(false));




        public bool IsShadowVisible
        {
            get { return (bool)GetValue(IsShadowVisibleProperty); }
            set { SetValue(IsShadowVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsShadowVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsShadowVisibleProperty =
            DependencyProperty.Register("IsShadowVisible", typeof(bool), typeof(SettingsSection), new PropertyMetadata(true));




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
                typeof(SettingsSection),
                new PropertyMetadata(PanelOrientation.Vertical));


        public static readonly DependencyProperty appsettingsproperty =
            DependencyProperty.Register("AppSettings",
                typeof(string),
                typeof(SettingsSection),
                new PropertyMetadata(""));

        public enum State
        {
            Collapsed,
            Expanded
        }



        public State CurrentState
        {
            get { return (State)GetValue(CurrentStateProperty); }
            set
            {
                SetValue(CurrentStateProperty, value);
                if (this.IsClickable)
                    return;

                ChangeView(value);
            }
        }

        // Using a DependencyProperty as the backing store for CurrentState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentStateProperty =
            DependencyProperty.Register("CurrentState", typeof(State), typeof(SettingsSection), new PropertyMetadata(State.Collapsed));


        private double PanelActualHeight
        {
            get;
            set;
        }

        public SettingsSection()
        {
            this.Loaded += SettingsSection_Loaded;
            this.InitializeComponent();
            this.GotFocus += SettingsSection_GotFocus;
            this.LostFocus += SettingsSection_LostFocus;
            this.SizeChanged += SettingsSection_SizeChanged;
            //Panel.Height = 0;
            Children = Items.Children;
            Orientation = new PanelOrientation();
            fakeOrientation = new PanelOrientation();
            //CurrentState = State.Collapsed;
        }

        private void SettingsSection_LostFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void SettingsSection_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.FocusState == FocusState.Keyboard || this.FocusState == FocusState.Programmatic)
            {
                if (this.CurrentState == State.Collapsed)
                    expandButton.Focus(FocusState.Keyboard);
                else
                    collapseButton.Focus(FocusState.Keyboard);
            }
        }

        private void SettingsSection_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void SettingsSection_Loaded(object sender, RoutedEventArgs e)
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

            if (this.IsClickable)
                ArrowIconRotation.Angle = -90;
            else
            {
                if (this.CurrentState == State.Collapsed)
                    ArrowIconRotation.Angle = 0;
                else
                    ArrowIconRotation.Angle = 180;
            }

            shadowBorder.ApplyShadow();
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
                ClosedBorder.Visibility = Visibility.Visible;
                ExpandedBorder.Visibility = Visibility.Collapsed;
                Panel.IsHitTestVisible = false;

                expandButton.Visibility = Visibility.Visible;
                if (collapseButton.FocusState == FocusState.Keyboard)
                {
                    expandButton.Focus(FocusState.Keyboard);
                }

                collapseButton.Visibility = Visibility.Collapsed;
                HeaderBorder.CornerRadius = new CornerRadius(5);

                Animation animation = new Animation();
                animation.AddFadeOutAnimation(Panel, 200);
                //animation.AddDoubleAnimation(0, 200, Panel, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.SineEase, EasingMode.EaseInOut), true);
                animation.AddDoubleAnimation(0, 200, ArrowIconRotation, "Angle", Animation.GenerateEasingFunction(EasingFunctionType.SineEase, EasingMode.EaseInOut), true);
                //animation.AddDoubleAnimation(0, 200, shadowBorder, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.SineEase, EasingMode.EaseInOut), true);
                animation.AddFadeOutAnimation(shadowBorder, 100);
                animation.Completed += (snd, args) =>
                {
                    Panel.Visibility = Visibility.Collapsed;
                    CollapseCompleted?.Invoke(this);
                };

                animation.Begin();
            }
            else if (CurrentState == State.Expanded)
            {
                ClosedBorder.Visibility = Visibility.Collapsed;
                ExpandedBorder.Visibility = Visibility.Visible;
                Panel.Visibility = Visibility.Visible;
                HeaderBorder.CornerRadius = new CornerRadius(5, 5, 0, 0);

                collapseButton.Visibility = Visibility.Visible;
                if (expandButton.FocusState == FocusState.Keyboard)
                {
                    collapseButton.Focus(FocusState.Keyboard);
                }
                expandButton.Visibility = Visibility.Collapsed;
                Panel.IsHitTestVisible = true;

                Animation animation = new Animation();
                animation.AddFadeInAnimation(Panel, 200);
                //animation.AddDoubleAnimation(1, 200, Panel, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.SineEase, EasingMode.EaseInOut), true);
                animation.AddDoubleAnimation(180, 200, ArrowIconRotation, "Angle", Animation.GenerateEasingFunction(EasingFunctionType.SineEase, EasingMode.EaseInOut), true);
                animation.AddFadeInAnimation(shadowBorder, 200);
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
            DependencyProperty.Register("Icon", typeof(string), typeof(SettingsSection), new PropertyMetadata(string.Empty));

        public int IconSize
        {
            get { return ((int)GetValue(IconSizeProperty)); }
            set
            {
                SetValue(IconSizeProperty, value);
            }
        }

        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register("IconSize", typeof(int), typeof(SettingsSection), new PropertyMetadata(30));


        public static readonly DependencyProperty titleProperty =
            DependencyProperty.Register("Title",
                typeof(string),
                typeof(SettingsSection),
                new PropertyMetadata(0));

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
            DependencyProperty.Register("Subtitle", typeof(string), typeof(SettingsSection), new PropertyMetadata(string.Empty));


        public static readonly DependencyProperty ChildrenProperty = DependencyProperty.Register(
            "Children",
            typeof(UIElementCollection),
            typeof(SettingsSection),
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
            if (this.IsClickable)
            {
                Click?.Invoke(this, e);

                return;
            }

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
