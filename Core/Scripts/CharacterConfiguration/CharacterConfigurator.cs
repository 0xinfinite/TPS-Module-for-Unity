using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Mathematics;
using Unity.Physics.Authoring;
//using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class CharacterConfigurator : MonoBehaviour
{
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private Collider[] colliders;

    public void SettingHitboxes()
    {
        if(characterPrefab == null)
        {
            Debug.LogWarning("You must assign Character Prefab to proceed configuration.");
            return;
        }

        GameObject newCharacter = Instantiate(characterPrefab, null); // = new GameObject("Third Person Character");
        newCharacter.transform.position = transform.position;
        newCharacter.transform.rotation = transform.rotation;

        List<GameObject> newColliders= new List<GameObject>();
        for(int i = 0; i < colliders.Length; i++)
        {
            var col = colliders[i];

            GameObject colObj = new GameObject(col.name);
            newColliders.Add(colObj);
            colObj.transform.parent = newCharacter.transform;
            colObj.transform.position = col.transform.position;
            colObj.transform.rotation = col.transform.rotation;
            colObj.transform.localScale = col.transform.parent.TransformVector(col.transform.localScale);

            switch (col.GetType().ToString())
            {
                case "UnityEngine.BoxCollider":
                    //var box = colObj.AddComponent<BoxCollider>();
                    var origBox = col.GetComponent<BoxCollider>();
                    //box.center = origBox.center;
                    //box.size = origBox.size;
                    //box.isTrigger = origBox.isTrigger;
                    var shape = colObj.AddComponent<PhysicsShapeAuthoring>();
                    shape.SetBox(new Unity.Physics.BoxGeometry() { 
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
                    shape = colObj.AddComponent<PhysicsShapeAuthoring>();
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
                    shape = colObj.AddComponent<PhysicsShapeAuthoring>();
                    shape.SetCapsule//SetCylinder 
                        (new Unity.Physics.Authoring.CapsuleGeometryAuthoring()
                        {
                            Center = origCapsule.center,
                            Height = origCapsule.height,
                            Radius = origCapsule.radius,
                            Orientation = quaternion.Euler(origCapsule.direction == 1 ? -90 : 0, origCapsule.direction == 0 ? -90 : 0, 0)
                        }) ;
                    shape.PreventMergedOnParent = true;
                    shape.CollisionResponse = Unity.Physics.CollisionResponsePolicy.CollideRaiseCollisionEvents;
                    break;
            }
            
            
            var hitAuthoring = colObj.AddComponent<ComplexHitboxAuthoring>();
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
        
        if(TryGetComponent(out ModelSyncObject modelObj))
        {
           newCharacter.AddComponent<ModelSyncAuthoring>().ID = modelObj.ID;
        }



        if(ExtendedComponentGrabber.TryGetComponentFromChildren(newCharacter.transform, out OrbitCameraAuthoring orbitCam,
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

public static class ExtendedComponentGrabber
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
}