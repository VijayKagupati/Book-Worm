using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EquationController : MonoBehaviour
{
    public static EquationController Instance;
    
    [Header("Game Setup")]
    public EquationDropZone leftZone;
    public EquationDropZone rightZone;
    public Transform tokenSpawnArea;
    public GameObject tokenPrefab;
    public int maxRounds = 5;
    public float equationThreshold = 0.001f;
    
    [Header("Visual Elements")]
    public Transform scaleBeam;
    public float maxTiltAngle = 15f;
    
    [Header("Game Events")]
    public UnityEvent onRoundComplete;
    public UnityEvent onGameComplete;
    
    private int currentRound = 0;
    private float startTime;
    private MathToken[] currentTokens;
    private bool roundActive = false;
    private float totalAccuracy = 0;
    public int CurrentRound => currentRound;
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
        StartNextRound();
    }
    
    private void StartNextRound()
    {
        currentRound++;
        if (currentRound > maxRounds)
        {
            EndGame();
            return;
        }
    
        // Clear previous tokens
        ClearTokens();
        leftZone.Reset();
        rightZone.Reset();
    
        // Generate new equation
        GenerateTokens();
    
        startTime = Time.time;
        roundActive = true;
        UpdateScaleTilt();
    
        // Add this line to update UI
        FindObjectOfType<LessonUIManager>()?.StartRound();
    }
    
    private void GenerateTokens()
    {
        // Create a simple equation like x + y = z
        float difficulty = Mathf.Min(1 + (currentRound * 0.2f), 2.5f);
        int maxValue = Mathf.RoundToInt(10 * difficulty);
        
        // Generate answer first
        int answer = Random.Range(1, maxValue);
        
        // Generate numbers that add up to answer
        int numTokens = Mathf.Min(3 + currentRound, 8);
        currentTokens = new MathToken[numTokens];
        
        // Create answer token (goes on right side)
        GameObject answerObj = CreateToken(answer);
        currentTokens[0] = answerObj.GetComponent<MathToken>();
        
        // Create tokens for left side (will add up to answer)
        int remaining = answer;
        for (int i = 1; i < numTokens - 1; i++)
        {
            int value;
            if (i == numTokens - 2)
                value = remaining;
            else
                value = Random.Range(1, Mathf.Max(2, remaining));
                
            GameObject tokenObj = CreateToken(value);
            currentTokens[i] = tokenObj.GetComponent<MathToken>();
            remaining -= value;
        }
        
        // Add a distractor token
        int distractorValue = Random.Range(1, maxValue);
        GameObject distractorObj = CreateToken(distractorValue);
        currentTokens[numTokens - 1] = distractorObj.GetComponent<MathToken>();
        
        // Shuffle token positions
        ShuffleTokenPositions();
    }
    
    private GameObject CreateToken(int value)
    {
        GameObject obj = Instantiate(tokenPrefab, tokenSpawnArea.position, Quaternion.identity);
        MathToken token = obj.GetComponent<MathToken>();
        token.value = value;
        token.UpdateText();
        return obj;
    }
    
    private void ShuffleTokenPositions()
    {
        for (int i = 0; i < currentTokens.Length; i++)
        {
            float x = tokenSpawnArea.position.x + (i - currentTokens.Length/2) * 0.2f;
            float y = tokenSpawnArea.position.y;
            float z = tokenSpawnArea.position.z;
            currentTokens[i].transform.position = new Vector3(x, y, z);
        }
    }
    
    public void CheckBalance()
    {
        float leftValue = leftZone.GetTotalValue();
        float rightValue = rightZone.GetTotalValue();
        
        UpdateScaleTilt(leftValue - rightValue);
        
        if (Mathf.Abs(leftValue - rightValue) <= equationThreshold && leftValue > 0 && rightValue > 0)
        {
            OnEquationBalanced();
        }
    }
    
    private void UpdateScaleTilt(float difference = 0)
    {
        if (scaleBeam != null)
        {
            float tiltAngle = Mathf.Clamp(difference * 2, -maxTiltAngle, maxTiltAngle);
            scaleBeam.localRotation = Quaternion.Euler(0, 0, tiltAngle);
        }
    }
    
    private void OnEquationBalanced()
    {
        if (!roundActive) return;
        roundActive = false;
        
        // Show feedback
        foreach (var token in leftZone.containedTokens)
            token.ShowFeedback(true);
        foreach (var token in rightZone.containedTokens)
            token.ShowFeedback(true);
            
        // Calculate score based on time
        float timeElapsed = Time.time - startTime;
        float timeAccuracy = Mathf.Clamp01(1.0f - (timeElapsed / 30.0f));
        totalAccuracy += timeAccuracy;
        
        onRoundComplete.Invoke();
        StartCoroutine(DelayNextRound());
    }
    
    private IEnumerator DelayNextRound()
    {
        yield return new WaitForSeconds(2.0f);
        StartNextRound();
    }
    
    private void EndGame()
    {
        float finalAccuracy = totalAccuracy / maxRounds;
        GameManager.Instance.CompleteLesson(finalAccuracy);
        onGameComplete.Invoke();
    }
    
    private void ClearTokens()
    {
        if (currentTokens != null)
        {
            foreach (var token in currentTokens)
            {
                if (token != null)
                    Destroy(token.gameObject);
            }
        }
    }
}