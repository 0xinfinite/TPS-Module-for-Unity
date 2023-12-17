using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ShooterSyncObject : ModelSyncObject
{
    public LineRenderer lineRenderer;

    // Start is called before the first frame update

    //public bool checkRootAnim;

    public override void Line(float3 startPos, float3 endPos)
    {
        if(lineRenderer != null) {
            lineRenderer.SetPositions(new Vector3[3] { startPos, Vector3.Lerp(startPos, endPos, 0.1f) ,endPos });

            if(animator != null)
            {
                animator.Play("LineBlinking");
            }
        }
    }

    //private void LateUpdate()
    //{
    //    //if(checkRootAnim)
    //    //{
    //    //    Debug.Log(animator.hasRootMotion); 
    //    //}
    //}

    //private void OnAnimatorMove()
    //{

    //    //if (checkRootAnim)
    //    //{ Debug.Log(animator.velocity); }
    //    //animator.rootPosition = 
    //}

}
