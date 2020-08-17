using AntPheromones.Data;
using Unity.Entities;
using UnityEngine.AddressableAssets;

namespace AntPheromones.Pheromones
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class PheromoneDecaySystem : SystemBase
    {
        float _decayRate;

        protected override async void OnCreate()
        {
            var configLoader = Addressables.LoadAssetAsync<SimulationConfig>("SimulationConfig");
            await configLoader.Task;
            _decayRate = configLoader.Result.PheromoneDecayRate;
        }

        protected override void OnUpdate()
        {
            var decayRate = _decayRate;

            Entities.ForEach((ref Strength strength) =>
                {
                    strength.Value *= decayRate;
                })
                .WithAll<PheromoneTag>()
                .ScheduleParallel();
        }
    }
}