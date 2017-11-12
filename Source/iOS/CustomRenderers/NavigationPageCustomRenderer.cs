using System;
using UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using XamSpeak.iOS;

[assembly: ExportRenderer(typeof(NavigationPage), typeof(NavigationPageCustomRenderer))]
namespace XamSpeak.iOS
{
    public class NavigationPageCustomRenderer : NavigationRenderer
    {
        readonly static UIColor _navigationBarTextColor = UIColor.Clear.FromHexString(ColorConstants.NavigationBarTextHex);
        readonly static UIColor _navigationBarBackgroundColor = UIColor.Clear.FromHexString(ColorConstants.NavigationBarBackgroundHex);

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                NavigationBar.PrefersLargeTitles = true;

                NavigationBar.LargeTitleTextAttributes = new UIStringAttributes
                {
                    ForegroundColor = _navigationBarTextColor
                };
            }

            NavigationBar.TintColor = _navigationBarTextColor;
        }
    }
}
