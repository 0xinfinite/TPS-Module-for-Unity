
using Unity.Entities;
using UnityEngine;

public class LaserPointerAuthoring : MonoBehaviour
{
    public GameObject OwnerGO;

    public class Baker : Baker<LaserPointerAuthoring>
    {
        public override void Bake(LaserPointerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);

            AddComponent(entity, new LaserPointer(){Owner = GetEntity(authoring.OwnerGO, TransformUsageFlags.Dynamic)});
        }
    }

}
