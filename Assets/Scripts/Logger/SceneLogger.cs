using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Entities;
using Logger;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLogger : MonoBehaviour
{
    public SceneLog SceneLog = new SceneLog();
    public List<SceneInfoType> SceneInfo;
    private List<BaseEntity> Possessables;
    private List<LevitateableObject> LevitateableObjects = new List<LevitateableObject>();
    private Log posessables = new Log();
    private Log levitatables = new Log();
    private List<Log> unusedLogs = new List<Log>();
    private TriggerLogger[] _triggerLoggers;

    private int playthroughSceneIndex = -1;
    private Playthrough _lastPlaythrough;

    private void Awake()
    {
        _triggerLoggers = gameObject.GetComponentsInChildren<TriggerLogger>();
        Possessables = FindObjectsOfType<BaseEntity>().ToList();
        foreach (LevitateableObject obj in FindObjectsOfType<LevitateableObject>())
        {
            if (obj.WillLogPossessCount) LevitateableObjects.Add(obj);
        }
    }

    private void Start()
    {
        SceneLog.Name = SceneManager.GetActiveScene().name;
        SceneLog.StartingTime = DateTime.UtcNow;
    }

    private void Update()
    {
        posessables = SavePossessionInfo();
        levitatables = SaveLevitationInfo();
    }

    private void OnDestroy()
    {
        unusedLogs = new List<Log>();
        foreach (TriggerLogger logger in _triggerLoggers)
        {
            if (!logger.hasLogged) unusedLogs.Add(new Log(logger.name));
        }
        SceneLog.Stats.Add(posessables);
        SceneLog.Stats.Add(levitatables);
        SceneLog.UnusedLogs = unusedLogs;
        try
        {
            Playthrough.Instance.Scenes.Add(SceneLog);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private Log SaveLevitationInfo()
    {
        Log log = new Log();
        log.Name = "Levitations";
        foreach (LevitateableObject levitateable in LevitateableObjects)
        {
            log.LogDetails.Add(
                new Log.Entry(levitateable.name, levitateable.TimesLevitated)
            );
        }

        return log;
    }
    private Log SavePossessionInfo()
    {
        Log log = new Log();
        log.Name = "Possesions";
        foreach (BaseEntity entity in Possessables)
        {
            log.LogDetails.Add(
                new Log.Entry(entity.name, entity.TimesPosessed)
            );
        }

        return log;
    }
}
