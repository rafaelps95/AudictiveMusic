using ClassLibrary.Db;
using Microsoft.QueryStringDotNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace ClassLibrary.Helpers
{
    public class NavigationHelper
    {
        public delegate void NavigationRequestedHandler(object sender, Type targetPage, object parameter, bool mainFrame, NavigationTransitionInfo navigationTransitionInfo);
        public static event NavigationRequestedHandler NavigationRequested;
        public delegate void NavigationClearRequestedHandler(object sender, bool mainFrame);
        public static event RoutedEventHandler BackRequested;
        public static event RoutedEventHandler ForwardRequested;
        public static event NavigationClearRequestedHandler ClearRequested;

        public static void Navigate(object sender, Type targetPage, object parameter = null, bool mainFrame = false, NavigationTransitionInfo navigationTransitionInfo = null) => NavigationRequested?.Invoke(sender, targetPage, parameter, mainFrame, navigationTransitionInfo);

        public static void Back(object sender) => BackRequested?.Invoke(sender, new RoutedEventArgs());

        public static void Forward(object sender) => ForwardRequested?.Invoke(sender, new RoutedEventArgs());

        public static void ClearBackstack(object sender, bool mainFrame) => ClearRequested?.Invoke(sender, mainFrame);

        public static string GetParameter(string args, string attribute)
        {
            if (string.IsNullOrWhiteSpace(args) == false)
            {
                QueryString arguments = QueryString.Parse(args);

                // See what action is being requested 
                if (arguments.Contains(attribute))
                {
                    return arguments[attribute];
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public static bool ContainsAttribute(string args, string attribute)
        {
            QueryString arguments = QueryString.Parse(args);

            return arguments.Contains(attribute);
        }
    }
}
