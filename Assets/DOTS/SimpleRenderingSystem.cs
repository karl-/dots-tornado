using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace DotsConversion
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public sealed class SimpleRenderingSystem : ComponentSystem
    {
        static readonly List<MeshRenderer> s_UniqueRenderersBuffer = new List<MeshRenderer>();
        readonly List<Matrix4x4> m_MatricesBuffer = new List<Matrix4x4>(1024);
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
                m_MatricesBuffer.Clear();
                Entities.With(m_MeshQuery).ForEach(m_AddToMatrices);

                Graphics.DrawMeshInstanced(renderer.mesh, 0, renderer.material, m_MatricesBuffer);
            }
        }

        void AddMatrixToBuffer(ref LocalToWorld matrix)
        {
            m_MatricesBuffer.Add(matrix.Value);
        }
    }
}