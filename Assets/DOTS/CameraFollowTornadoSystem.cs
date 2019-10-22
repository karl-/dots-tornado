using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

namespace DotsConversion
{
    [UpdateAfter(typeof(TornadoMoveSystem))]
    public sealed class CameraFollowTornadoSystem : JobComponentSystem
    {
        [BurstCompile]
        struct CalculateCameraPosition : IJob
        {
            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<Translation> Positions;
            [ReadOnly] public float3 LookDirection;
            [WriteOnly] public NativeArray<float3> Result;

            public void Execute()
            {
                float3 targetCenter = float3.zero;
                
                //Calculate Bounds' center
                if (Positions.Length != 0)
                {
                    float3 pos = Positions[0].Value;
                    float xMin = pos.x, xMax = pos.x;
                    float yMin = pos.y, yMax = pos.y;
                    float zMin = pos.z, zMax = pos.z;
                    for (int i = 1; i < Positions.Length; ++i)
                    {
                        pos = Positions[i].Value;
                        xMin = math.min(xMin, pos.x);
                        xMax = math.max(xMax, pos.x);
                        yMin = math.min(yMin, pos.y);
                        yMax = math.max(yMax, pos.y);
                        zMin = math.min(zMin, pos.z);
                        zMax = math.max(zMax, pos.z);
                    }
                    
                    float3 extents = new float3((xMax - xMin) * 0.5f, (yMax - yMin) * 0.5f, (zMax - zMin) * 0.5f);
                    targetCenter = new float3(xMin, yMin, zMin) + extents;
                }
                
                //Apply transformation on camera
                Result[0] = new float3(targetCenter.x, 10f, targetCenter.z) - LookDirection * 60f;
            }
        }

        struct ApplyCameraTransform : IJobParallelForTransform
        {
            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<float3> CenterResult;

            public void Execute(int index, TransformAccess transform)
            {
                transform.position = CenterResult[0];
            }
        }

        Transform m_Camera;
        EntityQuery m_TornadoQuery;

        protected override void OnCreate()
        {
            m_TornadoQuery = GetEntityQuery(typeof(Tornado), typeof(Translation));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (m_Camera == null)
                m_Camera = Camera.main.transform;

            NativeArray<float3> ResultBuffer = new NativeArray<float3>(1, Allocator.TempJob);

            var calculateJob = new CalculateCameraPosition
            {
                Positions = m_TornadoQuery.ToComponentDataArray<Translation>(Allocator.TempJob),
                LookDirection = m_Camera.forward,
                Result = ResultBuffer,
            };
            inputDeps = calculateJob.Schedule(inputDeps);

            var transforms = new TransformAccessArray(1);
            transforms.Add(m_Camera.transform);
            var applyTransformJob = new ApplyCameraTransform
            {
                CenterResult = ResultBuffer
            };

            var dependency = applyTransformJob.Schedule(transforms, inputDeps);
            transforms.Dispose();
            return dependency;
        }
    }
}