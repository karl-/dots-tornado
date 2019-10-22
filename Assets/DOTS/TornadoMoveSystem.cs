using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace DotsConversion
{
    public sealed class TornadoMoveSystem : JobComponentSystem
    {
        [BurstCompile]
        struct MoveJob : IJobForEach<Tornado, Translation>
        {
            [ReadOnly] public float Time;

            public void Execute([ReadOnly] ref Tornado tornado, [WriteOnly] ref Translation position)
            {
                position.Value = new float3(
                    tornado.initialTranslation.x + math.cos(Time / 6f) * 30f,
                    tornado.initialTranslation.y,
                    tornado.initialTranslation.z + math.cos(Time / 6f * 1.618f) * 30f);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new MoveJob
            {
                Time = Time.time,
            };

            return job.Schedule(this, inputDeps);
        }
    }
}