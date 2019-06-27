using System.Collections.Generic;
using System.Text;
using AsyncAwaitBestPractices;
using Xamarin.Essentials;

namespace XamSpeak
{
    public static class TextToSpeechServices
    {
        public static string SpeakText(List<string> textList)
        {
            var stringBuilder = new StringBuilder();

            foreach (var lineOfText in textList)
            {
                stringBuilder.AppendLine(lineOfText);

                TextToSpeech.SpeakAsync(lineOfText).SafeFireAndForget(false);
            }

            if (stringBuilder.Length > 1)
                stringBuilder.Remove(stringBuilder.Length - 1, 1);

            return stringBuilder.ToString();
        }
    }
}
