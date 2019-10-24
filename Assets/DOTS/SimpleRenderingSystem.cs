using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
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
                    int inMatsIndex = j;
                    
                    for (int k = 0; k < k_ChunkLimit && inMatsIndex< mats.Length; ++k)
                    {
                        m_ChunkBuffer[k] = mats[inMatsIndex++].Value;
                    }

                    Graphics.DrawMeshInstanced(renderer.mesh, 0, renderer.material, m_ChunkBuffer);
                }

                mats.Dispose();
            }
        }

    }
}
