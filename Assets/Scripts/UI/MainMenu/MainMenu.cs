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
        SaveHandler.Instance.DeleteSaveGame();
        SaveHandler.Instance.StartNewGame();
        SceneTransitionHandler.Instance.GoToScene("PreTutorialScene");
    }

    public void ContinueGame()
    {
        if (!SaveHandler.Instance.DoesSaveGameExist())
            return;

        SceneTransitionHandler.Instance.GoToScene(SaveHandler.Instance.LoadCurrentScene());
    }

    public void QuitGame()
    {
        PlaythroughLogger.Instance.WriteLogThenQuit();
    }
}
