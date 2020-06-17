using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Microsoft.Azure.CognitiveServices.Language.SpellCheck;
using Microsoft.Azure.CognitiveServices.Language.SpellCheck.Models;

namespace XamSpeak
{
    static class SpellCheckServices
    {
        const double _minimumConfidenceScore = 0.80;

        static readonly WeakEventManager _invalidBingSpellCheckAPIKeyEventManager = new WeakEventManager();
        static readonly WeakEventManager _error429_TooManySpellCheckAPIRequests = new WeakEventManager();

        static readonly Lazy<SpellCheckClient> _spellCheckApiClient =
            new Lazy<SpellCheckClient>(() => new SpellCheckClient(new ApiKeyServiceClientCredentials(CognitiveServicesConstants.BingSpellCheckAPIKey)));

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

        static SpellCheckClient SpellCheckApiClient => _spellCheckApiClient.Value;

        public static async IAsyncEnumerable<string> GetSpellCheckedStringList(IEnumerable<string> stringList)
        {
            foreach (string lineItem in stringList)
            {
                var correctLineItem = lineItem;

                var misspelledWordList = await SpellCheckString(lineItem).ConfigureAwait(false);

                foreach (var word in misspelledWordList)
                {
                    var firstSuggestion = word.Suggestions.FirstOrDefault();

                    if (firstSuggestion?.Score >= _minimumConfidenceScore)
                    {
                        correctLineItem = correctLineItem.Replace(word.Token, firstSuggestion.Suggestion);
                    }
                }

                yield return correctLineItem;
            }
        }

        static async Task<IEnumerable<SpellingFlaggedToken>> SpellCheckString(string text)
        {
            try
            {
                var spellCheckModel = await SpellCheckApiClient.SpellCheckerAsync(text).ConfigureAwait(false);

                return spellCheckModel.FlaggedTokens;
            }
            catch (ErrorResponseException e) when (e.Response.StatusCode is HttpStatusCode.Unauthorized)
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
    }
}
