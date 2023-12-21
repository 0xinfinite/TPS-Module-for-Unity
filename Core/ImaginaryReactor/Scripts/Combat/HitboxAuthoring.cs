using Unity.Entities;
using Unity.Physics.Authoring;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

namespace ImaginaryReactor
{
    public class HitboxAuthoring : MonoBehaviour
    {
        public GameObject OwnerGO;
        //public Transform parent;
        //public bool detachFromParent;
        public CustomPhysicsMaterialTags IFF_Key;
        //public float BoundSize = 1f;
        //public Vector3 localPos;
        //public Quaternion localRot;
        //public Vector3 localScl;
        public static float3 GetCenter(Transform transform)
        {
            if (transform.TryGetComponent(out BoxCollider box))
            {
                return box.center;
            }
            if (transform.TryGetComponent(out SphereCollider sphere))
            {
                return sphere.center;
            }
            if (transform.TryGetComponent(out CapsuleCollider capsule))
            {
                return capsule.center;
            }
            if (transform.TryGetComponent(out PhysicsShapeAuthoring ps))
            {
                switch (ps.ShapeType)
                {
                    case ShapeType.Box:
                        return ps.GetBoxProperties().Center;
                    case ShapeType.Capsule:
                        return ps.GetCapsuleProperties().Center;
                    case ShapeType.Sphere:
                        return ps.GetSphereProperties(out quaternion q).Center;
                    case ShapeType.Cylinder:
                        return ps.GetCylinderProperties().Center;
                    case ShapeType.Plane:
                        ps.GetPlaneProperties(out float3 c, out float2 size, out q);
                        return c;

                }
            }
            return float3.zero;
        }

        public class Baker : Baker<HitboxAuthoring>
        {



            public override void Bake(HitboxAuthoring authoring)
            {
                //if (authoring.parent != null)
                //{
                //    authoring.localPos = authoring.transform.parent.localPosition;
                //    authoring.localRot = authoring.transform.parent.localRotation;
                //    authoring.localScl = authoring.transform.parent.localScale;
                //    authoring.parent = authoring.transform.parent;
                //if (authoring.detachFromParent)
                //{ authoring.transform.parent = null; }
                //}

                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                //float boundSize = authoring.BoundSize;

                //if (authoring.transform.TryGetComponent(out Collider collider))
                //{
                //    boundSize = collider.bounds.size.sqrMagnitude;
                //}
                //else if(authoring.transform.TryGetComponent(out PhysicsShapeAuthoring shapeAuthoring))
                //{
                //    switch (shapeAuthoring.ShapeType)
                //    {
                //        case ShapeType.Box:
                //            boundSize = math.lengthsq(shapeAuthoring.GetBoxProperties().Size);
                //            break;
                //        case ShapeType.Capsule:
                //        case ShapeType.Cylinder:
                //            boundSize = math.lengthsq(math.max(shapeAuthoring.GetCylinderProperties().Height, shapeAuthoring.GetCylinderProperties().Radius));
                //            break;
                //        case ShapeType.Sphere:
                //            boundSize = shapeAuthoring.GetSphereProperties(out quaternion quat).Radius;
                //            break;
                //        case ShapeType.Plane:
                //            shapeAuthoring.GetPlaneProperties(out float3 center, out float2 size, out quat);
                //            boundSize = math.lengthsq(new float3(size.x,0,size.y)+center);
                //            break;
                //        case ShapeType.ConvexHull:
                //            break;
                //        case ShapeType.Mesh:
                //            //boundSize = shapeAuthoring.GetMeshProperties()
                //            break;
                //    }
                //}

                AddComponent(entity, new Hitbox()
                {
                    Owner = GetEntity(authoring.OwnerGO, TransformUsageFlags.Dynamic),
                    Center = GetCenter(authoring.transform),
                    IFF_Key = authoring.IFF_Key.Value,
                    DamageMultiply = 1f,
                    IsCritical = false
                    //BoundSize = boundSize
                });
                //if (authoring.parent != null)
                //{
                //    AddComponent(entity, new SeperatedChild()
                //    {
                //        Parent = GetEntity(authoring.parent, TransformUsageFlags.Dynamic),
                //        LocalPosition = authoring.parent.InverseTransformPoint(authoring.transform.position),
                //        LocalForward = authoring.parent.InverseTransformDirection(authoring.transform.forward),
                //        LocalUp = authoring.parent.InverseTransformDirection(authoring.transform.up),
                //        LocalRotation = Quaternion.LookRotation(authoring.parent.InverseTransformDirection(authoring.transform.forward), authoring.parent.InverseTransformDirection(authoring.transform.up))
                //    });
                //}

                //AddComponent(entity, new Parent() { Value = GetEntity(authoring.parent, TransformUsageFlags.Dynamic) });
                //SetComponent(entity, new LocalTransform() {
                //    Position = authoring.parent.InverseTransformPoint(authoring.transform.position), //authoring.localPos, 
                //    Rotation = Quaternion.LookRotation(authoring.parent.InverseTransformDirection(authoring.transform.forward), authoring.parent.InverseTransformDirection(authoring.transform.up)) ,//authoring.localRot, 
                //    Scale = 1//authoring.localScl.sqrMagnitude
                //});
            }
        }
    }
}