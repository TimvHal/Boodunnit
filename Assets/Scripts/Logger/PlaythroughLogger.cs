using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DefaultNamespace;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static Newtonsoft.Json.JsonConvert;

public class PlaythroughLogger : MonoBehaviour
{
    private string _userInfoFilePath;
    static PlaythroughLogger mInstance;

    public static PlaythroughLogger Instance
    {
        get
        {
            return !mInstance
                ? (mInstance = (new GameObject("GameAssets")).AddComponent<PlaythroughLogger>())
                : mInstance;
        }
    }


    private void Awake()
    {
        mInstance = this;
        Playthrough.Instance.GameStartTime = DateTime.UtcNow;
        
        _userInfoFilePath = $"{Application.dataPath}/UserInfo.json";
        // DontDestroyOnLoad(gameObject);
        CheckClientGUID();
    }


    public void WriteLogThenQuit()
    {
        if (Application.isEditor) Application.Quit();
        File.WriteAllText($"{Application.dataPath}/{Playthrough.Instance.GUID}.json",
            SerializeObject(Playthrough.Instance));
        Playthrough.Instance.GameEndTime = DateTime.UtcNow;
        Playthrough.Instance.Platform = Application.platform.ToString();
        
        StartCoroutine(Login(Playthrough.Instance, FirebaseHTTPController.GetLogin()));
    }

    private void CheckClientGUID()
    {
        if (File.Exists(_userInfoFilePath))
            Playthrough.Instance.ClientGUID = File.ReadAllText(_userInfoFilePath);
        else
        {
            string userId = Guid.NewGuid().ToString();
            File.WriteAllText(_userInfoFilePath, userId);
            Playthrough.Instance.ClientGUID = userId;
        }
    }

    IEnumerator PostPlaythrough(string url, string bodyJsonString, bool StopAppAfterExecution)
    {
        UnityWebRequest loginPostRequest = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        loginPostRequest.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        loginPostRequest.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        loginPostRequest.SetRequestHeader("Content-Type", "application/json");
        if (StopAppAfterExecution)
            SceneManager.MoveGameObjectToScene(Instance.gameObject, SceneManager.GetActiveScene());
        
        yield return loginPostRequest.SendWebRequest();
        if (StopAppAfterExecution)
        {
            Application.Quit();
        }
    }

    IEnumerator Login(Playthrough playthrough, FirebaseHTTPController.UserLoginCredentials login, bool stopApplicationAfterUpload = true, string logLocation = null)
    {
        if (logLocation == null)
        {
            logLocation = $"{Application.dataPath}/{Playthrough.Instance.GUID}.json";
        }
        string url = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=" + FirebaseHTTPController.apiKey;
        UnityWebRequest postRequest = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(SerializeObject(login));
        postRequest.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        postRequest.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        postRequest.SetRequestHeader("Content-Type", "application/json");
        yield return postRequest.SendWebRequest();
        if (postRequest.responseCode == 200) File.Delete(logLocation);
        FirebaseHTTPController.LoginResponse response = DeserializeObject<FirebaseHTTPController.LoginResponse>(
            Encoding.UTF8.GetString(postRequest.downloadHandler.data));
        StartCoroutine(PostPlaythrough(
            $"https://boodunnitcharts-default-rtdb.firebaseio.com/rest.json?auth={response.idToken}",
            SerializeObject(playthrough), stopApplicationAfterUpload)
        );
       
    }

    public void CheckUnuploadedLogs()
    {
        foreach (string fileName in Directory.GetFiles(Application.dataPath, "*.json", SearchOption.AllDirectories))
        {
            try
            {
                string fileContents = File.ReadAllText(fileName);
                StartCoroutine(    
                    Login(
                    DeserializeObject<Playthrough>(fileContents), FirebaseHTTPController.GetLogin(), 
                    false, fileName)
                    );
            }
            catch (Exception e)
            {
                //ignore any json objects that cannot be converted into the logresponse playthrough object, 
                //in case of tampering
            }

        }
    }

}
