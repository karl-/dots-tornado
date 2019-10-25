using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace DotsConversion
{
    [Serializable]
    public struct TornadoParticleSettings : IComponentData
    {
        public float SpinRate;
        public float UpwardSpeed;
        public float Height;
    }
}

namespace DotsConversion.Authoring
{
    [DisallowMultipleComponent]
    [RequiresEntityConversion]
    public class TornadoParticleSettings : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Range(10, 100000)]
        public int particleCount = 1000;
        [Range(10, 100)]
        public float radius = 50f;
        [Range(10, 100)]
        public float height = 50f;
        [Range(10, 100)]
        public float spinRate = 37f;
        [Range(1, 20)]
        public float upwardSpeed = 6f;

        public Vector2 particleSizeRange = new Vector2(.2f, .7f);
        public Mesh mesh;
        public Material material;

        public bool useRenderMesh = false;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new global::DotsConversion.TornadoParticleSettings
            {
                SpinRate = spinRate,
                UpwardSpeed = upwardSpeed,
                Height = height
            });

            InstantiateParticles(dstManager, particleCount, radius, height, particleSizeRange);
        }

        void InstantiateParticles(EntityManager entityManager, int count, float radius, float height, float2 size)
        {
            Random random = new Random(32);

            EntityArchetype particleArchetype = entityManager.CreateArchetype(
                typeof(TornadoParticle),
                typeof(Translation),
                typeof(Scale),
                typeof(MeshRenderer),
                typeof(LocalToWorld));

            for (int i = 1; i < count; i++)
            {
                var position = new float3(
                    random.NextFloat(-radius, radius),
                    random.NextFloat(0f, height),
                    random.NextFloat(-radius, radius));

                Entity entity = entityManager.CreateEntity(particleArchetype);

                entityManager.SetComponentData(entity, new TornadoParticle() { RadiusMultiplier = UnityEngine.Random.value });
                entityManager.SetComponentData(entity, new Translation() { Value = position });
                entityManager.SetComponentData(entity, new Scale() { Value = UnityEngine.Random.Range(size.x, size.y) });
                
                entityManager.SetSharedComponentData(entity, new MeshRenderer() { mesh = mesh, material = material });
            }
        }
    }
}
