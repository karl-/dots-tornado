using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DotsConversion
{
    public struct Tornado : IComponentData
    {
        public float3 initialTranslation;

        public float TornadoFader;
        public float InvDamping;
        public float Friction;
        public float TornadoForce;
        public float TornadoUpForce;
        public float TornadoInwardForce;
        public float TornadoMaxForceDist;
        public float TornadoHeight;
    }
}

namespace DotsConversion.Authoring
{
    [DisallowMultipleComponent]
    [RequiresEntityConversion]
    public sealed class Tornado : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float damping = .012f;
        public float friction = .4f;
        public float tornadoForce = .022f;
        public float tornadoUpForce = 1.4f;
        public float tornadoInwardForce = 9f;
        public float tornadoMaxForceDist = 30f;
        public float tornadoHeight = 50f;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<DotsConversion.Tornado>(entity);
            dstManager.SetComponentData(entity, new DotsConversion.Tornado()
            {
                initialTranslation = transform.position,

                InvDamping = 1f - damping,
                Friction = friction,
                TornadoForce = tornadoForce,
                TornadoUpForce = tornadoUpForce,
                TornadoInwardForce = tornadoInwardForce,
                TornadoMaxForceDist = tornadoMaxForceDist,
                TornadoHeight = tornadoHeight
            });
        }
    }
}
