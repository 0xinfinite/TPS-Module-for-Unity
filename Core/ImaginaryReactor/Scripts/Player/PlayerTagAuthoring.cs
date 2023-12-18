
using Unity.Entities;
using UnityEngine;

namespace ImaginaryReactor
{
    public class PlayerTagAuthoring : MonoBehaviour
    {
        public class Baker : Baker<PlayerTagAuthoring>
        {
            public override void Bake(PlayerTagAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerTag());
            }
        }
    }
}