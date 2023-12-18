using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics.Authoring;
using UnityEngine;

namespace ImaginaryReactor
{
    public class ComplexHitboxAuthoring : MonoBehaviour
    {
        public GameObject OwnerGO;
        public CustomPhysicsMaterialTags IFF_Key;
        public int ID;

        public class Baker : Baker<ComplexHitboxAuthoring>
        {

            public override void Bake(ComplexHitboxAuthoring authoring)
            {

                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Hitbox()
                {
                    Owner = GetEntity(authoring.OwnerGO, TransformUsageFlags.Dynamic),
                    Center = HitboxAuthoring.GetCenter(authoring.transform),
                    IFF_Key = authoring.IFF_Key.Value
                    //BoundSize = boundSize
                });
                AddComponent(entity, new ColliderSyncComponent()
                {
                    ID = authoring.ID,
                    Owner = GetEntity(authoring.OwnerGO, TransformUsageFlags.Dynamic)
                });
            }
        }
    }

    public struct ColliderSyncComponent : IComponentData
    {
        public int ID;
        public Entity Owner;
    }
}