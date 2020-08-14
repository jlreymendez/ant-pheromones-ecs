using AntPheromones.Ants;
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
    public class SpawningAntsSystem : SystemBase
    {
        protected override async void OnStartRunning()
        {
            var configLoader = Addressables.LoadAssetAsync<SimulationConfig>("SimulationConfig");
            var prefabLoader = Addressables.LoadAssetAsync<GameObject>("AntPrefab");
            await Task.WhenAll(configLoader.Task, prefabLoader.Task);
            CreateObstacleEntities(prefabLoader.Result, configLoader.Result);
        }

        void CreateObstacleEntities(GameObject prefab, SimulationConfig config)
        {
            var gameConversionSettings = GameObjectConversionSettings.FromWorld(World, null);
            var entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, gameConversionSettings);
            var random = new Random(config.Seed);

            var mapSize = config.MapSize;
            var antsCount = config.AntsCount;

            for (var i = 1; i <= antsCount; i++)
            {
                var entity = EntityManager.Instantiate(entityPrefab);
                var position = new float3(random.NextFloat(-5f, 5f) + mapSize * .5f, random.NextFloat(-5f, 5f) + mapSize * .5f, 0);
                EntityManager.SetComponentData(entity, new Translation { Value = position / mapSize });
                EntityManager.SetComponentData(entity, new Rotation { Value = quaternion.Euler(0, 0, random.NextFloat(0f, 2f * math.PI))});
                EntityManager.SetComponentData(entity, new AntTag());
            }

            Enabled = false;
        }

        protected override void OnUpdate() { }
    }
}