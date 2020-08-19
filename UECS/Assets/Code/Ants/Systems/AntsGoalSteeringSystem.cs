using AntPheromones.Common;
using AntPheromones.Data;
using AntPheromones.Obstacles;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.AddressableAssets;

namespace AntPheromones.Ants
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(AntsSteeringSystem))]
    public class AntsGoalSteeringSystem : SystemBase
    {
        EntityQuery _obstacleQuery;
        BucketData _bucketData;

        protected override async void OnCreate()
        {
            var configLoader = Addressables.LoadAssetAsync<SimulationConfig>("SimulationConfig");
            await configLoader.Task;
            _bucketData = new BucketData(configLoader.Result.BucketResolution);
            _obstacleQuery = GetEntityQuery(ComponentType.ReadOnly<ObstacleBucket>());
        }

        protected override void OnUpdate()
        {
            var obstacleBuckets = _obstacleQuery.ToComponentDataArray<ObstacleBucket>(Allocator.TempJob);
            var bucketData = _bucketData;

            Entities.ForEach((ref Steering steering, in Translation translation, in Target target) =>
                {
                    if (Linecast(translation.Value, target.Position, obstacleBuckets, bucketData)) return;

                    var direction = target.Position - translation.Value;
                    var targetAngle = math.atan2(direction.y, direction.x);
                    var angleDif = targetAngle - steering.Angle;
                    // Directly point to target if under 90 degrees.
                    if (math.abs(angleDif) < math.PI * .5f)
                    {
                        steering.GoalSteering.Value = angleDif;
                    }
                })
                .WithAll<AntTag>()
                .WithDisposeOnCompletion(obstacleBuckets)
                .ScheduleParallel();
        }

        static bool Linecast(float3 position, float3 target, NativeArray<ObstacleBucket> obstacleBuckets, BucketData bucketData)
        {
            var toTarget = target - position;
            var steps = (int)math.ceil(math.length(toTarget) * .5f * bucketData.BucketResolution);
            var direction = math.normalize(toTarget);
            var tRatio = 1 / steps;

            for (var i = 0; i < steps; i++)
            {
                var t = i * tRatio;
                var tPosition = position + direction * t;
                var bucket = math.clamp(bucketData.GetBucket(tPosition), int2.zero, new int2(bucketData.BucketResolution - 1));
                var bucketIndex = bucket.x + bucket.y * bucketData.BucketResolution;

                if (obstacleBuckets[bucketIndex].HasWalls) return true;
            }

            return false;
        }
    }
}