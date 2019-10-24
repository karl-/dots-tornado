using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace DotsConversion
{
    [UpdateAfter(typeof(ApplyBarTornadoSwayToBarSystem))]
    public sealed class BarBreakSystem : JobComponentSystem
    {
        [BurstCompile]
        struct CheckBreakageJob : IJob
        {
            [ReadOnly] public float BreakDistance;
            [NativeDisableParallelForRestriction] public ComponentDataFromEntity<Point> Points;
            [NativeDisableParallelForRestriction] public ComponentDataFromEntity<Bar> Bars;
            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<Entity> BarEntities;

            public void Execute()
            {
                for (int i = 0; i < BarEntities.Length; ++i)
                {
                    Entity entity = BarEntities[i];
                    Bar bar = Bars[entity];
                    if (bar.a == Entity.Null)
                        continue;

                    if (math.abs(bar.extraDist) > BreakDistance)
                    {
                        Point pointB = Points[bar.b];
                        if (pointB.neighborCount > 1)
                        {
                            //Set old point
                            pointB.neighborCount--;
                            Points[bar.b] = pointB;

                            bar.b = bar.backupB;

                            //Set new point
                            pointB.neighborCount = 1;
                            Points[bar.b] = pointB;

                            Bars[entity] = bar;
                            continue;
                        }

                        Point pointA = Points[bar.a];
                        if (pointA.neighborCount > 1)
                        {
                            //Set old point
                            pointA.neighborCount--;
                            Points[bar.a] = pointA;

                            bar.a = bar.backupA;

                            //Set new point
                            pointA.neighborCount = 1;
                            Points[bar.a] = pointA;

                            Bars[entity] = bar;
                        }
                    }
                }
            }
        }

        EntityQuery m_BarQuery;

        protected override void OnCreate()
        {
            m_BarQuery = GetEntityQuery(typeof(Bar));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            JobHandle entityQueryJob;
            var job = new CheckBreakageJob
            {
                BreakDistance = GetSingleton<BarSettings>().BreakDistance,
                Points = GetComponentDataFromEntity<Point>(),
                Bars = GetComponentDataFromEntity<Bar>(),
                BarEntities = m_BarQuery.ToEntityArray(Allocator.TempJob, out entityQueryJob),
            };

            return job.Schedule(JobHandle.CombineDependencies(inputDeps, entityQueryJob));
        }
    }
}