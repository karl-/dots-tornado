using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DotsConversion
{
    [UpdateAfter(typeof(TornadoMoveSystem))]
    public sealed class CameraFollowTornadoSystem : ComponentSystem
    {
        Transform m_MainCamera;
        EntityQuery m_TornadoQuery;

        protected override void OnCreate()
        {
            m_TornadoQuery = GetEntityQuery(typeof(Tornado), typeof(Translation));
        }

        protected override void OnUpdate()
        {
            if (m_MainCamera == null)
                m_MainCamera = Camera.main.transform;

            using (var positions = m_TornadoQuery.ToComponentDataArray<Translation>(Allocator.TempJob))
            {
                var bounds = CalculateTornadoBounds(positions);
                float3 center = bounds.center;
                m_MainCamera.position = new Vector3(center.x, 10f, center.z) - m_MainCamera.forward * 60f;
            }
        }

        Bounds CalculateTornadoBounds(NativeArray<Translation> positions)
        {
            if (positions.Length == 0)
                return new Bounds(Vector3.zero, Vector3.zero);

            Bounds bounds = new Bounds(positions[0].Value, Vector3.zero);
            for (int i = 1; i < positions.Length; ++i)
            {
                bounds.Encapsulate(positions[i].Value);
            }

            return bounds;
        }
    }
}