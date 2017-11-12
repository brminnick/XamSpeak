using System;
using System.Threading.Tasks;

using Plugin.Media;
using Plugin.Media.Abstractions;

namespace XamSpeak
{
    public static class MediaServices
    {
        #region Events 
        public static event EventHandler NoCameraDetected;
        #endregion

        #region Methods
        public static async Task<MediaFile> GetMediaFileFromCamera(string photoName)
        {
            await CrossMedia.Current.Initialize().ConfigureAwait(false);

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                OnNoCameraDetected();
                return null;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                PhotoSize = PhotoSize.Small,
                Directory = "XamSpeak",
                Name = photoName,
                DefaultCamera = CameraDevice.Rear,
            }).ConfigureAwait(false);

            return file;
        }

        static void OnNoCameraDetected() => NoCameraDetected?.Invoke(null, EventArgs.Empty);
        #endregion
    }
}
