using AntPheromones.Data;
using AntPheromones.Pheromones;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.AddressableAssets;

namespace Code.Pheromones
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class PheromoneSpawningSystem : SystemBase
    {

        protected override async void OnStartRunning()
        {
            var configLoader = Addressables.LoadAssetAsync<SimulationConfig>("SimulationConfig");
            await configLoader.Task;
            CreatePheromonesGrid(configLoader.Result);
        }

        void CreatePheromonesGrid(SimulationConfig config)
        {
            var mapSize = config.MapSize;
            var pheromoneArchetype = EntityManager.CreateArchetype(PheromonesArchetype.Components);
            var pheromones = new NativeArray<Entity>(mapSize * mapSize, Allocator.Temp);
            EntityManager.CreateEntity(pheromoneArchetype, pheromones);

        #if UNITY_EDITOR
            for (var i = 0; i < pheromones.Length; i++)
            {
                EntityManager.SetName(pheromones[i], $"Pheromones:{pheromones[i].Index}");
            }
        #endif

            Enabled = false;
        }

        protected override void OnUpdate() { }
    }
}