using AntPheromones.Common;
using AntPheromones.Data;
using AntPheromones.Food;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = Unity.Mathematics.Random;
using Task = System.Threading.Tasks.Task;

namespace AntPheromones.Obstacles.Systems
{
    public class SpawningFoodSystem : SystemBase
    {
        protected override async void OnStartRunning()
        {
            var configLoader = Addressables.LoadAssetAsync<SimulationConfig>("SimulationConfig");
            var prefabLoader = Addressables.LoadAssetAsync<GameObject>("FoodPrefab");
            await Task.WhenAll(configLoader.Task, prefabLoader.Task);
            CreateFood(prefabLoader.Result, configLoader.Result);
        }

        void CreateFood(GameObject prefab, SimulationConfig config)
        {
            var gameConversionSettings = GameObjectConversionSettings.FromWorld(World, null);
            var entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, gameConversionSettings);

            var mapSize = config.MapSize;
            var random = new Random(config.Seed);

            var radius = EntityManager.GetComponentData<Radius>(entityPrefab);
            radius.Value /= mapSize;
            var entity = EntityManager.Instantiate(entityPrefab);
            var angle = random.NextFloat(0f, 1f) * 2f * math.PI;
            var position = new float3(.5f + math.cos(angle) * .475f, .5f + math.sin(angle) * .475f, 0);
            EntityManager.SetComponentData(entity, new Translation { Value = position });
            EntityManager.SetComponentData(entity, radius);
            EntityManager.AddComponentData(entity, new NonUniformScale {Value = new float3(2, 2, .1f) * radius.Value });
            EntityManager.AddComponentData(entity, new FoodTag());

            Enabled = false;
        }

        protected override void OnUpdate() { }
    }
}