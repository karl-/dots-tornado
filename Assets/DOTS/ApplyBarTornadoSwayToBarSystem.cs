using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace DotsConversion
{
    [UpdateAfter(typeof(ApplyTornadoSwayToPointsSystem))]
    public class ApplyBarTornadoSwayToBarSystem : JobComponentSystem
    {
        [BurstCompile]
        struct ApplySwayJob : IJob
        {
            [NativeDisableParallelForRestriction] public ComponentDataFromEntity<Point> Points;
            [NativeDisableParallelForRestriction, WriteOnly] public ComponentDataFromEntity<Bar> BarsMap;
            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<Entity> BarEntities;
            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<Bar> Bars;

            public void Execute()
            {
                for (int i = 0; i < Bars.Length; ++i)
                {
                    var bar = Bars[i];
                    if (bar.a == Entity.Null)
                        return;

                    Point pA = Points[bar.a];
                    var pB = Points[bar.b];
                    float3 pointA = pA.position;
                    float3 pointB = pB.position;
                    float dx = pointB.x - pointA.x;
                    float dy = pointB.y - pointA.y;
                    float dz = pointB.z - pointA.z;
                    // Calculate how much distance has been added due to the tornado force affecting points
                    float dist = math.sqrt(dx * dx + dy * dy + dz * dz);
                    float extraDist = dist - bar.barLength;
                    // When a point is affected by the suction, move it in the direction of the suck by half of the distance traveled
                    float pushX = (dx / dist * extraDist) * .5f;
                    float pushY = (dy / dist * extraDist) * .5f;
                    float pushZ = (dz / dist * extraDist) * .5f;
                    // Debug.Log( "Push " + pushX + " " + pushY + " " + pushZ );
                    if (Points[bar.a].anchor == false && Points[bar.b].anchor == false)
                    {
                        pointA.x += pushX;
                        pointA.y += pushY;
                        pointA.z += pushZ;
                        pointB.x -= pushX;
                        pointB.y -= pushY;
                        pointB.z -= pushZ;
                    }
                    else if (Points[bar.a].anchor)
                    {
                        pointB.x -= pushX * 2f;
                        pointB.y -= pushY * 2f;
                        pointB.z -= pushZ * 2f;
                    }
                    else if (Points[bar.b].anchor)
                    {
                        pointA.y += pushY * 2f;
                        pointA.z += pushZ * 2f;
                        pointA.x += pushX * 2f;
                    }

                    pA.position = pointA;
                    pB.position = pointB;

                    Points[bar.a] = pA;
                    Points[bar.b] = pB;
                    bar.extraDist = extraDist;
                    BarsMap[BarEntities[i]] = bar;
                }
            }
        }

        EntityQuery m_BarsQuery;

        protected override void OnCreate()
        {
            m_BarsQuery = GetEntityQuery(typeof(Bar));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            JobHandle barQueryHandle;
            JobHandle entityQueryHandle;

            var bars = m_BarsQuery.ToComponentDataArray<Bar>(Allocator.TempJob, out barQueryHandle);
            var swayJob = new ApplySwayJob
            {
                Bars = bars,
                Points = GetComponentDataFromEntity<Point>(),
                BarsMap = GetComponentDataFromEntity<Bar>(),
                BarEntities = m_BarsQuery.ToEntityArray(Allocator.TempJob, out entityQueryHandle),
            };

            inputDeps = JobHandle.CombineDependencies(inputDeps, barQueryHandle);
            inputDeps = JobHandle.CombineDependencies(inputDeps, entityQueryHandle);
            
            return swayJob.Schedule(inputDeps);
        }
    }
}