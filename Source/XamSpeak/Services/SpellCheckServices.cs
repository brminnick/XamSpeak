using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AsyncAwaitBestPractices;
using Microsoft.Azure.CognitiveServices.Language.SpellCheck;
using Microsoft.Azure.CognitiveServices.Language.SpellCheck.Models;

namespace XamSpeak
{
    static class SpellCheckServices
    {
        #region Constant Fields
        const double _minimumConfidenceScore = 0.80;

        static readonly WeakEventManager _invalidBingSpellCheckAPIKeyEventManager = new WeakEventManager();
        static readonly WeakEventManager _error429_TooManySpellCheckAPIRequests = new WeakEventManager();

        static readonly Lazy<SpellCheckClient> _spellCheckApiClient =
            new Lazy<SpellCheckClient>(() => new SpellCheckClient(new ApiKeyServiceClientCredentials(CognitiveServicesConstants.BingSpellCheckAPIKey)));
        #endregion

        #region Events
        public static event EventHandler InvalidBingSpellCheckAPIKey
        {
            add => _invalidBingSpellCheckAPIKeyEventManager.AddEventHandler(value);
            remove => _invalidBingSpellCheckAPIKeyEventManager.RemoveEventHandler(value);
        }

        public static event EventHandler Error429_TooManySpellCheckAPIRequests
        {
            add => _error429_TooManySpellCheckAPIRequests.AddEventHandler(value);
            remove => _error429_TooManySpellCheckAPIRequests.RemoveEventHandler(value);
        }
        #endregion

        #region Properties
        static SpellCheckClient SpellCheckApiClient => _spellCheckApiClient.Value;
        #endregion

        #region Methods
        public static async Task<List<string>> GetSpellCheckedStringList(List<string> stringList)
        {
            int listIndex = 0;
            var correctedLineItemList = new List<string>();

            foreach (string lineItem in stringList)
            {
                correctedLineItemList.Add(lineItem);

                var misspelledWordList = await SpellCheckString(lineItem).ConfigureAwait(false);

                foreach (var misspelledWord in misspelledWordList)
                {
                    var firstSuggestion = misspelledWord.Suggestions.FirstOrDefault();

                    if (firstSuggestion?.Score >= _minimumConfidenceScore)
                    {
                        var correctedLineItem = correctedLineItemList[listIndex].Replace(misspelledWord.Token, firstSuggestion?.Suggestion);

                        correctedLineItemList[listIndex] = correctedLineItem;
                    }
                }

                listIndex++;
            }

            return correctedLineItemList;
        }


        static async Task<List<SpellingFlaggedToken>> SpellCheckString(string text)
        {
            try
            {
                var spellCheckModel = await SpellCheckApiClient.SpellCheckerAsync(text).ConfigureAwait(false);

                return new List<SpellingFlaggedToken>(spellCheckModel.FlaggedTokens);
            }
            catch (ErrorResponseException e) when (e.Response.StatusCode.Equals(HttpStatusCode.Unauthorized))
            {
                DebugHelpers.PrintException(e);
                OnInvalidBingSpellCheckAPIKey();

                throw;
            }
            catch (ErrorResponseException e) when (e.Response.StatusCode.Equals(429))
            {
                DebugHelpers.PrintException(e);
                OnError429_TooManySpellCheckAPIRequests();

                throw;
            }
            catch (ArgumentNullException e)
            {
                DebugHelpers.PrintException(e);
                OnInvalidBingSpellCheckAPIKey();

                throw;
            }
        }

        static void OnInvalidBingSpellCheckAPIKey() =>
            _invalidBingSpellCheckAPIKeyEventManager.HandleEvent(null, EventArgs.Empty, nameof(InvalidBingSpellCheckAPIKey));

        static void OnError429_TooManySpellCheckAPIRequests() =>
            _error429_TooManySpellCheckAPIRequests.HandleEvent(null, EventArgs.Empty, nameof(Error429_TooManySpellCheckAPIRequests));
        #endregion
    }
}
