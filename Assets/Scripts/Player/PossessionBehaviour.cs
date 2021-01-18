using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entities;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PossessionBehaviour : MonoBehaviour
{
    public bool IsPossessing;

    [HideInInspector]
    public BaseEntity TargetBehaviour;

    public static GameObject PossessionTarget;
    private CameraController _cameraController;
    public float UnpossessRadius;
    public int UnPossessRetriesOnYAxis;

    public float Cooldown = 1f;
    public bool IsOnCooldown = false;

    private float _playerEndPositionRadius;

    private void Awake()
    {
        _playerEndPositionRadius = GetComponent<Collider>().bounds.extents.z;
        _cameraController = Camera.main.GetComponent<CameraController>();
    }

    private void Update()
    {
        if(IsPossessing) transform.position = PossessionTarget.transform.position;
    }

    public void LeavePossessedTarget()
    {
        if (PossessionTarget && IsPossessing && !TargetBehaviour.HasToggledAbility)
        {
            IsPossessing = false;
            TargetBehaviour.IsPossessed = false;
            transform.position = PossessionTarget.transform.position + (Vector3.up * 2);
            _cameraController.CameraRotationTarget = transform;
            gameObject.GetComponent<PlayerBehaviour>().IsGrounded = false;

            TargetBehaviour.Rigidbody.constraints = RigidbodyConstraints.FreezeAll;

            if (TargetBehaviour.NavMeshAgent)
            {
                TargetBehaviour.NavMeshAgent.enabled = true;
            }

            TargetBehaviour.ResetDestination();
            
            
            // EnableOrDisablePlayerMeshRenderers(true);
            EnableOrDisablePlayerSkinnedMeshRenderers(true);

            EnableOrDisablePlayerColliders(true);
            EnableOrDisableObjectChildColliders(true);
            EnableOrDisableObjectRigidBody(true);

            TargetBehaviour = null;
            PossessionTarget = null;
            
            IsOnCooldown = true;

            TeleportPlayerToRandomPosition();

            //ToDo: This code is left by the lead dev, change this to how it should be!, but right now i dont care
            GetComponent<Rigidbody>().velocity = Vector3.zero;

            StartCoroutine(PossessionTimer());
        }
    }

    private void EnableOrDisablePlayerMeshRenderers(bool activateMeshRenderers)
    {
        //Changes players mesh renderer
        MeshRenderer playerMeshRenderer = GetComponent<MeshRenderer>();
        if (playerMeshRenderer)
        {
            playerMeshRenderer.enabled = activateMeshRenderers;
        }

        //Changes all of the players children mesh renderers
        MeshRenderer[] arrayPlayerChildMeshRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer childMeshRenderer in arrayPlayerChildMeshRenderers)
        {
            childMeshRenderer.enabled = activateMeshRenderers;
        }
    }

    private void EnableOrDisablePlayerSkinnedMeshRenderers(bool activateMeshRenderers)
    {
        //Changes players mesh renderer
        SkinnedMeshRenderer playerMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        if (playerMeshRenderer)
        {
            playerMeshRenderer.enabled = activateMeshRenderers;
        }

        //Changes all of the players children mesh renderers
        SkinnedMeshRenderer[] arrayPlayerChildMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer childMeshRenderer in arrayPlayerChildMeshRenderers)
        {
            childMeshRenderer.enabled = activateMeshRenderers;
        }
    }

    private void EnableOrDisablePlayerColliders(bool activateColliders)
    {
        //Changes players collider
        Collider playerCollider = GetComponent<Collider>();
        if (playerCollider)
        {
            playerCollider.enabled = activateColliders;
        }

        //Changes all of the players children colliders
        Collider[] arrayPlayerChildColliders = GetComponentsInChildren<Collider>();
        foreach (Collider childCollider in arrayPlayerChildColliders)
        {
            childCollider.enabled = activateColliders;
        }
    }

    public void PossessTarget(Collider possesionTarget)
    {
        if (IsPossessing || IsOnCooldown) return;

        //Check if the targetBehaviour is possessable.
        TargetBehaviour = possesionTarget.GetComponent<BaseEntity>();
        
        if (!TargetBehaviour.CanPossess || TargetBehaviour.IsTraversingOfMeshLink) return;

        PossessionTarget = possesionTarget.gameObject;
        _cameraController.CameraRotationTarget = possesionTarget.transform;
        TargetBehaviour.Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        TargetBehaviour.TimesPosessed +=1;

        if (TargetBehaviour.NavMeshAgent)
        {
            TargetBehaviour.NavMeshAgent.enabled = false;
        }

        TargetBehaviour.IsPossessed = true;
        TargetBehaviour.ResetFearDamage();
        IsPossessing = true;
        transform.position = possesionTarget.gameObject.transform.position;

        EnableOrDisableObjectChildColliders(false);
        EnableOrDisableObjectRigidBody(false);

        // EnableOrDisablePlayerMeshRenderers(false);
        EnableOrDisablePlayerSkinnedMeshRenderers(false);

        EnableOrDisablePlayerColliders(false);
    }

    private void TeleportPlayerToRandomPosition()
    {
        float minNewPlayerPositionInRadiusX = transform.position.x - UnpossessRadius;
        float minNewPlayerPositionInRadiusZ = transform.position.z - UnpossessRadius;

        float maxNewPlayerPositionInRadiusX = transform.position.x + UnpossessRadius;
        float maxNewPlayerPositionInRadiusY = transform.position.y + UnpossessRadius;
        float maxNewPlayerPositionInRadiusZ = transform.position.z + UnpossessRadius;

        int newPositionTries = 0;
        while (newPositionTries < UnPossessRetriesOnYAxis)
        {
            Vector3 playerNewPositionAfterUnpossessing = GetNewPlayerVector3Position(minNewPlayerPositionInRadiusX, maxNewPlayerPositionInRadiusX,
                minNewPlayerPositionInRadiusZ, maxNewPlayerPositionInRadiusZ);
            Collider[] newPositionCollision = Physics.OverlapSphere(playerNewPositionAfterUnpossessing, _playerEndPositionRadius).Where(collider => collider.isTrigger == false).ToArray();

            if (newPositionCollision != null)
            {
                if (newPositionCollision.Length == 0)
                {
                    Vector3 playerToNewPosition = (transform.position - playerNewPositionAfterUnpossessing).normalized;
                    RaycastHit[] hits = Physics.RaycastAll(transform.position, playerToNewPosition,
                       Vector3.Distance(transform.position, playerToNewPosition));
                    foreach(RaycastHit hit in hits)
                    {
                        if(hit.collider != null && !hit.collider.isTrigger || hit.collider != null && hit.collider.isTrigger)
                        {
                            transform.position = playerNewPositionAfterUnpossessing;
                            return;
                        }
                    }
                }
            }
            newPositionTries++;
        }

        Vector3 playerNewPositionAfterUnpossessingOnY = GetNewPlayerVector3Position(minNewPlayerPositionInRadiusX, maxNewPlayerPositionInRadiusX, transform.position.y, maxNewPlayerPositionInRadiusY,
            minNewPlayerPositionInRadiusZ, maxNewPlayerPositionInRadiusZ);
        transform.position = playerNewPositionAfterUnpossessingOnY;
    }
    private Vector3 GetNewPlayerVector3Position(float minPositionX, float maxPositionX, float minPositionY, float maxPositionY, float minPositionZ, float maxPositionZ)
    {
        return new Vector3(
            Random.Range(minPositionX, maxPositionX),
            Random.Range(minPositionY, maxPositionY),
            Random.Range(minPositionZ, maxPositionZ)
        );
    }

    private Vector3 GetNewPlayerVector3Position(float minPositionX, float maxPositionX, float minPositionZ, float maxPositionZ)
    {
        return new Vector3(
            Random.Range(minPositionX, maxPositionX),
            transform.position.y,
            Random.Range(minPositionZ, maxPositionZ)
        );
    }

    private void EnableOrDisableObjectRigidBody(bool shouldBeEnabled)
    {
        foreach (Rigidbody rigidBodyOfChild in TargetBehaviour.GetComponentsInChildren<Rigidbody>())
        {
            if (rigidBodyOfChild.gameObject != TargetBehaviour.gameObject)
                rigidBodyOfChild.isKinematic = shouldBeEnabled;
        }
    }

    private void EnableOrDisableObjectChildColliders(bool shouldBeEnabled)
    {
        foreach (Collider colliderOfChild in TargetBehaviour.GetComponentsInChildren<Collider>())
        {
            if (colliderOfChild.gameObject != TargetBehaviour.gameObject)
                colliderOfChild.enabled = shouldBeEnabled;
        }
    }

    private IEnumerator PossessionTimer()
    {
        float currentTime = 0;
        while (currentTime < Cooldown)
        {
            yield return null;
            currentTime += Time.deltaTime;
        }

        IsOnCooldown = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, UnpossessRadius);
    }

}
