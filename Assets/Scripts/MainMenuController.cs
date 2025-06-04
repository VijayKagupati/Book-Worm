using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{

    [Header("UI Elements")]
    public TextMeshProUGUI streakText;
    
    private void Start()
    {
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (streakText)
        {
            int streak = DataManager.Instance.playerData.currentStreak;
            streakText.text = streak > 0 ? $"Your Current Streak is {streak} Day(s)" : "Start your streak today!";
        }
    }
}