using Unity.Entities;

namespace AntPheromones.Obstacles
{
    [GenerateAuthoringComponent]
    public struct Radius : IComponentData
    {
        public float Value;
    }
}