using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheatHandler : MonoBehaviour
{
    
    public static Dictionary<String, Vector3> SceneEndPositions = new Dictionary<string, Vector3>
    {
        {"PreTutorialScene", new Vector3(1.5f, 15f, 170f)},
        {"CemeteryScene_1", new Vector3(-19.5f, 1.5f, 65f)},
        {"CemeteryScene_2", new Vector3(-19.5f, 1.5f, 65f)}
    };
    public static GameObject Player = FindObjectOfType<PlayerBehaviour>().gameObject;
    public static PossessionBehaviour PossessionBehaviour = Player.GetComponent<PossessionBehaviour>();

    public static void AddSelectedClue()
    {
        //check what clue is selected 
        //Addclue
    }

    public static void TeleportToEndOfLevel()
    {
        if (!Player || !PossessionBehaviour)
        {
            Player = FindObjectOfType<PlayerBehaviour>().gameObject;
            PossessionBehaviour = Player.GetComponent<PossessionBehaviour>();
        }
        if (ConversationManager.HasConversationStarted || PossessionBehaviour.IsPossessing) return;
        string levelName = SceneManager.GetActiveScene().name;
        if (!SceneEndPositions.ContainsKey(levelName)) return;
        Vector3 endOfLevel = SceneEndPositions[levelName];
        Player.transform.position = endOfLevel;
    }
}