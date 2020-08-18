using Unity.Entities;
using Unity.Mathematics;

namespace AntPheromones.Ants
{
    public struct Target : IComponentData
    {
        public float3 Position;
        public float Radius;
    }
}