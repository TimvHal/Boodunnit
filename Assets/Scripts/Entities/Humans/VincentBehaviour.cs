using Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VincentBehaviour : BaseEntity
{
    private void Awake()
    {
        CanPossess = false;
        InitBaseEntity();
        Rigidbody.constraints = RigidbodyConstraints.FreezePosition;
    }

    public override void UseFirstAbility()
    {
        // Yeet skeet, my behaviour is completely obsolete
    }

}
