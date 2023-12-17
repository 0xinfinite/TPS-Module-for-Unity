using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ModelSyncObject : MonoBehaviour, ISync
{
    public int ID;

    public Animator animator;

    public bool alwaysTrackingSight;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        if(animator == null)
        {
            animator = GetComponent<Animator>();
        }
        yield return null;
        while (!ModelSyncManager.instance)
        {
            yield return new WaitForEndOfFrame();
        }
        ModelSyncManager.instance.AddModel(ID, this);
    }

    public virtual void Line(float3 startPos, float3 endPos)
    {

    }

    //private void OnEnable()
    //{
        
    //}

    private void OnDestroy()
    {
        if (ModelSyncManager.instance)
        {
            ModelSyncManager.instance.RemoveModel(ID, this);
        }
    }

    

    public void ClearThisCharacterBody()
    {
        if (ModelSyncManager.instance)
        {
            ModelSyncManager.instance.ClearingCharacterBody(ID);
        }
    }

    // Update is called once per frame
    //void LateUpdate()
    //{
        
    //}
}
