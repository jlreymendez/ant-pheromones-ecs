using AntPheromones.Ants;
using AntPheromones.Common;
using AntPheromones.Data;
using AntPheromones.Food;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Code.Ants.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class AntsGatheringSystem : SystemBase
    {
        EntityQuery _foodQuery;
        EntityQuery _colonyQuery;
        float _excitementCarrying;
        float _excitementEmpty;

        protected override void OnCreate()
        {
            _foodQuery = GetEntityQuery(
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<Radius>(),
                ComponentType.ReadOnly<FoodTag>()
            );
            _colonyQuery = GetEntityQuery(
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<Radius>(),
                ComponentType.ReadOnly<ColonyTag>()
            );
        }

        protected override void OnUpdate()
        {
            var foodPositions = _foodQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            var foodRadii = _foodQuery.ToComponentDataArray<Radius>(Allocator.TempJob);
            var colonyPositions = _colonyQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            var colonyRadii = _colonyQuery.ToComponentDataArray<Radius>(Allocator.TempJob);

            Entities.ForEach((ref Target target, ref Resource resource, ref Steering steering, in Translation translation) =>
                {
                    PickTarget(ref target, translation.Value, resource.Value ? colonyPositions : foodPositions, resource.Value ? colonyRadii : foodRadii);

                    if (math.distancesq(target.Position, translation.Value) < math.lengthsq(target.Radius))
                    {
                        steering.Angle += math.PI;
                        resource.Value = !resource.Value;
                    }
                })
                .WithDisposeOnCompletion(foodPositions)
                .WithDisposeOnCompletion(foodRadii)
                .WithDisposeOnCompletion(colonyPositions)
                .WithDisposeOnCompletion(colonyRadii)
                .WithAll<AntTag>()
                .ScheduleParallel();
        }

        static void PickTarget(ref Target target, float3 position, NativeArray<Translation> targetPositions, NativeArray<Radius> targetRadii)
        {
            var minDistance = float.MaxValue;
            for (var i = 0; i < targetPositions.Length; i++)
            {
                var distance = math.distance(position, targetPositions[i].Value);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    target.Position = targetPositions[i].Value;
                    target.Radius = targetRadii[i].Value;
                }
            }
        }
    }
}