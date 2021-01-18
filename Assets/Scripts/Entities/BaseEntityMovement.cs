using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Enums;
using UnityEngine;
using UnityEngine.AI;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public abstract class BaseEntityMovement : BaseMovement
{
    public OffMeshLinkMethod OffMeshLinkTraverseType;
    public bool IsTraversingOfMeshLink;
    
    public GameObject TargetToFollow;

    [HideInInspector]
    public NavMeshAgent NavMeshAgent;

    [HideInInspector]
    public Animator Animator;

    public bool IsOnCountdown;

    [Header("Pathfinding")]
    public PathFindingState _pathFindingState;
    public float MinimumFollowRange, MaximumFollowRange;
    public List<EntityArea> SequencePatrolAreas;
    private int _sequencePatrolAreaCounter = 0;
    private bool _isPathFinding;
    private bool _hasPositionInArea;
    private Quaternion _spawnRotation;
    private Vector3 _spawnLocation;
    private Vector3 _patrolDestination;
    private EntityArea _currentArea;
    
    protected void InitEntityMovement()
    {
        InitBaseMovement();
        NavMeshAgent = GetComponent<NavMeshAgent>();
        
        Animator = GetComponent<Animator>();
        if (!Animator) Animator = GetComponentInChildren<Animator>();

        if (NavMeshAgent)
        {
            NavMeshAgent.autoBraking = true;
            NavMeshAgent.speed = PathfindingSpeed;
            _spawnRotation = transform.rotation;
            _spawnLocation = transform.position;
            NavMeshAgent.autoTraverseOffMeshLink = (OffMeshLinkTraverseType == OffMeshLinkMethod.None);
        }
    }

    public void MoveWithPathFinding()
    {
        switch (_pathFindingState)
        {
            case PathFindingState.Stationary:
                ReturnToSpawn();
                break;
            case PathFindingState.Patrolling:
                PatrolArea();
                break;
            case PathFindingState.PatrolAreas:
                PatrolSequence();
                break;
            case PathFindingState.Following:
                FollowTarget();
                break;
            case PathFindingState.None:
                break;
        }
    }
    
    private void FollowTarget()
    {
        if (TargetToFollow)
        {
            float distanceToTarget = Vector3.Distance(transform.position, TargetToFollow.transform.position);
            if (distanceToTarget > MinimumFollowRange && distanceToTarget < MaximumFollowRange)
            {
                PauseEntityNavAgent(false);

                NavMeshAgent.SetDestination(TargetToFollow.transform.position);
                return;
            }
            PauseEntityNavAgent(true);
        }
    }

    private void ReturnToSpawn()
    {
        if (HasReachedDestination(_spawnLocation)) {
            if (transform.rotation == _spawnRotation) return;
            Quaternion lerpToRotation = Quaternion.Lerp(transform.rotation, _spawnRotation,
                Time.deltaTime * 5f);
            transform.rotation = lerpToRotation;
            NavMeshAgent.ResetPath();
            return;
        }

        if (NavMeshAgent.hasPath) return;
        NavMeshAgent.destination = _spawnLocation;
        PauseEntityNavAgent(false);
    }

    private void PatrolArea()
    {
        if (!_currentArea) MoveToNextArea();
        if (!_hasPositionInArea)
        {
            MoveToNextPosition();
            _hasPositionInArea = true;
        }
        if (HasReachedDestination(_patrolDestination) && !IsOnCountdown)
        {
            IsOnCountdown = true;
            StartCoroutine(StartCountdownInArea(_currentArea.GetEntityTimeInArea(gameObject)));
        }
    }

    private void PatrolSequence()
    {
        if (SequencePatrolAreas.Count <= 0)
            return;

        if (!_hasPositionInArea)
        {
            _currentArea = SequencePatrolAreas[_sequencePatrolAreaCounter];
            MoveToNextPosition();
            _hasPositionInArea = true;
        }

        if(HasReachedDestination(_patrolDestination) && !IsOnCountdown)
        {
            IsOnCountdown = true;
            _sequencePatrolAreaCounter++;
            StartCoroutine(StartCountdownInArea(0f));
        }

        if(_sequencePatrolAreaCounter == SequencePatrolAreas.Count)
            _sequencePatrolAreaCounter = 0;
    }

    private bool HasReachedDestination(Vector3 destination)
    {
        float distanceToDestination = Vector3.Distance(transform.position, destination);
        return distanceToDestination < (Collider.bounds.extents.y / 2) + 1f;
    }

    public IEnumerator StartCountdownInArea(float amountOfTime)
    {
        yield return new WaitForSeconds(amountOfTime);
        _currentArea = null;
        IsOnCountdown = false;
        _hasPositionInArea = false;
    }

    private void MoveToNextArea()
    {
        _currentArea = EntityAreaHandler.Instance.GetRandomAreaForEntity(gameObject);
        _hasPositionInArea = false;
        NavMeshAgent.ResetPath();
    }

    private void MoveToNextPosition()
    {
        _patrolDestination = EntityAreaHandler.Instance.GetRandomPositionInArea(_currentArea, gameObject);
        NavMeshAgent.destination = _patrolDestination;
    }
    
    public void ChangePathFindingState(PathFindingState pathFindingState)
    {
        NavMeshAgent.ResetPath();
        _pathFindingState = pathFindingState;
    }
    
    public void SetPathFindingStateToNone() 
    {
        NavMeshAgent.ResetPath();
        NavMeshAgent.enabled = false;
        _pathFindingState = PathFindingState.None;
    }

    public void ResetDestination()
    {
        PauseEntityNavAgent(false);
        ChangePathFindingState(_pathFindingState);
        _hasPositionInArea = false;
    }

    public void PauseEntityNavAgent(bool shouldPause)
    {
        if (NavMeshAgent && NavMeshAgent.enabled)
        {
            NavMeshAgent.isStopped = shouldPause;
        }
    }
    
    //Allows entities to traverse the offmeshlink in a parabola.
    public IEnumerator Parabola(NavMeshAgent agent)
    {
        
        IsTraversingOfMeshLink = true;
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        OffMeshLinkProperties offMeshLinkProperties = data.offMeshLink.gameObject.GetComponent<OffMeshLinkProperties>();
        
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        float height = offMeshLinkProperties.ParabolaHeight;
        float duration = offMeshLinkProperties.ParabolaDuration;
        float normalizedTime = 0.0f;
        
        while (normalizedTime < 1.0f)
        {
            var lookPos = endPos - transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = rotation;
                    
            float yOffset = height * 4.0f * (normalizedTime - normalizedTime * normalizedTime);
            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }

        agent.CompleteOffMeshLink();
        IsTraversingOfMeshLink = false;
    }
}