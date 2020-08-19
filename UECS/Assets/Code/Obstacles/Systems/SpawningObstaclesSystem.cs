using AntPheromones.Common;
using AntPheromones.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = Unity.Mathematics.Random;
using Task = System.Threading.Tasks.Task;

namespace AntPheromones.Obstacles.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class SpawningObstaclesSystem : SystemBase
    {
        protected override async void OnStartRunning()
        {
            var configLoader = Addressables.LoadAssetAsync<SimulationConfig>("SimulationConfig");
            var obstacleLoader = Addressables.LoadAssetAsync<GameObject>("ObstaclePrefab");
            await Task.WhenAll(configLoader.Task, obstacleLoader.Task);

            var obstacles = new NativeList<Entity>(Allocator.Temp);
            CreateObstacleEntities(obstacleLoader.Result, configLoader.Result, obstacles);
            CreateObstacleBuckets(obstacles, configLoader.Result);
        }

        void CreateObstacleEntities(GameObject obstaclePrefab, SimulationConfig config, NativeList<Entity> obstacles)
        {
            var gameConversionSettings = GameObjectConversionSettings.FromWorld(World, null);
            var entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(obstaclePrefab, gameConversionSettings);
            var random = new Random(config.Seed);

            var obstacleRings = config.ObstacleRingsCount;
            var obstaclesPerRing = config.ObstaclesPerRing;
            var mapSize = config.MapSize;

            var obstacleRadius = EntityManager.GetComponentData<Radius>(entityPrefab).Value;
            var scale = new float3(2, 2, .1f) * obstacleRadius;
            obstacleRadius *= mapSize;

            for (var i = 1; i <= obstacleRings; i++)
            {
                var ringRadius = (i / (obstacleRings + 1f)) * (mapSize * .5f);
                var circumference = ringRadius * 2f * math.PI;
                var maxCount = (int)math.ceil(circumference / (2f * obstacleRadius) * 2f);
                var offset = random.NextInt(0, maxCount);
                var holeCount = random.NextInt(1, 3);

                for (int j = 0; j < maxCount; j++)
                {
                    var t = (float)j / maxCount;
                    if ((t * holeCount) % 1f < obstaclesPerRing)
                    {
                        var angle = (j + offset) / (float)maxCount * (2f * Mathf.PI);
                        var position = new float3(mapSize * .5f + Mathf.Cos(angle) * ringRadius, mapSize * .5f + Mathf.Sin(angle) * ringRadius, 0);
                        // Debug.DrawRay(position / mapSize,-Vector3.forward * .05f,Color.green,10000f);
                        var obstacle = EntityManager.Instantiate(entityPrefab);
                        EntityManager.AddComponentData(obstacle, new Translation { Value = position / mapSize });
                        EntityManager.AddComponentData(obstacle, new NonUniformScale { Value = scale });
                        EntityManager.AddComponentData(obstacle, new ObstacleTag());

                        obstacles.Add(obstacle);

                    #if UNITY_EDITOR
                        EntityManager.SetName(obstacle, $"Obstacle:{obstacle.Index}");
                    #endif
                    }
                }
            }

            Enabled = false;
        }

        void CreateObstacleBuckets(NativeList<Entity> obstacles, SimulationConfig config)
        {
            var obstacleBucketArchetype = EntityManager.CreateArchetype(ObstaclesBucketArchetype.Components);
            var bucketResolution = config.BucketResolution;
            var obstaclesInBucket = false;
            var bucketRatio = 1f / bucketResolution;

            int x, y;
            for (x = 0; x < bucketResolution; x++)
            {
                for (y = 0; y < bucketResolution; y++)
                {
                    var bucketPosition = new int2(x, y);
                    var bucketAABB = (AABB)new MinMaxAABB
                    {
                        Max = new float3( (x + 1) * bucketRatio, (y + 1) * bucketRatio, 0),
                        Min = new float3( x * bucketRatio, y * bucketRatio, 0),
                    };
                    obstaclesInBucket = false;

                    // Find all obstacles in bucket.
                    for (var i = 0; i < obstacles.Length; i++)
                    {
                        var position = EntityManager.GetComponentData<Translation>(obstacles[i]).Value;
                        var radius = EntityManager.GetComponentData<Radius>(obstacles[i]).Value;
                        var obstacleAABB = new AABB { Center = position, Extents = new float3(1f, 1f, 0) * radius };

                        // Check all directions to see if buck
                        if (bucketAABB.Overlaps(obstacleAABB) == false) continue;

                        obstaclesInBucket = true;
                        break;
                    }

                    // Create obstacle buckets.
                    var bucket = EntityManager.CreateEntity(obstacleBucketArchetype);
                    EntityManager.SetComponentData(bucket,
                        new ObstacleBucket { HasWalls = obstaclesInBucket, Position = bucketPosition }
                    );

                #if UNITY_EDITOR
                    EntityManager.SetName(bucket, $"ObstacleBucket:{bucket.Index}");
                #endif
                }
            }
        }

        protected override void OnUpdate() { }
    }
}