using System.Threading.Tasks;
using AntPheromones.Data;
using AntPheromones.Pheromones;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Code.Pheromones
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class PheromoneRenderingSystem : SystemBase
    {
        Texture2D _pheromoneTexture;
        Renderer _pheromoneRenderer;
        int _mapSize;

        protected override async void OnCreate()
        {
            var configLoader = Addressables.LoadAssetAsync<SimulationConfig>("SimulationConfig");
            var rendererLoader = Addressables.LoadAssetAsync<GameObject>("PheromoneRendererPrefab");
            await Task.WhenAll(configLoader.Task, rendererLoader.Task);

            _mapSize = configLoader.Result.MapSize;
            _pheromoneTexture = new Texture2D(_mapSize, _mapSize);
            _pheromoneRenderer = GameObject.Instantiate(rendererLoader.Result).GetComponent<Renderer>();
            _pheromoneRenderer.sharedMaterial.mainTexture = _pheromoneTexture;
        }

        protected override void OnUpdate()
        {
            var texturePixels = _pheromoneTexture.GetRawTextureData<Color32>();
            Entities.ForEach((int entityInQueryIndex, in Strength strength) =>
                {
                    texturePixels[entityInQueryIndex] = new Color32((byte)math.ceil(strength.Value * 255), 0, 0, 0);
                })
                .WithAll<PheromoneTag>()
                .ScheduleParallel();

            Dependency.Complete();

            _pheromoneTexture.Apply(true);
        }

        protected override void OnDestroy()
        {
            if (_pheromoneRenderer != null)
            {
                GameObject.Destroy(_pheromoneRenderer.gameObject);
            }
        }
    }
}