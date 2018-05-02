using System;
using System.Net;
using System.Net.Http;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

using Plugin.Media.Abstractions;

using Xamarin.Forms;

namespace XamSpeak
{
	public class TextToSpeechViewModel : BaseViewModel
    {
        #region Fields
        int _isInternetConnectionInUseCount;
        string _spokenTextLabelText, _activityIndicatorLabelText;
        bool _isActivityIndicatorDisplayed;
        ICommand _takePictureButtonCommand;
        #endregion

        #region Events
        public event EventHandler OCRFailed;
        public event EventHandler SpellCheckFailed;
        public event EventHandler InternetConnectionUnavailable;
        #endregion

        #region Properties
        public ICommand TakePictureButtonCommand => _takePictureButtonCommand ??
            (_takePictureButtonCommand = new Command(async () => await ExecuteTakePictureButtonCommand().ConfigureAwait(false)));

        public string SpokenTextLabelText
        {
            get => _spokenTextLabelText;
            set => SetProperty(ref _spokenTextLabelText, value);
        }

        public string ActivityIndicatorLabelText
        {
            get => _activityIndicatorLabelText;
            set => SetProperty(ref _activityIndicatorLabelText, value);
        }

        public bool IsActivityIndicatorDisplayed
        {
            get => _isActivityIndicatorDisplayed;
            set => SetProperty(ref _isActivityIndicatorDisplayed, value);
        }
        #endregion

        #region Methods
        Task ExecuteTakePictureButtonCommand()
        {
            SpokenTextLabelText = string.Empty;

            return ExecuteNewPictureWorkflow();
        }

        async Task ExecuteNewPictureWorkflow()
        {
            var mediaFile = await MediaServices.GetMediaFileFromCamera(Guid.NewGuid().ToString()).ConfigureAwait(false);
            if (mediaFile is null)
                return;

            try
            {
				var ocrResults = await GetOcrResults(mediaFile).ConfigureAwait(false);
				var listOfStringsFromOcrResults = OCRServices.GetTextFromOcrResults(ocrResults);

				var spellCheckedlistOfStringsFromOcrResults = await GetSpellCheckedStringList(listOfStringsFromOcrResults).ConfigureAwait(false);

				SpokenTextLabelText = TextToSpeechServices.SpeakText(spellCheckedlistOfStringsFromOcrResults);
			}
			catch(HttpRequestException e) when (((e?.InnerException as WebException)?.Status.Equals(WebExceptionStatus.ConnectFailure) ?? false)
			                                    || ((e?.InnerException as WebException)?.Status.Equals(WebExceptionStatus.NameResolutionFailure) ?? false))
			{
				DebugHelpers.PrintException(e);
				OnInternetConnectionUnavailable();
			}
			catch(Exception e)
			{
				DebugHelpers.PrintException(e);
			}
        }

        async Task<OcrResult> GetOcrResults(MediaFile mediaFile)
        {
            ActivateActivityIndicator("Reading Text");

            try
            {
                return await OCRServices.GetOcrResultsFromMediaFile(mediaFile).ConfigureAwait(false);
            }
            finally
            {
                DeactivateActivityIndicator();
            }
        }

        async Task<List<string>> GetSpellCheckedStringList(List<string> stringList)
        {
            ActivateActivityIndicator("Performing Spell Check");

            try
            {
                return await SpellCheckServices.GetSpellCheckedStringList(stringList).ConfigureAwait(false);
            }
            finally
            {
                DeactivateActivityIndicator();
            }
        }

        void ActivateActivityIndicator(string activityIndicatorLabelText)
        {
            IsInternetConnectionActive = ++_isInternetConnectionInUseCount > 0;
            IsActivityIndicatorDisplayed = true;
            ActivityIndicatorLabelText = activityIndicatorLabelText;
        }

        void DeactivateActivityIndicator()
        {
            IsInternetConnectionActive = --_isInternetConnectionInUseCount != 0;
            IsActivityIndicatorDisplayed = false;
            ActivityIndicatorLabelText = default;
        }

        void OnSpellCheckFailed() =>
            SpellCheckFailed?.Invoke(this, EventArgs.Empty);

        void OnInternetConnectionUnavailable() =>
            InternetConnectionUnavailable?.Invoke(this, EventArgs.Empty);

        void OnOCRFailed() =>
            OCRFailed?.Invoke(this, EventArgs.Empty);
        #endregion
    }
}
