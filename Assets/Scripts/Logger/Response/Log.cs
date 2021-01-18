using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Log
{
    public string Name;
    public List<Entry> LogDetails = new List<Entry>();

    /// <summary>
    /// Describes time in seconds after starting
    /// </summary>
    public Log()
    {
        
    }

    public Log(string name)
    {
        Name = name;
    }

    public class Entry
    {
        public object info;
        public List<float> LogTimes = new List<float>();

        public Entry(object _info, double _logTime)
        {
            info = _info;
            _logTime = Math.Round(_logTime, 3);
            LogTimes.Add((float)_logTime);
        }

        public Entry(object _info, List<float> _logTimes)
        {
            info = _info;
            LogTimes = _logTimes;
        }

        public Entry()
        {
            info = "";
            LogTimes = new List<float>();
        }
    }

}

