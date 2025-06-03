using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    Lesson,
    Garden,
    Settings
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("GameManager");
                _instance = obj.AddComponent<GameManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }
    
    [Header("Game Settings")]
    public string currentSubject = "Math";
    public float gameDifficulty = 1.0f;
    
    [Header("State Management")]
    public GameState currentState = GameState.MainMenu;
    
    [Header("UI References")]
    public GameObject mainMenuUI;
    public GameObject lessonUI;
    public GameObject gardenUI;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        // Ensure DataManager is initialized
        DataManager dataManager = DataManager.Instance;
        
        // Set initial UI state
        SwitchState(GameState.MainMenu);
    }
    
    public void SwitchState(GameState newState)
    {
        currentState = newState;
        
        // Hide all UI panels first
        if (mainMenuUI) mainMenuUI.SetActive(false);
        if (lessonUI) lessonUI.SetActive(false);
        if (gardenUI) gardenUI.SetActive(false);
        
        // Show appropriate UI based on state
        switch (currentState)
        {
            case GameState.MainMenu:
                if (mainMenuUI) mainMenuUI.SetActive(true);
                break;
            case GameState.Lesson:
                if (lessonUI) lessonUI.SetActive(true);
                break;
            case GameState.Garden:
                if (gardenUI) gardenUI.SetActive(true);
                break;
        }
    }
    
    public void StartGame()
    {
        SwitchState(GameState.Lesson);
        // Additional setup for starting the lesson would go here
    }
    
    public void ViewGarden()
    {
        SwitchState(GameState.Garden);
    }
    
    public void ReturnToMainMenu()
    {
        SwitchState(GameState.MainMenu);
    }

    public void EndGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.Exit(0);
        #else
        Application.Quit();
        #endif
    }
    
    // Called when a lesson is completed
    public void CompleteLesson(float accuracy)
    {
        // Update player data
        DataManager.Instance.CompleteSession(accuracy, currentSubject);
        
        // Return to main menu
        ReturnToMainMenu();
    }
}