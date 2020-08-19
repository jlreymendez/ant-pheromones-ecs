using AntPheromones.Food;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace AntPheromones.Ants
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(AntsSteeringSystem))]
    public class AntsColonyPullSteeringSystem : SystemBase
    {
        EntityQuery _colonyQuery;

        protected override void OnCreate()
        {
            _colonyQuery = GetEntityQuery(ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<ColonyTag>());
        }

        protected override void OnUpdate()
        {
            var colonyPositions = _colonyQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            var clockwiseRotation = quaternion.Euler(0, 0, -math.PI * 0.5f);

            Entities.ForEach((ref Steering steering, in Rotation rotation, in Translation translation, in Resource resource) =>
            {
                var colony = GetClosestColonyPosition(translation.Value, colonyPositions);
                var forward = math.normalizesafe(math.mul(rotation.Value, new float3(1, 0, 0)));
                var right = math.mul(clockwiseRotation, forward);
                var direction = colony - translation.Value * (resource.Value ? -1 : 1);
                var dot = math.dot(math.normalizesafe(direction), right);
                steering.ColonyPullSteering.Value = math.sign(dot) * (1f - math.abs(dot)) * (1f - math.clamp(math.length(direction), 0, 1));
            })
                .WithDisposeOnCompletion(colonyPositions)
                .ScheduleParallel();
        }

        static float3 GetClosestColonyPosition(float3 position, NativeArray<Translation> colonyPositions)
        {
            var output = float3.zero;
            var minDistance = float.MaxValue;
            for (var i = 0; i < colonyPositions.Length; i++)
            {
                var distance = math.distancesq(position, colonyPositions[i].Value);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    output = colonyPositions[i].Value;
                }
            }

            return output;
        }
    }
}