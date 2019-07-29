using Xamarin.Forms;

namespace XamSpeak
{
    public static class ColorConstants
    {
        public const string NavigationBarBackgroundHex = "38A496";
        public const string ContentPageBackgroundColorHex = "A4EBE2";
        public const string NavigationBarTextHex = "FFFFFF";

        public static Color NavigationBarTextColor { get; } = Color.FromHex(NavigationBarTextHex);
        public static Color NavigationBarBackgroundColor { get; } = Color.FromHex(NavigationBarBackgroundHex);
        public static Color ContentPageBackgroundColor { get; } = Color.FromHex(ContentPageBackgroundColorHex);
    }
}
