using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StreakGardenController : MonoBehaviour
{
    [Header("Plant Prefabs")]
    public GameObject smallPlantPrefab;
    public GameObject mediumPlantPrefab;
    public GameObject tallPlantPrefab;
    public Transform gardenPlot;
    
    [Header("Visual Settings")]
    public float plantSpacing = 0.4f;
    public int plantsPerRow = 3;
    public float growthAnimationSpeed = 2f;
    
    [Header("UI Elements")]
    public TextMeshProUGUI streakText;
    public TextMeshProUGUI totalSessionsText;
    
    private List<GameObject> spawnedPlants = new List<GameObject>();
    private int displayedPlants = 0;
    
    private void OnEnable()
    {
        // Refresh garden whenever this object is enabled
        RefreshGarden();
    }
    
    public void RefreshGarden()
    {
        UpdateUI();
        StartCoroutine(GrowPlants());
    }
    
    private System.Collections.IEnumerator GrowPlants()
    {
        // Get data from DataManager
        PlayerData data = DataManager.Instance.playerData;
        int totalPlants = data.totalSessionsCompleted;
        
        // Clear existing plants if mismatch
        if (spawnedPlants.Count > totalPlants)
        {
            foreach (GameObject plant in spawnedPlants)
            {
                Destroy(plant);
            }
            spawnedPlants.Clear();
            displayedPlants = 0;
        }
        
        // Add new plants
        for (int i = displayedPlants; i < totalPlants; i++)
        {
            // Calculate position in grid layout
            int row = i / plantsPerRow;
            int col = i % plantsPerRow;
            
            float xPos = (col - plantsPerRow / 2) * plantSpacing;
            float zPos = row * -plantSpacing; // Negative to grow away from player
            
            // Determine plant type based on achievement level
            GameObject prefabToUse;
            if (i < totalPlants / 3)
            {
                prefabToUse = smallPlantPrefab; // Small plants for beginners
            }
            else if (i < (totalPlants * 2) / 3)
            {
                prefabToUse = mediumPlantPrefab; // Medium plants for intermediate
            }
            else
            {
                prefabToUse = tallPlantPrefab; // Tall plants for advanced users
            }
            
            // Create plant
            GameObject newPlant = Instantiate(prefabToUse, gardenPlot);
            
            // Position the plant
            newPlant.transform.localPosition = new Vector3(xPos, 0, zPos);
            
            // Rotate slightly for natural look
            float randomRotation = Random.Range(-15f, 15f);
            newPlant.transform.localRotation = Quaternion.Euler(0, randomRotation, 0);
            
            // Start small and grow
            newPlant.transform.localScale = Vector3.zero;
            
            spawnedPlants.Add(newPlant);
            displayedPlants++;
            
            // Animate growth
            float growTime = 0;
            while (growTime < 1)
            {
                growTime += Time.deltaTime * growthAnimationSpeed;
                newPlant.transform.localScale = Vector3.one * Mathf.SmoothStep(0, 1, growTime);
                yield return null;
            }
            
            // Slight pause between spawning plants
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    private void UpdateUI()
    {
        PlayerData data = DataManager.Instance.playerData;
        
        if (streakText != null)
        {
            streakText.text = $"Current Streak: {data.currentStreak} days";
        }
        
        if (totalSessionsText != null)
        {
            totalSessionsText.text = $"Total Plants: {data.totalSessionsCompleted}";
        }
    }
}