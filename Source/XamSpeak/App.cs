using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace XamSpeak
{
    public class App : Xamarin.Forms.Application
    {
        public App()
        {
            Xamarin.Forms.Device.SetFlags(new[] { "Markup_Experimental" });

            var navigationPage = new Xamarin.Forms.NavigationPage(new TextToSpeechPage())
            {
                BarTextColor = ColorConstants.NavigationBarTextColor,
                BarBackgroundColor = ColorConstants.NavigationBarBackgroundColor
            };

            navigationPage.On<iOS>().SetPrefersLargeTitles(true);

            MainPage = navigationPage;
        }
    }
}
