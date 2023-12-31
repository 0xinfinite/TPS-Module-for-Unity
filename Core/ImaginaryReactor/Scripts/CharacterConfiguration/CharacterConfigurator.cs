using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Mathematics;
using Unity.Physics.Authoring;
//using Unity.VisualScripting;
using UnityEngine;

namespace ImaginaryReactor
{
    [ExecuteInEditMode]
    public class CharacterConfigurator : MonoBehaviour
    {
        [SerializeField] private GameObject characterPrefab;
        [SerializeField] private Collider[] colliders;
        [SerializeField] private bool fixMode;
        [SerializeField] private GameObject targetGOToFixCollider;
        public void SettingHitboxes()
        {

            if (fixMode)
            {
                if(targetGOToFixCollider==null)
                {
                    Debug.LogWarning("You must assign Target GO To Fix Collider to proceed fixing colliders.");
                }

                for(int i = 0; i< targetGOToFixCollider.transform.childCount; i++)
                {
                    var targetChild = targetGOToFixCollider.transform.GetChild(i);
                    if(targetChild.TryGetComponent(out ComplexHitboxAuthoring authoring))
                    {
                        Debug.Log("Get Authoring : "+ targetChild.gameObject.name);
                        if (ExtendedChildGrabber.TryGetSameNameTransformFromChildrenRecursively(transform, targetChild.gameObject.name,out Transform selectedChild))
                        {
                            Debug.Log("Find Same Name Object");
                            if (selectedChild.TryGetComponent(out ColliderSyncObject syncObj))
                            {
                                Debug.Log("Sync ID");
                                syncObj.ID = authoring.ID;
                            }
                        }
                    }
                }
                return;
            }

            if (characterPrefab == null)
            {
                Debug.LogWarning("You must assign Character Prefab to proceed configuration.");
                return;
            }


            GameObject newCharacter = Instantiate(characterPrefab, null); // = new GameObject("Third Person Character");
            newCharacter.transform.position = transform.position;
            newCharacter.transform.rotation = transform.rotation;

            List<GameObject> newColliders = new List<GameObject>();
            for (int i = 0; i < colliders.Length; i++)
            {
                var col = colliders[i];

                GameObject colObj;
                bool colliderAuthoringExist = false;
                if (ExtendedChildGrabber.TryGetSameNameTransformFromChildren(newCharacter.transform, col.gameObject.name, out Transform selectedChild))
                {
                    colObj = selectedChild.gameObject;
                    colliderAuthoringExist = true;
                }
                else
                {
                    colObj = new GameObject(col.name);
                }
                newColliders.Add(colObj);
                colObj.transform.parent = newCharacter.transform;
                colObj.transform.position = col.transform.position;
                colObj.transform.rotation = col.transform.rotation;
                colObj.transform.localScale = col.transform.parent.TransformVector(col.transform.localScale);

                PhysicsShapeAuthoring shape;
                if (colliderAuthoringExist)
                {
                    shape = colObj.GetComponent<PhysicsShapeAuthoring>();
                }
                else
                {
                    shape = colObj.AddComponent<PhysicsShapeAuthoring>();
                }
                switch (col.GetType().ToString())
                {
                    case "UnityEngine.BoxCollider":
                        //var box = colObj.AddComponent<BoxCollider>();
                        var origBox = col.GetComponent<BoxCollider>();
                        //box.center = origBox.center;
                        //box.size = origBox.size;
                        //box.isTrigger = origBox.isTrigger;
                       
                        shape.SetBox(new Unity.Physics.BoxGeometry()
                        {
                            Center = origBox.center,
                            Size = origBox.size,
                            BevelRadius = 0,
                            Orientation = quaternion.identity
                        }); //SetBakedBoxSize(origBox.size, 0);
                            //shape.
                        shape.PreventMergedOnParent = true;
                        shape.CollisionResponse = Unity.Physics.CollisionResponsePolicy.CollideRaiseCollisionEvents;
                        break;
                    case "UnityEngine.SphereCollider":
                        //var sphere = colObj.AddComponent<SphereCollider>();
                        var origSphere = col.GetComponent<SphereCollider>();
                        //sphere.radius = origSphere.radius;
                        //sphere.center = origSphere.center;
                        //sphere.isTrigger = origSphere.isTrigger;
                        shape.SetSphere(new Unity.Physics.SphereGeometry()
                        {
                            Center = origSphere.center,
                            Radius = origSphere.radius
                        }, quaternion.identity);
                        shape.PreventMergedOnParent = true;
                        shape.CollisionResponse = Unity.Physics.CollisionResponsePolicy.CollideRaiseCollisionEvents;
                        break;
                    case "UnityEngine.CapsuleCollider":
                        //var capsule = colObj.AddComponent<CapsuleCollider>();
                        var origCapsule = col.GetComponent<CapsuleCollider>();
                        //capsule.radius = origCapsule.radius;
                        //capsule.center = origCapsule.center;
                        //capsule.height = origCapsule.height;
                        //capsule.isTrigger = origCapsule.isTrigger;
                        shape.SetCapsule//SetCylinder 
                            (new Unity.Physics.Authoring.CapsuleGeometryAuthoring()
                            {
                                Center = origCapsule.center,
                                Height = origCapsule.height,
                                Radius = origCapsule.radius,
                                Orientation = new Unity.Physics.Authoring.EulerAngles() { Value = new float3(origCapsule.direction == 1 ? -90 : 0, origCapsule.direction == 0 ? -90 : 0, 0) }
                                //quaternion.Euler(origCapsule.direction == 1 ? -90 : 0, origCapsule.direction == 0 ? -90 : 0, 0)
                            });
                        shape.PreventMergedOnParent = true;
                        shape.CollisionResponse = Unity.Physics.CollisionResponsePolicy.CollideRaiseCollisionEvents;
                        break;
                }


                ComplexHitboxAuthoring hitAuthoring;
                if (colliderAuthoringExist)
                {
                    hitAuthoring = colObj.GetComponent<ComplexHitboxAuthoring>();
                }
                else
                {
                    hitAuthoring = colObj.AddComponent<ComplexHitboxAuthoring>(); 
                }
                hitAuthoring.ID = col.GetInstanceID();
                hitAuthoring.OwnerGO = newCharacter;

                if (col.transform.gameObject.TryGetComponent<ColliderSyncObject>(out ColliderSyncObject syncObj))
                {
                    syncObj.ID = col.GetInstanceID();
                }
                else
                {
                    syncObj = col.transform.gameObject.AddComponent<ColliderSyncObject>();
                    syncObj.ID = col.GetInstanceID();
                    //syncObj.collider = col;
                }
            }
            //var charAuthoring = newCharacter.GetComponent<ThirdPersonCharacterAuthoring>();
            newCharacter.GetComponent<HealthAuthoring>().myHitboxList = newColliders.ToArray();

            if (TryGetComponent(out ModelSyncObject modelObj))
            {
                if(modelObj.ID == -1)
                {
                    modelObj.ID = modelObj.gameObject.GetInstanceID();
                }

                if (newCharacter.TryGetComponent(out ModelSyncAuthoring existSyncAuthoring))
                {
                    existSyncAuthoring.ID = modelObj.ID;
                }
                else
                {
                    newCharacter.AddComponent<ModelSyncAuthoring>().ID = modelObj.ID;
                }
            }



            if (ExtendedChildGrabber.TryGetComponentFromChildren(newCharacter.transform, out OrbitCameraAuthoring orbitCam,
                out Transform childTf))
            {
                orbitCam.IgnoredEntities = newColliders;
                orbitCam.IgnoredEntities.Add(newCharacter);
            }
        }



