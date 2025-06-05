using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GardenUIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI streakDetailsText;
    public TextMeshProUGUI achievementText;
    
    [Header("Streak Milestones")]
    public string[] streakMilestones = new string[] {
        "Just getting started!",
        "Building a habit!",
        "Consistency is key!",
        "Impressive dedication!",
        "Master learner!"
    };
    
    private void OnEnable()
    {
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        PlayerData data = DataManager.Instance.playerData;
        
        // Update streak details
        if (streakDetailsText != null)
        {
            string lastPlayed = data.lastPlayedDate.ToString("MMMM d, yyyy");
            streakDetailsText.text = $"You've maintained your streak since {lastPlayed}";
        }
        
        // Update achievement message based on streak
        if (achievementText != null)
        {
            int milestoneIndex = Mathf.Min(data.currentStreak / 5, streakMilestones.Length - 1);
            achievementText.text = streakMilestones[milestoneIndex];
        }
    }
}