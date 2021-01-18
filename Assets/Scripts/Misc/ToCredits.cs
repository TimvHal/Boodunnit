using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToCredits : MonoBehaviour
{
    private void Awake()
    {
        GameManager.CursorIsLocked = false;
    }

    public void ToCreditsScene()
    {
        SceneManager.LoadScene("CreditScene");
    }
}
