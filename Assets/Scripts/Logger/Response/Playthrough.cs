using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class Playthrough
{
    public string GUID;
    public string Platform;
    public DateTime GameStartTime;
    public DateTime GameEndTime;
    public string ClientGUID;
    public List<SceneLog> Scenes;
    
    private static Playthrough _instance;
    public static Playthrough Instance { get => _instance; }

    static Playthrough() => _instance = new Playthrough(Guid.NewGuid().ToString());

    private Playthrough(string GUID)
    {
        GameStartTime = DateTime.UtcNow;
        Scenes = new List<SceneLog>();
        this.GUID = GUID;
    }
}
