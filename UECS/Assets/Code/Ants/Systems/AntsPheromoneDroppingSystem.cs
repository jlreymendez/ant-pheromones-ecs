using AntPheromones.Common;
using AntPheromones.Data;
using AntPheromones.Pheromones;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.AddressableAssets;

namespace AntPheromones.Ants
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [UpdateAfter(typeof(AntsGatheringSystem))]
    [UpdateAfter(typeof(PheromoneDecaySystem))]
    public class AntsPheromoneDroppingSystem : SystemBase
    {
        BucketData _bucketData;
        EntityQuery _pheromonesQuery;
        float _excitementPheromoneRatio;

        protected override async void OnCreate()
        {
            var configLoader = Addressables.LoadAssetAsync<SimulationConfig>("SimulationConfig");
            await configLoader.Task;
            _bucketData = new BucketData(configLoader.Result.MapSize);
            _pheromonesQuery = GetEntityQuery(ComponentType.ReadWrite<Strength>());
            _excitementPheromoneRatio = configLoader.Result.ExcitementPheromoneRatio;
        }

        protected override void OnUpdate()
        {
            var pheromonesStrength = _pheromonesQuery.ToComponentDataArray<Strength>(Allocator.TempJob);
            var bucketData = _bucketData;
            var dt = Time.fixedDeltaTime;
            var excitementPheromoneRatio = _excitementPheromoneRatio;

            Entities.ForEach((ref Excitement excitement, in Translation translation, in Resource resource) =>
                {
                    excitement.Value = resource.Value ? excitement.CarryingValue : excitement.EmptyValue;

                    var antBucket = bucketData.GetBucket(translation.Value);
                    var bucketIndex = antBucket.x + antBucket.y * bucketData.BucketResolution;
                    if (bucketIndex < 0 || bucketIndex >= pheromonesStrength.Length) return;

                    var strength = pheromonesStrength[bucketIndex].Value;
                    strength +=
                        (excitement.Value * dt * excitementPheromoneRatio) *
                        (1f - pheromonesStrength[bucketIndex].Value);
                    strength = math.min(strength, 1f);

                    pheromonesStrength[bucketIndex] = new Strength { Value = strength };
                })
                .Schedule();

            Dependency.Complete();

            _pheromonesQuery.CopyFromComponentDataArray(pheromonesStrength);
            pheromonesStrength.Dispose();
        }
    }
}