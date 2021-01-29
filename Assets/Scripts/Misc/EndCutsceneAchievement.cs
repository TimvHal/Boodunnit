using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndCutsceneAchievement : MonoBehaviour
{
    public void AwardSolvedAchievement()
    {
        AchievementHandler.Instance.AwardAchievement(SteamAchievements.ACH_SOLVED);
    }
}
