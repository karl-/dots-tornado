using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace DotsConversion
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public sealed class SimpleRenderingSystem : ComponentSystem
    {
        const int k_ChunkLimit = 1023;

        static readonly List<MeshRenderer> s_UniqueRenderersBuffer = new List<MeshRenderer>();
        Matrix4x4[] m_ChunkBuffer = new Matrix4x4[k_ChunkLimit];
        Vector4[] m_ColorBuffer = new Vector4[k_ChunkLimit];
        EntityQueryBuilder.F_D<LocalToWorld> m_AddToMatrices;
        EntityQuery m_MeshQuery;

        protected override void OnCreate()
        {
            m_MeshQuery = GetEntityQuery(typeof(MeshRenderer), typeof(LocalToWorld), typeof(ColorData));
        }

        protected override void OnUpdate()
        {
            s_UniqueRenderersBuffer.Clear();
            EntityManager.GetAllUniqueSharedComponentData(s_UniqueRenderersBuffer);

            for (int i = 0, c = s_UniqueRenderersBuffer.Count; i < c; ++i)
            {
                var renderer = s_UniqueRenderersBuffer[i];

                if (renderer.mesh == null
                    || renderer.material == null
                    || !renderer.material.enableInstancing)
                    continue;

                var colorId = Shader.PropertyToID("_Color");

                var block = new MaterialPropertyBlock();
                m_MeshQuery.SetFilter(renderer);
                var mats = m_MeshQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
                var cols = m_MeshQuery.ToComponentDataArray<ColorData>(Allocator.TempJob);

                for (int j = 0; j < mats.Length; j += k_ChunkLimit)
                {
                    var len = math.min(mats.Length - j, k_ChunkLimit);
                    CopyToArray(mats, j, len, m_ChunkBuffer);
                    CopyToArray(cols, j, len, m_ColorBuffer);
                    block.SetVectorArray(colorId, m_ColorBuffer);
                    Graphics.DrawMeshInstanced(renderer.mesh, 0, renderer.material, m_ChunkBuffer, len, block);
                }

                mats.Dispose();
                cols.Dispose();
            }
        }

        static void CopyToArray(NativeArray<LocalToWorld> localToWorlds, int index, int count, Matrix4x4[] matrices)
        {
            NativeArray<Matrix4x4>.Copy(localToWorlds.GetSubArray(index, count).Reinterpret<Matrix4x4>(), matrices, count);
        }

        static void CopyToArray(NativeArray<ColorData> colors, int index, int count, Vector4[] buffer)
        {
            NativeArray<Vector4>.Copy(colors.GetSubArray(index, count).Reinterpret<Vector4>(), buffer, count);
        }
    }
}
