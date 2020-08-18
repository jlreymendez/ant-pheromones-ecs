using Unity.Entities;
using UnityEngine;

namespace AntPheromones.Ants
{
    [GenerateAuthoringComponent]
    public struct Excitement : IComponentData
    {
        [HideInInspector] public float Value;
        public float CarryingValue;
        public float EmptyValue;
    }
}