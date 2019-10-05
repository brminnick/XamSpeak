using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Plugin.Media.Abstractions;

namespace XamSpeak
{
    public static class OCRServices
    {
        readonly static WeakEventManager _invalidComputerVisionAPIKeyEventManager = new WeakEventManager();

        readonly static Lazy<ComputerVisionClient> _computerVisionApiClientHolder =
            new Lazy<ComputerVisionClient>(() => new ComputerVisionClient(new ApiKeyServiceClientCredentials(CognitiveServicesConstants.ComputerVisionAPIKey)) { Endpoint = CognitiveServicesConstants.ComputerVisionBaseUrl });

        public static event EventHandler InvalidComputerVisionAPIKey
        {
            add => _invalidComputerVisionAPIKeyEventManager.AddEventHandler(value);
            remove => _invalidComputerVisionAPIKeyEventManager.RemoveEventHandler(value);
        }

        static ComputerVisionClient ComputerVisionApiClient => _computerVisionApiClientHolder.Value;

        public static async Task<OcrResult> GetOcrResultsFromMediaFile(MediaFile mediaFile)
        {
            try
            {
                return await ComputerVisionApiClient.RecognizePrintedTextInStreamAsync(true, MediaServices.GetPhotoStream(mediaFile, false)).ConfigureAwait(false);
            }
            catch (ComputerVisionErrorException e) when (e.Response.StatusCode is HttpStatusCode.Unauthorized)
            {
                DebugHelpers.PrintException(e);

                OnInvalidComputerVisionAPIKey();

                throw;
            }
        }

        public static IEnumerable<string> GetTextFromOcrResults(OcrResult ocrResults)
        {
            var ocrModelList = new List<OcrTextLocationModel>();

            foreach (var region in ocrResults.Regions)
            {
                foreach (var line in region.Lines)
                {
                    var lineStringBuilder = new StringBuilder();

                    foreach (var word in line.Words)
                    {
                        lineStringBuilder.Append(word.Text);
                        lineStringBuilder.Append(" ");
                    }
                    var (left, top, width, height) = ConvertBoundingBox(line.BoundingBox);
                    ocrModelList.Add(new OcrTextLocationModel(lineStringBuilder.ToString(), top, left));
                }
            }

            return CreateStringFromOcrModelList(ocrModelList);
        }

        static IEnumerable<string> CreateStringFromOcrModelList(List<OcrTextLocationModel> ocrModelList)
        {
            if (ocrModelList is null || !ocrModelList.Any())
                yield break;

            var maximumTop = ocrModelList.OrderBy(x => x.Top).First().Top;

            var sortedOcrModelList = ocrModelList.OrderBy(x => x.Top).ThenBy(x => x.Left).ToList();

            OcrTextLocationModel? previousOcrModel = null;

            var lineStringBuilder = new StringBuilder();

            foreach (var ocrModel in sortedOcrModelList)
            {
                var isCurrentTextOnSameLine = IsCurrentTextOnSameLine(ocrModel, previousOcrModel, maximumTop);
                previousOcrModel = ocrModel;

                if (sortedOcrModelList.Last().Equals(ocrModel))
                    yield return ocrModel.Text;

                if (isCurrentTextOnSameLine)
                {
                    lineStringBuilder.Append(" ");
                    lineStringBuilder.Append($"{ocrModel.Text}");
                }
                else if (lineStringBuilder.Length > 0)
                {
                    var previousLineText = lineStringBuilder.ToString();

                    lineStringBuilder.Clear();
                    lineStringBuilder.Append($"{ocrModel.Text}");

                    yield return previousLineText;
                }
                else
                {
                    lineStringBuilder.Append($"{ocrModel.Text}");
                }
            }

            static bool IsCurrentTextOnSameLine(in OcrTextLocationModel currentOcrModel, in OcrTextLocationModel? previousOcrModel, in double topMaximum)
            {
                var percentageBelowPreviousOcrModel = (currentOcrModel.Top - previousOcrModel?.Top ?? 0) / topMaximum;
                return percentageBelowPreviousOcrModel <= 0.01;
            }
        }

        static (int left, int top, int width, int height) ConvertBoundingBox(string boundingBox)
        {
            var boundingBoxArray = boundingBox.Split(',');

            int.TryParse(boundingBoxArray[0], out int left);
            int.TryParse(boundingBoxArray[1], out int top);
            int.TryParse(boundingBoxArray[2], out int width);
            int.TryParse(boundingBoxArray[3], out int height);

            return (left, top, width, height);
        }

        static void OnInvalidComputerVisionAPIKey() =>
            _invalidComputerVisionAPIKeyEventManager.HandleEvent(null, EventArgs.Empty, nameof(InvalidComputerVisionAPIKey));

        class OcrTextLocationModel
        {
            public OcrTextLocationModel(string text, int top, int left) => (Text, Top, Left) = (text, top, left);

            public string Text { get; }
            public double Top { get; }
            public double Left { get; }
        }
    }
}
