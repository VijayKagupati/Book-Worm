using System;
using UnityEngine;
using UnityEngine.Events;

public class CountryTarget : MonoBehaviour
{
    public string countryName;
    public float acceptanceRadius = 0.1f;
    public UnityEvent onCorrectPlacement;
    
    private bool isCorrect = false;
    
    
    private void OnTriggerEnter(Collider other)
    {
        CountryMarker marker = other.GetComponent<CountryMarker>();
        if (marker != null && marker.countryName == countryName && !isCorrect)
        {
            isCorrect = true;
            marker.ShowFeedback(true);
            onCorrectPlacement.Invoke();
            GeographyController.Instance.CheckCountry(this);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        CountryMarker marker = other.GetComponent<CountryMarker>();
        if (marker != null && marker.countryName == countryName && isCorrect)
        {
            isCorrect = false;
        }
    }
    
    public void Reset()
    {
        isCorrect = false;
    }
}