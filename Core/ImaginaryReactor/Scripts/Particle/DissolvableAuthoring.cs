using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace ImaginaryReactor
{
    public class DissolvableAuthoring : MonoBehaviour
    {
        public float duration;

        public class Baker : Baker<DissolvableAuthoring>
        {
            public override void Bake(DissolvableAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Dissolvable() { RemainTime = authoring.duration });
            }
        }
    }
}