using System.Collections;
using DefaultNamespace.Enums;
using UnityEngine;

public class LevitateableObject : MonoBehaviour, ILevitateable
{
    [SerializeField] private bool _canRespawnWhenOutOfRange;
    private Vector3 _spawnLocation;
    private Quaternion _spawnRotation;
    private Rigidbody _rigidbody;
    private GameObject _player;
    private float _maxDistanceToPlayerWhileFrozen;
    
    private Outline _outline;
    private Color _purple;
    private Color _coolerPurple;
    
    public LevitationState State { get; set; }
    public int TimesLevitated { get; set; }
    
    public float DespawnDistance = 10f;
    public bool WillLogPossessCount;

    private void Awake()
    {
        State = LevitationState.NotLevitating;
        _rigidbody = GetComponent<Rigidbody>();
        _player = FindObjectOfType<PlayerBehaviour>().gameObject;

        Outline outlineComponent = gameObject.GetComponent<Outline>();
        if (!outlineComponent) _outline = gameObject.AddComponent<Outline>();

        AddOutline();
        SetSpawnLocationAndRotation();
        StartCoroutine(CheckForDistance());
    }

    private void Start()
    {
        _maxDistanceToPlayerWhileFrozen = _player.GetComponent<LevitateBehaviour>().CurrentLevitateRadius + 0.1f;
    }

    private void Update()
    {
        float distance = Vector3.Distance(gameObject.transform.position, _player.transform.position);
        ChangeOutlineWidthWithDistance(distance);
        if (distance > _maxDistanceToPlayerWhileFrozen) Release();
    }

    public void Freeze()
    {
        if (gameObject.GetComponentInChildren<LevitateableObjectIsInsideTrigger>().PlayerIsInsideObject) return;
        _outline.OutlineColor = _coolerPurple;
        ChangeLayerMask(0);
        ToggleIsKinematic(true);
        State = LevitationState.Frozen;
    }

    public void Release()
    {
        if (_outline) _outline.OutlineColor = _purple;
        ToggleIsKinematic(false);
        State = LevitationState.NotLevitating;
    }

    public void ToggleIsKinematic(bool isKinematic)
    {
        _rigidbody.useGravity = !isKinematic;
        _rigidbody.isKinematic = isKinematic;
    }

    private void ChangeLayerMask(int layerMaskParam)
    {
        foreach (Transform transform in gameObject.GetComponentsInChildren<Transform>(true))
        {
            if (!transform.gameObject.GetComponent<LevitateableObjectIsInsideTrigger>())
                transform.gameObject.layer = layerMaskParam;
        }   
    }

    private void SetSpawnLocationAndRotation()
    {
        _spawnLocation = transform.position;
        _spawnRotation = transform.rotation;
    }

    private void AddOutline()
    {
        if (_outline)
        {
            ColorUtility.TryParseHtmlString("#6F4F61", out _purple);
            ColorUtility.TryParseHtmlString("C6BAC4", out _coolerPurple);

            _outline.OutlineColor = _purple;
            _outline.OutlineMode = Outline.Mode.OutlineVisible;
            _outline.OutlineWidth = 0.0f;
            _outline.enabled = false;
        }
    }

    private void ChangeOutlineWidthWithDistance(float distance)
    {
        if (_outline) _outline.OutlineWidth = 10f * (1f - (distance / (_maxDistanceToPlayerWhileFrozen - 0.1f)));
    }

    private IEnumerator CheckForDistance()
    {
        yield return new WaitForSeconds(2f);
        if (CanRespawnWhenOutOfRange && 
            Mathf.Abs(Vector3.Distance(transform.position, _spawnLocation)) >= DespawnDistance && 
            Mathf.Abs(Vector3.Distance(transform.position, CameraController.RotationTarget.position)) >= DespawnDistance && 
            Mathf.Abs(Vector3.Distance(_spawnLocation, CameraController.RotationTarget.position)) >= DespawnDistance)
        {
            Despawn();
        }

        StartCoroutine(CheckForDistance());
    }
    
    private void Despawn()
    {
        transform.position = _spawnLocation;
        transform.rotation = _spawnRotation;
        _rigidbody.velocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (State != LevitationState.Levitating) return;
        float collisionForce = collision.impulse.magnitude / Time.fixedDeltaTime;
        if(collisionForce > 200f) SoundManager.Instance.PlaySound("Levitate_bump");
    }
    
    public bool CanRespawnWhenOutOfRange
    {
        get => _canRespawnWhenOutOfRange;
        set => _canRespawnWhenOutOfRange = value;
    }
}