using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

using Newtonsoft.Json;

using Plugin.Media.Abstractions;

namespace XamSpeak
{
	public static class OCRServices
	{
		#region Constant Fields
		readonly static Lazy<ComputerVisionAPI> _computerVisionApiClientHolder =
			new Lazy<ComputerVisionAPI>(() => new ComputerVisionAPI(new ApiKeyServiceClientCredentials(CognitiveServicesConstants.ComputerVisionAPIKey)) { AzureRegion = AzureRegions.Westus });
		#endregion

		#region Events 
		public static event EventHandler InvalidComputerVisionAPIKey;
		#endregion

		#region Properties
		static ComputerVisionAPI ComputerVisionApiClient => _computerVisionApiClientHolder.Value;
		#endregion

		#region Methods
		public static async Task<OcrResult> GetOcrResultsFromMediaFile(MediaFile mediaFile)
		{
			try
			{
				return await ComputerVisionApiClient.RecognizePrintedTextInStreamAsync(true, ConverterHelpers.ConvertMediaFileToStream(mediaFile, false)).ConfigureAwait(false);
			}
			catch (ComputerVisionErrorException e) when (e.Response.StatusCode.Equals(HttpStatusCode.Unauthorized))
			{
				DebugHelpers.PrintException(e);

				OnInvalidComputerVisionAPIKey();

				throw;
			}
            catch (Microsoft.Rest.SerializationException e)
			{
				var xamSpeakOcrResult = JsonConvert.DeserializeObject<XamSpeakOcrResult>(e.Content);

				return new OcrResult
				{
					Language = default,
					Orientation = xamSpeakOcrResult.Orientation,
					Regions = xamSpeakOcrResult.Regions,
                    TextAngle = xamSpeakOcrResult.TextAngle
				};
			}
		}

		public static List<string> GetTextFromOcrResults(OcrResult ocrResults)
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

		static List<string> CreateStringFromOcrModelList(List<OcrTextLocationModel> ocrModelList)
		{
			if (ocrModelList is null || ocrModelList.Count <= 0)
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
