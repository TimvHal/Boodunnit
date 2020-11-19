﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutscene : MonoBehaviour
{
    public List<Action> ActionsInCutscene;
    public static bool IsCutSceneFinished;

    /**
        * 1. Check if a gameobject is set.
        *      1.1 Set begin position and rotation.
        *      1.2 Set end position and rotation.
        *      1.3 Keep in mind the time to transition.
        * 2. Check if action contains a Popup.
        *      2.1 Trigger Popup.
        * 3. Check if action contains a dialogue.
        *      3.1 Trigger Dialogue.
        * 4. Check if action is a blocking action. ???
    **/

    private void Awake()
    {
        GameManager.IsCutscenePlaying = true;
        DisableOrEnablePlayer(false);
        DisableOrEnablePlayerCamera(false);
    }

    private void Update()
    {
        StartCoroutine(StartCutscene());
    }

    public IEnumerator StartCutscene()
    {
        int actionCounter = 0;
        if (ActionsInCutscene == null && ActionsInCutscene.Count == 0)
        {
            yield break;
        }

        Action action = ActionsInCutscene[actionCounter];
        action.IsExecuting = true;

        while (GameManager.IsCutscenePlaying)
        {
            switch (action.ActionType)
            {
                case ActionType.Position:
                    StartCoroutine(ChangePositionOfGameObject(action));
                    break;
            }

            if (!action.IsExecuting)
            {
                actionCounter++;

                if (actionCounter <= ActionsInCutscene.Count - 1)
                {
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    GameManager.IsCutscenePlaying = false;
                    yield break;
                }
            }
            yield return null;
        }
    }

    private IEnumerator ChangePositionOfGameObject(Action currentAction)
    {
        while (currentAction.IsExecuting)
        {
            GameObject gameObject = currentAction.ObjectForCutscene;
            if (currentAction.IsInstant)
            {
                gameObject.transform.position = currentAction.EndPosition;
                currentAction.IsExecuting = false;
            }

            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, currentAction.EndPosition, currentAction.TransitionSpeed * Time.deltaTime);
            if(gameObject.transform.position == currentAction.EndPosition)
            {
                currentAction.IsExecuting = false;
            }

            yield return null;
        }
    }


    /// <summary>
    /// Enable or disable everything bound the player movement and ability.
    /// </summary>
    /// <param name="shouldPlayerBeEnabled">Should player be enabled or disabled.</param>
    private void DisableOrEnablePlayer(bool shouldPlayerBeEnabled)
    {
        GameObject player = FindObjectInScene("Player");
        player.GetComponent<PlayerBehaviour>().enabled = shouldPlayerBeEnabled;
    }

    /// <summary>
    /// Enable or disable everything bound the the player camera.
    /// </summary>
    /// <param name="shouldCamereaBeEnabled"></param>
    private void DisableOrEnablePlayerCamera(bool shouldCamereaBeEnabled)
    {
        GameObject camera = Camera.main.gameObject;
        camera.GetComponent<CameraController>().enabled = shouldCamereaBeEnabled;
    }

    /// <summary>
    /// Find Gameobject by name.
    /// </summary>
    /// <param name="nameOfObject">Name of object you want to find.</param>
    /// <returns></returns>
    private GameObject FindObjectInScene(string nameOfObject)
    {
        GameObject objectToFind = GameObject.Find(nameOfObject);
        if (objectToFind)
        {
            return objectToFind;
        }
        return null;
    }
}