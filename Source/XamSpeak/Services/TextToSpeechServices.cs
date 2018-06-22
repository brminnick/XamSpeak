using System.Text;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Essentials;

namespace XamSpeak
{
	public static class TextToSpeechServices
	{
		static Command<string> _speakTextCommand;

		static Command<string> SpeakTextCommand => _speakTextCommand ??
			(_speakTextCommand = new Command<string>(async text => await TextToSpeech.SpeakAsync(text).ConfigureAwait(false)));

		public static string SpeakText(List<string> textList)
		{
			var stringBuilder = new StringBuilder();

			foreach (var lineOfText in textList)
			{
				stringBuilder.AppendLine(lineOfText);

				SpeakTextCommand?.Execute(lineOfText);
			}

			if (stringBuilder.Length > 1)
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			
			return stringBuilder.ToString();
		}
	}
}
