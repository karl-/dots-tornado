using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DotsConversion
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public sealed class SimpleRenderingSystem : ComponentSystem
    {
        const int k_ChunkLimit = 1023;

        static readonly List<MeshRenderer> s_UniqueRenderersBuffer = new List<MeshRenderer>();
        Matrix4x4[] m_ChunkBuffer = new Matrix4x4[1023];
        EntityQueryBuilder.F_D<LocalToWorld> m_AddToMatrices;
        EntityQuery m_MeshQuery;

        protected override void OnCreate()
        { 
            m_MeshQuery = GetEntityQuery(typeof(MeshRenderer), typeof(LocalToWorld));
        }

        protected override void OnUpdate()
        {
            s_UniqueRenderersBuffer.Clear();
            EntityManager.GetAllUniqueSharedComponentData(s_UniqueRenderersBuffer);
            
            for (int i = 0; i < s_UniqueRenderersBuffer.Count; ++i)
            {
                var renderer = s_UniqueRenderersBuffer[i];
                if (renderer.mesh == null
                    || renderer.material == null
                    || !renderer.material.enableInstancing)
                    continue;

                m_MeshQuery.SetFilter(renderer);
                var mats = m_MeshQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
                for (int j = 0; j < mats.Length; j += k_ChunkLimit)
                {
                    CopyToArray(mats, j, math.min(mats.Length - j, k_ChunkLimit), m_ChunkBuffer);
                    Graphics.DrawMeshInstanced(renderer.mesh, 0, renderer.material, m_ChunkBuffer);
                }

                mats.Dispose();
            }
        }

        static void CopyToArray(NativeArray<LocalToWorld> localToWorlds, int index, int count, Matrix4x4[] matrices)
        {
            NativeArray<Matrix4x4>.Copy(localToWorlds.GetSubArray(index, count).Reinterpret<Matrix4x4>(), matrices, count);
        }
    }
}
