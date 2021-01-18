using Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class CrimeSceneStateHandler : MonoBehaviour
{
    private void Awake()
    {
        if (CheckIfPlayerHasAllClues())
        {
            GameManager.PlayerHasAllClues = true;
        }

        if (CheckIfPlayerInEndingState())
        {
            GameManager.PlayerIsInEndState = true;
            LoadEndingStateGameObjects();
        } else
        {
            GameObject gameObject = GameObject.Find("Cloudportal");
            if (gameObject)
            {
                gameObject.SetActive(false);
            }
        }
    }

    public bool CheckIfPlayerInEndingState()
    {
        if (SaveHandler.Instance.GetPropertyValueFromUniqueKey("PlayerIsInEndState", "bool", out bool isPlayerIsInEndState))
        {
            return isPlayerIsInEndState;
        }
        return false;
    }

    public bool CheckIfPlayerHasAllClues()
    {
        if (SaveHandler.Instance.GetPropertyValueFromUniqueKey("PlayerHasAllClues", "bool", out bool hasPlayerAllClues))
        {
            return hasPlayerAllClues;
        }
        return false;
    }

    public void LoadEndingStateGameObjects()
    {
        GameObject policeMan = GameObject.Find("BlackPoliceMan");
        GameObject burt = GameObject.Find("Burt");
        GameObject bort = GameObject.Find("Bort");
        GameObject emmie = GameObject.Find("CrimeSceneEmmie Variant");
        GameObject marketStalls = GameObject.Find("MarketStalls");
        GameObject cloudportal = GameObject.Find("Cloudportal");
        GameObject player = GameObject.Find("PlayerV2");

        SetGameObjectTransform(policeMan);
        SetGameObjectTransform(burt);
        SetGameObjectTransform(bort);
        SetGameObjectTransform(emmie);
        SetGameObjectTransform(player);
        LoadObjectVisibility(marketStalls);
        LoadObjectVisibility(cloudportal);
        SetNavMeshAgent(policeMan);
        SetNavMeshAgent(emmie);
    }

    public void SaveEndingStateGameObjects()
    {
        //TODO: Save Sally
        GameObject police = GameObject.Find("BlackPoliceMan");
        GameObject burt = GameObject.Find("Burt");
        GameObject bort = GameObject.Find("Bort");
        GameObject emmie = GameObject.Find("CrimeSceneEmmie Variant");
        GameObject marketStalls = GameObject.Find("MarketStalls");
        GameObject cloudportal = GameObject.Find("Cloudportal");
        GameObject player = GameObject.Find("PlayerV2");

        SaveGameObjectPositionAndRotation(police);
        SaveGameObjectPositionAndRotation(burt);
        SaveGameObjectPositionAndRotation(bort);
        SaveGameObjectPositionAndRotation(emmie);
        SaveGameObjectPositionAndRotation(player);

        SetObjectVisibility(marketStalls, false);
        SetObjectVisibility(cloudportal, true);

        SaveNavMeshAgentState(police, false);
        SaveNavMeshAgentState(emmie, false);
    }

    public void PlayerEnteredEndState()
    {
        SaveHandler handler = SaveHandler.Instance;
        handler.SaveGameProperty("PlayerIsInEndState", "bool", true);
        GameManager.PlayerIsInEndState = true;
    }

    private void SetGameObjectTransform(GameObject gameObject)
    {
        if (gameObject)
        {

            SaveHandler.Instance.GetPropertyValueFromUniqueKey<float>(gameObject.name, "positionX", out float posX);
            SaveHandler.Instance.GetPropertyValueFromUniqueKey<float>(gameObject.name, "positionY", out float posY);
            SaveHandler.Instance.GetPropertyValueFromUniqueKey<float>(gameObject.name, "positionZ", out float posZ);

            SaveHandler.Instance.GetPropertyValueFromUniqueKey<float>(gameObject.name, "rotationX", out float rotX);
            SaveHandler.Instance.GetPropertyValueFromUniqueKey<float>(gameObject.name, "rotationY", out float rotY);
            SaveHandler.Instance.GetPropertyValueFromUniqueKey<float>(gameObject.name, "rotationZ", out float rotZ);

            Vector3 position = new Vector3(posX, posY, posZ);
            Vector3 rotation = new Vector3(rotX, rotY, rotZ);

            gameObject.transform.position = position;
            gameObject.transform.eulerAngles = rotation;

        }
    }

    private void SaveGameObjectPositionAndRotation(GameObject gameObject)
    {
        if (gameObject)
        {
            float posX = gameObject.transform.position.x;
            float posY = gameObject.transform.position.y;
            float posZ = gameObject.transform.position.z;
            float rotX = gameObject.transform.eulerAngles.x;
            float rotY = gameObject.transform.eulerAngles.y;
            float rotZ = gameObject.transform.eulerAngles.z;

            SaveHandler.Instance.SaveGameProperty(gameObject.name, "positionX", posX);
            SaveHandler.Instance.SaveGameProperty(gameObject.name, "positionY", posY);
            SaveHandler.Instance.SaveGameProperty(gameObject.name, "positionZ", posZ);
            SaveHandler.Instance.SaveGameProperty(gameObject.name, "rotationX", rotX);
            SaveHandler.Instance.SaveGameProperty(gameObject.name, "rotationY", rotY);
            SaveHandler.Instance.SaveGameProperty(gameObject.name, "rotationZ", rotZ);
        }
    }

    private void SetObjectVisibility(GameObject gameObject, bool shouldBeVisible)
    {
        if (gameObject)
        {
            SaveHandler.Instance.SaveGameProperty(gameObject.name, "visible", shouldBeVisible);
        }
    }

    private void LoadObjectVisibility(GameObject gameObject)
    {
        if (gameObject)
        {
            SaveHandler.Instance.GetPropertyValueFromUniqueKey<bool>(gameObject.name, "visible", out bool shouldBeVisible);
            gameObject.SetActive(shouldBeVisible);
        }
    }

    private void SetNavMeshAgent(GameObject gameObject)
    {
        if (gameObject)
        {
            NavMeshAgent agent = gameObject.GetComponent<NavMeshAgent>();
            if (agent)
            {
                if(SaveHandler.Instance.GetPropertyValueFromUniqueKey<bool>(gameObject.name, "navmesh", out bool navmesh))
                {
                    agent.enabled = navmesh;
                }
            }
        }
    }

    private void SaveNavMeshAgentState(GameObject gameObject, bool shouldBeEnabled)
    {
        SaveHandler.Instance.SaveGameProperty(gameObject.name, "navmesh", shouldBeEnabled);
    }

    private void TownCanTalkToBoolia()
    {
        GameObject police = GameObject.Find("BlackPoliceMan");
        GameObject burt = GameObject.Find("Burt");
        GameObject bort = GameObject.Find("Bort");

        BaseEntity policeEntity = police.GetComponent<BaseEntity>();
        if (policeEntity)
        {
            policeEntity.CanTalkToBoolia = true;
            // TODO: Set dialogue;
        }

        BaseEntity burtEntity = burt.GetComponent<BaseEntity>();
        if (burtEntity)
        {
            burtEntity.CanTalkToBoolia = true;
            // TODO: Set dialogue;
        }

        BaseEntity bortEntity = bort.GetComponent<BaseEntity>();
        if (bortEntity)
        {
            bortEntity.CanTalkToBoolia = true;
            // TODO: Set dialogue;
        }
    }
}
