using System.IO;

using Xamarin.Forms;

using Plugin.Media.Abstractions;

namespace XamSpeak
{
    public static class ConverterHelpers
    {
        public static ImageSource ConvertPhotoFileToImageSource(string photoFilePath) => ImageSource.FromFile(photoFilePath);
    }
}
