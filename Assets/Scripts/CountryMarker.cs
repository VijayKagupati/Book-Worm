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
    public Material snapIndicatorMaterial;
    
    [Header("Snapping Settings")]
    public float snapDistance = 0.2f;
    public float snapForce = 5f;
    public GameObject snapIndicatorPrefab;
    
    private OVRGrabbable grabbable;
    private Vector3 originalPosition;
    private bool isPlaced = false;
    private bool isSnapped = false;
    private GameObject snapIndicator;
    private GameObject globe;
    private Rigidbody rb;
    private bool wasGrabbed = false;
    
    private void Awake()
    {
        grabbable = GetComponent<OVRGrabbable>();
        rb = GetComponent<Rigidbody>();
        originalPosition = transform.position;
        UpdateText();
        
        // Find the globe object
        globe = GameObject.FindGameObjectWithTag("Globe");
        if (globe == null)
            globe = GameObject.Find("Globe");
            
        // Create snap indicator
        CreateSnapIndicator();
    }
    
    private void CreateSnapIndicator()
    {
        if (snapIndicatorPrefab != null)
        {
            snapIndicator = Instantiate(snapIndicatorPrefab);
            snapIndicator.SetActive(false);
        }
        else
        {
            // Create a simple indicator if no prefab is specified
            snapIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(snapIndicator.GetComponent<Collider>());
            snapIndicator.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            
            // Apply indicator material
            if (snapIndicatorMaterial != null)
                snapIndicator.GetComponent<MeshRenderer>().material = snapIndicatorMaterial;
            else
                snapIndicator.GetComponent<MeshRenderer>().material.color = Color.green;
                
            snapIndicator.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (grabbable != null && globe != null)
        {
            bool isCurrentlyGrabbed = grabbable.isGrabbed;
            
            // Track when grab state changes
            if (isCurrentlyGrabbed && !wasGrabbed)
            {
                // Just grabbed
                isSnapped = false;
                transform.parent = null;
            }
            else if (!isCurrentlyGrabbed && wasGrabbed)
            {
                // Just released
                if (isSnapped)
                {
                    // If in snap range, parent to globe
                    transform.parent = globe.transform;
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                    }
                }
            }
            
            // Handle snap logic when grabbed
            if (isCurrentlyGrabbed && !isPlaced)
            {
                HandleSnapping();
            }
            
            // Update grab state
            wasGrabbed = isCurrentlyGrabbed;
        }
        
        // Hide indicator if not being grabbed
        if (snapIndicator != null && !wasGrabbed)
        {
            snapIndicator.SetActive(false);
        }
    }
    
    private void HandleSnapping()
    {
        if (globe == null) return;
        
        // Get globe properties
        Vector3 globeCenter = globe.transform.position;
        float globeRadius = GetGlobeRadius(globe);
        
        // Calculate distance from marker to globe center
        float distanceToCenter = Vector3.Distance(transform.position, globeCenter);
        
        // Check if within snap range of the globe surface
        if (Mathf.Abs(distanceToCenter - globeRadius) < snapDistance)
        {
            // Calculate the nearest point on globe surface
            Vector3 dirToGlobe = (transform.position - globeCenter).normalized;
            Vector3 snapPoint = globeCenter + dirToGlobe * globeRadius;
            
            // Show snap indicator
            if (snapIndicator != null)
            {
                snapIndicator.SetActive(true);
                snapIndicator.transform.position = snapPoint;
            }
            
            // Apply gentle force toward snap point when close
            if (rb != null && !rb.isKinematic)
            {
                Vector3 snapForceVector = (snapPoint - transform.position) * snapForce;
                rb.AddForce(snapForceVector, ForceMode.Acceleration);
            }
            
            isSnapped = true;
        }
        else
        {
            // Hide indicator when not in range
            if (snapIndicator != null)
            {
                snapIndicator.SetActive(false);
            }
            isSnapped = false;
        }
    }
    
    private float GetGlobeRadius(GameObject globeObj)
    {
        if (globeObj == null) return 1f;
        
        // Try to get radius from a sphere collider
        SphereCollider sphereCollider = globeObj.GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            return sphereCollider.radius * Mathf.Max(
                globeObj.transform.lossyScale.x,
                Mathf.Max(globeObj.transform.lossyScale.y, globeObj.transform.lossyScale.z)
            );
        }
        
        // If no sphere collider, estimate from renderer bounds
        Renderer renderer = globeObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.extents.magnitude / 2f;
        }
        
        // Default fallback
        return 0.5f;
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
            
        // Hide snap indicator when placed
        if (snapIndicator != null)
            snapIndicator.SetActive(false);
            
        isPlaced = true;
    }
    
    public void Reset()
    {
        markerRenderer.material = defaultMaterial;
        transform.position = originalPosition;
        transform.parent = null;
        
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        isPlaced = false;
        isSnapped = false;
    }
    
    public void ReturnToTray(Transform markerTray)
    {
        transform.parent = null;
        
        if (markerTray != null)
            transform.position = markerTray.position;
        else
            transform.position = originalPosition;
            
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        isPlaced = false;
        isSnapped = false;
    }
    
    private void OnDestroy()
    {
        if (snapIndicator != null)
            Destroy(snapIndicator);
    }
}