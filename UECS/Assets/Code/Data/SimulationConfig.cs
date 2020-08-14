using UnityEngine;
using UnityEngine.Serialization;

namespace AntPheromones.Data
{
    [CreateAssetMenu(fileName = "SimulationConfig", menuName = "Simulation/Config", order = 0)]
    public class SimulationConfig : ScriptableObject
    {
        public uint Seed { get => _seed == 0 ? (uint) Random.Range(int.MinValue, int.MaxValue) : _seed; }
        [SerializeField] uint _seed;

        public int MapSize = 128;
        public int BucketResolution = 64;

        public int ObstacleRingsCount = 3;
        [Range(0f, 1f)] public float ObstaclesPerRing = 0.8f;
    }
}