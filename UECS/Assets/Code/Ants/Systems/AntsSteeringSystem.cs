using AntPheromones.Common;
using AntPheromones.Data;
using AntPheromones.Obstacles;
using AntPheromones.Pheromones;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.AddressableAssets;
using Random = Unity.Mathematics.Random;

namespace AntPheromones.Ants
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public class AntsSteeringSystem : SystemBase
    {
        Random _random;
        BucketData _wallBucketData;
        BucketData _pheromoneBucketData;
        EntityQuery _obstacleQuery;
        EntityQuery _pheromoneQuery;

        protected override async void OnCreate()
        {
            var configLoader = Addressables.LoadAssetAsync<SimulationConfig>("SimulationConfig");
            await configLoader.Task;
            var config = configLoader.Result;
            _random = new Random(config.Seed);
            _wallBucketData = new BucketData(config.BucketResolution);
            _pheromoneBucketData = new BucketData(config.MapSize);

            _obstacleQuery = GetEntityQuery(
                ComponentType.ReadOnly<ObstacleTag>(),
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<Radius>()
            );

            _pheromoneQuery = GetEntityQuery(
                ComponentType.ReadOnly<Strength>(),
                ComponentType.ReadOnly<PheromoneTag>()
            );
        }

        protected override void OnUpdate()
        {
            var obstacleRadii = _obstacleQuery.ToComponentDataArray<Radius>(Allocator.TempJob);
            var obstaclePositions = _obstacleQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

            var pheromoneStrengths = _pheromoneQuery.ToComponentDataArray<Strength>(Allocator.TempJob);

            var wallBucketData = _wallBucketData;
            var pheromoneBucketData = _pheromoneBucketData;
            var random = new Random(_random.NextUInt());
            Entities.WithAll<AntTag>()
                .ForEach((ref Steering steering, ref Rotation rotation, in Translation position) =>
                {
                    PheromoneSteering(ref steering.PheromoneSteering, steering.Angle, position.Value, pheromoneStrengths, pheromoneBucketData);
                    WallSteering(ref steering.WallSteering, steering.Angle, position.Value, obstaclePositions, obstacleRadii, wallBucketData);
                    steering.WanderSteering.Value = random.NextFloat(-1, 1);

                    steering.Delta = steering.WanderSteering.Force + steering.WallSteering.Force
                                    + steering.PheromoneSteering.Force + steering.ColonyPullSteering.Force
                                    + steering.GoalSteering.Force;

                    steering.Angle += steering.Delta;

                    // Keep angle in normalized ranges.
                    if (math.abs(steering.Angle) > math.PI)
                    {
                        steering.Angle -= math.sign(steering.Angle) * math.PI * 2;
                    }

                    rotation.Value = quaternion.Euler(0, 0, steering.Angle);
                })
                    .WithDisposeOnCompletion(obstacleRadii)
                    .WithDisposeOnCompletion(obstaclePositions)
                    .WithDisposeOnCompletion(pheromoneStrengths)
                    .ScheduleParallel();
        }

        static void WallSteering(ref SteeringForce steering, float facingAngle, float3 position,
            NativeArray<Translation> obstaclePositions, NativeArray<Radius> obstacleRadii, BucketData bucketData)
        {
            steering.Value = 0;
            var wallCheckDistance = 1f / bucketData.BucketResolution;
            for (var i = -1; i <= 1; i += 2)
            {
                float angle = facingAngle + i * math.PI *.25f;
                position += math.mul(quaternion.Euler(0, 0, angle), new float3(wallCheckDistance, 0, 0));
                var bucket = bucketData.GetBucket(position);
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
                        steering.Value -= i;
                        break;
                    }
                }
            }
        }

        static void PheromoneSteering(ref SteeringForce steering, float facingAngle, float3 position,
            NativeArray<Strength> pheromoneStrengths, BucketData bucketData)
        {
            steering.Value = 0;
            var checkDistance = 1f / bucketData.BucketResolution;
            for (var i = -1; i <= 1; i += 2)
            {
                float angle = facingAngle + i * math.PI *.25f;
                position += math.mul(quaternion.Euler(0, 0, angle), new float3(checkDistance, 0, 0));
                var bucket = math.clamp(bucketData.GetBucket(position), int2.zero, new int2(bucketData.BucketResolution - 1));

                steering.Value = pheromoneStrengths[bucket.x + bucket.y * bucketData.BucketResolution].Value * i;
            }

            steering.Value = math.sign(steering.Value);
        }
    }
}