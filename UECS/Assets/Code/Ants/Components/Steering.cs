using Unity.Entities;
using UnityEngine;

namespace AntPheromones.Ants
{
    [GenerateAuthoringComponent]
    public struct Steering : IComponentData
    {
        [HideInInspector] public float Angle;
        [HideInInspector] public float Delta;
        public SteeringForce WanderSteering;
        public SteeringForce WallSteering;
        public SteeringForce PheromoneSteering;
        public SteeringForce ColonyPullSteering;
    }

    [System.Serializable]
    public struct SteeringForce
    {
        public float Force => Value * Strength;
        [HideInInspector] public float Value;
        public float Strength;
    }
}