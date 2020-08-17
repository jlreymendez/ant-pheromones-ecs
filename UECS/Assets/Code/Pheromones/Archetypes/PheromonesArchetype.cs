using Unity.Entities;

namespace AntPheromones.Pheromones
{
    public static class PheromonesArchetype
    {
        public static ComponentType[] Components
        {
            get => new ComponentType[]
            {
                typeof(PheromoneTag),
                typeof(Strength),
            };
        }
    }
}