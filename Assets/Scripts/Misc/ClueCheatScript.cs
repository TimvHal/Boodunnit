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
    private List<string> _clueNames;

    private void Awake()
    {
        _pauseMenu = FindObjectOfType<PauseMenu>();
        _clueNames = new List<string>()
        {
            "Broken flower pot",
            "Red fabric",
            "Hard hat",
            "Bloody knife",
            "Trash cake"
        };
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
        RemoveClueFromScene();

        List<string> obtainedClues = _clueNames
            .Where(clue => SaveHandler.Instance.DoesPlayerHaveClue(clue))
            .ToList();
        
        if(obtainedClues.All(clue => SaveHandler.Instance.DoesPlayerHaveClue(clue)))
        {
            SaveHandler.Instance.SaveGameProperty("PlayerHasAllClues", "bool", true, "CrimeSceneQuest");
            GameManager.PlayerHasAllClues = true;
            GameManager.ToggleQuestMarker = true;
        }
    }

    public void RemoveClueFromScene()
    {
        List<WorldSpaceClue> clueList = FindObjectsOfType<WorldSpaceClue>().ToList();
        List<WorldSpaceClue> currentClue = clueList
            .Where(clue => clue.ClueScriptableObject.Name.Equals(ClueScriptableObject.Name))
            .ToList();
        if (currentClue.Count != 0) currentClue[0].gameObject.SetActive(false);
    }
}
