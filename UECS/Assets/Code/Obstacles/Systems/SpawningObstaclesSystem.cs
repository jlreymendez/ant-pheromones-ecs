using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = Unity.Mathematics.Random;

namespace AntPheromones.Obstacles.Systems
{
    public class SpawningObstaclesSystem : SystemBase
    {
        static readonly int _obstacleRings = 3;
        static readonly float _obstaclesPerRing = 0.8f;
        static readonly float _obstacleRadius = 2f;
        static readonly int _mapSize = 128;
        static readonly uint _seed = 4;

        protected override async void OnStartRunning()
        {
            var obstacleLoader = Addressables.LoadAssetAsync<GameObject>("ObstaclePrefab");
            await obstacleLoader.Task;
            CreateObstacleEntities(obstacleLoader.Result);
        }

        void CreateObstacleEntities(GameObject obstaclePrefab)
        {
            var gameConversionSettings = GameObjectConversionSettings.FromWorld(World, null);
            var entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(obstaclePrefab, gameConversionSettings);
            var random = new Random(_seed);

            var obstacleRadius = EntityManager.GetComponentData<Radius>(entityPrefab).Value;
            var scale = new float3(obstacleRadius * 2, obstacleRadius * 2, 1) / _mapSize;

            for (var i = 1; i <= _obstacleRings; i++)
            {
                var ringRadius = (i / (_obstacleRings + 1f)) * (_mapSize * .5f);
                var circumference = ringRadius * 2f * math.PI;
                var maxCount = (int)math.ceil(circumference / (2f * obstacleRadius) * 2f);
                var offset = random.NextInt(0, maxCount);
                var holeCount = random.NextInt(1, 3);

                for (int j = 0; j < maxCount; j++)
                {
                    var t = (float)j / maxCount;
                    if ((t * holeCount) % 1f < _obstaclesPerRing)
                    {
                        var angle = (j + offset) / (float)maxCount * (2f * Mathf.PI);
                        var position = new float3(_mapSize * .5f + Mathf.Cos(angle) * ringRadius, _mapSize * .5f + Mathf.Sin(angle) * ringRadius, 0);
                        // Debug.DrawRay(position / _mapSize,-Vector3.forward * .05f,Color.green,10000f);
                        var obstacle = EntityManager.Instantiate(entityPrefab);
                        EntityManager.AddComponentData(obstacle, new Translation { Value = position / _mapSize });
                        EntityManager.AddComponentData(obstacle, new NonUniformScale { Value = scale });
                    }
                }
            }
        }

        protected override void OnUpdate()
        {

        }
    }
}