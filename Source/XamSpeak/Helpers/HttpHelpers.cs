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
        static readonly Lazy<HttpClient> _clientHolder = new Lazy<HttpClient>(CreateHttpClient);
        static readonly JsonSerializer _serializer = new JsonSerializer();
        #endregion

        #region Events
        public static event EventHandler InvalidBingSpellCheckAPIKey;
        public static event EventHandler Error429_TooManySpellCheckAPIRequests;
        #endregion

        #region Properties
        static HttpClient Client => _clientHolder.Value;
        #endregion

        #region Methods
        public static async Task<List<MisspelledWordModel>> SpellCheckString(string text)
        {
            Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", CognitiveServicesConstants.BingSpellCheckAPIKey);

            var flaggedTokenList = await GetDataObjectFromAPI<MisspelledWordRootObjectModel>($"https://api.cognitive.microsoft.com/bing/v5.0/spellcheck/?text={text}");

            Client.DefaultRequestHeaders.Remove("Ocp-Apim-Subscription-Key");

            return flaggedTokenList?.FlaggedTokens;
        }

        static async Task<T> GetDataObjectFromAPI<T>(string apiUrl)
        {
            try
            {
                using (var stream = await Client.GetStreamAsync(apiUrl).ConfigureAwait(false))
                using (var reader = new StreamReader(stream))
                using (var json = new JsonTextReader(reader))
                {
                    if (json == null)
                        return default(T);

                    return await Task.Run(() => _serializer.Deserialize<T>(json));
                }
            }
            catch (HttpRequestException e)
            {
                DebugHelpers.PrintException(e);

                if (e.Message.Contains("401"))
                    OnInvalidBingSpellCheckAPIKey();
                else if (e.Message.Contains("429"))
                    OnError429_TooManySpellCheckAPIRequests();

                return default(T);
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

        static void OnInvalidBingSpellCheckAPIKey() =>
            InvalidBingSpellCheckAPIKey?.Invoke(null, EventArgs.Empty);

        static void OnError429_TooManySpellCheckAPIRequests() =>
            Error429_TooManySpellCheckAPIRequests?.Invoke(null, EventArgs.Empty);
        #endregion
    }
}
