using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatOnStatueAchievementTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        RatBehaviour ratBehaviour = other.GetComponent<RatBehaviour>();
        if (ratBehaviour && ratBehaviour.IsPossessed)
        {
            AchievementHandler.Instance.AwardAchievement(SteamAchievements.ACH_VERMIN_PARKOUR);
        }
    }
}
