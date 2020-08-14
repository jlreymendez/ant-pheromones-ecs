using Unity.Entities;
using UnityEngine;

namespace AntPheromones.Ants
{
    [GenerateAuthoringComponent]
    public struct Steering : IComponentData
    {
        [HideInInspector] public float Angle;
        [HideInInspector] public float Delta;
        public float WanderSteering;
    }
}