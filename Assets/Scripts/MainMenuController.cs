using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{

    [Header("UI Elements")]
    public TextMeshProUGUI streakText;
    public TextMeshProUGUI welcomeText;
    
    private void Start()
    {
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (streakText)
        {
            int streak = DataManager.Instance.playerData.currentStreak;
            streakText.text = streak > 0 ? $"You're on a {streak}-day streak!" : "Start your streak today!";
        }
        
        if (welcomeText)
        {
            if (DataManager.Instance.playerData.lastPlayedDate == System.DateTime.MinValue)
            {
                welcomeText.text = "Welcome to BookWorm Learning!";
            }
            else
            {
                welcomeText.text = "Welcome back to BookWorm Learning!";
            }
        }
    }
}