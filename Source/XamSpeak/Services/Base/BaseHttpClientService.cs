using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;

using Newtonsoft.Json;

using Xamarin.Forms;

namespace XamSpeak
{
    abstract class BaseHttpClientService
    {
        #region Constant Fields
        static readonly Lazy<JsonSerializer> _serializerHolder = new Lazy<JsonSerializer>();
        static readonly Lazy<HttpClient> _clientHolder = new Lazy<HttpClient>(() => CreateHttpClient(TimeSpan.FromSeconds(60)));
        #endregion

        #region Fields
        static int _networkIndicatorCount = 0;
        #endregion

        #region Properties
        static HttpClient Client => _clientHolder.Value;
        static JsonSerializer Serializer => _serializerHolder.Value;
        #endregion

        #region Methods
        protected static Task<T> GetDataObjectFromAPI<T>(string apiUrl) =>
            GetDataObjectFromAPI<T, object>(apiUrl);

        protected static async Task<TDataObject> GetDataObjectFromAPI<TDataObject, TPayloadData>(string apiUrl, TPayloadData data = default(TPayloadData))
        {
            var stringPayload = string.Empty;

            if (data != null)
                stringPayload = await Task.Run(() => JsonConvert.SerializeObject(data)).ConfigureAwait(false);

            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            using (var stream = await Client.GetStreamAsync(apiUrl).ConfigureAwait(false))
            using (var reader = new StreamReader(stream))
            using (var json = new JsonTextReader(reader))
            {
                if (json == null)
                    return default(TDataObject);

                return await Task.Run(() => Serializer.Deserialize<TDataObject>(json)).ConfigureAwait(false);
            }
        }

        static HttpClient CreateHttpClient(TimeSpan timeout)
        {
            HttpClient client;

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                case Device.Android:
                    client = new HttpClient();
                    break;
                default:
                    client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip });
                    break;

            }
            client.Timeout = timeout;
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", CognitiveServicesConstants.BingSpellCheckAPIKey);

            return client;
        }
        #endregion
    }
}
