using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CloudPortalEnding : MonoBehaviour
{
    public Image image;
    private void Awake()
    {
        image.canvasRenderer.SetAlpha(0.0f);
    }

    private void OnTriggerEnter()
    {
        StartCoroutine(FadeToWhite());
    }

    private IEnumerator FadeToWhite()
    {
        PlayerBehaviour player = FindObjectOfType<PlayerBehaviour>();
        if (player)
        {
            player.enabled = false;
        }

        image.CrossFadeAlpha(1.0f, 2, false);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("EndScreenScene");
    }
}
