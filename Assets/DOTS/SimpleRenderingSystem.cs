using System.Collections.Generic;
using System.Linq;
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
        Matrix4x4[] m_MatricesBuffer = new Matrix4x4[1023];
        Matrix4x4[] m_ChunkBuffer = new Matrix4x4[1023];
        EntityQueryBuilder.F_D<LocalToWorld> m_AddToMatrices;
        EntityQuery m_MeshQuery;

        protected override void OnCreate()
        {
            m_AddToMatrices = AddMatrixToBuffer;
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

                Entities.With(m_MeshQuery).ForEach(m_AddToMatrices);

                for (int j = 0; j < m_MatricesBuffer.Length; j += k_ChunkLimit)
                {
                    CopyRangeToList(m_MatricesBuffer, j, math.min(m_MatricesBuffer.Length - j, k_ChunkLimit), ref m_ChunkBuffer);
                    Graphics.DrawMeshInstanced(renderer.mesh, 0, renderer.material, m_ChunkBuffer);
                }
            }
        }

        void AddMatrixToBuffer(ref LocalToWorld matrix)
        {
            m_MatricesBuffer.Append(matrix.Value);
        }

        static void CopyRangeToList(Matrix4x4[] original, int index, int length, ref Matrix4x4[] target)
        {
            for (int i = index, count = index + length; i < count; ++i)
            {
                target[i] = original[i];
            }
        }
    }
}
