using UnityEngine;

public class LevitateableObjectIsInsideTrigger : MonoBehaviour
{
    public bool PlayerIsInsideObject { get; set; }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerBehaviour>())
        {
            PlayerIsInsideObject = true;
        }     
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerBehaviour>())
        {
            PlayerIsInsideObject = false;
        }
    }
}
