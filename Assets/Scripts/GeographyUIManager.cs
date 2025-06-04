using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GeographyUIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI roundText;
    public GameObject successPanel;
    public TextMeshProUGUI successText;
    
    [Header("Animation")]
    public float fadeTime = 1.5f;
    public CanvasGroup successCanvasGroup;
    
    private GeographyController gameController;
    
    private void Start()
    {
        gameController = GeographyController.Instance;
        
        // Subscribe to events
        if (gameController != null)
        {
            gameController.onRoundComplete.AddListener(ShowRoundSuccess);
            gameController.onGameComplete.AddListener(ShowGameComplete);
        }
        
        // Hide success panel initially
        if (successPanel)
            successPanel.SetActive(false);
            
        UpdateRoundText();
    }
    
    public void UpdateRoundText()
    {
        if (roundText != null && gameController != null)
        {
            roundText.text = $"Round {gameController.currentRound}/{gameController.maxRounds}";
        }
    }
    
    private void ShowRoundSuccess()
    {
        if (successPanel)
        {
            successPanel.SetActive(true);
            successText.text = "Countries Placed Correctly!";
            
            if (successCanvasGroup != null)
                StartCoroutine(FadeCanvasGroup(successCanvasGroup, 0f, 1f, fadeTime));
        }
    }
    
    private void ShowGameComplete()
    {
        if (successPanel)
        {
            successPanel.SetActive(true);
            successText.text = "Geography Lesson Complete!";
            
            if (successCanvasGroup != null)
                StartCoroutine(FadeCanvasGroup(successCanvasGroup, 0f, 1f, fadeTime));
        }
    }
    
    private System.Collections.IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            cg.alpha = Mathf.Lerp(start, end, elapsed/duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cg.alpha = end;
    }
}