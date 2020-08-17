using Unity.Entities;

namespace AntPheromones.Ants
{
    [GenerateAuthoringComponent]
    public struct Excitement : IComponentData
    {
        public float Value;
    }
}