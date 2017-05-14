using System.Collections.Generic;

using Newtonsoft.Json;

namespace XamSpeak
{
	public class BingSpellCheckSuggestionModel
	{
		[JsonProperty("suggestion")]
		public string Suggestion { get; set; }

		[JsonProperty("score")]
		public string ConfidenceScore { get; set; }
	}

	public class MisspelledWordModel
	{
		[JsonProperty("offset")]
		public int StringOffset { get; set; }

		[JsonProperty("token")]
		public string MisspelledWord { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("suggestions")]
		public List<BingSpellCheckSuggestionModel> Suggesstions { get; set; }
	}

	public class MisspelledWordRootObjectModel
	{
		[JsonProperty("_type")]
		public string Type { get; set; }

		[JsonProperty("flaggedTokens")]
		public List<MisspelledWordModel> FlaggedTokens { get; set; }
	}
}
