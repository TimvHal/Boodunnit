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
    private void Awake()
    {
        GameManager.PlayerIsInEndState = false;
        if (CheckIfPlayerHasAllClues())
        {
            GameManager.PlayerHasAllClues = true;
            ToggleQuestMarker(true);

        } else
        {
            ToggleQuestMarker(false);
        }

        GameObject gameObject = GameObject.Find("Cloudportal");
        if (gameObject)
        {
            gameObject.SetActive(false);
        }
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

    public static void ToggleQuestMarker(bool toggle)
    {
        GameObject questMarker = FindObjectOfType<RotateQuestMarker>().gameObject;
        if (questMarker)
            questMarker.SetActive(toggle);
    }
}
