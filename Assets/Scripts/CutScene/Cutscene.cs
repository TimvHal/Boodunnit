using Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutscene : MonoBehaviour
{
    public bool StartCutsceneOnAwake;
    public List<Action> ActionsInCutscene;
    private int _transitionSpeedMultiplier = 10;

    private void Awake()
    {
        if(StartCutsceneOnAwake)
            StartCutscene();
    }

    public void StartCutscene()
    {
        GameManager.IsCutscenePlaying = true;
        DisableOrEnablePlayer(false);
        DisableOrEnablePlayerCamera(false);

        StartCoroutine(ExecuteActions());
    }

    private IEnumerator ExecuteActions()
    {
        int actionCounter = 0;
        if (ActionsInCutscene == null || ActionsInCutscene.Count == 0)
        {
            yield break;
        }

        while (GameManager.IsCutscenePlaying)
        {
            Action action = ActionsInCutscene[actionCounter];
            action.IsExecuting = true;

            switch (action.ActionType)
            {
                case ActionType.Position:
                    yield return StartCoroutine(ChangePositionOfGameObject(action));
                    break;
                case ActionType.Rotation:
                    yield return StartCoroutine(ChangeRotationOfGameObject(action));
                    break;
                case ActionType.Scaling:
                    yield return StartCoroutine(ChangeScalingOfGameObject(action));
                    break;
                case ActionType.Popup:
                    yield return StartCoroutine(OpenPopUp(action));
                    break;
                case ActionType.Dialogue:
                    yield return StartCoroutine(StartDialogue(action));
                    break;
                case ActionType.DialogueNPC:
                    yield return StartCoroutine(StartDialogueNPC(action));
                    break;
                case ActionType.Method:
                    CallMethodFromMonoBehaviour(action);
                    break;
            }
           
            if (!action.IsExecuting)
            {
                actionCounter++;
                if (actionCounter <= ActionsInCutscene.Count - 1)
                {
                    yield return new WaitForSeconds(action.TimeBeforeNextAction);
                }
                else
                {
                    GameManager.IsCutscenePlaying = false;
                    DisableOrEnablePlayer(true);
                    DisableOrEnablePlayerCamera(true);
                    yield break;
                }
            }
            yield return null;
        }
    }

    #region Cutscene actions
    private IEnumerator ChangePositionOfGameObject(Action currentAction)
    {
        if (!currentAction.ObjectForCutscene)
            yield break;

        GameObject gameObject = currentAction.ObjectForCutscene;

        if (currentAction.ShouldBeKinematic)
            SetRigidBodyKinematic(gameObject, true);

        if (currentAction.IsInstant)
        {
            gameObject.transform.position = currentAction.EndPosition;
            currentAction.IsExecuting = false;
        }

        while (currentAction.IsExecuting)
        {
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, currentAction.EndPosition, currentAction.TransitionSpeed * Time.deltaTime);

            if (gameObject.transform.position == currentAction.EndPosition)
                currentAction.IsExecuting = false;

            yield return null;
        }

        SetRigidBodyKinematic(gameObject, false);
    }

    private IEnumerator ChangeRotationOfGameObject(Action currentAction)
    {
        if (!currentAction.ObjectForCutscene)
            yield break;

        GameObject gameObject = currentAction.ObjectForCutscene;
        if (currentAction.IsInstant)
        {
            Quaternion rotateToEndRotation = Quaternion.Euler(currentAction.EndRotation.x, currentAction.EndRotation.y, currentAction.EndRotation.z);
            gameObject.transform.rotation = rotateToEndRotation;
            currentAction.IsExecuting = false;
        }

        while (currentAction.IsExecuting)
        {
            Quaternion rotateToEndRotation = Quaternion.Euler(currentAction.EndRotation.x, currentAction.EndRotation.y, currentAction.EndRotation.z);
            gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, rotateToEndRotation, (currentAction.TransitionSpeed * _transitionSpeedMultiplier) * Time.deltaTime);

            if (gameObject.transform.rotation == rotateToEndRotation)
                currentAction.IsExecuting = false;

            yield return null;
        }
    }

    private IEnumerator ChangeScalingOfGameObject(Action currentAction)
    {
        if (!currentAction.ObjectForCutscene)
            yield break;

        GameObject gameObject = currentAction.ObjectForCutscene;
        if (currentAction.IsInstant)
        {
            gameObject.transform.localScale = currentAction.EndScale;
            currentAction.IsExecuting = false;
        }

        float timeLerping = 0.0f;
        while (currentAction.IsExecuting)
        {
            timeLerping += Time.deltaTime;
            gameObject.transform.localScale = Vector3.Lerp(gameObject.transform.localScale, currentAction.EndScale, timeLerping / (currentAction.TransitionSpeed * _transitionSpeedMultiplier));

            if (gameObject.transform.localScale == currentAction.EndScale)
                currentAction.IsExecuting = false;

            yield return null;
        }
    }

    private IEnumerator OpenPopUp(Action currentAction)
    {
        if (currentAction.Popup == null) 
            yield break;

        currentAction.Popup.OpenPopup();
        
        while (currentAction.IsExecuting)
        {
            currentAction.IsExecuting = Popup.isPopUpOpen;
            yield return null;
        }
    }

    private IEnumerator StartDialogue(Action currentAction)
    {
        // Conversation between player and entity.
        PlayerBehaviour playerBehaviour = currentAction.ObjectForCutscene.GetComponent<PlayerBehaviour>();
        if (playerBehaviour)
        {
            ConversationManager conversationManager = playerBehaviour.GetComponent<ConversationManager>();
            PossessionBehaviour possessionBehaviour = playerBehaviour.GetComponent<PossessionBehaviour>();

            if (conversationManager && possessionBehaviour)
            {
                bool isPossessing = possessionBehaviour.IsPossessing;
                conversationManager.TriggerConversation(isPossessing, currentAction.Dialogue, currentAction.Question);
            }
        }

        while (currentAction.IsExecuting)
        {
            currentAction.IsExecuting = ConversationManager.HasConversationStarted;
            yield return null;
        }
    }

    private IEnumerator StartDialogueNPC(Action currentAction)
    {
        if (ConversationManager.HasConversationStarted)
            yield break;

        ConversationManager conversationManager = currentAction.ConversationManager;
        BaseEntity targetToTalkTo = currentAction.TargetToTalkTo;
        if (conversationManager && targetToTalkTo)
        {
            conversationManager.TriggerCutsceneConversation(targetToTalkTo, currentAction.targetIsPlayer, currentAction.Dialogue, currentAction.Question);
        }

        while (currentAction.IsExecuting)
        {
            currentAction.IsExecuting = ConversationManager.HasConversationStarted;
            yield return null;
        }
    }

    private void CallMethodFromMonoBehaviour(Action currentAction)
    {
        if (currentAction.MethodToCallFromScript == null)
            return;

        currentAction.MethodToCallFromScript.Invoke();
        currentAction.IsExecuting = false;
    }
    #endregion

    #region Cutscene utility
    private void SetRigidBodyKinematic(GameObject gameObject, bool enableOrDisable)
    {
        Rigidbody objectRigidBody = gameObject.GetComponent<Rigidbody>();
        if (objectRigidBody)
        {
            objectRigidBody.isKinematic = enableOrDisable;
            objectRigidBody.useGravity = !enableOrDisable;
        }
    }
    
    /// <summary>
    /// Enable or disable everything bound the player movement and ability.
    /// </summary>
    /// <param name="shouldPlayerBeEnabled">Should player be enabled or disabled.</param>
    private void DisableOrEnablePlayer(bool shouldPlayerBeEnabled)
    {
        FindObjectOfType<PlayerBehaviour>().enabled = shouldPlayerBeEnabled;
        if(shouldPlayerBeEnabled)
            FindObjectOfType<PlayerBehaviour>().PossessionSpeed = 10;
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
    #endregion
}