using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton class. To access the Instance you type: SaveHandler.Instance.MethodYouWantToCall();
/// </summary>
public class SaveHandler
{
    private static SaveHandler _instance;
    public static SaveHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SaveHandler();
            }

            return _instance;
        }
    }

    private readonly string _cluesSaveKey = "PlayerClues";
    private readonly string _currentSceneSaveKey = "CurrentScene";
    private readonly string _newGameSaveKey = "NewGameStarted";

    /// <summary>
    /// Call this when starting a new game.
    /// Now we can also check if the player has already started a game.
    /// We use int to represent true and false.
    /// </summary>
    public void StartNewGame() {
        PlayerPrefs.SetInt(_newGameSaveKey, 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// This method will remove the current save game.
    /// </summary>
    public void DeleteSaveGame()
    {
        PlayerPrefs.DeleteAll();
    }

    /// <summary>
    /// This method is used to save a property of a gameobject in a specific scene
    /// </summary>
    /// <param name="nameOfGameObject">Name of game object</param>
    /// <param name="nameOfProperty">Name of property you want to save</param>
    /// <param name="propertyValue">Value of the property</param>
    public void SaveGameProperty(string nameOfGameObject, string nameOfProperty, object propertyValue)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string uniqueKey = (nameOfGameObject + "_" + nameOfProperty).ToLower();

        string scene = PlayerPrefs.GetString(sceneName);

        if (scene != null && !scene.Equals(""))
        {
            Dictionary<string, object> propertiesInScene = JsonConvert.DeserializeObject<Dictionary<string, object>>(scene);

            if (propertiesInScene.ContainsKey(uniqueKey))
            {
                propertiesInScene[uniqueKey] = propertyValue;
            }
            else
            {
                propertiesInScene.Add(uniqueKey, propertyValue);
            }

            PlayerPrefs.SetString(sceneName, JsonConvert.SerializeObject(propertiesInScene));
        }
        else
        {
            Dictionary<string, object> propertiesInScene = new Dictionary<string, object>();
            propertiesInScene.Add(uniqueKey, propertyValue);

            PlayerPrefs.SetString(sceneName, JsonConvert.SerializeObject(propertiesInScene));
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// This method lets you get a value from a property of a scene
    /// </summary>
    /// <typeparam name="T">Type of value you return</typeparam>
    /// <param name="nameOfGameObject">Name of the game object</param>
    /// <param name="nameOfProperty">Name of the property</param>
    /// <param name="sceneName">Name of the scene, default it's null, so it'll get the current scene.  
    /// If you want to access properties from a different scene other then your current, pass the scene name.
    /// </param>
    /// <returns>Return the value from the unique key combined of nameOfGameObject and nameOfProperty</returns>
    public bool GetPropertyValueFromUniqueKey<T>(string nameOfGameObject, string nameOfProperty, out T propertyValue, string sceneName = null)
    {
        propertyValue = default;
        bool isValueFound = false;
        if(sceneName == null)
        {
            sceneName = SceneManager.GetActiveScene().name;
        }

        string uniqueKey = (nameOfGameObject + "_" + nameOfProperty).ToLower();

        string sceneProperties = PlayerPrefs.GetString(sceneName);
        if (!string.IsNullOrEmpty(sceneProperties))
        {
            Dictionary<string, object> propertiesInScene = JsonConvert.DeserializeObject<Dictionary<string, object>>(sceneProperties);
            if (propertiesInScene.ContainsKey(uniqueKey))
            {
                propertyValue = (T)Convert.ChangeType(propertiesInScene[uniqueKey], typeof(T));
                Debug.Log("Boemba: " + propertyValue);
                isValueFound = true;
            }
        }

        return isValueFound;
    }

    /// <summary>
    /// Save a clue the player found.
    /// </summary>
    /// <param name="nameOfClue">name of the clue you want to save</param>
    public void SaveClue(string nameOfClue)
    {
        List<string> clueList;
        string clues = PlayerPrefs.GetString(_cluesSaveKey);
        if (!string.IsNullOrEmpty(clues))
        {
            clueList = JsonConvert.DeserializeObject<List<string>>(clues);
            if (!clueList.Contains(nameOfClue))
            {
                clueList.Add(nameOfClue);
                PlayerPrefs.SetString(_cluesSaveKey, JsonConvert.SerializeObject(clueList));
                PlayerPrefs.Save();
                return;
            }
        }

        clueList = new List<String>();
        clueList.Add(nameOfClue);

        PlayerPrefs.SetString(_cluesSaveKey, JsonConvert.SerializeObject(clueList));
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Check if the player found the clue
    /// </summary>
    /// <param name="nameOfClue">Name of the clue</param>
    /// <returns>boolean if the player found the clue</returns>
    public bool DoesPlayerHaveClue(string nameOfClue)
    {
        string clues = PlayerPrefs.GetString(_cluesSaveKey);
        if (!string.IsNullOrEmpty(clues))
        {
            List<string> clueList = JsonConvert.DeserializeObject<List<string>>(clues);
            return clueList.Contains(nameOfClue) ? true : false;       
        }

        return false;
    }

    /// <summary>
    /// Get all names of the collected clues
    /// </summary>
    /// <returns>list with names of all collected clues</returns>
    public List<string> GetSavedClueNames()
    {
        string clues = PlayerPrefs.GetString(_cluesSaveKey);
        if (!string.IsNullOrEmpty(clues))
        {
            List<string> clueList = JsonConvert.DeserializeObject<List<string>>(clues);
            return clueList;
        }
        return null;
    }

    /// <summary>
    /// Save the current scene
    /// </summary>
    /// <param name="currentScene">Name of scene you want to save</param>
    public void SaveCurrentScene(string currentScene)
    {
        PlayerPrefs.SetString(_currentSceneSaveKey, currentScene);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load the current scene
    /// </summary>
    /// <returns>Name of the current scene to load</returns>
    public string LoadCurrentScene()
    {
        string currentScene = PlayerPrefs.GetString(_currentSceneSaveKey);
        return !String.IsNullOrEmpty(currentScene) ? currentScene : null;
    }

    /// <summary>
    /// Method to save any data containing a data container.
    /// </summary>
    /// <param name="containerToSave">Container you want to save</param>
    public void SaveDataContainer(BaseDataContainer containerToSave)
    {
        string saveKey = containerToSave.GetType().Name;
        containerToSave.ValidateData();
        PlayerPrefs.SetString(saveKey, JsonConvert.SerializeObject(containerToSave));
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Generic method to load any data that has a data container.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T LoadDataContainer<T>()
    {
        string saveKey = typeof(T).Name;
        string containerData = PlayerPrefs.GetString(saveKey);
        return !string.IsNullOrEmpty(containerData) ? JsonConvert.DeserializeObject<T>(containerData) : default;
    }

    /// <summary>
    /// Check if savegame exists
    /// </summary>
    /// <returns></returns>
    public bool DoesSaveGameExist()
    {
        return PlayerPrefs.GetInt(_newGameSaveKey) == 1;
    }
}