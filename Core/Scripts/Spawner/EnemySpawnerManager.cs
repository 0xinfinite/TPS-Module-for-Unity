using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerManager : MonoBehaviour
{
    public static EnemySpawnerManager instance;

    public bool spawnNow;

    public int targetID;

    private void Awake()
    {
        if (instance == null || instance.gameObject == null)
        {
            instance = this; 
      //      DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
}
