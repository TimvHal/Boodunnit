using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClueCheatScript : MonoBehaviour, IPointerDownHandler
{
    public Clue ClueScriptableObject;
    private PauseMenu _pauseMenu;

    private void Awake()
    {
        _pauseMenu = FindObjectOfType<PauseMenu>();
        PlayerPrefs.DeleteAll();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (!SaveHandler.Instance.DoesPlayerHaveClue(ClueScriptableObject.Name))
            {
                StartCoroutine(SaveClueAfterSeconds());
            }
        }
    }

    public IEnumerator SaveClueAfterSeconds()
    {
        yield return new WaitForSecondsRealtime(1f);
        SaveHandler.Instance.SaveClue(ClueScriptableObject.Name);
        _pauseMenu.UpdateClueUI();
        RemoveClue();
    }

    public void RemoveClue()
    {
        List<WorldSpaceClue> clueList = FindObjectsOfType<WorldSpaceClue>().ToList();
        List<WorldSpaceClue> currentClue = clueList
            .Where(clue => clue.ClueScriptableObject.Name.Equals(ClueScriptableObject.Name))
            .ToList();
        if (currentClue.Count != 0) currentClue[0].gameObject.SetActive(false);
    }
}
