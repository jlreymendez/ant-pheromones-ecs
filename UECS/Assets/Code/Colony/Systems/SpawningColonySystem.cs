using AntPheromones.Common;
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
    public class SpawningColonySystem : SystemBase
    {
        protected override async void OnStartRunning()
        {
            var configLoader = Addressables.LoadAssetAsync<SimulationConfig>("SimulationConfig");
            var prefabLoader = Addressables.LoadAssetAsync<GameObject>("ColonyPrefab");
            await Task.WhenAll(configLoader.Task, prefabLoader.Task);
            CreateColony(prefabLoader.Result, configLoader.Result);
        }

        void CreateColony(GameObject prefab, SimulationConfig config)
        {
            var gameConversionSettings = GameObjectConversionSettings.FromWorld(World, null);
            var entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, gameConversionSettings);

            var mapSize = config.MapSize;

            var radius = EntityManager.GetComponentData<Radius>(entityPrefab).Value;
            var colony = EntityManager.Instantiate(entityPrefab);
            EntityManager.SetComponentData(colony, new Translation { Value = new float3(1, 1, 0) * 0.5f });
            EntityManager.AddComponentData(colony, new NonUniformScale {Value = new float3(1, 1, .1f) * radius / mapSize});

            Enabled = false;
        }

        protected override void OnUpdate() { }
    }
}