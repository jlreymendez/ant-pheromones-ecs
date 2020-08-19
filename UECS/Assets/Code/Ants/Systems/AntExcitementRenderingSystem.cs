using AntPheromones.Common;
using AntPheromones.Data;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AntPheromones.Ants
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class AntExcitementRenderingSystem : SystemBase
    {
        float4 _excitedColor;
        float4 _unexcitedColor;

        protected override async void OnCreate()
        {
            var configLoader = Addressables.LoadAssetAsync<SimulationConfig>("SimulationConfig");
            await configLoader.Task;
            var color = configLoader.Result.AntExcitedColor;
            _excitedColor = new float4(color.r, color.g, color.b, color.a);
            color = configLoader.Result.AntUnexcitedColor;
            _unexcitedColor = new float4(color.r, color.g, color.b, color.a);
        }


        protected override void OnUpdate()
        {
            var unexcitedColor = _unexcitedColor;
            var excitedColor = _excitedColor;

            Entities.ForEach((ref MaterialColor color, in Excitement excitement) =>
                {
                    color.Value = math.lerp(new float4(0.3f, 0.45f, 0.48f, 1f),
                        new float4(1f, 0.84f, 0f, 1f), excitement.Value);
                    color.Value = math.lerp(unexcitedColor, excitedColor, excitement.Value);
                })
                .ScheduleParallel();

        }
    }
}