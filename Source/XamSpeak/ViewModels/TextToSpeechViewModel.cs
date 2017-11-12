using System;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;

using Plugin.Media;
using Plugin.TextToSpeech;
using Plugin.Media.Abstractions;

using Xamarin.Forms;

namespace XamSpeak
{
    public class TextToSpeechViewModel : BaseViewModel
    {
        #region Fields
        int _isInternetConnectionInUseCount;
        string _spokenTextLabelText, _activityIndicatorLabelText;
        bool _isActivityIndicatorDisplayed, _isInternetConnectionInUse;
        ICommand _takePictureButtonCommand;
        #endregion

        #region Events
        public event EventHandler OCRFailed;
        public event EventHandler SpellCheckFailed;
        public event EventHandler NoCameraDetected;
        public event EventHandler InvalidComputerVisionAPIKey;
        public event EventHandler InternetConnectionUnavailable;
        #endregion

        #region Properties
        public ICommand TakePictureButtonCommand => _takePictureButtonCommand ??
            (_takePictureButtonCommand = new Command(async () => await ExecuteTakePictureButtonCommand()));

        public bool IsInternetConnectionInUse
        {
            get => _isInternetConnectionInUse;
            set => SetProperty(ref _isInternetConnectionInUse, value);
        }

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
        async Task ExecuteTakePictureButtonCommand()
        {
            SpokenTextLabelText = string.Empty;

            await ExecuteNewPictureWorkflow();
        }

        async Task ExecuteNewPictureWorkflow()
        {
            var mediaFile = await GetMediaFileFromCamera(Guid.NewGuid().ToString());
            if (mediaFile == null)
            {
                OnDisplayNoCameraDetected();
                return;
            }

            var ocrResults = await GetOcrResultsFromMediaFile(mediaFile);
            if (ocrResults == null)
            {
                OnOCRFailed();
                return;
            }

            var listOfStringsFromOcrResults = GetTextFromOcrResults(ocrResults);

            var spellCheckedlistOfStringsFromOcrResults = await GetSpellCheckedStringList(listOfStringsFromOcrResults);
            if (spellCheckedlistOfStringsFromOcrResults == null)
            {
                OnSpellCheckFailed();
                return;
            }

            SpeakText(spellCheckedlistOfStringsFromOcrResults);
        }

