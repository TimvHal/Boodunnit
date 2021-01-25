using System;
using UnityEngine;
using UnityEngine.AI;

public abstract class BaseMovement : MonoBehaviour
{
    [HideInInspector] public Rigidbody Rigidbody;

    [Header("Movement")]
    public float JumpForce = 10.0f;
    public float PossessionSpeed;
    public float PathfindingSpeed;
    public Collider Collider;


    //[HideInInspector]
    public bool IsGrounded = false;

    public bool IsJumping;
    
    [HideInInspector]
    public bool CanJump;

    private float _rotationSpeed = 10f;
    private bool _hasCollidedWithWall;
    private ContactPoint[] _contacts;
    [SerializeField] private Vector3 bottomHitPoint = Vector3.zero; 

    protected void InitBaseMovement()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }
    
    public void MoveEntityInDirection(Vector3 direction, float speed)
    {
        
        if (false)
        {
            foreach (var contact in _contacts)
            {
                Vector3 contactDirection = (contact.point - transform.position);
                
                bool raycast = Physics.Raycast(transform.position, contactDirection, out RaycastHit hit,
                    Vector3.Distance(transform.position, contact.point) + 0.2f, 
                    ~LayerMask.GetMask("Player", "PlayerDash", "Possessable"));
                  if (raycast)
                  {
                      if (
                          (hit.normal.y > 0.75f || hit.normal.y < 0) &&
                          (contact.point.y < transform.position.y - Collider.bounds.size.y / 2
                           || contact.point.y > transform.position.y + Collider.bounds.size.y / 2)
                      ) continue;
                      
                    float contactAngle = Vector3.Angle(IgnoreY(direction), IgnoreY(contactDirection));
                    Vector3 contactCross = Vector3.Cross(IgnoreY(direction), IgnoreY(contactDirection));

                    if (contactCross.y < 0) contactAngle = -contactAngle;
                    
                    
                    if (Math.Abs(contactAngle) > 10 && Math.Abs(contactAngle) <= 90)
                    {
                        float angle = contactAngle > 0 ? 90 : contactAngle < 0 ? -90 : 0;
                        direction = Quaternion.Euler(0, angle, 0) * hit.normal * (Math.Abs(contactAngle)/90);
                        direction.y = 0;
                    }    
                    else if (Math.Abs(contactAngle) > 90)
                    {
                        continue;
                    }
                    else
                    {
                        direction = Vector3.zero;
                    }
                }
            }
        }
        float yVelocity = 0;
        if (IsGrounded && 
            Physics.Raycast(transform.position, -transform.up, out RaycastHit castHit, Collider.bounds.size.y/2 + Mathf.Abs(Collider.bounds.center.y) + 0.1f, 
                LayerMask.GetMask("Default")) && 
            castHit.normal.y > 0.5  && Rigidbody.velocity.y < JumpForce - 0.3f)
        {
            bottomHitPoint = castHit.normal;
            yVelocity = (castHit.normal.y -1) * -1;
            if (yVelocity < 0) yVelocity = 0;
            Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, yVelocity, Rigidbody.velocity.z);
        } 
        yVelocity = Rigidbody.velocity.y;
        Rigidbody.velocity = direction * speed;
        Rigidbody.velocity += new Vector3(0f, yVelocity, 0f);
      
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, 
                Quaternion.LookRotation(direction.normalized), Time.deltaTime * _rotationSpeed);
        }
        if (!IsGrounded)
        {
            if (MovementCheck(Rigidbody.velocity, true) && MovementCheck(Rigidbody.velocity, false)) 
                Rigidbody.velocity = new Vector3(0, Rigidbody.velocity.y, 0); 
        }
    }
    private bool MovementCheck(Vector3 velocity, bool positive, int angleChangedAmount = 0)
    {
        // Get the velocity
        Vector3 horizontalMove = velocity;
        // Don't use the vertical velocity
        horizontalMove.y = 0;
        // Calculate the approximate distance that will be traversed
        float distance =  horizontalMove.magnitude * Time.fixedDeltaTime;
        // Normalize horizontalMove since it should be used to indicate direction
        horizontalMove.Normalize();
        RaycastHit hit;

        // Check if the body's current velocity will result in a collision
        if (Rigidbody.SweepTest(horizontalMove, out hit, distance, QueryTriggerInteraction.Ignore))
        {
            if (Math.Abs(angleChangedAmount) < 90)
            {
                return MovementCheck(Quaternion.Euler(0, positive ? 1 : -1, 0) * velocity, positive, angleChangedAmount + 1);
            }
            return true;
        }

        float y = Rigidbody.velocity.y;
        Vector3 newVel = velocity / (1 + (angleChangedAmount / 25f));
        newVel.y = y;
        Rigidbody.velocity = newVel;
        return false;
        
    }

    public virtual void MoveEntityInDirection(Vector3 direction)
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent)
        {
            if (agent.enabled && agent.isStopped == false)
            {
                MoveEntityInDirection(direction, PathfindingSpeed);
            }
            else
            {
                MoveEntityInDirection(direction, PossessionSpeed);
            }
            return;
        }
        MoveEntityInDirection(direction, PossessionSpeed);
    }

    public void Jump()
    {
        if (CanJump && IsGrounded)
        {
            IsJumping = true;
            IsGrounded = false;
            Rigidbody.AddForce(Vector3.up * JumpForce, ForceMode.VelocityChange);   
        }
    }

    private void OnCollisionStay(Collision other)
    {
        _contacts = other.contacts;
        _hasCollidedWithWall = !IsGrounded;
    }
    
    private Vector3 IgnoreY(Vector3 input)
    {
        input.y = 0;
        return input;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
        {
            if (other.name == "GameObject Air flow")
            {
                return;
            }

            IsGrounded = true;
            IsJumping = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.isTrigger)
        {
            if (other.name == "GameObject Air flow")
            {
                return;
            }

            IsGrounded = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger)
        {
            if (other.name == "GameObject Air flow")
            {
                return;
            }

            IsGrounded = false;
        }
    }

    
}

