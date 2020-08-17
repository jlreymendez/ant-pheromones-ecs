using Unity.Mathematics;

namespace AntPheromones.Common
{
    public struct BucketData
    {
        public int BucketResolution;

        public BucketData(int bucketResolution)
        {
            BucketResolution = bucketResolution;
        }

        public int2 GetBucket(float3 position)
        {
            return new int2(
                (int) math.floor(position.x * BucketResolution),
                (int) math.floor(position.y * BucketResolution)
            );
        }
    }
}