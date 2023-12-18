
using Unity.Entities;
using UnityEngine;

public class LaserPointerAuthoring : MonoBehaviour
{
    public GameObject OwnerGO;
    public bool isPlayer;

    public class Baker : Baker<LaserPointerAuthoring>
    {
        public override void Bake(LaserPointerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);

            if (authoring.isPlayer)
            {
                AddComponent(entity, new PlayerLaserPointer() { Owner = GetEntity(authoring.OwnerGO, TransformUsageFlags.Dynamic) });
            }
            else
            {
                AddComponent(entity, new LaserPointer() { Owner = GetEntity(authoring.OwnerGO, TransformUsageFlags.Dynamic) }); 
            }
        }
    }

}
