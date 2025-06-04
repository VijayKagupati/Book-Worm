using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GeographyController : MonoBehaviour
{
    public static GeographyController Instance;
    
    [Header("Game Setup")]
    public GameObject countryMarkerPrefab;
    public Transform markerSpawnArea;
    public int maxRounds = 5;
    
    [Header("Country Pool")]
    public List<string> availableCountries = new List<string>();
    public List<CountryTarget> countryTargets = new List<CountryTarget>();
    
    [Header("Game Events")]
    public UnityEvent onRoundComplete;
    public UnityEvent onGameComplete;
    
    public int currentRound = 0;
    private int countriesPlaced = 0;
    private int targetCountriesPerRound = 3; // Number of countries to place per round
    private List<CountryTarget> currentTargets = new List<CountryTarget>();
    private List<CountryMarker> currentMarkers = new List<CountryMarker>();
    private float startTime;
    private float totalAccuracy = 0;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }
    
    public void StartGame()
    {
        currentRound = 0;
        totalAccuracy = 0;
        FindAllCountryTargets();
        StartNextRound();
    }
    
    private void FindAllCountryTargets()
    {
        countryTargets.Clear();
        CountryTarget[] targets = FindObjectsOfType<CountryTarget>();
        foreach (var target in targets)
        {
            countryTargets.Add(target);
            availableCountries.Add(target.countryName);
        }
    }
    
    private void StartNextRound()
    {
        currentRound++;
        if (currentRound > maxRounds)
        {
            EndGame();
            return;
        }
        
        // Reset from previous round
        ResetCurrentGame();
        
        // Select random countries for this round
        SelectRandomCountries();
        
        // Create markers for selected countries
        SpawnCountryMarkers();
        
        // Start round timer
        startTime = Time.time;
        
        // Stop globe rotation for easier placement
    }
    
    private void ResetCurrentGame()
    {
        // Reset all existing markers
        foreach (var marker in currentMarkers)
        {
            if (marker != null)
                Destroy(marker.gameObject);
        }
        
        // Reset all targets
        foreach (var target in currentTargets)
        {
            if (target != null)
                target.Reset();
        }
        
        currentMarkers.Clear();
        currentTargets.Clear();
        countriesPlaced = 0;
    }
    
    private void SelectRandomCountries()
    {
        // Shuffle available countries
        List<string> shuffledCountries = new List<string>(availableCountries);
        ShuffleList(shuffledCountries);
        
        // Select a subset for this round
        int countriesToSelect = Mathf.Min(targetCountriesPerRound, shuffledCountries.Count);
        
        // Find corresponding targets
        for (int i = 0; i < countriesToSelect; i++)
        {
            string country = shuffledCountries[i];
            CountryTarget target = countryTargets.Find(t => t.countryName == country);
            if (target != null)
                currentTargets.Add(target);
        }
    }
    
    private void SpawnCountryMarkers()
    {
        for (int i = 0; i < currentTargets.Count; i++)
        {
            // Calculate position in spawn area
            Vector3 spawnPos = markerSpawnArea.position + 
                new Vector3(i * 0.2f - ((currentTargets.Count - 1) * 0.1f), 0, 0);
                
            // Instantiate marker
            GameObject markerObj = Instantiate(countryMarkerPrefab, spawnPos, Quaternion.identity);
            CountryMarker marker = markerObj.GetComponent<CountryMarker>();
            
            // Configure marker
            marker.countryName = currentTargets[i].countryName;
            marker.UpdateText();
            
            currentMarkers.Add(marker);
        }
    }
    
    public void CheckCountry(CountryTarget target)
    {
        countriesPlaced++;
        
        // Check if all countries for this round are placed correctly
        if (countriesPlaced >= currentTargets.Count)
        {
            // Calculate accuracy based on time
            float timeElapsed = Time.time - startTime;
            float timeAccuracy = Mathf.Clamp01(1.0f - (timeElapsed / 60.0f));
            totalAccuracy += timeAccuracy;
            
            onRoundComplete.Invoke();
            StartCoroutine(DelayedNextRound());
        }
    }
    
    private IEnumerator DelayedNextRound()
    {
        yield return new WaitForSeconds(3.0f); // Show success for 3 seconds
        StartNextRound();
    }
    
    private void EndGame()
    {
        float finalAccuracy = totalAccuracy / maxRounds;
        GameManager.Instance.CompleteLesson(finalAccuracy);
        onGameComplete.Invoke();
        
        // Resume globe rotation
    }
    
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}