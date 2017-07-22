using Xamarin.Forms;

namespace XamSpeak
{
	public class App : Application
	{
		public App()
		{
			MainPage = new NavigationPage(new TextToSpeechPage())
			{
				BarTextColor = Color.White,
				BarBackgroundColor = Color.FromHex("38A496")
			};
		}
	}
}