        private void OnEnable()
        {
            SettingHitboxes();

            this.enabled = false;
        }
    }

    public static class ExtendedChildGrabber
    {
        public static bool TryGetComponentFromChildren<T>(Transform transform, out T component, out Transform selectedChild)
        {
            selectedChild = null;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent<T>(out T childComponent))
                {
                    component = childComponent;
                    selectedChild = transform.GetChild(i);
                    return true;
                }
            }
            selectedChild = transform;
            return transform.TryGetComponent(out component) && false;
        }

        public static bool TryGetSameNameTransformFromChildren(Transform transform, string targetName, out Transform selectedChild)
        {
            selectedChild = null;
            for (int i = 0; i < transform.childCount; i++)
            {
                if(targetName == transform.GetChild(i).gameObject.name)
                {
                    selectedChild = transform.GetChild(i);
                    return true;
                }
            }
            selectedChild = transform;
            return false;
        }
        public static bool TryGetSameNameTransformFromChildrenRecursively(Transform transform, string targetName, out Transform selectedChild)
        {
            selectedChild = null;
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (targetName == child.gameObject.name)
                {
                    selectedChild = transform.GetChild(i);
                    return true;
                }
                else
                {
                    if (TryGetSameNameTransformFromChildrenRecursively(child, targetName, out Transform selectedChild2))
                    {
                        selectedChild = selectedChild2;
                        return true;
                    }
                }
            }
            selectedChild = transform;
            return false;
        }
    }
}