using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace ImaginaryReactor
{
    public class HealthAuthoring : MonoBehaviour
    {
        public float InitHealth = 200f;
        public GameObject[] myHitboxList;

        public class Baker : Baker<HealthAuthoring>
        {
            public override void Bake(HealthAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Health() { RemainHealth = authoring.InitHealth });
                DynamicBuffer<StackedEnergy> energyBuffer = AddBuffer<StackedEnergy>();

                DynamicBuffer<IgnoreHitboxData> ignoreBuffer = AddBuffer<IgnoreHitboxData>(entity);
                for (int i = 0; i < authoring.myHitboxList.Length; i++)
                {
                    Entity ignoreEntity = GetEntity(authoring.myHitboxList[i], TransformUsageFlags.Dynamic);
                    //UnityEngine.Debug.Log(ignoreEntityID+" have to ignore");
                    ignoreBuffer.Add(new IgnoreHitboxData { hitboxEntity = ignoreEntity });
                }
            }
        }
    }
}