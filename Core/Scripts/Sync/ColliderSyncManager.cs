using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ColliderSyncManager : MonoBehaviour
{
    public static ColliderSyncManager instance;

    public Dictionary<int, ColliderSyncObject> colliderDict;

    //public void SetLocalPosition(int id, float3 _position)
    //{
    //    if (colliderDict.ContainsKey(id))
    //    {
    //        colliderDict[id].transform.position = _position;
    //    }
    //}
    //public void SetLocalRotation(int id, quaternion _rotation)
    //{
    //    if (colliderDict.ContainsKey(id))
    //    {
    //        colliderDict[id].transform.rotation = _rotation;
    //    }
    //}

    //public void SetBool(int id, string paramName, bool value) {
    //    if (colliderDict.ContainsKey(id) && colliderDict[id].animator)
    //    {
    //        colliderDict[id].animator.SetBool(paramName, value);
    //    }
    //}

    //public void SetFloat(int id, string paramName, float value)
    //{
    //    if (colliderDict.ContainsKey(id) && colliderDict[id].animator)
    //    {
    //        colliderDict[id].animator.SetFloat(paramName, value);
    //    }
    //}

    public void AddCollider(int id, ColliderSyncObject obj)
    {
        if (colliderDict.ContainsKey(id))
        {
            if (colliderDict[id] != obj)
            {
                colliderDict[id] = obj;
            }
        }
        else
        {
            colliderDict.Add(id, obj);
        }
    }

    public void RemoveCollider(int id, ColliderSyncObject obj)
    {
        if (colliderDict.ContainsKey(id))
        {
            if (colliderDict[id] == obj)
            {
                colliderDict.Remove(id);
            }
        }
    }

    private void Awake()
    {
        if (instance == null)
        { instance = this; }
        else
        {
            Destroy(gameObject);
        }

        colliderDict = new Dictionary<int, ColliderSyncObject>(); 
    }

}
