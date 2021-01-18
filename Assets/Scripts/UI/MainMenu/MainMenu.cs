using System;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private void Awake()
    {
        GameManager.CursorIsLocked = false;
        if (!SaveHandler.Instance.DoesSaveGameExist())
        {
            GameObject continueButton = GameObject.Find("ContinueGameButton");
            if (continueButton)
                continueButton.SetActive(false);
        }

        PlaythroughLogger.Instance.CheckUnuploadedLogs();
    }

    public void NewGame()
    {
        // Todo start a new game
        PlayerPrefs.DeleteAll();
        SaveHandler.Instance.StartNewGame();
        SceneTransitionHandler.Instance.GoToScene("PreTutorialScene");
    }

    public void ContinueGame()
    {
        if (!SaveHandler.Instance.DoesSaveGameExist())
            return;
        // Todo load the game from the last save point

        SceneTransitionHandler.Instance.GoToScene(SaveHandler.Instance.LoadCurrentScene());
    }

    public void QuitGame()
    {
        PlaythroughLogger.Instance.WriteLogThenQuit();
    }
}
