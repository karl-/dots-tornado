using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;
using Random = Unity.Mathematics.Random;

namespace DotsConversion
{
    [Serializable]
    struct TornadoSwayRandom : IComponentData
    {
        public Random Value;
    }

    [Serializable]
    struct TornadoSwayFader : IComponentData
    {
        public float Value;
    }

    public class ApplyTornadoSwayToPointsSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            Entity random = World.Active.EntityManager.CreateEntity(typeof(TornadoSwayRandom));
            Entity fader = World.Active.EntityManager.CreateEntity(typeof(TornadoSwayFader));
            EntityManager.SetComponentData(random, new TornadoSwayRandom() { Value = new Random(42) });
            EntityManager.SetComponentData(fader, new TornadoSwayFader() { Value = 0f });
        }

        [BurstCompile]
        struct ApplyTornadoSwayToPointsSystemJob : IJobForEach<Point>
        {
            public float TornadoFader;
            public float3 TornadoPosition;
            public float TimeValue;
            public Random RandomValue;

            public float InvDamping;
            public float Friction;
            public float TornadoForce;
            public float TornadoUpForce;
            public float TornadoInwardForce;
            public float TornadoMaxForceDist;
            public float TornadoHeight;

            public void Execute(ref Point point)
            {
                // Anchor is a point that forms the ground triangle of a building
                if (point.anchor == false)
                {
                    float startX = point.position.x;
                    float startY = point.position.y;
                    float startZ = point.position.z;

                    point.previous.y += .01f;

                    // Calculate the tornado force on this point
                    float tdx = TornadoPosition.x + TornadoUtility.ApplySway(point.position.y, TimeValue) - point.position.x;
                    float tdz = TornadoPosition.z - point.position.z;
                    float tornadoDist = sqrt(tdx * tdx + tdz * tdz);
                    tdx /= tornadoDist;
                    tdz /= tornadoDist;

                    // If the point is within max force distance of the tornado, apply force to this point
                    if (tornadoDist < TornadoMaxForceDist)
                    {
                        float force = (1f - tornadoDist / TornadoMaxForceDist);
                        float yFader = clamp(1f - point.position.y / TornadoHeight, 0, 1);

                        // apply greater force at the base of the tornado, tapering as we ascend the Y axis
                        force *= TornadoFader * TornadoForce * (-.3f + RandomValue.NextFloat() * 1.6f);
                        float forceY = TornadoUpForce;
                        point.previous.y -= forceY * force;
                        float forceX = -tdz + tdx * TornadoInwardForce * yFader;
                        float forceZ = tdx + tdz * TornadoInwardForce * yFader;
                        point.previous.x -= forceX * force;
                        point.previous.z -= forceZ * force;
                    }

                    // dampen the effect of the tornado force (if applied)
                    point.position.x += (point.position.x - point.previous.x) * InvDamping;
                    point.position.y += (point.position.y - point.previous.y) * InvDamping;
                    point.position.z += (point.position.z - point.previous.z) * InvDamping;

                    point.previous.x = startX;
                    point.previous.y = startY;
                    point.previous.z = startZ;

                    if (point.position.y < 0f)
                    {
                        point.position.y = 0f;
                        point.previous.y = -point.previous.y;
                        point.previous.x += (point.position.x - point.previous.x) * Friction;
                        point.previous.z += (point.position.z - point.previous.z) * Friction;
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var tornadoSwayRandom = GetSingleton<TornadoSwayRandom>();
            var tornadoSwayFader = GetSingleton<TornadoSwayFader>();
            tornadoSwayFader.Value = clamp(tornadoSwayFader.Value + Time.deltaTime / 10f, 0f, 1f);

            EntityQuery tornadoQuery = GetEntityQuery(typeof(Tornado), typeof(Translation));
            NativeArray<Tornado> tornadoes = tornadoQuery.ToComponentDataArray<Tornado>(Allocator.TempJob);
            NativeArray<Translation> positions = tornadoQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            int tornadoCount = positions.Length;
            JobHandle handle = inputDependencies;

            for (int i = 0; i < tornadoCount; i++)
            {
                var tornado = tornadoes[i];
                var job = new ApplyTornadoSwayToPointsSystemJob();

                job.RandomValue = tornadoSwayRandom.Value;
                job.TornadoFader = tornadoSwayFader.Value;
                job.TimeValue = Time.time;
                job.TornadoPosition = positions[i].Value;

                job.InvDamping = tornado.InvDamping;
                job.Friction = tornado.Friction;
                job.TornadoForce = tornado.TornadoForce;
                job.TornadoUpForce = tornado.TornadoUpForce;
                job.TornadoInwardForce = tornado.TornadoInwardForce;
                job.TornadoMaxForceDist = tornado.TornadoMaxForceDist;
                job.TornadoHeight = tornado.TornadoHeight;

                handle = job.Schedule(this, handle);
            }

            tornadoes.Dispose();
            positions.Dispose();

            return handle;
        }
    }
}
