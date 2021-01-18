using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IIconable
{
    EmotionalState GetEmotionalState();

    bool GetCanBePossessed();

    bool GetCanTalkToBoolia();
}