using Enums;
using System;
using System.Collections.Generic;
using Entities;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class VillagerBehaviour : BaseEntity
{
    void Awake()
    {
        InitBaseEntity();

        ScaredOfEntities = new Dictionary<CharacterType, float>()
        {
            [CharacterType.Rat] = 3f,
        };
    }

    private void LateUpdate()
    {
        if ((CharacterName == CharacterType.Burt || CharacterName == CharacterType.Bort) 
            && EmotionalState == EmotionalState.Fainted 
            && SceneManager.GetActiveScene().name == "CemeteryScene_1")
        {
            AchievementHandler.Instance.AwardAchievement(SteamAchievements.ACH_LEFTOVER_COFFINS);
        }
    }

    public override void MoveEntityInDirection(Vector3 direction)
    {
        if (!ConversationManager.HasConversationStarted) base.MoveEntityInDirection(direction);
    }

    public override void UseFirstAbility()
    {
        //TODO Villager first ability.
    }
}
