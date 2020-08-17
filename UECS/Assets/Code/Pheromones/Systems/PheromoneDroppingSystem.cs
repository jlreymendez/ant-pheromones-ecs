using AntPheromones.Ants;
using AntPheromones.Common;
using AntPheromones.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.AddressableAssets;

namespace AntPheromones.Pheromones
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [UpdateAfter(typeof(PheromoneDecaySystem))]
    public class PheromoneDroppingSystem : SystemBase
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

            Entities.ForEach((in Translation translation, in Excitement excitement) =>
                {
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