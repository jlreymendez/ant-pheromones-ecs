using Unity.Entities;
using Unity.Mathematics;

namespace AntPheromones.Obstacles
{
    [InternalBufferCapacity(2)]
    public struct ObstacleBucket : IBufferElementData
    {
        public float3 Position;
        public float Radius;
    }
}