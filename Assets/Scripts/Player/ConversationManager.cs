using System;
using Enums;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Entities;
using Logger;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ConversationManager : MonoBehaviour
{
    public static bool HasConversationStarted;
    public static Transform ConversationTarget;

    private BaseEntity _currentPossedEntity;
    private Animator _animator;
    private Text _entityNameTextbox;
    private Text _dialogueTextbox;
    private Button _buttonPrefab;
    private Button _continueButton;
    private GameObject _questionPool;

    private Queue<string> _sentences;
    private Queue<Button> _choiceButtons;
    private Question _dialogueContainedQuestion;
    private Sentence[] _defaultAnswers;
    private bool _isSentenceFinished = true;
    private bool _hasNoRelation;
    private float _typeSpeed;
    private int _maxDefaultSencteces = 0;
    private bool _skipDialogue;

    private SceneLogger _sceneLogger;
    private Log _conversationLog = new Log();

    private void Awake()
    {
        _sentences = new Queue<string>();
        _choiceButtons = new Queue<Button>();
        //Assign all variables in Dialogue Canvas
        _animator = GameObject.Find("DialogBox").GetComponent<Animator>();
        _entityNameTextbox = GameObject.Find("Name").GetComponent<Text>();
        _dialogueTextbox = GameObject.Find("Dialog").GetComponent<Text>();
        _buttonPrefab = Resources.Load<Button>("ScriptableObjects/Dialogue/Button");
        _continueButton = GameObject.Find("Continue").GetComponent<Button>();
        _questionPool = GameObject.Find("QuestionPool");
        _sceneLogger = FindObjectOfType<SceneLogger>();
    }

    #region Conversation Trigger 
    public void TriggerConversation(bool isPossesing, Dialogue dialogue = null, Question question = null)
    {
        _hasNoRelation = false;
        _maxDefaultSencteces = 0;

        //Get textspeed from the playersettings
        PlayerSettings playerSettings = SaveHandler.Instance.LoadDataContainer<PlayerSettings>();
        if (playerSettings != null)
        {
            _typeSpeed = (float)playerSettings.TextSpeed;

            switch (_typeSpeed)
            {
                case 0:
                    _typeSpeed = 0.4f;
                    break;
                case 1:
                    _typeSpeed = 0.2f;
                    break;
                case 2:
                    _typeSpeed = 0.1f;
                    break;
            }
        }

        //ToDo: Remove this line later when interaction with the world is thought about by the lead dev and lead game designer.]
        Collider[] hitColliderArray = Physics.OverlapSphere(transform.position, 5); // Player
        foreach (Collider entityCollider in hitColliderArray)
        {
            //Check which entity boolia is possesing
            if (isPossesing)
            {
                _currentPossedEntity = PossessionBehaviour.PossessionTarget.GetComponent<BaseEntity>();
            }

            if (entityCollider.TryGetComponent(out BaseEntity entityToTalkTo)) // Target to talk too
            {
                //When possesing check if the 2 interacting NPC have a relationship
                //If realtionship count is 0 they have a realtionship with everyone
                if (isPossesing && entityToTalkTo != _currentPossedEntity && !entityToTalkTo.Relationships.Contains(_currentPossedEntity.CharacterName) && entityToTalkTo.Relationships.Count != 0)
                {
                    _hasNoRelation = true;
                    _defaultAnswers = entityToTalkTo.DefaultAnswers;
                }

                //start conversation if boolia is possesing another npc
                //or if she is not possesing anyone check if she can talk to the NPC
                if (((!isPossesing && entityToTalkTo.CanTalkToBoolia)) ||
                    (isPossesing && entityToTalkTo != _currentPossedEntity))
                {
                    HasConversationStarted = true;
                    ConversationTarget = entityCollider.gameObject.transform;
                    _entityNameTextbox.text = EnumValueToString(entityToTalkTo.CharacterName);
                    _animator.SetBool("IsOpen", true);

                    GameManager.CursorIsLocked = false;
                    CheckWhichTypeOfConversationToExecute(dialogue, question, entityToTalkTo);
                }
            }
        }
    }


    public void TriggerCutsceneConversation(BaseEntity target, bool targetIsPlayer, Dialogue dialogue = null, Question question = null)
    {
        _hasNoRelation = false;
        HasConversationStarted = true;
        ConversationTarget = targetIsPlayer ? transform : target.transform;
        _entityNameTextbox.text = targetIsPlayer ? "Boolia" : EnumValueToString(target.CharacterName);
        _animator.SetBool("IsOpen", true);

        GameManager.CursorIsLocked = false;
        CheckWhichTypeOfConversationToExecute(dialogue, question, target);
    }
    #endregion

    #region Manage conversation 
    public void ManageConversation(Dialogue dialogue, Question question)
    {
        ResetQuestions();
        _sentences.Clear();

        //ResetQuestions();
        if (!ConversationTarget) return;

        BaseEntity entity = ConversationTarget.gameObject.GetComponent<BaseEntity>();

        if (entity && entity._pathFindingState != PathFindingState.None && entity != entity.GetComponent<SirBoonkleBehaviour>())
        {
            entity.PauseEntityNavAgent(true);
        }
        //Check to ask question, start dialogue or end conversation
        if (dialogue && !_hasNoRelation)
        {
            StartDialogue(dialogue);
        }

        if (question && !_hasNoRelation)
        {
            AskQuestion(question);
        }

        if (_hasNoRelation)
        {
            StartDefaultDialogue(_defaultAnswers);
        }

        if (!question && !dialogue)
        {
            CloseConversation();
        }
    }

    public void CloseConversation()
    {
        BaseEntity entity = ConversationTarget.gameObject.GetComponent<BaseEntity>();
        if (entity && entity.NavMeshAgent && entity != entity.GetComponent<SirBoonkleBehaviour>())
        {
            entity.PauseEntityNavAgent(false);
        }
        HasConversationStarted = false;
        ConversationTarget = null;
        _animator.SetBool("IsOpen", false);
        GameManager.CursorIsLocked = true;
        if (_sceneLogger) _sceneLogger.SceneLog.Stats.Add(_conversationLog);
        _conversationLog = new Log();
    }
    #endregion

    #region Dialogue
    private void StartDialogue(Dialogue dialogue)
    {
        _dialogueContainedQuestion = dialogue.question;
        _continueButton.gameObject.SetActive(true);
        foreach (Sentence sentence in dialogue.sentences)
        {
            _sentences.Enqueue(sentence.Text.ToString());
        }
        DisplayNextSentence();
    }

    private void CheckWhichTypeOfConversationToExecute(Dialogue dialogue, Question question, BaseEntity entity)
    {
        if (dialogue != null)
        {
            ManageConversation(dialogue, null);
            return;
        }

        if (question != null)
        {
            ManageConversation(null, question);
            return;
        }

        if (dialogue == null && question == null)
        {
            ManageConversation(entity.Dialogue, entity.Question);
            return;
        }
    }

    private void StartDefaultDialogue(Sentence[] dialogue)
    {
        _continueButton.gameObject.SetActive(true);

        string sentence;

        if (dialogue.Length != 0)
        {
            int randomNumber = Random.Range(0, dialogue.Length);
            sentence = dialogue[randomNumber].Text.ToString();
        }
        else
        {
            sentence = "...";
        }

        StopAllCoroutines();
        _maxDefaultSencteces = 1;
        StartCoroutine(TypeSentence(sentence));
    }

    public void DisplayNextSentence()
    {
        if (!_isSentenceFinished)
        {
            _skipDialogue = true;
            return;
        }

        if ((_sentences.Count == 0 || _maxDefaultSencteces == 1) && _isSentenceFinished)
        {
            if (_dialogueContainedQuestion)
            {
                ManageConversation(null, _dialogueContainedQuestion);
                return;
            }

            ManageConversation(null, null);
            return;
        }

        string sentence = _sentences.Dequeue();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        _dialogueTextbox.text = "";
        _isSentenceFinished = false;

        foreach (char letter in sentence.ToCharArray())
        {
            if (!_skipDialogue)
            {
                _dialogueTextbox.text += letter;
                yield return new WaitForSeconds(_typeSpeed);
            }
        }

        _dialogueTextbox.text = sentence;

        _skipDialogue = false;
        _isSentenceFinished = true;
    }
    #endregion

    #region Question
    private void AskQuestion(Question question)
    {
        StopAllCoroutines();
        StartCoroutine(TypeSentence(question.Text.ToString()));
        _continueButton.gameObject.SetActive(false);

        for (int i = 0; i < question.Choices.Length; i++)
        {
            Choice choice = question.Choices[i];

            //This choice has a clue
            if (choice.ClueToUnlock)
            {
                //The player has this clue, && SaveHandler.Instance.DoesPlayerHaveClue(choice.ClueToUnlock.Name)
                if (SaveHandler.Instance.DoesPlayerHaveClue(choice.ClueToUnlock.Name))//SaveHandler check should be here, this is for test purposes
                {
                    //A variable of question.Choices[index] can not be made because a Choice is a value type thus it remains unchanged outside this loop.
                    //So i change it directly from inside the collection
                    if (question.Choices[choice.ChoiceIndexToHide].PropertiesToChangeDuringRuntime == null)
                    {
                        question.Choices[choice.ChoiceIndexToHide].PropertiesToChangeDuringRuntime = new Choice.ChoicePropertiesToChangeDuringRuntime()
                        {
                            Hide = true
                        };
                    }
                    else
                    {
                        question.Choices[choice.ChoiceIndexToHide].PropertiesToChangeDuringRuntime.Hide = true;
                    }
                }
                else
                {
                    //A variable of question.Choices[index] can not be made because a Choice is a value type thus it remains unchanged outside this loop.
                    //So i change it directly from inside the collection
                    if (question.Choices[i].PropertiesToChangeDuringRuntime == null)
                    {
                        question.Choices[i].PropertiesToChangeDuringRuntime = new Choice.ChoicePropertiesToChangeDuringRuntime()
                        {
                            Hide = true
                        };
                    }
                    else
                    {
                        question.Choices[i].PropertiesToChangeDuringRuntime.Hide = true;
                    }
                }
            }
        }


        foreach (Choice choice in question.Choices)
        {
            //This choice is hiding, do not add it to the queue and continue from the next iteration
            if (choice.PropertiesToChangeDuringRuntime != null && choice.PropertiesToChangeDuringRuntime.Hide)
            {
                continue;
            }

            Button choiceButton = Instantiate(_buttonPrefab, Vector3.zero, Quaternion.identity);
            choiceButton.transform.SetParent(_questionPool.transform, false);
            choiceButton.GetComponentInChildren<Text>().text = choice.Text.ToString();
            choiceButton.onClick.AddListener(() =>
            {
                if (_sceneLogger && (_sceneLogger.SceneInfo.Contains(SceneInfoType.ConversationChoicesPerConversation) || 
                    _sceneLogger.SceneInfo.Contains(SceneInfoType.All)))
                {
                    _conversationLog.Name = ConversationTarget.name;
                     _conversationLog.LogDetails.Add(
                        new Log.Entry(choice.Text, (DateTime.UtcNow - _sceneLogger.SceneLog.StartingTime).TotalSeconds)
                        );
                }
                ManageConversation(choice.Dialogue, choice.Question);
            });

            //If boolia is possesing the wrong NPC disable certain choiceButtons
            //If CharacterUnlocksChoice is 0 enable all choiceButtons
            if ((_currentPossedEntity && !choice.CharacterUnlocksChoice.Contains(_currentPossedEntity.CharacterName) && choice.CharacterUnlocksChoice.Count != 0) || 
                (choice.ClueToUnlock && !SaveHandler.Instance.DoesPlayerHaveClue(choice.ClueToUnlock.Name)))
            {
                choiceButton.interactable = false;
            }

            _choiceButtons.Enqueue(choiceButton);
        }
    }

    private void ResetQuestions()
    {
        //Reset all buttons to orignial state if not used or there is a next question
        foreach(Button button in _choiceButtons)
        {
            if (button)
            {
                Destroy(button.gameObject);
            }
        }
        _dialogueContainedQuestion = null;
    }
    #endregion
    private string EnumValueToString(CharacterType character)
    {
        string newValue = Regex.Replace(character.ToString(), "([a-z])([A-Z])", "$1 $2");

        return newValue;
    }
}
