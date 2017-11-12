using Xamarin.Forms;

namespace XamSpeak
{
    public static class ColorConstants
    {
        public const string NavigationBarBackgroundHex = "38A496";
        public const string ContentPageBackgroundColorHex = "A4EBE2";
        public const string NavigationBarTextHex = "FFFFFF";

        public static readonly Color NavigationBarTextColor = Color.FromHex(NavigationBarTextHex);
        public static readonly Color NavigationBarBackgroundColor = Color.FromHex(NavigationBarBackgroundHex);
        public static readonly Color ContentPageBackgroundColor = Color.FromHex(ContentPageBackgroundColorHex);
    }
}
