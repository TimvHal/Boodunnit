using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeInAndOut : MonoBehaviour
{
    public Image image;

    // Start is called before the first frame update
    private void Awake()
    {
        image.canvasRenderer.SetAlpha(0.0f);
    }

    public void FadeIn(int fadeInSpeed)
    {
        image.CrossFadeAlpha(1.0f, fadeInSpeed, false);
    }
    public void FadeOut(int fadeOutSpeed)
    {
        image.CrossFadeAlpha(0, fadeOutSpeed, false);
    }
}
