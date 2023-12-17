using Unity.Entities;
using UnityEngine;

public class ModelSyncAuthoring : MonoBehaviour
{
    public int ID;

    public class Baker : Baker<ModelSyncAuthoring>
    {
        public override void Bake(ModelSyncAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new ModelSyncComponent() { 
            ID = authoring.ID,
            });
        }
    }
}
