using Unity.Entities;
using UnityEngine;

namespace AntPheromones.Ants
{
    [GenerateAuthoringComponent]
    public struct Speed : IComponentData
    {
        [Range(0f, 1f)] public float Max;
        [HideInInspector] public float Value;
        [HideInInspector] public float Target;
    }
}
