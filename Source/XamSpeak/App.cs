using Xamarin.Forms;

namespace XamSpeak
{
	public class App : Application
	{
		public App()
		{
			MainPage = new NavigationPage(new SelectionPage())
			{
				BarTextColor = ColorConstants.NavigationBarTextColor,
                BarBackgroundColor = ColorConstants.NavigationBarBackgroundColor
			};
		}
	}
}
