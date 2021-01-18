using System;
using System.Collections.Generic;
using Entities;
using Enums;
using UnityEngine;

public class BirdBehaviour : BaseEntity
{
    private GlideBehaviour _glideBehaviour;
    private Animator _animator;

    private void Awake()
    {
        InitBaseEntity();
        _glideBehaviour = GetComponent<GlideBehaviour>();
        _animator = GetComponentInChildren<Animator>();
        CanJump = true;
        
        FearThreshold = 20;
        FearDamage = 0;
        FaintDuration = 10;
        EmotionalState = EmotionalState.Calm;
        IsScaredOfLevitatableObject = true;
        ScaredOfEntities = new Dictionary<CharacterType, float>()
        {
            [CharacterType.PoliceMan] = 3f,
            [CharacterType.Villager] = 3f
        };
    }

    private void LateUpdate()
    {
        IsWalking = (IsGrounded && Rigidbody.velocity != Vector3.zero);
        if (_animator)
        {
            _animator.SetBool("IsWalking", IsWalking);
            _animator.SetBool("IsGrounded", IsGrounded);
        }
    }

    public override void MoveEntityInDirection(Vector3 direction)
    {
        if (_glideBehaviour.IsGliding && !IsGrounded)
        {
            base.MoveEntityInDirection(direction, PossessionSpeed / 1.5f);
            PlayAudioOnMovement(1);
        }
        else
        {
            base.MoveEntityInDirection(direction);
            PlayAudioOnMovement(0);
        }
    }

    public override void UseFirstAbility()
    {
        _glideBehaviour.ToggleGlide();
    }
}
