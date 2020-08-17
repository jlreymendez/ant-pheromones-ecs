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

        public AABB GetBucketAABB(int2 bucket)
        {
            return (AABB) new MinMaxAABB
            {
                Min = new float3(bucket.x, bucket.y, 0) / BucketResolution,
                Max = new float3(bucket.x + 1, bucket.y + 1, 0) / BucketResolution
            };
        }
    }
}