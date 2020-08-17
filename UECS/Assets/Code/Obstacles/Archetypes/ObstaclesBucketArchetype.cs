using AntPheromones.Common;
using Unity.Entities;

namespace AntPheromones.Obstacles
{
    public static class ObstaclesBucketArchetype
    {
        public static ComponentType[] Components
        {
            get => new ComponentType[]
            {
                ComponentType.ReadWrite<MapBucket>(),
                ComponentType.ReadWrite<ObstacleBucket>(),
            };
        }
    }
}