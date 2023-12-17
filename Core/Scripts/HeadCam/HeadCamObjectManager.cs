using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadCamObjectManager : MonoBehaviour
{
    public static HeadCamObjectManager instance;

    public Dictionary<int, HeadCamObject> headCamDict;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this; 
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        headCamDict = new Dictionary<int, HeadCamObject>();
        //headCamList = new List<HeadCamObject>();
    }

}
