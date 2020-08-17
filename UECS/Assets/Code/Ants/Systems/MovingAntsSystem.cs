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
                    ComponentType.ReadOnly<MapBucket>(),
                    ComponentType.ReadOnly<ObstacleBucket>()
                );
            var groupedObstacles = obstaclesQuery.ToEntityArray(Allocator.TempJob);
            var obstaclesBuffer = GetBufferFromEntity<ObstacleBucket>(true);
            var mapBuckets = GetComponentDataFromEntity<MapBucket>(true);

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

                    PreventCollision(ref velocity, ref translation.Value, groupedObstacles, mapBuckets, obstaclesBuffer, bucketData);

                    steering.Angle = math.atan2(velocity.y, velocity.x);
                })
                .WithReadOnly(mapBuckets)
                .WithReadOnly(obstaclesBuffer)
                .WithDisposeOnCompletion(groupedObstacles)
                .ScheduleParallel();
        }

        static void PreventCollision(ref float3 velocity, ref float3 position, NativeArray<Entity> groupedObstacles, ComponentDataFromEntity<MapBucket> mapBuckets, BufferFromEntity<ObstacleBucket> obstacleBuffers, BucketData bucketData)
        {
            var bucket = bucketData.GetBucket(position);
            for (var i = 0; i < groupedObstacles.Length; i++)
            {
                var obstacleGroup = groupedObstacles[i];
                if (bucket.Equals(mapBuckets[obstacleGroup].Position) == false) continue;

                var obstacles = obstacleBuffers[obstacleGroup];
                for (var j = 0; j < obstacles.Length; j++)
                {
                    var difference = position - obstacles[j].Position;
                    var radius = obstacles[j].Radius;
                    if (math.lengthsq(difference) < math.lengthsq(radius))
                    {
                        difference = math.normalize(difference);
                        position = obstacles[j].Position + difference * radius;
                        velocity -= difference * math.dot(difference, velocity) * 1.5f;
                    }
                }

                break;
            }
        }
    }
}