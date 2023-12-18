using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics.Authoring;
using UnityEngine;
using Unity.Physics;
using Unity.Mathematics;

public class SeekerAuthoring : MonoBehaviour
{
    public GameObject BrainGO;
    public Transform Parent;
    //public float Range;
    //public float FieldOfView;
    //    public PhysicsMaterialProperties ShapeFilterProperties;
    //public PhysicsCategoryTags ShapeFilterBelongsTo;
    //public PhysicsCategoryTags ShapeFilterCollidesWith;
    public bool CheckRaycast = true;
    //public PhysicsCategoryTags ColliderFilterBelongsTo;
    //public PhysicsCategoryTags ColliderFilterCollidesWith;
    public PhysicsCategoryTags RaycastFilterBelongsTo;
    public PhysicsCategoryTags RaycastFilterCollidesWith;
    public CustomPhysicsMaterialTags sideTag;
    public CustomPhysicsMaterialTags targetSideTag;
    public float3 SeekerOffset;
    public bool GetDirection;
    public GameObject[] SeekerIgnoreGOList;

    public class Baker : Baker<SeekerAuthoring>
    {

        public override void Bake(SeekerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new Seeker() {
                BrainEntity = GetEntity(authoring.BrainGO, TransformUsageFlags.Dynamic),
                //Range = authoring.Range, FieldOfView = authoring.FieldOfView ,

                //ShapeFilter = new CollisionFilter() { BelongsTo = authoring.ShapeFilterBelongsTo.Value, CollidesWith = authoring.ShapeFilterCollidesWith.Value },
                CheckRaycast = authoring.CheckRaycast,
                //ColliderFilter = new CollisionFilter() { BelongsTo = authoring.ColliderFilterBelongsTo.Value, CollidesWith = authoring.ColliderFilterCollidesWith.Value },
                RaycastFilter = new CollisionFilter() { BelongsTo = authoring.RaycastFilterBelongsTo.Value, CollidesWith = authoring.RaycastFilterCollidesWith.Value },
                SeekerOffset = authoring.SeekerOffset,
                TargetSideKey = authoring.targetSideTag.Value,
                SideKey = authoring.sideTag.Value,
                LastKnownVector = new float3(-100,-100,-100),
                GetDirection = authoring.GetDirection,
            });
            
            DynamicBuffer<IgnoreHitboxData> ignoreBuffer = AddBuffer<IgnoreHitboxData>(entity);
            for (int i = 0; i<authoring.SeekerIgnoreGOList.Length; i++)
            {
                Entity ignoreEntity = GetEntity(authoring.SeekerIgnoreGOList[i], TransformUsageFlags.Dynamic);
                //UnityEngine.Debug.Log(ignoreEntityID+" have to ignore");
                ignoreBuffer.Add(new IgnoreHitboxData { hitboxEntity = ignoreEntity });
            }
            

            //if (authoring.Parent != null)
            //{
            //    AddComponent(entity, new SeperatedChild()
            //    {
            //        Parent = GetEntity(authoring.Parent, TransformUsageFlags.Dynamic),
            //        LocalPosition = authoring.Parent? authoring.Parent.InverseTransformPoint(authoring.transform.position): authoring.transform.localPosition,
            //        LocalForward = authoring.Parent ? authoring.Parent.InverseTransformDirection(authoring.transform.forward) : authoring.transform.localRotation * Vector3.forward ,
            //        LocalUp = authoring.Parent ? authoring.Parent.InverseTransformDirection(authoring.transform.up) : authoring.transform.localRotation * Vector3.up,
            //        LocalRotation = authoring.Parent ?  Quaternion.LookRotation(authoring.Parent.InverseTransformDirection(authoring.transform.forward), authoring.Parent.InverseTransformDirection(authoring.transform.up))
            //        : authoring.transform.localRotation
            //    });
            //}
            //AddComponent(entity, new Neural() { BrainEntity = GetEntity(authoring.BrainGO, TransformUsageFlags.Dynamic) });
        }
    }
}
