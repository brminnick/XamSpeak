# XamSpeak
XamSpeak allows you to take a photo of text and it will dictate it aloud!

## About
XamSpeak is an iOS and Android app that will dictate text from a photo. 

We are leveraging [Microsoft Cognitive Services](https://www.microsoft.com/cognitive-services/) to perform Optical Character Recognition (OCR) and Spell Check.

## ToDo
To access the Microsoft Coginitive Services API from this app, sign up for a [free API Key](https://www.microsoft.com/cognitive-services/) and insert it to the code [here](./Source/XamSpeak/Constants/CognitiveServicesConstants.cs#L8). After adding your API Key, remove the diagnostic directive [located here](./Source/XamSpeak/Constants/CognitiveServicesConstants.cs#L5).

![](./Demos/XamSpeakGif.gif)

### Author
Brandon Minnick

Xamarin Customer Success Engineer
