using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace AudictiveMusicUWP.Gui.Util
{
    public static class ShadowHelper
    {
        //private static void ApplyShadow(this Grid panel)
        //{
        //    Compositor _compositor = ElementCompositionPreview.GetElementVisual(panel).Compositor;
        //    SpriteVisual _sprite = _compositor.CreateSpriteVisual();
        //    //_sprite.Brush = _compositor.CreateColorBrush(Colors.Blue);
        //    _sprite.Size = new Vector2((float)panel.ActualWidth, (float)panel.ActualHeight);

        //    var basicShadow = _compositor.CreateDropShadow();
        //    basicShadow.BlurRadius = 25f;
        //    basicShadow.Offset = new Vector3(0, 5, 2);

        //    _sprite.Shadow = basicShadow;

        //    ElementCompositionPreview.SetElementChildVisual(panel, _sprite);
        //}

        public static void ApplyShadow(this FrameworkElement panel)
        {
            Compositor _compositor = ElementCompositionPreview.GetElementVisual(panel).Compositor;
            SpriteVisual _sprite = _compositor.CreateSpriteVisual();
            //_sprite.Brush = _compositor.CreateColorBrush(Colors.Blue);
            _sprite.Size = new Vector2((float)panel.ActualWidth, (float)panel.ActualHeight);

            var basicShadow = _compositor.CreateDropShadow();
            basicShadow.BlurRadius = 25f;
            basicShadow.Offset = new Vector3(0, 5, 5);

            _sprite.Shadow = basicShadow;

            ElementCompositionPreview.SetElementChildVisual(panel, _sprite);

            panel.SizeChanged += (s, args) => { _sprite.Size = args.NewSize.ToVector2(); };
        }

    }
}
