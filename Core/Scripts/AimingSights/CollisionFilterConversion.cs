
//using Unity.Physics;
//using UnityEngine;
//using UnityEngine.Rendering;

//public static class CollisionFilterConversion 
//{
//    public static CollisionFilter LayerMaskToFilter(LayerMask mask)
//    {
//        CollisionFilter filter = new CollisionFilter()
//        {
//            BelongsTo = (uint)mask.value,
//            CollidesWith = (uint)mask.value
//        };
//        return filter;
//    }

//    public static CollisionFilter LayerToFilter(int layer)
//    {
//        if (layer == -1)
//        {
//            return CollisionFilter.Zero;
//        }

//        BitArray32 mask = new BitArray32();
//        mask[(uint)layer] = true;

//        CollisionFilter filter = new CollisionFilter()
//        {
//            BelongsTo = mask.Bi Bits,
//            CollidesWith = mask.Bits
//        };
//        return filter;
//    }
//}
