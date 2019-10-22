using DotsConversion;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

public class TornadoParticleSpinSystem : JobComponentSystem
{
    [BurstCompile]
    struct TornadoParticleSpinSystemJob : IJobForEach<TornadoParticle, Translation>
    {
        public float time;
        public float deltaTime;
        public float3 tornadoPosition;
        public Tornado tornado;

        public void Execute([ReadOnly] ref TornadoParticle particle, ref Translation translation)
        {
            if (particle.tornadoId != tornado.id)
                return;
            float3 position = translation.Value;
            float3 tornadoPos = new float3(tornadoPosition.x + (sin(position.y / 5f + time/4f) * 3f), position.y, tornadoPosition.z);
            float3 delta = (tornadoPos - position);
            float dist = length(delta);
            delta /= dist;
            float inForce = dist - Mathf.Clamp01(tornadoPos.y / tornado.height) * 30f * particle.RadiusMultiplier + 2f;
            position += new float3(-delta.z * tornado.spinRate + delta.x * inForce, tornado.upwardSpeed, delta.x * tornado.spinRate + delta.z * inForce) * deltaTime;
            if (position.y > tornado.height)
                position = new Vector3(position.x, 0f, position.z);
            translation.Value = position;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        EntityQuery tornadoQuery = GetEntityQuery(typeof(Tornado), typeof(Translation));
        NativeArray<Tornado> tornadoes = tornadoQuery.ToComponentDataArray<Tornado>(Allocator.TempJob);
        NativeArray<Translation> positions = tornadoQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        int tornadoCount = tornadoes.Length;
        JobHandle handle = inputDependencies;

        for(int i = 0; i < tornadoCount; i++)
        {
            var job = new TornadoParticleSpinSystemJob();
            job.time = Time.time;
            job.deltaTime = Time.deltaTime;
            job.tornadoPosition = positions[i].Value;
            job.tornado = tornadoes[i];
            handle = job.Schedule(this, handle);
        }

        tornadoes.Dispose();
        positions.Dispose();

        return handle;
    }
}


