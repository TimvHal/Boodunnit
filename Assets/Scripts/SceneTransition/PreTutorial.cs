using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreTutorial : MonoBehaviour
{
    public string SceneName;

    public void LeavePretutorial()
    {
        AchievementHandler.Instance.AwardAchievement(SteamAchievements.ACH_A_NEW_LIFE);
        if (SceneName != null)
        {
            SceneTransitionHandler.Instance.GoToScene(SceneName);
        } 

        else
        {
            SceneTransitionHandler.Instance.GoToMainMenu();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        LeavePretutorial();
    }
}
