using UnityEngine;

namespace AntPheromones.Data
{
    [CreateAssetMenu(fileName = "SimulationConfig", menuName = "Simulation/Config", order = 0)]
    public class SimulationConfig : ScriptableObject
    {
        [Header("Random")]
        [SerializeField] uint _seed = 0;
        public uint Seed { get => _seed == 0 ? (uint) Random.Range(int.MinValue, int.MaxValue) : _seed; }

        [Header("Map")]
        public int MapSize = 128;
        public int BucketResolution = 64;

        [Header("Obstacles")]
        public int ObstacleRingsCount = 3;
        [Range(0f, 1f)] public float ObstaclesPerRing = 0.8f;

        [Header("Ants")]
        public int AntsCount = 1000;
        public Color AntExcitedColor;
        public Color AntUnexcitedColor;

        [Header("Pheromones")]
        public float ExcitementPheromoneRatio = 0.3f;
        public float PheromoneDecayRate = 0.9985f;
    }
}