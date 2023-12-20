
using Unity.Entities;
using UnityEngine;

namespace ImaginaryReactor
{
    public class PitcherAuthoring : MonoBehaviour
    {
        public float throwingPower = 10;
        public GameObject throwObject;
        public GameObject hand;
       public class Baker : Baker<PitcherAuthoring>
        {
            public override void Bake(PitcherAuthoring authroing)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Pitcher()
                {
                    GrabEntity = GetEntity( authroing.throwObject, TransformUsageFlags.Dynamic),
                    ThrowingPower = authroing.throwingPower,
                    Hand = GetEntity(authroing.hand, TransformUsageFlags.Dynamic)
                }) ;
            }
        }
    }
}
