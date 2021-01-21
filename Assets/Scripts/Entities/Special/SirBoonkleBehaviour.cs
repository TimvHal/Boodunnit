using System.Collections;
using Entities;
using UnityEngine;
using UnityEngine.AI;

public class SirBoonkleBehaviour : BaseEntity
{
    [Header("SirBoonkle")]
    public float FadeDuration = 1f;
    private Dialogue[] _dialogues;

    private Transform _newSpawnTransform;

    public void Awake()
    {
        InitBaseEntity();
        CanPossess = false;
        _dialogues = Resources.LoadAll<Dialogue>($"ScriptableObjects/Conversations/Boonkle/BoonkleBaseDialogue");
    }

    public void SpawnToNewLocation(Transform newTransform, int index)
    {
        if (transform.parent.position != newTransform.position)
        {
            _newSpawnTransform = newTransform;
            if (index == 0)
                Dialogue = null;
            else
                Dialogue = _dialogues[index];
            ChangePosition();
            //StartCoroutine(FadeInAndOut());
        }
    }

    private IEnumerator FadeInAndOut()
    {
        SkinnedMeshRenderer meshRend = GetComponent<SkinnedMeshRenderer>();
        Material[] materials = meshRend.materials;

        // Fade out
        float currentTime = 0;
        while (currentTime < FadeDuration)
        {
            yield return null;
            currentTime += Time.deltaTime;
            ChangeColorOpacity(materials, 1 - currentTime / FadeDuration);
        }

        ChangePosition();

        // Fade in
        currentTime = 0;
        while (currentTime < FadeDuration)
        {
            yield return null;
            currentTime += Time.deltaTime;
            ChangeColorOpacity(materials, currentTime / FadeDuration);
        }
    }

    private void ChangePosition()
    {
        if (_newSpawnTransform)
        {
            transform.parent.position = _newSpawnTransform.position;
            transform.parent.rotation = _newSpawnTransform.rotation;
            transform.localPosition = Vector3.zero;
        }
    }

    private void ChangeColorOpacity(Material[] materials, float opacity)
    {
        foreach (Material m in materials)
        {
            Color newColor = m.color;
            newColor.a = opacity;
            m.color = newColor;
        }
    }
    
    public override void UseFirstAbility()
    {
        //TODO sir Bonkel ability? WHEEZE
    }
}
