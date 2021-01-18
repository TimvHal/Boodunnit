using System;
using DefaultNamespace.Enums;
using UnityEngine;
using System.Linq;

public class LevitateBehaviour : MonoBehaviour
{
    [Header("Levitate Options")]
    [SerializeField] private float _levitationMoveSpeed = 250f;
    [SerializeField] private float _objectStartingDistance = 10f;
    [SerializeField] private float _objectStartingHeight = 8f;
    
    [Header("Layers")]
    [SerializeField] private LayerMask _ignoredLayerMask;

    private Camera _mainCamera;
    private Rigidbody _selectedRigidbody;
    private Collider _currentHighlightedObject;
    private Vector3 _originalRigidbodyPosition;

    public float CurrentLevitateRadius { get; set; }
    public Collider[] CurrentLevitateableObjects { get; set; }
    public bool IsLevitating { get; set; }
    public bool PushingObjectIsToggled { get; set; }

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        InstantiateAllDuplicateLevitateableObjects();
    }

    #region Levitation Handler
    public void LevitationStateHandler()
    {
        if (!_selectedRigidbody) Levitate();
        else
        {
            if (!_selectedRigidbody.gameObject.GetComponentInChildren<LevitateableObjectIsInsideTrigger>()
                .PlayerIsInsideObject) StopLevitation();
        }
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
        
        _selectedRigidbody.velocity = 
            (_originalRigidbodyPosition + GetCameraRayPossitionOffset() - _selectedRigidbody.transform.position) 
            * (_levitationMoveSpeed * Time.deltaTime);
    }

    private Vector3 GetCameraRayPossitionOffset()
    {
        Transform cameraTransform = Camera.main.transform;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        Vector3 endOfRayCast = ray.GetPoint(_objectStartingDistance);
        Vector3 heightOffset = new Vector3(0, _objectStartingHeight * Time.deltaTime * _objectStartingDistance, 0);
        Vector3 targetPosition = endOfRayCast + heightOffset;
        return targetPosition - _originalRigidbodyPosition;
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

        float distanceBetweenObjectAndCamera =
            Vector3.Distance(gameObject.transform.position, _mainCamera.transform.position) 
            + CurrentLevitateRadius;

        RaycastHit[] hits;
        hits = Physics.RaycastAll(
            _mainCamera.transform.position,
            _mainCamera.transform.forward,
            distanceBetweenObjectAndCamera,
            ~_ignoredLayerMask
        );
        
        RaycastHit firstHit = hits
            .OrderBy(hit => Vector3.Distance(hit.transform.position, transform.position))
            .FirstOrDefault(hit => hit.collider.gameObject.GetComponent<ILevitateable>() != null);

        if (!firstHit.collider) return null;
        
        Rigidbody rigidbody = firstHit.collider.gameObject.GetComponent<Rigidbody>();

        if (!rigidbody ||
            firstHit.collider != _currentHighlightedObject ||
            !firstHit.collider.gameObject.GetComponent(typeof(ILevitateable))) return null;

        int levitatingObjectLayerMask = LayerMask.NameToLayer("LevitatingObject");
        
        foreach (Transform transform in rigidbody.GetComponentsInChildren<Transform>())
        {
            if (!transform.gameObject.GetComponent<LevitateableObjectIsInsideTrigger>())
            {
                transform.gameObject.layer = levitatingObjectLayerMask;
            }
        }     
            
        _originalRigidbodyPosition = firstHit.collider.transform.position;
        return rigidbody;
    }
    
    private void ChangeLevitationStateToLevitating()
    {
        _selectedRigidbody = GetRigidbodyFromCameraRay();
        
        if (!_selectedRigidbody) return;
        
        IsLevitating = true;
        ILevitateable levitateable = _selectedRigidbody.gameObject.GetComponent<ILevitateable>();
        levitateable.TimesLevitated += 1;
        LevitateableObject levitateableObject = _selectedRigidbody.gameObject.GetComponent<LevitateableObject>();
        
        if (levitateableObject && levitateableObject.State == LevitationState.Frozen)
            levitateableObject.ToggleIsKinematic(false);
        
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

        if (_selectedRigidbody && levitateable != null) levitateable.Freeze();
        RemoveSelectedRigidbody();
    }
    #endregion
    
    #region Finding Levitateable Objects In Front Of Player.
    public Collider[] FindLevitateableObjectsInFrontOfPlayer()
    {
        return Physics
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
        if (levitateable != null) levitateable.State = LevitationState.NotLevitating;
        RemoveSelectedRigidbody();
    }

    private void RemoveSelectedRigidbody()
    {
        IsLevitating = false;
        _selectedRigidbody = null;
    }

    private void InstantiateAllDuplicateLevitateableObjects()
    {
        LevitateableObject[] listOfLevitateableObjects = FindObjectsOfType<LevitateableObject>();
        foreach (LevitateableObject levitateableObject in listOfLevitateableObjects)
        {
            GameObject duplicate = Instantiate(levitateableObject.gameObject);
            duplicate.name = "IsTriggerForLevitationObject";
            duplicate.layer = LayerMask.NameToLayer("IgnoreCameraZoom");
            
            foreach (Component component in duplicate.GetComponents<Component>())
            {
                if (!(component is Collider || component is Transform)) Destroy(component);
            }
            
            foreach (Transform child in duplicate.transform) {
                if (child.name != "IsTriggerForLevitationObject") Destroy(child.gameObject);
            }

            foreach (Collider collider in duplicate.GetComponents<Collider>())
            {
                collider.isTrigger = true;
            }
            
            duplicate.AddComponent(typeof(LevitateableObjectIsInsideTrigger));
            duplicate.transform.SetParent(levitateableObject.transform, true);
            duplicate.transform.localPosition = Vector3.zero;
        }
    }

    private void ToggleGravity(bool useGravity)
    {
        if (!_selectedRigidbody) return;
        _selectedRigidbody.useGravity = useGravity;
    }

    public void SetCurrentHighlightedObject(Collider gameObject)
    {
        if (!gameObject || _currentHighlightedObject == gameObject) return;
        _currentHighlightedObject = gameObject;
    }
    #endregion
}