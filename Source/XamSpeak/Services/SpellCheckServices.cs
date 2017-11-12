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
    abstract class SpellCheckServices : BaseHttpClientService
    {
        #region Events
        public static event EventHandler InvalidBingSpellCheckAPIKey;
        public static event EventHandler Error429_TooManySpellCheckAPIRequests;
        #endregion

        #region Methods
        public static async Task<List<MisspelledWordModel>> SpellCheckString(string text)
        {
            try
            {
                var flaggedTokenList = await GetDataObjectFromAPI<MisspelledWordRootObjectModel>($"https://api.cognitive.microsoft.com/bing/v5.0/spellcheck/?text={text}");

                return flaggedTokenList?.FlaggedTokens;
            }
            catch (HttpRequestException e) when (e.Message.Contains("401"))
            {
                DebugHelpers.PrintException(e);
                OnInvalidBingSpellCheckAPIKey();

                return null;
            }
            catch (HttpRequestException e) when (e.Message.Contains("429"))
            {
                DebugHelpers.PrintException(e);
                OnError429_TooManySpellCheckAPIRequests();

                return null;
            }
            catch (Exception e)
            {
                DebugHelpers.PrintException(e);
                return null;
            }
        }

        static void OnInvalidBingSpellCheckAPIKey() =>
            InvalidBingSpellCheckAPIKey?.Invoke(null, EventArgs.Empty);

        static void OnError429_TooManySpellCheckAPIRequests() =>
            Error429_TooManySpellCheckAPIRequests?.Invoke(null, EventArgs.Empty);
        #endregion
    }
}
