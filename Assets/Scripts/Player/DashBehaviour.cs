using Entities;
using Entities.Humans;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UI;

public class DashBehaviour : MonoBehaviour
{
    public bool IsDashing = false;
    public bool DashOnCooldown = false;

    public float DashCooldown = 2f;
    public float DashDuration = 0.2f;
    public float DashDistance = 7f;

    public int DashFearDamage = 5;

    private float _dashSpeed;

    private Rigidbody _rigidbodyPlayer;

    private IEnumerator _dashCoroutine;

    private float _playerSize;
    private float _defaultDashDuration;

    private RaycastHit[] _raycastHits;
    private RaycastHit _furthestHit;
    private Collider[] _collidesAtEndOFRaycast;

    private bool _canDash;

    private List<IPossessable> _fearDashPossessables = new List<IPossessable>();

    private void Awake()
    {
        _rigidbodyPlayer = GetComponent<Rigidbody>();
        _dashSpeed = DashDistance / DashDuration;
        _defaultDashDuration = DashDuration;

        foreach (Collider collider in GetComponents<CapsuleCollider>())
        {
            if (!collider.isTrigger)
            {
                _playerSize = collider.bounds.size.z;
            }
        }
    }

    public void Dash()
    {
        _dashCoroutine = PerformDash();
        StartCoroutine(_dashCoroutine);
        StartCoroutine(DashTimer());
    }

    private void OnCollisionStay(Collision collision)
    {
        Vector3 normal = collision.contacts[0].normal;
        Vector3 vel = _rigidbodyPlayer.velocity;

        if (Vector3.Angle(vel, -normal) < 50 && IsDashing && !DashOnCooldown)
        {
            StopCoroutine(_dashCoroutine);
            StopDash();
        }
    }

    private IEnumerator PerformDash()
    {
        IsDashing = true;

        _rigidbodyPlayer.useGravity = false;

        Vector3 oldVelocity = _rigidbodyPlayer.velocity;
        Vector3 newVelocity = transform.forward * _dashSpeed;

        if (CheckDashEndPosition())
        {
            gameObject.layer = 9;
        }

        _rigidbodyPlayer.velocity = newVelocity;

        yield return new WaitForSeconds(DashDuration);

        oldVelocity.y = 0;
        _rigidbodyPlayer.velocity = oldVelocity;
        gameObject.layer = 8;

        DashDuration = _defaultDashDuration;

        StopDash();
    }
    private void StopDash()
    {
        DashOnCooldown = true;
        IsDashing = false;
        _rigidbodyPlayer.useGravity = true;

        _fearDashPossessables.Clear();
    }

    private bool CheckDashEndPosition()
    {
        _raycastHits = Physics.RaycastAll(transform.position, transform.forward, DashDistance);

        _raycastHits = CheckTriggerHits(_raycastHits);

        if(_raycastHits.Length == 0)
        {
            return true;
        }

        if (_raycastHits.Length == 1)
        {
            return RayCastLengthIsOne();
        }
        
        CheckFurthestRaycastHit();

        _collidesAtEndOFRaycast = Physics.OverlapSphere(_furthestHit.point, _playerSize);
        _canDash = _collidesAtEndOFRaycast.Length < 2;

        CheckRoomBetweenTwoDashables();

        return _canDash;
    }

    private RaycastHit[] CheckTriggerHits(RaycastHit[] raycastHits)
    {
        List<RaycastHit> raycastHitsWithoutTriggers = new List<RaycastHit>();
        foreach (RaycastHit raycastHit in raycastHits)
        {
            if (!raycastHit.collider.isTrigger)
            {
                raycastHitsWithoutTriggers.Add(raycastHit);
            }
        }
        return raycastHitsWithoutTriggers.ToArray();
    }

    private bool RayCastLengthIsOne()
    {
        Vector3 endPosition = transform.position + (transform.forward * DashDistance);
        Collider[] collidesAtEndPosition = Physics.OverlapSphere(endPosition, _playerSize);

        foreach (Collider collide in collidesAtEndPosition)
        {
            if (collide == _raycastHits[0].collider)
            {
                return false;
            }
        }
        return true;
    }

    private void CheckFurthestRaycastHit()
    {
        _furthestHit = _raycastHits[0];

        foreach (RaycastHit hit in _raycastHits)
        {
            if (Vector3.Distance(transform.position, _furthestHit.point) < Vector3.Distance(transform.position, hit.point))
            {
                _furthestHit = hit;
            }
        }
    }

    private void CheckRoomBetweenTwoDashables()
    {
        if (_furthestHit.transform.gameObject.layer == 10)
        {
            if (_collidesAtEndOFRaycast.Length < 2)
            {
                float dashDistance = Vector3.Distance(transform.position, _furthestHit.point) - _playerSize;

                DashDuration = dashDistance / _dashSpeed;
            } 
            
            else
            {
                Vector3 endPosition = transform.position + (transform.forward * DashDistance);

                Collider[] endPositionColliderArray = Physics.OverlapSphere(endPosition, _playerSize);

                if (endPositionColliderArray != null)
                {
                    _canDash = endPositionColliderArray.Length == 0;
                }
            }
        } 
    }

    private IEnumerator DashTimer()
    {
        float currentTime = 0;
        float interval = DashCooldown + DashDuration;

        while (currentTime < interval)
        {
            yield return null;
            currentTime += Time.deltaTime;
        }
        DashOnCooldown = false;
    }

    public void CheckDashThroughPossessables()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.5f);
        foreach (Collider hitCollider in hitColliders)
        {
            IPossessable possessable = hitCollider.GetComponent<IPossessable>();
            if (possessable != null 
                && !_fearDashPossessables.Contains(possessable) 
                && !hitCollider.GetComponent<EmmieBehaviour>()
                && !hitCollider.GetComponent<SirBoonkleBehaviour>() 
                && !hitCollider.GetComponent<VincentBehaviour>())
            {
                _fearDashPossessables.Add(possessable);
                possessable.DealFearDamageAfterDash(DashFearDamage);
            }
        }
    }
}
