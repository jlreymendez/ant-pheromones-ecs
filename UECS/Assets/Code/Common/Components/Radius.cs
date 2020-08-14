using Unity.Entities;

namespace AntPheromones.Common
{
    [GenerateAuthoringComponent]
    public struct Radius : IComponentData
    {
        public float Value;
    }
}