using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ThirdPersonTestingAIAuthoring : MonoBehaviour
{
    public GameObject ControlledCharacter;
    public GameObject ControlledCamera;
    //public GameObject ControlledWeapon;
    //public bool DestroyWeapon;

    public class Baker : Baker<ThirdPersonTestingAIAuthoring>
    {
        public override void Bake(ThirdPersonTestingAIAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new ThirdPersonPlayer
            {
                ControlledCharacter = GetEntity(authoring.ControlledCharacter, TransformUsageFlags.Dynamic),
                ControlledCamera = GetEntity(authoring.ControlledCamera, TransformUsageFlags.Dynamic),
                //InitWeapon = GetEntity(authoring.ControlledWeapon, TransformUsageFlags.Dynamic),
            });
            AddComponent(entity, new ThirdPersonPlayerInputs());
            AddComponent(entity, new TestAITag());

            
        }
    }
}


public struct TestAITag : IComponentData
{

}
