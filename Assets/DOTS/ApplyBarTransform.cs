using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsConversion
{
    public sealed class ApplyBarTransform : JobComponentSystem
    {
        struct ApplyTransformJob : IJobForEach<Bar, BarThickness, LocalToWorld>
        {
            public void Execute(
                [ReadOnly] ref Bar bar, 
                [ReadOnly] ref BarThickness thickness, 
                [WriteOnly] ref LocalToWorld localToWorld)
            {
                float3 pointA = bar.a.position;
                float3 pointB = bar.b.position;
                float dx = pointB.x - pointA.x;
                float dy = pointB.y - pointA.y;
                float dz = pointB.z - pointA.z;

                localToWorld.Value = float4x4.TRS(
                    new float3((pointA.x + pointB.x) * .5f, (pointA.y + pointB.y) * .5f, (pointA.z + pointB.z) * .5f),
                    quaternion.LookRotation(new float3(dx, dy, dz), math.up()),
                    new float3(thickness.thickness, thickness.thickness, bar.a.barLength));/*math.distance(pointA, pointB)*/
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new ApplyTransformJob();

            return job.Schedule(this, inputDeps);
        }
    }
}