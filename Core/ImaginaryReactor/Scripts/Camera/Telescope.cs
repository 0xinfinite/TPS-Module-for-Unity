using Unity.Entities;


namespace ImaginaryReactor
{
    public struct Telescope : IComponentData
    {
        public float FovWhenZoom;
        public bool FirstPerson;
        public float ZoomSpeed;
        public float ZoomProgress;
    }
}
