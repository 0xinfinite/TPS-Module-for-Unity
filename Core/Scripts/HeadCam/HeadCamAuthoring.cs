using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class HeadCamAuthoring : MonoBehaviour
{
    public GameObject targetCharacter;
    public GameObject targetOrbitCamera;
    public Vector3 headCamOffset;
    public int id;

    //public HeadCam HeadCam = HeadCam.GetDefault();

    public class Baker : Baker<HeadCamAuthoring>
    {
        public override void Bake(HeadCamAuthoring authoring)
        {
            //authoring.HeadCam.PlanarForward = -math.forward();

            Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);

            AddComponent(entity, //authoring.HeadCam);
                new HeadCam()
                {
                    PlanarForward = -math.forward(),
                    FollowedCharacterEntity = GetEntity(authoring.targetCharacter, TransformUsageFlags.Dynamic),
                    FollowedCameraEntity = GetEntity(authoring.targetOrbitCamera, TransformUsageFlags.Dynamic),
                    HeadCamOffset = authoring.headCamOffset,
                    ID = authoring.id
                });
            //AddComponent(entity, new HeadCamControl());
            
        }
    }
}