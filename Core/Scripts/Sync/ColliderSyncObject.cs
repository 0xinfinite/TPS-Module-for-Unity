using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderSyncObject : MonoBehaviour
{
    public int ID;
    //public Collider collider;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        while (!ColliderSyncManager.instance)
        {
            yield return new WaitForEndOfFrame();
        }
        ColliderSyncManager.instance.AddCollider(ID, this);
    }

    private void OnDestroy()
    {
        if (ColliderSyncManager.instance)
        {
            ColliderSyncManager.instance.RemoveCollider(ID, this);
        }
    }


}
