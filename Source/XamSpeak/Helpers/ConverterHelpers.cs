using Xamarin.Forms;

namespace XamSpeak
{
    public static class ConverterHelpers
    {
        public static ImageSource ConvertPhotoFileToImageSource(this string photoFilePath) => ImageSource.FromFile(photoFilePath);
    }
}
