using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace DotsConversion
{
    struct TornadoSwayRandom : IComponentData
    {
        public Random random;
    }

    struct TornadoSwayFader : IComponentData
    {
        public float Value;
    }

    public class ApplyTornadoSwayToPointsSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            Entity random = World.Active.EntityManager.CreateEntity();
            Entity fader = World.Active.EntityManager.CreateEntity();
            EntityManager.SetComponentData(random, new TornadoSwayRandom() { random = new Random() });
            EntityManager.SetComponentData(fader, new TornadoSwayFader() { Value = 0f });
        }

        [BurstCompile]
        struct ApplyTornadoSwayToPointsSystemJob : IJobForEach<Point>
        {
            public float tornadoFader;
            public float invDamping;
            public float friction;
            public float tornadoForce;
            public float tornadoUpForce;
            public float tornadoInwardForce;
            public float tornadoMaxForceDist;
            public float tornadoHeight;
            public float3 tornadoPosition;
            public float time;
            public Random random;

            public void Execute(ref DotsConversion.Point point)
            {
                // Anchor is a point that forms the ground triangle of a building
                if (point.anchor == false)
                {
                    float startX = point.position.x;
                    float startY = point.position.y;
                    float startZ = point.position.z;

                    point.previous.y += .01f;

                    // Calculate the tornado force on this point
                    float tdx = tornadoPosition.x + TornadoUtility.ApplySway(point.position.y, time) - point.position.x;
                    float tdz = tornadoPosition.z - point.position.z;
                    float tornadoDist = sqrt(tdx * tdx + tdz * tdz);
                    tdx /= tornadoDist;
                    tdz /= tornadoDist;

                    // If the point is within max force distance of the tornado, apply force to this point
                    if (tornadoDist < tornadoMaxForceDist)
                    {
                        float force = (1f - tornadoDist / tornadoMaxForceDist);
                        float yFader = clamp(1f - point.position.y / tornadoHeight, 0, 1);

                        // apply greater force at the base of the tornado, tapering as we ascend the Y axis
                        force *= tornadoFader * tornadoForce * (-.3f + random.NextFloat() * 1.6f);
                        float forceY = tornadoUpForce;
                        point.previous.y -= forceY * force;
                        float forceX = -tdz + tdx * tornadoInwardForce * yFader;
                        float forceZ = tdx + tdz * tornadoInwardForce * yFader;
                        point.previous.x -= forceX * force;
                        point.previous.z -= forceZ * force;
                    }

                    // dampen the effect of the tornado force (if applied)
                    point.position.x += (point.position.x - point.previous.x) * invDamping;
                    point.position.y += (point.position.y - point.previous.y) * invDamping;
                    point.position.z += (point.position.z - point.previous.z) * invDamping;

                    point.previous.x = startX;
                    point.previous.y = startY;
                    point.previous.z = startZ;

                    if (point.position.y < 0f)
                    {
                        point.position.y = 0f;
                        point.previous.y = -point.previous.y;
                        point.previous.x += (point.position.x - point.previous.x) * friction;
                        point.previous.z += (point.position.z - point.previous.z) * friction;
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new ApplyTornadoSwayToPointsSystemJob();

            var tornadoSwayRandom = GetSingleton<TornadoSwayRandom>();
            var tornadoSwayFader = GetSingleton<TornadoSwayFader>();

//        tornadoFader = Mathf.Clamp01(tornadoFader + Time.deltaTime / 10f);
//        float invDamping = 1f - damping;

            // Now that the job is set up, schedule it to be run.
            return job.Schedule(this, inputDependencies);
        }
    }
}
