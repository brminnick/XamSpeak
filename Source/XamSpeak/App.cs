using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace XamSpeak
{
	public class App : Xamarin.Forms.Application
	{
		public App()
		{
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
