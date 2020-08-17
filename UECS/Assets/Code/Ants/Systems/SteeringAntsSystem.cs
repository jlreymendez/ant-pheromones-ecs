using AntPheromones.Ants;
using AntPheromones.Common;
using AntPheromones.Data;
using Unity.Collections;
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
        BucketData _bucketData;

        protected override async void OnCreate()
        {
            var configLoader = Addressables.LoadAssetAsync<SimulationConfig>("SimulationConfig");
            await configLoader.Task;
            var config = configLoader.Result;
            _random = new Random(config.Seed);
            _bucketData = new BucketData(config.BucketResolution);
        }

        protected override void OnUpdate()
        {
            // todo: since obstacles are static we could create a datastructure at the beginning of runtime to simplify this calculation.
            var obstacleQuery = GetEntityQuery(ComponentType.ReadOnly<ObstacleBucket>(), ComponentType.ReadOnly<MapBucket>());
            var obstacleBuckets = obstacleQuery.ToComponentDataArray<MapBucket>(Allocator.TempJob);

            var bucketData = _bucketData;
            var random = new Random(_random.NextUInt());
            Entities.WithAll<AntTag>()
                .ForEach((ref Steering steering, ref Rotation rotation, in Translation position) =>
                {
                    WallSteering(ref steering, position, obstacleBuckets, bucketData);

                    steering.Delta =
                        steering.WallSteering * steering.WallAvoidanceStrength +
                        random.NextFloat(-steering.WanderStrength, steering.WanderStrength);

                    steering.Angle += steering.Delta;
                    rotation.Value = quaternion.Euler(0, 0, steering.Angle);
                }).WithDisposeOnCompletion(obstacleBuckets)
                    .ScheduleParallel();
        }

        static void WallSteering(ref Steering steering, Translation position, NativeArray<MapBucket> obstacles, BucketData bucketData)
        {
            steering.WallSteering = 0;
            var wallCheckDistance = 1f / bucketData.BucketResolution;
            for (var i = -1; i <= 1; i += 2)
            {
                float angle = steering.Angle + i * math.PI *.25f;
                position.Value += math.mul(quaternion.Euler(0, 0, angle), new float3(wallCheckDistance, 0, 0));
                var bucket = bucketData.GetBucket(position.Value);

                for (var j = 0; j < obstacles.Length; j++)
                {
                    if (obstacles[j].Position.Equals(bucket))
                    {
                        steering.WallSteering -= i;
                        break;
                    }
                }
            }
        }
    }
}