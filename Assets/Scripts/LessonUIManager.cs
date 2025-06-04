using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LessonUIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI roundText;
    public GameObject successPanel;
    public TextMeshProUGUI successText;
    
    [Header("Animation")]
    public float fadeTime = 2.0f;
    public CanvasGroup successCanvasGroup;
    
    private EquationController equationController;
    private float roundStartTime;
    
    private void Start()
    {
        equationController = EquationController.Instance;
        
        // Subscribe to events
        if (equationController != null)
        {
            equationController.onRoundComplete.AddListener(ShowSuccessMessage);
            equationController.onGameComplete.AddListener(ShowGameComplete);
        }
        
        // Hide success panel initially
        if (successPanel) 
            successPanel.SetActive(false);
            
        UpdateRoundText();
    }
    
    private void Update()
    {
        UpdateTimerText();
    }
    
    public void StartRound()
    {
        roundStartTime = Time.time;
        UpdateRoundText();
        if (successPanel) 
            successPanel.SetActive(false);
    }
    
    private void UpdateTimerText()
    {
        if (timerText != null)
        {
            float elapsed = Time.time - roundStartTime;
            timerText.text = $"Time: {elapsed:F1}s";
        }
    }
    
    private void UpdateRoundText()
    {
        if (roundText != null && equationController != null)
        {
            roundText.text = $"Round {equationController.CurrentRound}/{equationController.maxRounds}";
        }
    }
    
    private void ShowSuccessMessage()
    {
        if (successPanel)
        {
            successPanel.SetActive(true);
            successText.text = "Equation Balanced!";
            
            // Optional: animate fade-in
            if (successCanvasGroup != null)
                StartCoroutine(FadeCanvasGroup(successCanvasGroup, 0f, 1f, fadeTime));
        }
    }
    
    private void ShowGameComplete()
    {
        if (successPanel)
        {
            successPanel.SetActive(true);
            successText.text = "All Rounds Complete!";
            
            // Optional: animate fade-in
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