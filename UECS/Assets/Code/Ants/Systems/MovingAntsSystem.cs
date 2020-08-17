using AntPheromones.Ants;
using AntPheromones.Common;
using AntPheromones.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.AddressableAssets;

namespace AntPheromones.Obstacles.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(SteeringAntsSystem))]
    public class MovingAntsSystem : SystemBase
    {
        BucketData _bucketData;

        protected override async void OnCreate()
        {
            var configLoader = Addressables.LoadAssetAsync<SimulationConfig>("SimulationConfig");
            await configLoader.Task;
            var config = configLoader.Result;
            _bucketData = new BucketData(config.BucketResolution);
        }

        protected override void OnUpdate()
        {
            var bucketData = _bucketData;
            var obstaclesQuery = GetEntityQuery(
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<Radius>(),
                    ComponentType.ReadOnly<ObstacleTag>()
                );
            var obstaclePositions = obstaclesQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            var obstacleRadius = obstaclesQuery.ToComponentDataArray<Radius>(Allocator.TempJob);

            Entities.WithAll<AntTag>()
                .ForEach((ref Translation translation, ref Speed speed, ref Steering steering, in Acceleration acceleration, in Rotation rotation) =>
                {
                    speed.Target = speed.Max * (1f - math.abs(steering.Delta) * 0.33f);
                    speed.Value += (speed.Target - speed.Value) * acceleration.Value;
                    var forward = math.mul(rotation.Value, new float3(1, 0, 0));
                    var velocity = forward * speed.Value;

                    var position = translation.Value + velocity;
                    if (position.x < 0f || position.x > 1f)
                    {
                        velocity.x *= -1;
                    }
                    else
                    {
                        translation.Value.x += velocity.x;
                    }
                    if (position.y < 0f || position.y > 1f)
                    {
                        velocity.y *= -1;
                    }
                    else
                    {
                        translation.Value.y += velocity.y;
                    }

                    PreventCollision(ref velocity, ref translation.Value, obstaclePositions, obstacleRadius, bucketData);

                    steering.Angle = math.atan2(velocity.y, velocity.x);
                })
                .WithDisposeOnCompletion(obstacleRadius)
                .WithDisposeOnCompletion(obstaclePositions)
                .ScheduleParallel();
        }

        static void PreventCollision(ref float3 velocity, ref float3 position, NativeArray<Translation> obstaclePositions, NativeArray<Radius> obstacleRadii, BucketData bucketData)
        {
            var bucket = bucketData.GetBucket(position);
            var bucketAABB = bucketData.GetBucketAABB(bucket);
            for (var i = 0; i < obstaclePositions.Length; i++)
            {
                var obstaclePosition = obstaclePositions[i].Value;
                var obstacleRadius = obstacleRadii[i].Value;
                var obstacleAABB = new AABB { Center = obstaclePosition, Extents = new float3(1, 1, 0) * obstacleRadius };
                if (bucketAABB.Overlaps(obstacleAABB) == false) continue;

                var difference = position - obstaclePosition;
                if (math.lengthsq(difference) < math.lengthsq(obstacleRadius))
                {
                    difference = math.normalize(difference);
                    position = obstaclePosition + difference * obstacleRadius;
                    velocity -= difference * math.dot(difference, velocity) * 1.5f;
                }
            }
        }
    }
}