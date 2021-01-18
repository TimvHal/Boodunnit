using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SceneLog
{
    public string Name;
    public DateTime StartingTime;
    public List<Log> Logs = new List<Log>();
    public List<Log> Stats = new List<Log>();
    public List<Log> UnusedLogs = new List<Log>();
}
