
using System;
using System.Collections.Generic;
using Entities;
using Enums;
using UnityEngine;

public class PoliceManBehaviour : BaseEntity
{
    public Transform ConsumableEndPosition;
    public GameObject PoliceHead;
    
    [SerializeField][Range(0, 10)] private float _donutDetectionRadius = 10f;
    [SerializeField][Range(0, 360)] private float _donutDetectionAngle = 90f;

    private Donut _targetDonut;
    
    private void Awake()
    {
        InitBaseEntity();

        ConsumableEndPosition = transform.Find("ConsumableEndPosition");
        
        FearThreshold = 20;
        FearDamage = 0;
        FaintDuration = 10;
        EmotionalState = EmotionalState.Calm;
        IsScaredOfLevitatableObject = true;
        LevitatableObjectFearDamage = 5f;
        ScaredOfEntities = new Dictionary<CharacterType, float>()
        {
            [CharacterType.Rat] = 3f,
        };
    }
    
    private void LateUpdate()
    {
        if (!IsPossessed && !ConversationManager.HasConversationStarted) CheckDonutsInSurrounding();
        if(TargetToFollow && !IsPossessed) CheckDistanceToDonut();

        if (EmotionalState == EmotionalState.Scared)
        {
            AchievementHandler.Instance.AwardAchievement(SteamAchievements.ACH_SCAREDY_COP);
        }
    }

    private void CheckDonutsInSurrounding()
    {
        Collider[] hitColliders = Physics.OverlapSphere(PoliceHead.transform.forward, _donutDetectionRadius);
        
        foreach (Collider hitCollider in hitColliders)
        {
            RaycastHit hit;

            Vector3 fromPosition = transform.position;
            Vector3 toPosition = hitCollider.transform.position;
            Vector3 direction = toPosition - fromPosition;
            
            float donutAngle = Vector3.Angle(direction, PoliceHead.transform.forward);

            if (Physics.Raycast(fromPosition, direction, out hit, _donutDetectionRadius))
            {
                Donut isDonut = hit.collider.gameObject.GetComponent<Donut>();
                
                if (isDonut && !isDonut.IsTargeted)
                {
                    if (donutAngle > -(_donutDetectionAngle / 2) && donutAngle < _donutDetectionAngle / 2)
                    {
                        FollowDonut(isDonut);
                        return;
                    }
                }
            }
        }
    }

    public override void MoveEntityInDirection(Vector3 direction)
    {
        if (!ConversationManager.HasConversationStarted) base.MoveEntityInDirection(direction);
    }

    public override void UseFirstAbility()
    {
        //TODO implement PoliceManBehaviour.
    }

    private void FollowDonut(Donut donut)
    {
        _targetDonut = donut;
        TargetToFollow = donut.gameObject;
        _targetDonut.PoliceMan = gameObject;
        ChangePathFindingState(PathFindingState.Following);
    }

    public void CheckDistanceToDonut()
    {
        float distance = Vector3.Distance(transform.position, TargetToFollow.transform.position);
        if (distance <= 2.1f)
        {
            if (_targetDonut.PoliceMan.Equals(gameObject) && !_targetDonut.IsTargeted)
            {
                _targetDonut.IsTargeted = true;
                NavMeshAgent.isStopped = true;
                NavMeshAgent.velocity = Vector3.zero;
                StartCoroutine(_targetDonut.MoveToPosition());
                return;
            }
            if(!_targetDonut.PoliceMan.Equals(gameObject)) ResetDestination();
        }
    }
}
