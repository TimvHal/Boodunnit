﻿using Enums;
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

    private void OnTriggerEnter(Collider other)
    {
        PlayerBehaviour player = other.gameObject.GetComponent<PlayerBehaviour>();
        if (player)
        {
            AchievementHandler.Instance.AwardAchievement(SteamAchievements.ACH_I_AM_JULIA_LAWSON);
            StartCoroutine(FadeToWhite());
        }
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
