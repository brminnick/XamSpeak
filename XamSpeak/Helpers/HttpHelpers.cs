using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace XamSpeak
{
	public static class HttpHelpers
	{
		#region Constant Fields
		static readonly HttpClient _client = CreateHttpClient();
		static readonly JsonSerializer _serializer = new JsonSerializer();
		#endregion

		#region Methods
		public static async Task<List<MisspelledWordModel>> SpellCheckStringList(string text)
		{
			_client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", CognitiveServicesConstants.BingSpellCheckAPIKey);

			var flaggedTokenList = await GetDataObjectFromAPI<MisspelledWordRootObjectModel>($"https://api.cognitive.microsoft.com/bing/v5.0/spellcheck/?text={text}");

			_client.DefaultRequestHeaders.Remove("Ocp-Apim-Subscription-Key");

			return flaggedTokenList?.FlaggedTokens;
		}

		static async Task<T> GetDataObjectFromAPI<T>(string apiUrl)
		{
			try
			{
				using (var stream = await _client.GetStreamAsync(apiUrl))
				using (var reader = new StreamReader(stream))
				using (var json = new JsonTextReader(reader))
				{
					if (json == null)
						return default(T);

					return _serializer.Deserialize<T>(json);
				}
			}
			catch (Exception e)
			{
				DebugHelpers.PrintException(e);
				return default(T);
			}
		}

		static HttpClient CreateHttpClient()
		{
			var httpTimeout = TimeSpan.FromSeconds(30);

			var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip })
			{
				Timeout = httpTimeout
			};

			client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

			return client;
		}
		#endregion
	}
}
