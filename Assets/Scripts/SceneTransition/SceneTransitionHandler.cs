using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionHandler
{
    private List<string> _excludedScenesFromSavedScenes = new List<string>()
        {
            "MainMenu",
            "LoadingScene",
            "EndScreenScene",
            "CreditScene"
        };

    public static SceneTransitionHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SceneTransitionHandler();
            }
            
            return _instance;
        }
    }
    
    private static SceneTransitionHandler _instance;

    public void GoToScene(string sceneNameToLoad)
    {   
        LoadingScreen.GoToMainMenu = false;
        if (!sceneNameToLoad.Trim().Equals(""))
        {
            if (!_excludedScenesFromSavedScenes.Contains(sceneNameToLoad))
            {
                SaveHandler.Instance.SaveCurrentScene(sceneNameToLoad);
            }
            SceneManager.LoadScene("LoadingScene"); 
        }
    }

    public void GoToMainMenu()
    {
        LoadingScreen.GoToMainMenu = true;
        SceneManager.LoadScene("LoadingScene");
    }
}
