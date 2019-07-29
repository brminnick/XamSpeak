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
}
