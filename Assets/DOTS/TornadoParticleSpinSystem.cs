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
            [ReadOnly] public float time;
            [ReadOnly] public float deltaTime;
            [ReadOnly] public float3 tornadoPosition;
            [ReadOnly] public TornadoParticleSettings tornado;

            public void Execute([ReadOnly] ref TornadoParticle particle, ref Translation translation)
            {
                float3 position = translation.Value;
                float3 tornadoPos = new float3(tornadoPosition.x + (sin(position.y / 5f + time / 4f) * 3f), position.y, tornadoPosition.z);
                float3 delta = (tornadoPos - position);
                float dist = length(delta);
                delta /= dist;
                float inForce = dist - math.clamp(tornadoPos.y / tornado.Height,0f, 1f) * 30f * particle.RadiusMultiplier + 2f;
                position += new float3(-delta.z * tornado.SpinRate + delta.x * inForce, tornado.UpwardSpeed, delta.x * tornado.SpinRate + delta.z * inForce) * deltaTime;
                if (position.y > tornado.Height)
                    position = new Vector3(position.x, 0f, position.z);
                translation.Value = position;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            TornadoParticleSettings tornado = GetSingleton<TornadoParticleSettings>();
            Entity tornadoEntity = GetSingletonEntity<Tornado>();
            Translation tornadoTranslation = EntityManager.GetComponentData<Translation>(tornadoEntity);

            var job = new TornadoParticleSpinSystemJob();
            job.time = Time.time;
            job.deltaTime = Time.deltaTime;
            job.tornado = tornado;
            job.tornadoPosition = tornadoTranslation.Value;
            return job.Schedule(this, inputDependencies);
        }
    }
}
