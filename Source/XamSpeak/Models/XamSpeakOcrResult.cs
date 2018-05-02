using System.Collections.Generic;

using Newtonsoft.Json;

using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace XamSpeak
{
	public class XamSpeakOcrResult
	{
		[JsonProperty("language")]
		public OcrLanguages Language { get; set; }

		[JsonProperty("orientation")]
		public string Orientation { get; set; }

		[JsonProperty("textAngle")]
		public double TextAngle { get; set; }

		[JsonProperty("regions")]
		public List<OcrRegion> Regions { get; set; }
	}
}
