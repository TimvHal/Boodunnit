using System;
using Enums;
using Steamworks;
using UnityEngine;
public class AchievementHandler
{
    private static AchievementHandler _instance;

    public static AchievementHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AchievementHandler();
            }

            return _instance;
        }
    }
    
    //DEBUGGING PURPOSES ONLY.
    public void ResetAchievements()
    {
        if (!SteamManager.Initialized) return;
        SteamUserStats.ResetAllStats(true);
    }

    public void AwardAchievement(SteamAchievements achievement)
    {
        if (!SteamManager.Initialized) return;
        SteamUserStats.SetAchievement(achievement.ToString());
        SteamUserStats.StoreStats();
    }
}