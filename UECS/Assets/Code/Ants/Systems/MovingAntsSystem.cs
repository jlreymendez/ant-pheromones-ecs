using AntPheromones.Ants;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace AntPheromones.Obstacles.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(SteeringAntsSystem))]
    public class MovingAntsSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<AntTag>()
                .ForEach((ref Translation translation, ref Speed speed, in Acceleration acceleration, in Steering steering, in Rotation rotation) =>
                {
                    speed.Target = speed.Max * (1f - math.abs(steering.Delta) * 0.33f);
                    speed.Value += (speed.Target - speed.Value) * acceleration.Value;
                    var forward = math.mul(rotation.Value, new float3(1, 0, 0));
                    var delta = forward * speed.Value;

                    var position = translation.Value + delta;
                    if (position.x < 0f || position.x > 1f)
                    {
                        delta.x *= -1;
                    }
                    if (position.y < 0f || position.y > 1f)
                    {
                        delta.y *= -1;
                    }

                    translation.Value += delta;
                }).Schedule();
        }
    }
}