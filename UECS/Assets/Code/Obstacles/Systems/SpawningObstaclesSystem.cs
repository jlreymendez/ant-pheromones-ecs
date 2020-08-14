using AntPheromones.Data;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = Unity.Mathematics.Random;
using Task = System.Threading.Tasks.Task;

namespace AntPheromones.Obstacles.Systems
{
    public class SpawningObstaclesSystem : SystemBase
    {
        protected override async void OnStartRunning()
        {
            var configLoader = Addressables.LoadAssetAsync<SimulationConfig>("SimulationConfig");
            var obstacleLoader = Addressables.LoadAssetAsync<GameObject>("ObstaclePrefab");
            await Task.WhenAll(configLoader.Task, obstacleLoader.Task);
            CreateObstacleEntities(obstacleLoader.Result, configLoader.Result);
        }

        void CreateObstacleEntities(GameObject obstaclePrefab, SimulationConfig config)
        {
            var gameConversionSettings = GameObjectConversionSettings.FromWorld(World, null);
            var entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(obstaclePrefab, gameConversionSettings);
            var random = new Random(config.Seed);

            var obstacleRings = config.ObstacleRingsCount;
            var obstaclesPerRing = config.ObstaclesPerRing;
            var mapSize = config.MapSize;
            var bucketResolution = config.BucketResolution;

            var obstacleRadius = EntityManager.GetComponentData<Radius>(entityPrefab).Value;
            var scale = new float3(obstacleRadius * 2, obstacleRadius * 2, 1) / mapSize;

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

                        EntityManager.AddComponentData(obstacle, new MapBucket
                        {
                            Position = new int2(
                                (int)math.floor((position.x - obstacleRadius) / mapSize * bucketResolution),
                                (int)math.floor((position.y - obstacleRadius) / mapSize * bucketResolution)
                            )
                        });
                    }
                }
            }

            Enabled = false;
        }

        protected override void OnUpdate() { }
    }
}