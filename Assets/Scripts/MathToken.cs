using UnityEngine;
using TMPro;

public class MathToken : MonoBehaviour
{
    [Header("Token Properties")]
    public float value;
    public TextMeshProUGUI valueText;
    
    [Header("Visual Feedback")]
    public MeshRenderer tokenRenderer;
    public Material defaultMaterial;
    public Material correctMaterial;
    public Material incorrectMaterial;
    
    private OVRGrabbable grabbable;
    private bool isPlaced = false;
    private Vector3 originalPosition;
    
    private void Awake()
    {
        grabbable = GetComponent<OVRGrabbable>();
        originalPosition = transform.position;
        UpdateText();
    }
    
    public void UpdateText()
    {
        if (valueText != null)
            valueText.text = value.ToString();
    }
    
    public void ShowFeedback(bool isCorrect)
    {
        if (tokenRenderer != null)
            tokenRenderer.material = isCorrect ? correctMaterial : incorrectMaterial;
    }
    
    public void Reset()
    {
        tokenRenderer.material = defaultMaterial;
        transform.position = originalPosition;
        isPlaced = false;
    }
    public void ReturnToTray(Transform tokenTray)
    {
        // Return token to tray or to original position if tray not specified
        if (tokenTray != null)
        {
            transform.position = tokenTray.transform.position;
        }
        else
        {
            transform.position = originalPosition;
        }
        
        // Reset velocity if there's a rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    
}
