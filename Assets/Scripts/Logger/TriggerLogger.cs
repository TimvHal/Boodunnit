using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logger;
using Newtonsoft.Json.Linq;
using UnityEngine;
using LogType = Logger.LogType;

public class TriggerLogger : MonoBehaviour
{
    public string Name;
    public List<LogType> LoggingTypes;
    public bool hasLogged;
    private SceneLogger _scenelogger;
    

    private void Awake()
    {
        _scenelogger = FindObjectOfType<SceneLogger>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        //TODO do something with layers here LayerMask does not seem to work for this
        if (!other.gameObject.name.Contains("Player"))
            if (!(PossessionBehaviour.PossessionTarget &&
                  PossessionBehaviour.PossessionTarget.name == other.gameObject.name))
                return;
        Log last = new Log();
        if (_scenelogger.SceneLog.Logs.Count > 0)
        {
            last = _scenelogger.SceneLog.Logs[_scenelogger.SceneLog.Logs.Count - 1];
        }
        double logTime = (DateTime.UtcNow - _scenelogger.SceneLog.StartingTime).TotalSeconds;
        logTime = Math.Round(logTime, 3);
        if (last.Name == Name)
        {
            Log.Entry entry = DetermineLogInformation();
            if (last.LogDetails.Count > 0 && last.LogDetails.LastOrDefault().info.Equals(entry.info))
            {
                Log.Entry lastLogEntry = last.LogDetails.LastOrDefault();
                lastLogEntry.LogTimes.Add((float) logTime);
                last.LogDetails[last.LogDetails.Count - 1] = lastLogEntry;
            }
            else
                last.LogDetails.Add(entry);
            
            _scenelogger.SceneLog.Logs[_scenelogger.SceneLog.Logs.Count - 1] = last;
        }
        else
        {
            Log log = new Log();
            log.Name = Name;
            log.LogDetails.Add(DetermineLogInformation());
        
            if (log.LogDetails.Count > 0)
                _scenelogger.SceneLog.Logs.Add(log);
        }

        hasLogged = true;
    }

    // private Log.Entry CreatePossessionEntry()
    // {
    //     string infoValue = PossessionBehaviour.PossessionTarget
    //         ? "Possessing: " + PossessionBehaviour.PossessionTarget.name
    //         : "Possessing: None";
    //     Log.Entry entry = new Log.Entry(infoValue, (float)(DateTime.UtcNow - _scenelogger.SceneLog.StartingTime).TotalSeconds);
    //     return entry;
    // }

    private Log.Entry DetermineLogInformation()
    {
        DashBehaviour dashBehaviour = null;
        try
        {
            dashBehaviour = CameraController.RotationTarget.GetComponent<DashBehaviour>();
        }
        catch ( Exception e)
        {
            //ignore
        }
        BaseMovement movement = CameraController.RotationTarget.GetComponent<BaseMovement>();
        LevitateBehaviour levitateBehaviour = CameraController.RotationTarget.GetComponent<LevitateBehaviour>();
        string result = ""; 
        foreach (LogType type in LoggingTypes)
        {
            switch (type)
            {
                case LogType.Inputs:
                    result = InputLog(result, dashBehaviour);
                    break;
                case LogType.Levitating:
                    result = LevitationLog(result, levitateBehaviour);
                    break;
                case LogType.Possession:
                    result = PossessionLog(result);
                    break;
                case LogType.IsGrounded:
                    result = GroundLog(result, movement);
                    break;
                case LogType.All:
                    result = InputLog(result, dashBehaviour);
                    result = LevitationLog(result, levitateBehaviour);
                    result = PossessionLog(result);
                    result = GroundLog(result, movement);
                    return new Log.Entry(result, (DateTime.UtcNow - _scenelogger.SceneLog.StartingTime).TotalSeconds);
                    break;
            }
        }
        return new Log.Entry(result, (DateTime.UtcNow - _scenelogger.SceneLog.StartingTime).TotalSeconds);
    }

    private string PossessionLog(string result)
    {
        result += PossessionBehaviour.PossessionTarget
            ? "Possessing: " + PossessionBehaviour.PossessionTarget.name
            : "Possessing: None";
        result += "\n";
        return result;
    }

    private string LevitationLog(string result, LevitateBehaviour levitateBehaviour)
    {
        if (!levitateBehaviour) return result;
        result += levitateBehaviour.IsLevitating ? "Levitating" : "Not Levitating";
        result += "\n";
        return result;
    }

    private string InputLog(string result, DashBehaviour dashBehaviour)
    {
        float hor = Input.GetAxisRaw("Horizontal");
        float ver = Input.GetAxisRaw("Vertical");
        string input = ( hor > 0 ? "D" : hor < 0 ?"A" : "") + ( ver > 0 ? "W" : ver < 0 ? "S" : "");
        if (!input.Equals(""))
            result += "Input: " + input + " ";
        if (dashBehaviour)
            result += dashBehaviour ? dashBehaviour.IsDashing ? "Dashing" : "Not Dashing" : "";
        if (input.Equals("")) return result;
        result += "\n";
        return result;
    }

    private string GroundLog(string result, BaseMovement movement)
    {
        if (!movement) return "";
        result += movement.IsGrounded ? "Grounded" : "Not Grounded";
        result += "\n";
        return result;
    }
}
