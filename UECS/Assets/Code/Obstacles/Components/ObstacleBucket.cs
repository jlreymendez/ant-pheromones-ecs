using Unity.Entities;
using Unity.Mathematics;

namespace AntPheromones.Obstacles
{
    public struct ObstacleBucket : IComponentData
    {
        public int2 Position;
        public bool HasWalls;
    }
}