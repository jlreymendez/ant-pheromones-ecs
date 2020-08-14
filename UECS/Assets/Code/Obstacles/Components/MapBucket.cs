using Unity.Entities;
using Unity.Mathematics;

namespace AntPheromones.Obstacles
{
    public struct MapBucket : IComponentData
    {
        public int2 Position;
    }
}