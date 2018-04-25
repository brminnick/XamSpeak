using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace XamSpeak
{
    abstract class SpellCheckServices : BaseHttpClientService
    {
        #region Constant Fields
        const double _minimumConfidenceScore = 0.80;
        #endregion

        #region Events
        public static event EventHandler InvalidBingSpellCheckAPIKey;
        public static event EventHandler Error429_TooManySpellCheckAPIRequests;
        #endregion

        #region Methods
        public static async Task<List<string>> GetSpellCheckedStringList(List<string> stringList)
        {
            int listIndex = 0;
            var correctedLineItemList = new List<string>();

            foreach (string lineItem in stringList)
            {
                correctedLineItemList.Add(lineItem);

                var misspelledWordList = await SpellCheckString(lineItem);

                if (misspelledWordList == null)
                    return null;

                foreach (var misspelledWord in misspelledWordList)
                {
                    var firstSuggestion = misspelledWord.Suggesstions.FirstOrDefault();

                    double.TryParse(firstSuggestion?.ConfidenceScore, out double confidenceScore);

                    if (confidenceScore >= _minimumConfidenceScore)
                    {
                        var correctedLineItem = correctedLineItemList[listIndex].Replace(misspelledWord.MisspelledWord, firstSuggestion?.Suggestion);

                        correctedLineItemList[listIndex] = correctedLineItem;
                    }
                }

                listIndex++;
            }

            return correctedLineItemList;
        }


        static async Task<List<MisspelledWordModel>> SpellCheckString(string text)
        {
            try
            {
                var flaggedTokenList = await GetObjectFromAPI<MisspelledWordRootObjectModel>($"https://api.cognitive.microsoft.com/bing/v7.0/spellcheck/?text={text}");

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
