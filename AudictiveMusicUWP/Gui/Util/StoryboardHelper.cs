using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace AudictiveMusicUWP.Gui.Util
{
    public class Animation
    {
        public event EventHandler<object> Completed;

        private Storyboard Storyboard { get; set; }

        public Animation()
        {
            this.Storyboard = new Storyboard();
            this.Storyboard.Completed += Storyboard_Completed;
        }

        private void Storyboard_Completed(object sender, object e)
        {
            this.Completed?.Invoke(sender, e);
        }


        #region DOUBLE ANIMATION
        public void AddDoubleAnimation(double to, double duration, DependencyObject target, string targetProperty, bool enableDependentAnimation = true, double beginTime = 0)
        {
            CreateDoubleAnimation(to, duration, target, targetProperty, null, enableDependentAnimation, double.NaN, beginTime);
        }

        public void AddDoubleAnimation(double to, double duration, DependencyObject target, string targetProperty, EasingFunctionBase easingFunction, bool enableDependentAnimation = true, double beginTime = 0)
        {
            CreateDoubleAnimation(to, duration, target, targetProperty, easingFunction, enableDependentAnimation, double.NaN, beginTime);
        }

        public void AddDoubleAnimation(double from, double to, double duration, DependencyObject target, string targetProperty, bool enableDependentAnimation = true, double beginTime = 0)
        {
            CreateDoubleAnimation(to, duration, target, targetProperty, null, enableDependentAnimation, from, beginTime);
        }

        public void AddDoubleAnimation(double from, double to, double duration, DependencyObject target, string targetProperty, EasingFunctionBase easingFunction, bool enableDependentAnimation = true, double beginTime = 0)
        {
            CreateDoubleAnimation(to, duration, target, targetProperty, easingFunction, enableDependentAnimation, from, beginTime);
        }

        public void AddDoubleAnimation(double from, double to, double duration, double beginTime, DependencyObject target, string targetProperty, EasingFunctionBase easingFunction, bool enableDependentAnimation = true)
        {
            CreateDoubleAnimation(to, duration, target, targetProperty, easingFunction, enableDependentAnimation, from, beginTime);
        }

        private void CreateDoubleAnimation(double to, double duration, DependencyObject target, string targetProperty, EasingFunctionBase easingFunction, bool enableDependentAnimation, double from, double beginTime)
        {
            DoubleAnimation da = new DoubleAnimation()
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(duration),
                EnableDependentAnimation = enableDependentAnimation,
                BeginTime = TimeSpan.FromMilliseconds(beginTime)
            };

            if (easingFunction != null)
                da.EasingFunction = easingFunction;
            if (double.IsNaN(from) == false)
                da.From = from;

            Windows.UI.Xaml.Media.Animation.Storyboard.SetTarget(da, target);
            Storyboard.SetTargetProperty(da, targetProperty);

            this.Storyboard.Children.Add(da);
        }

        #endregion

        #region COLOR ANIMATION

        public void AddColorAnimation(Color to, double duration, DependencyObject target, string targetProperty, bool enableDependentAnimation = true, double beginTime = 0)
        {
            CreateColorAnimation(to, duration, target, targetProperty, null, enableDependentAnimation, null, beginTime);
        }

        public void AddColorAnimation(Color to, double duration, DependencyObject target, string targetProperty, EasingFunctionBase easingFunction, bool enableDependentAnimation = true, double beginTime = 0)
        {
            CreateColorAnimation(to, duration, target, targetProperty, easingFunction, enableDependentAnimation, null, beginTime);
        }

        public void AddColorAnimation(Color from, Color to, double duration, DependencyObject target, string targetProperty, bool enableDependentAnimation = true, double beginTime = 0)
        {
            CreateColorAnimation(to, duration, target, targetProperty, null, enableDependentAnimation, from, beginTime);
        }

        public void AddColorAnimation(Color from, Color to, double duration, DependencyObject target, string targetProperty, EasingFunctionBase easingFunction, bool enableDependentAnimation = true, double beginTime = 0)
        {
            CreateColorAnimation(to, duration, target, targetProperty, easingFunction, enableDependentAnimation, from, beginTime);
        }

        private void CreateColorAnimation(Color to, double duration, DependencyObject target, string targetProperty, EasingFunctionBase easingFunction, bool enableDependentAnimation, Color? from, double beginTime)
        {
            ColorAnimation da = new ColorAnimation()
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(duration),
                EnableDependentAnimation = enableDependentAnimation,
                BeginTime = TimeSpan.FromMilliseconds(beginTime)
            };

            if (easingFunction != null)
                da.EasingFunction = easingFunction;
            if (from != null)
                da.From = from;

            Windows.UI.Xaml.Media.Animation.Storyboard.SetTarget(da, target);
            Storyboard.SetTargetProperty(da, targetProperty);

            this.Storyboard.Children.Add(da);
        }

        #endregion


        #region DEFAULT ANIMATIONS

        public static Animation BeginFadeAnimation(DependencyObject target, double duration = 300)
        {
            Animation animation = new Animation();
            animation.AddDoubleAnimation(0, 1, duration, target, "Opacity");
            animation.Begin();

            return animation;
        }

        #endregion

        public static EasingFunctionBase GenerateEasingFunction(EasingFunctionType easingFunctionType, EasingMode easingMode)
        {
            EasingFunctionBase easingFunction;

            switch (easingFunctionType)
            {
                case EasingFunctionType.BackEase:
                    easingFunction = new BackEase();
                    break;
                case EasingFunctionType.BounceEase:
                    easingFunction = new BounceEase();
                    break;
                case EasingFunctionType.CircleEase:
                    easingFunction = new CircleEase();
                    break;
                case EasingFunctionType.CubicEase:
                    easingFunction = new CubicEase();
                    break;
                case EasingFunctionType.ElasticEase:
                    easingFunction = new ElasticEase();
                    break;
                case EasingFunctionType.ExponentialEase:
                    easingFunction = new ExponentialEase();
                    break;
                case EasingFunctionType.PowerEase:
                    easingFunction = new PowerEase();
                    break;
                case EasingFunctionType.QuadraticEase:
                    easingFunction = new QuadraticEase();
                    break;
                case EasingFunctionType.QuarticEase:
                    easingFunction = new QuarticEase();
                    break;
                case EasingFunctionType.QuinticEase:
                    easingFunction = new QuinticEase();
                    break;
                default:
                    easingFunction = new SineEase();
                    break;
            }

            easingFunction.EasingMode = easingMode;

            return easingFunction;
        }

        public static void RunAnimation(object storyboard)
        {
            (storyboard as Storyboard).Begin();
        }

        public void Begin()
        {
            this.Storyboard.Begin();
        }
    }

    public enum EasingFunctionType
    {
        BackEase,
        BounceEase,
        CircleEase,
        CubicEase,
        ElasticEase,
        ExponentialEase,
        PowerEase,
        QuadraticEase,
        QuarticEase,
        QuinticEase,
        SineEase
    }
}
