using System;
using System.IO;

using Xamarin.Forms;

using Plugin.Media.Abstractions;

namespace XamSpeak
{
	public static class ConverterHelpers
	{
		public static Stream ConvertMediaFileToStream(MediaFile mediaFile, bool shouldDisposeMediaFile)
		{
			var stream = mediaFile.GetStream();

			if (shouldDisposeMediaFile)
				mediaFile.Dispose();

			return stream;
		}

		public static ImageSource ConvertPhotoFileToImageSource(string photoFilePath)
		{
			return ImageSource.FromFile(photoFilePath);
		}
	}

	public class InverseBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return !(bool)value;
		}
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return !(bool)value;
		}
	}
}
