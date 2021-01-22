using Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class CrimeSceneStateHandler : MonoBehaviour
{
    public Canvas QuestCanvas;

    private void Awake()
    {
        GameManager.PlayerIsInEndState = false;
        if (CheckIfPlayerHasAllClues())
        {
            GameManager.PlayerHasAllClues = true;
            GameManager.ToggleQuestMarker = true;

        }
        GameObject gameObject = GameObject.Find("Cloudportal");
        if (gameObject)
        {
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        ToggleQuestMarker(GameManager.ToggleQuestMarker);
    }

    public bool CheckIfPlayerHasAllClues()
    {
        if (SaveHandler.Instance.GetPropertyValueFromUniqueKey("PlayerHasAllClues", "bool", out bool hasPlayerAllClues))
        {
            return hasPlayerAllClues;
        }
        return false;
    }

    public void PlayerEnteredEndState()
    {
        GameManager.PlayerIsInEndState = true;
    }

    public void ToggleQuestMarker(bool toggle)
    {
        if (QuestCanvas)
        {
            QuestCanvas.enabled = toggle;
        }

    }
}
