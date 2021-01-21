using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirVent : MonoBehaviour
{
    public int AirPower;
    private void OnTriggerStay(Collider other)
    {
        GlideBehaviour glideBehaviour = other.gameObject.GetComponent<GlideBehaviour>();
        if (glideBehaviour && glideBehaviour.IsGliding)
        {
            if(other.gameObject == PossessionBehaviour.PossessionTarget)
            {
                Rigidbody otherRigidBody = other.gameObject.GetComponent<Rigidbody>();
                if (otherRigidBody)
                {
                    AchievementHandler.Instance.AwardAchievement(SteamAchievements.ACH_PHEASANT_PHYSICS);
                    otherRigidBody.AddForce(transform.forward * AirPower, ForceMode.Acceleration);
                }
            }
        }
    }
}
