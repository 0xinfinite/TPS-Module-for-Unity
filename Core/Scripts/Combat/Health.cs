using Unity.Entities;

namespace ImaginaryReactor
{
    public struct Health : IComponentData
    {
        //public Entity[] HitBoxes;
        public float RemainHealth;
    }

    public struct ClearThisCharacterBody : IComponentData
    {
    }

    //public struct IncludedHitbox : IBufferElementData
    //{
    //    public Entity HitboxEntity;
    //}
}