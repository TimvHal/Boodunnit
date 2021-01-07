﻿using DefaultNamespace.Enums;
using UnityEngine;
using System.Linq;

public class LevitateBehaviour : MonoBehaviour
{
    [Header("Levitate Options")]
    [SerializeField] private LayerMask _ignoredLayerMask;
    [SerializeField] private float _objectStartingHeight = 4f;
    
    private Camera _mainCamera;
    private Rigidbody _selectedRigidbody;
    private Collider _currentHighlightedObject;

    public float CurrentLevitateRadius { get; set; }
    public Collider[] CurrentLevitateableObjects { get; set; }
    public bool IsLevitating { get; set; }
    public bool PushingObjectIsToggled { get; set; }

    private void Awake()
    {
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    #region Levitatin Handler
    public void LevitationStateHandler()
    {
        if (!_selectedRigidbody) Levitate();
        else StopLevitation();
    }

    private void Levitate()
    {
        ChangeLevitationStateToLevitating();
    }

    private void StopLevitation()
    {
        ToggleGravity(true);
        _selectedRigidbody.constraints = RigidbodyConstraints.None;
        PushingObjectIsToggled = false;
        FreezeLevitateableObject();
    }
    #endregion
    
    
    #region Levitateable Object Movement
    public void MoveLevitateableObject()
    {
        if (!_selectedRigidbody) return;
        _selectedRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        Collider collider = _selectedRigidbody.GetComponent<Collider>();

        if (!IsObjectInLevitateablesArray(collider))
        {
            RemoveGameObjectFromCamera();
            return;
        }
        
        ToggleGravity(false);
        Transform cameraTransform = Camera.main.transform;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        Vector3 endOfRayCast = ray.GetPoint(10f);
        Vector3 heightOffset = new Vector3(0, _objectStartingHeight * Time.deltaTime * 10f, 0);
        Vector3 targetPosition = endOfRayCast + heightOffset;
        _selectedRigidbody.MovePosition(targetPosition);
    }
    #endregion

    
    #region Receiving a Rigidbody
    private Rigidbody GetRigidbodyFromCameraRay()
    {
        if (_selectedRigidbody)
        {
            ILevitateable levitateable = _selectedRigidbody.gameObject.GetComponent<ILevitateable>();
            Collider collider = _selectedRigidbody.GetComponent<Collider>();

            if (levitateable != null && !IsObjectInLevitateablesArray(collider)) return null;
        }

        if (Physics.Raycast(_mainCamera.transform.position, 
            _mainCamera.transform.forward,
            out RaycastHit hitInfo, 
            20f,
            ~_ignoredLayerMask))
        {
            Rigidbody rigidbody = hitInfo.collider.gameObject.GetComponent<Rigidbody>();

            if (!rigidbody) return null;
            if (hitInfo.collider != _currentHighlightedObject) return null;
            
            if (!hitInfo.collider.gameObject.GetComponent(typeof(ILevitateable))) return null;

            int levitatingObjectLayerMask = LayerMask.NameToLayer("LevitatingObject");
            foreach (Transform transform in rigidbody.GetComponentsInChildren<Transform>())
            {
                transform.gameObject.layer = levitatingObjectLayerMask;
            }     
            
            return rigidbody;
        }

        return null;
    }
    
    private void ChangeLevitationStateToLevitating()
    {
        _selectedRigidbody = GetRigidbodyFromCameraRay();
        
        if (!_selectedRigidbody) return;
        
        IsLevitating = true;
        ILevitateable levitateable = _selectedRigidbody.gameObject.GetComponent<ILevitateable>();
        LevitateableObject levitateableObject = _selectedRigidbody.gameObject.GetComponent<LevitateableObject>();

        if (levitateableObject && levitateableObject.State == LevitationState.Frozen)
            levitateableObject.SetRigidbodyAndLevitationBooleans(true, false);
        
        if (levitateable != null) levitateable.State = LevitationState.Levitating;
    }
    #endregion

    
    #region Freezing an Object
    public void FreezeLevitateableObject()
    {
        ILevitateable levitateable =
            _selectedRigidbody ? _selectedRigidbody.gameObject.GetComponent<ILevitateable>() : null;
        
        if (levitateable != null)
        {
            IsLevitating = false;
            levitateable.State = LevitationState.Frozen;
        }

        if (_selectedRigidbody && levitateable != null)
        {
            levitateable.Freeze();
        }
        
        RemoveSelectedRigidbody();
    }
    #endregion
    
    
    #region Finding Levitateable Objects In Front Of Player.
    public void FindLevitateableObjectsInFrontOfPlayer()
    {
        CurrentLevitateableObjects = Physics
            .OverlapSphere(transform.position, CurrentLevitateRadius)
            .Where(c => { return IsObjectInRange(c) && IsObjectLevitateble(c); })
            .ToArray();
    }
    
    private bool IsObjectInRange(Collider colliderParam)
    {
        return Vector3.Distance(transform.position, colliderParam.transform.position) <= CurrentLevitateRadius;
    }
    
    private bool IsObjectLevitateble(Collider colliderParam)
    {
        ILevitateable levitateableObject = colliderParam.GetComponent<ILevitateable>();
        return levitateableObject != null;
    }

    private bool IsObjectInLevitateablesArray(Collider colliderParam)
    {
        return CurrentLevitateableObjects.Length > 0 && CurrentLevitateableObjects.Contains(colliderParam);
    }
    #endregion
    
    
    #region Miscellaneous Functions
    private void RemoveGameObjectFromCamera()
    {
        IsLevitating = false;
        
        ILevitateable levitateable = _selectedRigidbody.gameObject.GetComponent<ILevitateable>();

        if (levitateable != null)
        {
            levitateable.State = LevitationState.NotLevitating;
        }

        RemoveSelectedRigidbody();
    }

    private void RemoveSelectedRigidbody()
    {
        IsLevitating = false;
        _selectedRigidbody = null;
    }

    private void ToggleGravity(bool useGravity)
    {
        if (!_selectedRigidbody) return;

        _selectedRigidbody.useGravity = useGravity;
    }

    public void SetCurrentHighlightedObject(Collider gameObject)
    {
        if (!gameObject) return;
        if (_currentHighlightedObject == gameObject) return;
        
        _currentHighlightedObject = gameObject;
    }

    public void SetCurrentHighlightLevitateRadius(float radius)
    {
        if (radius == null) return;
        CurrentLevitateRadius = radius;
    }
    #endregion
}