using System;
using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    private static DataManager _instance;
    public static DataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject dataManagerObj = new GameObject("DataManager");
                _instance = dataManagerObj.AddComponent<DataManager>();
                DontDestroyOnLoad(dataManagerObj);
            }
            return _instance;
        }
    }
    
    public PlayerData playerData;
    private string saveFilePath;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        saveFilePath = Path.Combine(Application.persistentDataPath, "playerData.json");
        LoadData();
    }
    
    public void LoadData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            playerData = JsonUtility.FromJson<PlayerData>(json);
        }
        else
        {
            playerData = new PlayerData();
            SaveData();
        }
        
        UpdateStreak();
    }
    
    public void SaveData()
    {
        string json = JsonUtility.ToJson(playerData);
        File.WriteAllText(saveFilePath, json);
    }
    
    public void UpdateStreak()
    {
        DateTime today = DateTime.Today;
        
        // If played today, nothing to update
        if (playerData.lastPlayedDate.Date == today)
            return;
            
        // If played yesterday, increment streak
        if (playerData.lastPlayedDate.Date == today.AddDays(-1))
        {
            playerData.currentStreak++;
        }
        // If more than one day gap, reset streak
        else if ((today - playerData.lastPlayedDate.Date).Days > 1)
        {
            playerData.currentStreak = 0;
        }
        
        playerData.lastPlayedDate = today;
        SaveData();
    }
    
    public void CompleteSession(float accuracy, string subject)
    {
        playerData.totalSessionsCompleted++;
        playerData.averageAccuracy = ((playerData.averageAccuracy * (playerData.totalSessionsCompleted - 1)) + accuracy) / playerData.totalSessionsCompleted;
        
        // Update subject progress
        if (!playerData.subjectProgress.ContainsKey(subject))
            playerData.subjectProgress[subject] = 0;
        playerData.subjectProgress[subject]++;
        
        // Add leaf for streak garden
        playerData.totalLeavesEarned++;
        
        UpdateStreak();
        SaveData();
    }
}