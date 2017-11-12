using System;
using System.Collections.Generic;
using System.Text;
using Plugin.TextToSpeech;

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

                CrossTextToSpeech.Current.Speak(lineOfText);
            }

            return stringBuilder.ToString();
        }
    }
}
