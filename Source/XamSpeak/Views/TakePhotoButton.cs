using Xamarin.Forms;

namespace XamSpeak
{
	public class TakePhotoButton : Button
	{
		public TakePhotoButton()
		{
            BackgroundColor = ColorConstants.NavigationBarBackgroundColor;
			TextColor = Color.White;
			FontAttributes = FontAttributes.Bold;
            Padding = new Thickness(10, 0);
		}
	}
}
