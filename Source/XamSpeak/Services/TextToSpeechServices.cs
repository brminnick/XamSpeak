using System.Text;
using System.Collections.Generic;

using Plugin.TextToSpeech;

using Xamarin.Forms;

namespace XamSpeak
{
    public static class TextToSpeechServices
    {
        static Command<string> _speakTextCommand;

        static Command<string> SpeakTextCommand => _speakTextCommand ??
            (_speakTextCommand = new Command<string>(async text => await CrossTextToSpeech.Current.Speak(text)));

        public static string SpeakText(List<string> textList)
        {
            var stringBuilder = new StringBuilder();

            foreach (var lineOfText in textList)
            {
                stringBuilder.AppendLine(lineOfText);

                SpeakTextCommand?.Execute(lineOfText);
            }

            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            return stringBuilder.ToString();
        }
    }
}
