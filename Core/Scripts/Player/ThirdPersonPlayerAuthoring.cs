using UnityEngine;
using Unity.Entities;
using Unity.Physics;

[DisallowMultipleComponent]
public class ThirdPersonPlayerAuthoring : MonoBehaviour
{
    public GameObject ControlledCharacter;
    public GameObject ControlledCamera;
    //public GameObject ControlledWeapon;
    //public bool DestroyWeapon;

    public class Baker : Baker<ThirdPersonPlayerAuthoring>
    {
        public override void Bake(ThirdPersonPlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new ThirdPersonPlayer
            {
                ControlledCharacter = GetEntity(authoring.ControlledCharacter, TransformUsageFlags.Dynamic),
                ControlledCamera = GetEntity(authoring.ControlledCamera, TransformUsageFlags.Dynamic),
                //InitWeapon = GetEntity(authoring.ControlledWeapon, TransformUsageFlags.Dynamic),
            });
            AddComponent(entity, new ThirdPersonPlayerInputs());
            AddComponent(entity, new PlayerTag() );
            
        }
    }
}