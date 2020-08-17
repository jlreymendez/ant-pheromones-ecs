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
        EntityQuery _obstacleQuery;

        protected override async void OnCreate()
        {
            var configLoader = Addressables.LoadAssetAsync<SimulationConfig>("SimulationConfig");
            await configLoader.Task;
            var config = configLoader.Result;
            _random = new Random(config.Seed);
            _bucketData = new BucketData(config.BucketResolution);

            _obstacleQuery = GetEntityQuery(
                ComponentType.ReadOnly<ObstacleTag>(),
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<Radius>()
            );
        }

        protected override void OnUpdate()
        {
            var obstacleRadii = _obstacleQuery.ToComponentDataArray<Radius>(Allocator.TempJob);
            var obstaclePositions = _obstacleQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

            var bucketData = _bucketData;
            var random = new Random(_random.NextUInt());
            Entities.WithAll<AntTag>()
                .ForEach((ref Steering steering, ref Rotation rotation, in Translation position) =>
                {
                    WallSteering(ref steering, position, obstaclePositions, obstacleRadii, bucketData);

                    steering.Delta =
                        steering.WallSteering * steering.WallAvoidanceStrength +
                        random.NextFloat(-steering.WanderStrength, steering.WanderStrength);

                    steering.Angle += steering.Delta;
                    rotation.Value = quaternion.Euler(0, 0, steering.Angle);
                })
                    .WithDisposeOnCompletion(obstacleRadii)
                    .WithDisposeOnCompletion(obstaclePositions)
                    .ScheduleParallel();
        }

        static void WallSteering(ref Steering steering, Translation position, NativeArray<Translation> obstaclePositions, NativeArray<Radius> obstacleRadii, BucketData bucketData)
        {
            steering.WallSteering = 0;
            var wallCheckDistance = 1f / bucketData.BucketResolution;
            for (var i = -1; i <= 1; i += 2)
            {
                float angle = steering.Angle + i * math.PI *.25f;
                position.Value += math.mul(quaternion.Euler(0, 0, angle), new float3(wallCheckDistance, 0, 0));
                var bucket = bucketData.GetBucket(position.Value);
                var bucketAABB = bucketData.GetBucketAABB(bucket);

                for (var j = 0; j < obstaclePositions.Length; j++)
                {
                    var obstacleAABB = new AABB
                    {
                        Center = obstaclePositions[j].Value,
                        Extents = new float3(1, 1, 0) * obstacleRadii[j].Value
                    };

                    if (bucketAABB.Overlaps(obstacleAABB))
                    {
                        steering.WallSteering -= i;
                        break;
                    }
                }
            }
        }
    }
}