using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;

public class SpeechTranslator : MonoBehaviour
{
    private string subscriptionKey = "cdbf331f6f44417ebc92dc696360d895";
    private string region = "southeastasia";

    public Text detectedTextField;
    public Text translatedTextField;
    public Dropdown detectedLanguage;
    public Dropdown translatedLanguage;
    public Button startButton;
    public Button stopButton;

    private TranslationRecognizer recognizer;
    private SpeechSynthesizer synthesizer;
    private ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();
    private bool isRecognizing = false;

    private void Start()
    {
        detectedLanguage.onValueChanged.AddListener(delegate { UpdateLanguages(); });
        translatedLanguage.onValueChanged.AddListener(delegate { UpdateLanguages(); });

        startButton.onClick.AddListener(OnStartButtonClick);
        stopButton.onClick.AddListener(OnStopButtonClick);

        InitializeRecognizer();
        InitializeSynthesizer();
    }

    private void InitializeRecognizer()
    {
        var config = SpeechTranslationConfig.FromSubscription(subscriptionKey, region);
        config.SpeechRecognitionLanguage = GetLanguageCode(detectedLanguage.options[detectedLanguage.value].text);
        config.AddTargetLanguage(GetTranslationLanguageCode(translatedLanguage.options[translatedLanguage.value].text));

        recognizer = new TranslationRecognizer(config);

        recognizer.Recognizing += (s, e) =>
        {
            Debug.Log($"Recognizing: {e.Result.Text}");
            QueueMainThreadAction(() => detectedTextField.text = $"Detected: {e.Result.Text}");
        };

        recognizer.Recognized += (s, e) =>
        {
            if (e.Result.Reason == ResultReason.TranslatedSpeech)
            {
                string translatedText = e.Result.Translations[GetTranslationLanguageCode(translatedLanguage.options[translatedLanguage.value].text)];
                Debug.Log($"Translated: {translatedText}");
                QueueMainThreadAction(() =>
                {
                    translatedTextField.text = $"Translated: {translatedText}";
                    SynthesizeSpeech(translatedText, GetTranslationLanguageCode(translatedLanguage.options[translatedLanguage.value].text));
                });
            }
        };

        recognizer.Canceled += (s, e) =>
        {
            Debug.Log($"Canceled: {e.Reason}");
            if (e.Reason == CancellationReason.Error)
            {
                Debug.LogError($"ErrorDetails: {e.ErrorDetails}");
            }
        };

        recognizer.SessionStarted += (s, e) =>
        {
            Debug.Log("Session started");
            isRecognizing = true;
        };

        recognizer.SessionStopped += (s, e) =>
        {
            Debug.Log("Session stopped");
            isRecognizing = false;
        };
    }

    private void InitializeSynthesizer()
    {
        var speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
        synthesizer = new SpeechSynthesizer(speechConfig);
    }

    private void QueueMainThreadAction(Action action)
    {
        mainThreadActions.Enqueue(action);
    }

    private void Update()
    {
        while (mainThreadActions.TryDequeue(out var action))
        {
            action();
        }
    }

    private async Task StartTranslationAsync()
    {
        if (!isRecognizing)
        {
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            Debug.Log("Say something...");
        }
    }

    private async Task StopTranslationAsync()
    {
        if (isRecognizing)
        {
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            Debug.Log("Recognition stopped.");
        }
    }

    private void OnStartButtonClick()
    {
        _ = StartTranslationAsync();
    }

    private void OnStopButtonClick()
    {
        _ = StopTranslationAsync();
    }

    private async void UpdateLanguages()
    {
        await StopTranslationAsync();

        // Dispose the old recognizer
        recognizer.Dispose();

        // Initialize a new recognizer with the updated languages
        InitializeRecognizer();

        await StartTranslationAsync();
    }

    private string GetLanguageCode(string language)
    {
        switch (language)
        {
            case "English":
                return "en-US";
            case "Chinese":
                return "zh-CN";
            case "Malay":
                return "ms-MY";
            default:
                return "en-US"; // Default to English if not found
        }
    }

    private string GetTranslationLanguageCode(string language)
    {
        switch (language)
        {
            case "English":
                return "en";
            case "Chinese":
                return "zh-Hans";
            case "Malay":
                return "ms";
            default:
                return "en"; // Default to English if not found
        }
    }

    private void SynthesizeSpeech(string text, string languageCode)
    {
        var config = SpeechConfig.FromSubscription(subscriptionKey, region);
        config.SpeechSynthesisVoiceName = GetVoiceName(languageCode);
        var synth = new SpeechSynthesizer(config);

        synth.SpeakTextAsync(text);
    }

    private string GetVoiceName(string languageCode)
    {
        switch (languageCode)
        {
            case "en":
                return "en-US-JennyNeural";
            case "zh-Hans":
                return "zh-CN-XiaoxiaoNeural";
            case "ms":
                return "ms-MY-OsmanNeural";
            default:
                return "en-US-JennyNeural"; // Default to English voice if not found
        }
    }
}