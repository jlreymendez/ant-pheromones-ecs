using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace AntPheromones.Common
{
    public static class AABBExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlaps(this AABB aabb, AABB other)
        {
            var dist = math.distance(aabb.Center, other.Center);
            var maxLength = math.length(aabb.Extents + other.Extents);
            return dist < maxLength;
        }
    }
}