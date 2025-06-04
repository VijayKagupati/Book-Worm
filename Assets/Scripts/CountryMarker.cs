using UnityEngine;
using TMPro;

public class CountryMarker : MonoBehaviour
{
    public string countryName;
    public TextMeshProUGUI countryText;
    
    [Header("Visual Feedback")]
    public MeshRenderer markerRenderer;
    public Material defaultMaterial;
    public Material correctMaterial;
    public Material incorrectMaterial;
    
    private OVRGrabbable grabbable;
    private Vector3 originalPosition;
    private bool isPlaced = false;
    
    private void Awake()
    {
        grabbable = GetComponent<OVRGrabbable>();
        originalPosition = transform.position;
        UpdateText();
    }
    
    public void UpdateText()
    {
        if (countryText != null)
            countryText.text = countryName;
    }
    
    public void ShowFeedback(bool isCorrect)
    {
        if (markerRenderer != null)
            markerRenderer.material = isCorrect ? correctMaterial : incorrectMaterial;
    }
    
    public void Reset()
    {
        markerRenderer.material = defaultMaterial;
        transform.position = originalPosition;
        isPlaced = false;
    }
    
    public void ReturnToTray(Transform markerTray)
    {
        if (markerTray != null)
            transform.position = markerTray.position;
        else
            transform.position = originalPosition;
            
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}