using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Azure.CognitiveServices.Language.SpellCheck;
using Microsoft.Azure.CognitiveServices.Language.SpellCheck.Models;

namespace XamSpeak
{
	static class SpellCheckServices
	{
		#region Constant Fields
		const double _minimumConfidenceScore = 0.80;
		static readonly Lazy<SpellCheckAPI> _spellCheckApiClient =
			new Lazy<SpellCheckAPI>(() => new SpellCheckAPI(new ApiKeyServiceClientCredentials(CognitiveServicesConstants.BingSpellCheckAPIKey)));
		#endregion

		#region Events
		public static event EventHandler InvalidBingSpellCheckAPIKey;
		public static event EventHandler Error429_TooManySpellCheckAPIRequests;
		#endregion

		#region Properties
		static SpellCheckAPI SpellCheckApiClient => _spellCheckApiClient.Value;
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
			catch (HttpRequestException e) when (e.Message.Contains("401"))
			{
				DebugHelpers.PrintException(e);
				OnInvalidBingSpellCheckAPIKey();
            
				throw;
			}
			catch (HttpRequestException e) when (e.Message.Contains("429"))
			{
				DebugHelpers.PrintException(e);
				OnError429_TooManySpellCheckAPIRequests();

				throw;
			}
		}

		static void OnInvalidBingSpellCheckAPIKey() =>
			InvalidBingSpellCheckAPIKey?.Invoke(null, EventArgs.Empty);

		static void OnError429_TooManySpellCheckAPIRequests() =>
			Error429_TooManySpellCheckAPIRequests?.Invoke(null, EventArgs.Empty);
		#endregion
	}
}
