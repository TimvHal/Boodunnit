﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interfaces;
using Enums;

public class PossessableSanboxBox : BaseMovement, IEntity, IPossessable
{
    private CameraController _cameraController;

    public float FearThreshold { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public float FearDamage { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public float FaintDuration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public EmotionalState EmotionalState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Dictionary<Type, float> ScaredOfGameObjects { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool IsPossessed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Dialogue Dialogue => throw new NotImplementedException();

    public Question Question => throw new NotImplementedException();

    public CharacterList CharacterName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public List<CharacterList> Relationships => throw new NotImplementedException();

    public Sentence[] DefaultAnswers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    private void Awake()
    {
        _cameraController = UnityEngine.Camera.main.GetComponent<CameraController>();
    }

    private void Update()
    {
    }

    public IEnumerator CalmDown()
    {
        yield return null;
    }

    public void CheckSurroundings()
    {
    }

    public void DealFearDamage(float amount)
    {
    }

    public void Faint()
    {
    }

    public void Move(Vector3 direction)
    {
        MoveEntityInDirection(direction);
    }

    public void UseFirstAbility()
    {
    }
}
