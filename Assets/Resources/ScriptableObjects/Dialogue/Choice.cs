using Enums;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Choice
{
    [TextArea(2, 5)]
    public string Text;
    public List<CharacterType> CharacterUnlocksChoice;
    public Dialogue Dialogue;
    public Question Question;

    /// <summary>
    /// Choice to hide when the player contains the 'ClueToUnlock'
    /// </summary>
    public int ChoiceIndexToHide;

    /// <summary>
    /// The clue needed to unlock this clue during a question.
    /// </summary>
    public Clue ClueToUnlock;

    public ChoicePropertiesToChangeDuringRuntime PropertiesToChangeDuringRuntime;

    public class ChoicePropertiesToChangeDuringRuntime
    {
        /// <summary>
        /// Exclude this choice from the current question
        /// </summary>
        public bool Hide { get; set; }
    }
}
