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
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public sealed class CameraFollowTornadoSystem : ComponentSystem
    {
        Transform m_Camera;

        protected override void OnUpdate()
        {
            if (m_Camera == null)
                m_Camera = Camera.main.transform;

            var entity = GetSingletonEntity<Tornado>();
            var position = EntityManager.GetComponentData<Translation>(entity).Value;
            m_Camera.position = new Vector3(position.x, 10f, position.y) - m_Camera.forward * 60f;
        }
    }
}