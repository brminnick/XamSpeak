using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.ProjectOxford.SpeakerRecognition;

namespace XamSpeak
{
    public static class SpeakerRecognitionServices
    {
        #region Constant Fields
        readonly static Lazy<SpeakerIdentificationServiceClient> _speakerIdentificationClientHolder = new Lazy<SpeakerIdentificationServiceClient>(() => new SpeakerIdentificationServiceClient(CognitiveServicesKeys.SpeakerRecognitionAPIKey));
        #endregion

        #region Properties
        static SpeakerIdentificationServiceClient SpeakerIdentificationClient => _speakerIdentificationClientHolder.Value;
        #endregion

        #region Methods
        public static async Task<string> IdentifySpeaker(Stream audioStream)
        {
            var temp = SpeakerIdentificationClient.IdentifyAsync(audioStream,,true);
        }
        #endregion
    }
}
