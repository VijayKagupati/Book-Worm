using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public int currentStreak = 0;
    public DateTime lastPlayedDate = DateTime.MinValue;
    public int totalSessionsCompleted = 0;
    public float averageAccuracy = 0f;
    public Dictionary<string, int> subjectProgress = new Dictionary<string, int>();
    
    // For the streak garden visualization
    public int totalLeavesEarned = 0;
}