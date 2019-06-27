using System;
using System.IO;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Plugin.Media;
using Plugin.Media.Abstractions;

using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

using Xamarin.Forms;

namespace XamSpeak
{
    public static class MediaServices
    {
        #region Constant Fields
        readonly static WeakEventManager _noCameraDetectedEventManager = new WeakEventManager();
        readonly static WeakEventManager _permissionsDeniedEventManager = new WeakEventManager();
        #endregion

        #region Events 
        public static event EventHandler NoCameraDetected
        {
            add => _noCameraDetectedEventManager.AddEventHandler(value);
            remove => _noCameraDetectedEventManager.RemoveEventHandler(value);
        }

        public static event EventHandler PermissionsDenied
        {
            add => _permissionsDeniedEventManager.AddEventHandler(value);
            remove => _permissionsDeniedEventManager.RemoveEventHandler(value);
        }
        #endregion

        #region Methods
        public static Stream GetPhotoStream(MediaFile mediaFile, bool disposeMediaFile)
        {
            var stream = mediaFile.GetStream();

            if (disposeMediaFile)
                mediaFile.Dispose();

            return stream;
        }

        public static async Task<MediaFile> GetMediaFileFromCamera(string photoName)
        {
            await CrossMedia.Current.Initialize().ConfigureAwait(false);

            var arePermissionsGranted = await ArePermissionsGranted().ConfigureAwait(false);
            if (!arePermissionsGranted)
            {
                OnPermissionsDenied();
                return null;
            }

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                OnNoCameraDetected();
                return null;
            }

            return await Device.InvokeOnMainThreadAsync(() =>
            {
                return CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    PhotoSize = PhotoSize.Small,
                    Directory = "XamSpeak",
                    Name = photoName,
                    DefaultCamera = CameraDevice.Rear,
                });
            }).ConfigureAwait(false);
        }

        static async Task<bool> ArePermissionsGranted()
        {
            var cameraStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera).ConfigureAwait(false);
            var storageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage).ConfigureAwait(false);

            if (cameraStatus != PermissionStatus.Granted || storageStatus != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera, Permission.Storage }).ConfigureAwait(false);
                cameraStatus = results[Permission.Camera];
                storageStatus = results[Permission.Storage];
            }

            if (cameraStatus == PermissionStatus.Granted && storageStatus == PermissionStatus.Granted)
                return true;

            return false;
        }

        static void OnNoCameraDetected() => _noCameraDetectedEventManager.HandleEvent(null, EventArgs.Empty, nameof(NoCameraDetected));
        static void OnPermissionsDenied() => _permissionsDeniedEventManager.HandleEvent(null, EventArgs.Empty, nameof(PermissionsDenied));
        #endregion
    }
}
