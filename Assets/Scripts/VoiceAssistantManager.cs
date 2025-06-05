using UnityEngine;
using System.Collections;
using System.Text.Json.Nodes;
using NaughtyAttributes;
using Meta.WitAi.Dictation;
using Meta.WitAi.Dictation.Events;
using GroqApiLibrary;
using Meta.WitAi.Dictation.Data;
using UnityEngine.XR;
using System.Collections.Generic;
using UnityEngine.Events;

public class VoiceAssistantManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private DictationService dictationService;
    [SerializeField] private DictationDisplay dictationDisplay;

    [Header("API Configuration")]
    [SerializeField] private string groqApiKey;
    [SerializeField] private PlayAIVoice selectedVoice = PlayAIVoice.Fritz_PlayAI;

    [Header("Conversation Settings")]
    [SerializeField, TextArea(3, 10)] private string baseSystemPrompt =
        "You are a helpful learning assistant for children. Keep answers simple, educational and engaging.";

    [Header("Events")]
    public UnityEvent onDictationStarted;
    public UnityEvent onDictationEnded;
    public UnityEvent onPromptSent;
    public UnityEvent onPromptReceived;
    public UnityEvent onTTSStarted;
    public UnityEvent onTTSEnded;
    
    // Game references
    private GameManager gameManager;
    private EquationController equationController;
    private GeographyController geographyController;

    // State variables
    private bool isListening = false;
    private GroqApiClient groqApi;
    private JsonArray messageHistory = new JsonArray();
    private string transcribedText = string.Empty;

    [SerializeField] private OVRInput.Controller _controller = OVRInput.Controller.RTouch;
    private bool previousPrimaryButtonState = false;
    
    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (dictationService == null)
            dictationService = FindObjectOfType<DictationService>();

        if (dictationService == null)
        {
            Debug.LogError("No DictationService found in the scene. Please add a Dictation Service.");
            return;
        }

        // Initialize game references
        gameManager = GameManager.Instance;
        equationController = FindObjectOfType<EquationController>();
        geographyController = FindObjectOfType<GeographyController>();

        // Initialize Groq API client
        groqApi = new GroqApiClient(groqApiKey);

        // Add system prompt to conversation
        AddToMessageHistory("system", baseSystemPrompt);

        // Set up dictation events
        dictationService.DictationEvents.OnDictationSessionStarted.AddListener(OnDictationStarted);
        dictationService.DictationEvents.OnFullTranscription.AddListener(OnTranscriptionComplete);
        dictationService.DictationEvents.OnDictationSessionStopped.AddListener(OnDictationStopped);
        dictationService.DictationEvents.OnError.AddListener(OnDictationError);
    }

    private void Update()
    {
        CheckForPrimaryButtonPress();
    
        // Editor testing
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.V))
        {
            ToggleVoiceAssistant();
        }
        #endif
    }

    private void CheckForPrimaryButtonPress()
    {
        bool primaryButtonPressed = OVRInput.Get(OVRInput.Button.One, _controller);

        // Check for button press (not hold, just the press event)
        if (primaryButtonPressed && !previousPrimaryButtonState)
        {
            ToggleVoiceAssistant();
        }

        previousPrimaryButtonState = primaryButtonPressed;
    }

    private void OnDestroy()
    {
        if (dictationService != null)
        {
            dictationService.DictationEvents.OnDictationSessionStarted.RemoveListener(OnDictationStarted);
            dictationService.DictationEvents.OnFullTranscription.RemoveListener(OnTranscriptionComplete);
            dictationService.DictationEvents.OnDictationSessionStopped.RemoveListener(OnDictationStopped);
            dictationService.DictationEvents.OnError.RemoveListener(OnDictationError);
        }
    }

    [Button]
    public void ToggleVoiceAssistant()
    {
        if (!isListening)
        {
            StartListening();
        }
        else
        {
            StopListening();
        }
    }

    private void StartListening()
    {
        Debug.Log("Starting voice recognition...");
        isListening = true;
        dictationService.Activate();
    }

    private void StopListening()
    {
        Debug.Log("Stopping voice recognition...");
        isListening = false;
        dictationService.Deactivate();
    }

    private void OnDictationStarted(DictationSession session)
    {
        Debug.Log("Dictation session started");
        onDictationStarted?.Invoke();
    }

    private void OnTranscriptionComplete(string text)
    {
        transcribedText = text;
        Debug.Log("Transcribed: " + transcribedText);
    
        if (dictationDisplay != null)
            dictationDisplay.DisplayTextWithTypingEffect(transcribedText);
    }

    private void OnDictationStopped(DictationSession session)
    {
        Debug.Log("Dictation session stopped");

        if (!string.IsNullOrEmpty(transcribedText))
        {
            StartCoroutine(ProcessTranscribedText(transcribedText));
        }
        else
        {
            Debug.LogWarning("No transcription received");
        }
        onDictationEnded?.Invoke();
    }

    private void OnDictationError(string error, string message)
    {
        Debug.LogError($"Dictation error: {error} - {message}");
        isListening = false;
    }

    private IEnumerator ProcessTranscribedText(string text)
    {
        // Clear previous conversation and start fresh with context-aware prompt
        messageHistory.Clear();
        
        // Get context-aware system prompt
        string contextSystemPrompt = GetGameContextPrompt();
        AddToMessageHistory("system", contextSystemPrompt);
        
        // Add user message
        AddToMessageHistory("user", text);
        
        string response = string.Empty;

        yield return StartCoroutine(SendToGroqChat((result) => {
            response = result;
        }));

        if (string.IsNullOrEmpty(response))
        {
            Debug.LogError("Failed to get response from Groq");
            yield break;
        }

        Debug.Log("Groq response: " + response);
        AddToMessageHistory("assistant", response);

        // Convert response to speech
        yield return GenerateSpeech(response);
    }

    private string GetGameContextPrompt()
    {
        // Make sure we have access to GameManager
        if (gameManager == null)
            gameManager = GameManager.Instance;

        if (gameManager == null)
            return baseSystemPrompt;

        string gameContext = baseSystemPrompt + "\n\n";

        switch (gameManager.currentState)
        {
            case GameState.Math:
                gameContext += GetMathGameContext();
                break;
                
            case GameState.Geography:
                gameContext += GetGeographyGameContext();
                break;
                
            case GameState.Garden:
                gameContext += "The child is looking at their achievement garden. " +
                              "Plants in the garden represent completed lessons. " +
                              "Each plant represents progress in their learning journey.";
                break;
                
            case GameState.MainMenu:
                gameContext += "The child is at the main menu. They can choose between Math or Geography lessons.";
                break;
                
            default:
                break;
        }

        return gameContext;
    }

    private string GetMathGameContext()
    {
        if (equationController == null)
            equationController = FindObjectOfType<EquationController>();
            
        if (equationController == null)
            return "The child is playing a Math game where they need to balance equations by placing number tokens.";

        string leftSideValues = "no tokens";
        string rightSideValues = "no tokens";
        
        // Get values from left side
        if (equationController.leftZone != null && equationController.leftZone.containedTokens.Count > 0)
        {
            leftSideValues = "";
            foreach (var token in equationController.leftZone.containedTokens)
            {
                leftSideValues += token.value + ", ";
            }
            leftSideValues = leftSideValues.TrimEnd(',', ' ');
        }
        
        // Get values from right side
        if (equationController.rightZone != null && equationController.rightZone.containedTokens.Count > 0)
        {
            rightSideValues = "";
            foreach (var token in equationController.rightZone.containedTokens)
            {
                rightSideValues += token.value + ", ";
            }
            rightSideValues = rightSideValues.TrimEnd(',', ' ');
        }
        
        return $"The child is playing a Math game. The objective is to make both sides of the scale equal by placing number tokens. " +
               $"Currently on Round {equationController.CurrentRound} of {equationController.maxRounds}. " +
               $"The left side of the scale has: {leftSideValues}. " +
               $"The right side of the scale has: {rightSideValues}. " +
               $"Provide hints to help balance the equation, but don't give the complete solution.";
    }

    private string GetGeographyGameContext()
    {
        if (geographyController == null)
            geographyController = FindObjectOfType<GeographyController>();
            
        if (geographyController == null)
            return "The child is playing a Geography game where they need to place country markers on a map.";

        List<string> countryNames = new List<string>();
        foreach (var target in geographyController.currentTargets)
        {
            if (target != null)
                countryNames.Add(target.countryName);
        }
        
        return $"The child is playing a Geography game. The objective is to place country markers in their correct locations on a map. " +
               $"Currently on Round {geographyController.currentRound} of {geographyController.maxRounds}. " +
               $"They have placed {geographyController.countriesPlaced} out of {countryNames.Count} countries correctly. " +
               $"The countries they need to place in this round are: {string.Join(", ", countryNames)}. " +
               $"Provide educational hints about these countries to help them, but don't directly tell them where to place the markers.";
    }

    private IEnumerator SendToGroqChat(System.Action<string> callback)
    {
        var messagesArray = new JsonArray();
        foreach (var message in messageHistory)
        {
            messagesArray.Add(message.DeepClone());
        }

        var request = new JsonObject
        {
            ["model"] = "llama-3.1-8b-instant",
            ["messages"] = messagesArray
        };
        
        onPromptSent?.Invoke();

        var task = groqApi.CreateChatCompletionAsync(request);

        while (!task.IsCompleted)
            yield return null;

        if (task.Exception != null)
        {
            Debug.LogError("Groq API error: " + task.Exception.Message);
            callback(string.Empty);
        }
        else
        {
            var result = task.Result;
            string content = result?["choices"]?[0]?["message"]?["content"]?.ToString();
            onPromptReceived?.Invoke();
            callback(content ?? string.Empty);
        }
    }

    private IEnumerator GenerateSpeech(string text)
    {
        // Find or add GroqTTS component
        GroqTTS tts = GetComponent<GroqTTS>();
        if (tts == null)
            tts = gameObject.AddComponent<GroqTTS>();

        // Enhanced sanitization to remove all special characters and formatting
        string sanitizedText = text
            .Replace("\n", " ")
            .Replace("\r", " ")
            .Replace("\t", " ")
            .Replace("*", "")    // Remove asterisks used for bold
            .Replace("_", "")    // Remove underscores used for italics
            .Replace("`", "")    // Remove backticks
            .Replace("#", "")    // Remove heading markers
            .Replace("\"", "'"); // Replace double quotes with single quotes

        onTTSStarted?.Invoke();
        var task = tts.GenerateAndPlaySpeech(sanitizedText);

        while (!task.IsCompleted)
            yield return null;

        if (task.Exception != null)
        {
            Debug.LogError("TTS error: " + task.Exception.Message);
        }

        onTTSEnded?.Invoke();
    }

    private void AddToMessageHistory(string role, string content)
    {
        messageHistory.Add(new JsonObject
        {
            ["role"] = role,
            ["content"] = content
        });
    }
}