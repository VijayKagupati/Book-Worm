using System.Collections;
using UnityEngine;
using TMPro;

public class DictationDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private string leadingText = "You said: ";
    
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (displayText == null)
            displayText = GetComponent<TextMeshProUGUI>();
            
        ClearText();
    }

    public void DisplayTextWithTypingEffect(string text)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
            
        typingCoroutine = StartCoroutine(TypeText(text));
    }

    public void ClearText()
    {
        if (displayText != null)
            displayText.text = "";
    }

    private IEnumerator TypeText(string text)
    {
        string fullText = leadingText + text;
        displayText.text = "";
        
        foreach (char c in fullText)
        {
            displayText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        typingCoroutine = null;
    }
}