using AntPheromones.Ants;
using AntPheromones.Data;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.AddressableAssets;
using Random = Unity.Mathematics.Random;

namespace AntPheromones.Obstacles.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public class SteeringAntsSystem : SystemBase
    {
        Random _random;

        protected override async void OnCreate()
        {
            var configLoader = Addressables.LoadAssetAsync<SimulationConfig>("SimulationConfig");
            await configLoader.Task;
            _random = new Random(configLoader.Result.Seed);
        }

        protected override void OnUpdate()
        {
            var random = new Random(_random.NextUInt());
            Entities.WithAll<AntTag>()
                .ForEach((ref Steering steering, ref Rotation rotation) =>
                {
                    steering.Delta = random.NextFloat(-steering.WanderSteering, steering.WanderSteering);
                    steering.Angle += steering.Delta;
                    rotation.Value = quaternion.Euler(0, 0, steering.Angle);
                }).Schedule();
        }
    }
}