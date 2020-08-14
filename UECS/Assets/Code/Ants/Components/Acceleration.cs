using Unity.Entities;
using UnityEngine;

namespace AntPheromones.Ants
{
    [GenerateAuthoringComponent]
    public struct Acceleration : IComponentData
    {
        [Range(0f, 1f)] public float Value;
    }
}