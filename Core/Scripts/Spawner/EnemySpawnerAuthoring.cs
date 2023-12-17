using Unity.Entities;
using UnityEngine;

public class EnemySpawnerAuthoring : MonoBehaviour
{
    public int ID;
    //public GameObject orbitCamera;
    ////private Entity orbitCameraEnity;
    //public GameObject laserPointer;
    ////private Entity headCamEnity;
    //public GameObject aiObject;
    //private Entity aiObjectEnity;
    //public GameObject hitbox;
    //private Entity hitboxEntity;
    public GameObject character;
    //private Entity characterEnity;

    public class Baker : Baker<EnemySpawnerAuthoring>
    {
        public override void Bake(EnemySpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new EnemySpawner()
            {
                ID = authoring.ID,
            //    OrbitCameraEnity = GetEntity(authoring.orbitCamera, TransformUsageFlags.Dynamic),
            //LaserPointerEnity = GetEntity(authoring.laserPointer, TransformUsageFlags.Dynamic),
            ////HeadCamEnity = GetEntity(authoring.headCam, TransformUsageFlags.Dynamic),
            //AiObjectEnity = GetEntity(authoring.aiObject, TransformUsageFlags.Dynamic),
            //HitboxEntity = GetEntity(authoring.hitbox, TransformUsageFlags.Dynamic),
            //HitboxParentEntity = GetEntity(authoring.) ,
            CharacterEnity = GetEntity(authoring.character, TransformUsageFlags.Dynamic)
        });

            
        }
    }

}

public struct EnemySpawner : IComponentData
{
    public int ID;
    //public Entity OrbitCameraEnity;
    //public Entity LaserPointerEnity;
    //public Entity AiObjectEnity;
    //public Entity HitboxEntity;
    public Entity CharacterEnity;
}