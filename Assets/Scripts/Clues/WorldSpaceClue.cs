﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSpaceClue : MonoBehaviour
{
    public Clue ClueScriptableObject;
    public Popup Popup;

    private void Awake()
    {
        if (SaveHandler.Instance.DoesPlayerHaveClue(ClueScriptableObject.Name))
        {
            gameObject.SetActive(false);
        }
    }

    public void AddToInventory()
    { 
        //Add this clue to the inventory of the player
        SaveHandler.Instance.SaveClue(ClueScriptableObject.Name);
        SoundManager.Instance.PlaySound("Clue_pickup");
        if(Popup)
        {
            Popup.OpenPopup();
        }

        gameObject.SetActive(false);
    }

    void OnMouseDown()
    {
        AddToInventory();
    }
}