using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class HighlightBehaviour : MonoBehaviour
{
    private Dictionary<string, float> _highlighRadius;
    private float _maxRadius;
    private Vector3 _position;
    private Collider _currentCollider;
    private Collider _previousCollider;
    private Collider _currentColliderDashable;
    private Collider _previousColliderDashable;
    private IconCanvas _iconCanvas;

    public LayerMask IgnoredLayerMasks;

    private void Awake()
    {
        _iconCanvas = FindObjectOfType<IconCanvas>();
    }

    public Collider HighlightGameobject(Dictionary<string, float> highlightRadius)
    {
        _highlighRadius = highlightRadius;
        _maxRadius = _highlighRadius.Values.Max();

        FindCollidersInFrontOfPlayer();

        return _currentCollider;
    }

    public void FindCollidersInFrontOfPlayer()
    {
        if (PossessionBehaviour.PossessionTarget)
        {
            _position = PossessionBehaviour.PossessionTarget.transform.position;
        } 
        else
        {
            _position = transform.position;
        }

        Collider[] currentObjects = Physics.OverlapSphere(_position, _maxRadius)
            .Where(c => { return IsHighlightable(c) && IsObjectInAngle(c); })
            .ToArray();

        Collider[] closetColliders = GetClosestCollider(currentObjects);
        _currentCollider = closetColliders[0];
        _currentColliderDashable = closetColliders[1];

        if (_currentCollider != null && checkForWall())
        {
            Outline outline = _currentCollider.gameObject.GetComponent<Outline>();

            if (outline)
            {
                ToggleOutlineScriptOnGameobject(outline, true);
            }

            if (!GameManager.IsCutscenePlaying)
                ShowIconsAboveHighlightedObject();
        }

        if (_currentColliderDashable != null && checkForWallDashable())
        {
            if (!GameManager.IsCutscenePlaying)
                ShowDashIconAboveDashable();
        }
        

        if (_previousCollider != _currentCollider)
        {
            if (_previousCollider != null)
            {
                Outline outline = _previousCollider.gameObject.GetComponent<Outline>();

                if (outline)
                {
                    ToggleOutlineScriptOnGameobject(outline, false);
                }

                DisableIconCanvasImages();
            }

            _previousCollider = _currentCollider;
        }

        if (_previousColliderDashable != _currentColliderDashable)
        {
            if (_previousColliderDashable != null)
            {
                DisableIconCanvasImages();
            }
            _previousColliderDashable = _currentColliderDashable;
        }
    }

    public bool IsHighlightable(Collider collider)
    {
        IPossessable possessableObject = collider.GetComponent<IPossessable>();
        ILevitateable levitateableObject = collider.GetComponent<ILevitateable>();
        WorldSpaceClue worldSpaceClue = collider.GetComponent<WorldSpaceClue>();
        AirVent airVent = collider.GetComponent<AirVent>();

        if (PossessionBehaviour.PossessionTarget)
        {
            return possessableObject != null || airVent != null || collider.gameObject.layer == 12;
        }
        else
        {
            return possessableObject != null || levitateableObject != null || worldSpaceClue != null || collider.gameObject.layer == 10;
        }
    }

    public bool IsObjectInAngle(Collider collider)
    {
        Vector3 colliderDirection;
        float angle;

        //if possesing and player is not possesion target
        if (PossessionBehaviour.PossessionTarget && gameObject != PossessionBehaviour.PossessionTarget)
        {
            //check if object is in front of the player
            colliderDirection = (collider.transform.position - PossessionBehaviour.PossessionTarget.transform.position).normalized;
            angle = Vector3.Angle(colliderDirection, PossessionBehaviour.PossessionTarget.transform.forward);
        }
        else
        {
            colliderDirection = (collider.transform.position - transform.position).normalized;
            angle = Vector3.Angle(colliderDirection, transform.forward);
        }

        return angle > -90 && angle < 90;
    }

    public Collider[] GetClosestCollider(Collider[] colliders)
    {
        float minRadius = _maxRadius;
        float minRadiusDashable = _maxRadius;
        Collider closestCollider = null;
        Collider closestColliderDashable = null;

        foreach (Collider collider in colliders)
        {
            //if collider is not null
            if (collider)
            {
                float distance = Vector3.Distance(_position, collider.transform.position);

                if (collider.gameObject.layer == 10 && distance < _highlighRadius["DashRadius"] && distance < minRadiusDashable)
                {
                    minRadiusDashable = distance;
                    closestColliderDashable = collider;
                }

                //check the extra distances distance < minDistance
                if (!PossessionBehaviour.PossessionTarget &&
                    (collider.GetComponent<ILevitateable>() != null && distance < _highlighRadius["LevitateRadius"] ||
                     collider.GetComponent<IPossessable>() != null && distance < _highlighRadius["PossesionRadius"] ||
                     collider.GetComponent<WorldSpaceClue>() != null && distance < _highlighRadius["ClueRadius"]) &&
                     distance < minRadius)
                {
                    minRadius = distance;
                    closestCollider = collider;
                }

                //if possesing
                else if (PossessionBehaviour.PossessionTarget && PossessionBehaviour.PossessionTarget != collider.gameObject)
                {
                    //if collider is Possesable and within radius
                    if (collider.gameObject.GetComponent<IPossessable>() != null && distance < _highlighRadius["ConversationRadius"] ||
                     collider.gameObject.GetComponent<AirVent>() != null && distance < _highlighRadius["AirVentRadius"] ||
                     collider.gameObject.layer == 12 && distance < _highlighRadius["ClimableRadius"])
                    {
                        if (distance < minRadius)
                        {
                            minRadius = distance;
                            closestCollider = collider;
                        }
                    }
                }
            }
        }

        return new Collider[] { closestCollider, closestColliderDashable };
    }

    private bool checkForWall()
    {
        if (_currentCollider)
        {
            RaycastHit hit;
            Vector3 direction = (_currentCollider.transform.position - transform.position).normalized;
            Debug.DrawRay(transform.position, direction);

            if (Physics.Raycast(transform.position, direction, out hit, 200, ~IgnoredLayerMasks))
            {
                return hit.collider.name == _currentCollider.name;
            }
        }

        return false;
    }

    private bool checkForWallDashable()
    {
        if (_currentColliderDashable)
        {
            RaycastHit hit;
            Vector3 direction = (_currentColliderDashable.transform.position - transform.position).normalized;

            if (Physics.Raycast(transform.position, direction, out hit))
            {
                return hit.collider.name == _currentColliderDashable.name;
            }
        }

        return false;
    }

    private  void ToggleOutlineScriptOnGameobject(Outline outline, bool active)
    {
        outline.enabled = active;
    }

    private void ShowIconsAboveHighlightedObject()
    {
        if (_iconCanvas && _currentCollider)
        {
            _iconCanvas.IconTarget = _currentCollider.gameObject;
            _iconCanvas.EnableIcons();
        }
    }

    private void ShowDashIconAboveDashable()
    {
        if (_iconCanvas && _currentColliderDashable)
        {
            _iconCanvas.IconTargetDashable = _currentColliderDashable.gameObject;
            _iconCanvas.EnableDashIcon();
        }
    }

    private void DisableIconCanvasImages()
    {
        _iconCanvas.DisableIcons();
    }
}