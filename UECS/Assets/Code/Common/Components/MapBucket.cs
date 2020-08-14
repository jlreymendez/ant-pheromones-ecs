using Unity.Entities;
using Unity.Mathematics;

namespace AntPheromones.Common
{
    public struct MapBucket : IComponentData
    {
        public int2 Position;
    }
}