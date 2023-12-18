
using Unity.Entities;
using UnityEngine;

namespace ImaginaryReactor
{
    public class PlayerLaserPointerAuthoring : MonoBehaviour
    {
        public class Baker : Baker<PlayerLaserPointerAuthoring>
        {
            public override void Bake(PlayerLaserPointerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);

                AddComponent(entity, new PlayerLaserPointer() { });
            }
        }

    }
}