        async Task<MediaFile> GetMediaFileFromCamera(string photoName)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                return null;

            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                Directory = "XamSpeak",
                Name = photoName,
                PhotoSize = PhotoSize.Small,
                DefaultCamera = CameraDevice.Rear,
            });

            return file;
        }

        async Task<OcrResults> GetOcrResultsFromMediaFile(MediaFile mediaFile)
        {
            ActivateActivityIndicator("Reading Text");

            try
            {
                var visionClient = new VisionServiceClient(CognitiveServicesConstants.ComputerVisionAPIKey);
                var ocrResults = await visionClient.RecognizeTextAsync(ConverterHelpers.ConvertMediaFileToStream(mediaFile, false));

                return ocrResults;
            }
            catch (Exception e)
            {
                DebugHelpers.PrintException(e);

                if ((e is ClientException) && ((ClientException)e).HttpStatus == 0)
                    OnInvalidComputerVisionAPIKey();
                else
                    OnInternetConnectionUnavailable();

                return null;
            }
            finally
            {
                DeactivateActivityIndicator();
            }
        }

        List<string> GetTextFromOcrResults(OcrResults ocrResults)
        {
            var ocrModelList = new List<OcrTextLocationModel>();

            foreach (Region region in ocrResults.Regions)
            {
                foreach (Line line in region.Lines)
                {
                    var lineStringBuilder = new StringBuilder();

                    foreach (Word word in line.Words)
                    {
                        lineStringBuilder.Append(word.Text);
                        lineStringBuilder.Append(" ");
                    }

                    ocrModelList.Add(new OcrTextLocationModel(lineStringBuilder.ToString(), line.Rectangle.Top, line.Rectangle.Left));
                }
            }

            return CreateStringFromOcrModelList(ocrModelList);
        }

        List<string> CreateStringFromOcrModelList(List<OcrTextLocationModel> ocrModelList)
        {
            if (ocrModelList == null || ocrModelList.Count <= 0)
                return new List<string>();

            var stringList = new List<string>();
            var stringBuilder = new StringBuilder();

            var maximumTop = ocrModelList.OrderBy(x => x.Top).FirstOrDefault()?.Top;

            var sortedOcrModelList = ocrModelList.OrderBy(x => x.Top).ThenBy(x => x.Left).ToList();

            var previousTop = 0.0;
            foreach (OcrTextLocationModel ocrModel in sortedOcrModelList)
            {
                var percentageBelowPreviousOcrModel = (ocrModel.Top - previousTop) / maximumTop;

                if (percentageBelowPreviousOcrModel <= 0.01)
                {
                    stringBuilder.Append(" ");
                    stringBuilder.Append($"{ocrModel.Text}");
                }
                else
                {
                    if (!string.IsNullOrEmpty(stringBuilder.ToString()))
                        stringList.Add(stringBuilder.ToString());

                    stringBuilder.Clear();
                    stringBuilder.Append($"{ocrModel.Text}");
                }

                previousTop = ocrModel.Top;

                if (sortedOcrModelList.LastOrDefault().Equals(ocrModel))
                    stringList.Add(stringBuilder.ToString());
            }

            return stringList;
        }

        async Task<List<string>> GetSpellCheckedStringList(List<string> stringList)
        {
            ActivateActivityIndicator("Performing Spell Check");

            int listIndex = 0;
            var correctedLineItemList = new List<string>();

            try
            {
                foreach (string lineItem in stringList)
                {
                    correctedLineItemList.Add(lineItem);

                    var misspelledWordList = await SpellCheckServices.SpellCheckString(lineItem);

                    if (misspelledWordList == null)
                        return null;

                    foreach (var misspelledWord in misspelledWordList)
                    {
                        var firstSuggestion = misspelledWord.Suggesstions.FirstOrDefault();

                        double.TryParse(firstSuggestion?.ConfidenceScore, out double confidenceScore);

                        if (confidenceScore > 0.80)
                        {
                            var correctedLineItem = correctedLineItemList[listIndex].Replace(misspelledWord.MisspelledWord, firstSuggestion?.Suggestion);

                            correctedLineItemList[listIndex] = correctedLineItem;
                        }
                    }

                    listIndex++;
                }
            }
            finally
            {
                DeactivateActivityIndicator();
            }

            return correctedLineItemList;

        }

        void SpeakText(List<string> textList)
        {
            var stringBuilder = new StringBuilder();

            foreach (var lineOfText in textList)
            {
                stringBuilder.AppendLine(lineOfText);
                SpokenTextLabelText = stringBuilder.ToString();

                CrossTextToSpeech.Current.Speak(lineOfText);
            }
        }

        void ActivateActivityIndicator(string activityIndicatorLabelText)
        {
            IsInternetConnectionInUse = ++_isInternetConnectionInUseCount > 0;
            IsActivityIndicatorDisplayed = true;
            ActivityIndicatorLabelText = activityIndicatorLabelText;
        }

        void DeactivateActivityIndicator()
        {
            IsInternetConnectionInUse = --_isInternetConnectionInUseCount != 0;
            IsActivityIndicatorDisplayed = false;
            ActivityIndicatorLabelText = default(string);
        }

        void OnDisplayNoCameraDetected() =>
            NoCameraDetected?.Invoke(this, EventArgs.Empty);

        void OnOCRFailed() =>
            OCRFailed?.Invoke(this, EventArgs.Empty);

        void OnSpellCheckFailed() =>
            SpellCheckFailed?.Invoke(this, EventArgs.Empty);

        void OnInvalidComputerVisionAPIKey() =>
            InvalidComputerVisionAPIKey?.Invoke(this, EventArgs.Empty);

        void OnInternetConnectionUnavailable() =>
            InternetConnectionUnavailable?.Invoke(this, EventArgs.Empty);
        #endregion

        #region Classes
        class OcrTextLocationModel
        {
            public OcrTextLocationModel(string text, int top, int left)
            {
                Text = text;
                Top = top;
                Left = left;
            }

            public string Text { get; }
            public double Top { get; }
            public double Left { get; }
        }
        #endregion
    }
}
