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
    const float spinRate = 37f;
    const float upwardSpeed = 6f;

    // This declares a new kind of job, which is a unit of work to do.
    // The job is declared as an IJobForEach<Translation, Rotation>,
    // meaning it will process all entities in the world that have both
    // Translation and Rotation components. Change it to process the component
    // types you want.
    //
    // The job is also tagged with the BurstCompile attribute, which means
    // that the Burst compiler will optimize it for the best performance.
    [BurstCompile]
    struct TornadoParticleSpinSystemJob : IJobForEach<TornadoParticle, Translation>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        public float time;
        public float deltaTime;
        public float3 tornadoPosition;

        public void Execute([ReadOnly] ref TornadoParticle particle, ref Translation translation)
        {
            float3 position = translation.Value;
            float3 tornadoPos = new float3(tornadoPosition.x + (sin(position.y / 5f + time/4f) * 3f), position.y, tornadoPosition.z);
            float3 delta = (tornadoPos - position);
            float dist = length(delta);
            delta /= dist;
            float inForce = dist - Mathf.Clamp01(tornadoPos.y / 50f) * 30f * particle.RadiusMultiplier + 2f;
            position += new float3(-delta.z * spinRate + delta.x * inForce, upwardSpeed, delta.x * spinRate + delta.z * inForce) * deltaTime;
            if (position.y > 50f)
                position = new Vector3(position.x, 0f, position.z);
            translation.Value = position;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        EntityQuery tornadoQuery = GetEntityQuery(typeof(Tornado), typeof(Translation));
        NativeArray<Translation> tornado = tornadoQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

        var job = new TornadoParticleSpinSystemJob();

        // todo Support multiple
        foreach (var translation in tornado)
        {
            job.time = Time.time;
            job.deltaTime = Time.deltaTime;
            job.tornadoPosition = translation.Value;
            tornado.Dispose();
            break;
        }

        return job.Schedule(this);
    }
}


