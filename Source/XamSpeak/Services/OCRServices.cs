using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Plugin.Media.Abstractions;

namespace XamSpeak
{
    public static class OCRServices
    {
        #region Constant Fields
        readonly static Lazy<VisionServiceClient> _visionClientHolder = new Lazy<VisionServiceClient>(() =>
                                                  new VisionServiceClient(CognitiveServicesKeys.ComputerVisionAPIKey));
        #endregion

        #region Events 
        public static event EventHandler InvalidComputerVisionAPIKey;
        #endregion

        #region Properties
        static VisionServiceClient VisionClient => _visionClientHolder.Value;
        #endregion

        #region Methods
        public static async Task<OcrResults> GetOcrResultsFromMediaFile(MediaFile mediaFile)
        {
            try
            {
                var ocrResults = await VisionClient.RecognizeTextAsync(ConverterHelpers.ConvertMediaFileToStream(mediaFile, false));

                return ocrResults;
            }
            catch (ClientException e) when (e.HttpStatus == 0)
            {
                DebugHelpers.PrintException(e);

                OnInvalidComputerVisionAPIKey();

                return null;
            }
        }

        public static List<string> GetTextFromOcrResults(OcrResults ocrResults)
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

        static List<string> CreateStringFromOcrModelList(List<OcrTextLocationModel> ocrModelList)
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

        static void OnInvalidComputerVisionAPIKey() =>
            InvalidComputerVisionAPIKey?.Invoke(null, EventArgs.Empty);
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
