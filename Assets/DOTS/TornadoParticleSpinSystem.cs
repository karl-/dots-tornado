using DotsConversion;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

namespace DotsConversion
{
    public class TornadoParticleSpinSystem : JobComponentSystem
    {
        [BurstCompile]
        struct TornadoParticleSpinSystemJob : IJobForEach<TornadoParticle, Translation>
        {
            public float time;
            public float deltaTime;
            public float3 tornadoPosition;
            public TornadoParticleSettings tornado;

            public void Execute([ReadOnly] ref TornadoParticle particle, ref Translation translation)
            {
                float3 position = translation.Value;
                float3 tornadoPos = new float3(tornadoPosition.x + (sin(position.y / 5f + time / 4f) * 3f), position.y, tornadoPosition.z);
                float3 delta = (tornadoPos - position);
                float dist = length(delta);
                delta /= dist;
                float inForce = dist - Mathf.Clamp01(tornadoPos.y / tornado.Height) * 30f * particle.RadiusMultiplier + 2f;
                position += new float3(-delta.z * tornado.SpinRate + delta.x * inForce, tornado.UpwardSpeed, delta.x * tornado.SpinRate + delta.z * inForce) * deltaTime;
                if (position.y > tornado.Height)
                    position = new Vector3(position.x, 0f, position.z);
                translation.Value = position;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            TornadoParticleSettings tornado = GetSingleton<TornadoParticleSettings>();
            EntityQuery tornadoQuery = GetEntityQuery(typeof(Tornado), typeof(Translation));
            NativeArray<Translation> positions = tornadoQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

            int tornadoCount = positions.Length;
            JobHandle handle = inputDependencies;

            // todo This doesn't actually support any more than 1 tornado
            for (int i = 0; i < tornadoCount; i++)
            {
                var job = new TornadoParticleSpinSystemJob();
                job.time = Time.time;
                job.deltaTime = Time.deltaTime;
                job.tornado = tornado;
                job.tornadoPosition = positions[i].Value;
                handle = job.Schedule(this, handle);
            }

            positions.Dispose();

            return handle;
        }
    }
}
