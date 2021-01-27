using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SirBoonkleSpawnLocationBehaviour : MonoBehaviour
{
    public Transform Spawnpoint;
    public int Index;

    private SirBoonkleBehaviour _sirBoonkleBehaviour;

    private void Awake()
    {
        _sirBoonkleBehaviour = GameObject.Find("SirBoonkle").GetComponentInChildren<SirBoonkleBehaviour>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerBehaviour player = other.GetComponent<PlayerBehaviour>();
        if (GameObject.Find("SirBoonkle"))
        {
            _sirBoonkleBehaviour = GameObject.Find("SirBoonkle").GetComponentInChildren<SirBoonkleBehaviour>();
        }
        
        if (player && _sirBoonkleBehaviour)
        {
            _sirBoonkleBehaviour.SpawnToNewLocation(Spawnpoint, Index);
        }
    }

    public void SpawnBoonkleOnNewLocation()
    {
        if (_sirBoonkleBehaviour)
        {
            _sirBoonkleBehaviour.SpawnToNewLocation(Spawnpoint, Index);
        }
    }

    public void DespawnBoonkle()
    {
        _sirBoonkleBehaviour.transform.parent.gameObject.SetActive(false);
    }
}
