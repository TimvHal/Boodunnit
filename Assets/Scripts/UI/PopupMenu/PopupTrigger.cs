using UnityEngine;

public class PopupTrigger : MonoBehaviour
{
    public Canvas Popup;

    public bool IsTriggerForCloud = false;

    public GameObject CloudSnapLocation;


    private bool _hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (Popup != null && !_hasBeenTriggered && FindObjectOfType<PlayerBehaviour>() == other.GetComponent<PlayerBehaviour>())
        {

            ActivateCloudOutline();

            GameManager.IsPaused = true;
            GameManager.CursorIsLocked = false;
            Time.timeScale = 0;

            other.GetComponent<LevitateBehaviour>().FreezeLevitateableObject();
            Popup.gameObject.SetActive(true);
            _hasBeenTriggered = true;
        }
    }

    private void ActivateCloudOutline()
    {
        if (IsTriggerForCloud && CloudSnapLocation)
        {
            Outline outline = CloudSnapLocation.gameObject.GetComponent<Outline>();
            if (outline)
            {
                outline.enabled = true;
            }
        }
    }
}