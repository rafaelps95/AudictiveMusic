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

        public SolidColorBrush BackgroundBrush
        {
            get { return (SolidColorBrush)GetValue(BackgroundBrushProperty); }
            set { SetValue(BackgroundBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundBrushProperty =
            DependencyProperty.Register("BackgroundBrush", typeof(SolidColorBrush), typeof(SettingsSection), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));



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

        public SettingsSection()
        {
            this.Loaded += SettingsSection_Loaded;
            this.InitializeComponent();
            this.SizeChanged += SettingsSection_SizeChanged;
            //Panel.Height = 0;
            CurrentState = new State();
            Children = Items.Children;
            Orientation = new PanelOrientation();
            fakeOrientation = new PanelOrientation();
            fakeState = new State();
            //CurrentState = State.Collapsed;
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
                expandButton.Visibility = Visibility.Visible;
                collapseButton.Visibility = Visibility.Collapsed;
                HeaderBorder.CornerRadius = new CornerRadius(5);

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
                    CollapseCompleted?.Invoke(this);
                };

                sb.Begin();
            }
            else if (CurrentState == State.Expanded)
            {
                Panel.Visibility = Visibility.Visible;
                HeaderBorder.CornerRadius = new CornerRadius(5, 5, 0, 0);
                expandButton.Visibility = Visibility.Collapsed;
                collapseButton.Visibility = Visibility.Visible;
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

                sb.Completed += (snd, args) =>
                {
                    ExpandCompleted?.Invoke(this);
                };

                sb.Begin();
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
