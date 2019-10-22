using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DotsConversion.Authoring
{
    [DisallowMultipleComponent]
    [RequiresEntityConversion]
    public sealed class Tornado : MonoBehaviour, IConvertGameObjectToEntity
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

        [Header("Particles")]
        public Mesh mesh;
        public Material material;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<DotsConversion.Tornado>(entity);
            dstManager.SetComponentData(entity, new DotsConversion.Tornado()
            {
                initialTranslation = transform.position,
                spinRate = spinRate,
                upwardSpeed = upwardSpeed,
                height = height
            });

            InstantiateParticles(particleCount, radius, height, particleSizeRange);
        }

        void InstantiateParticles(int count, float radius, float height, float2 size)
        {
            EntityManager entityManager = World.Active.EntityManager;

            EntityArchetype particleArchetype = entityManager.CreateArchetype(
                typeof(TornadoParticle),
                typeof(Translation),
                typeof(Scale),
                typeof(MeshRenderer),
                typeof(LocalToWorld));

            for (int i = 1; i < count; i++)
            {
                var position = new float3(
                    Random.Range(-radius, radius),
                    Random.Range(0f, height),
                    Random.Range(-radius, radius));

                Entity entity = entityManager.CreateEntity(particleArchetype);
                entityManager.SetComponentData(entity, new TornadoParticle() { RadiusMultiplier = Random.value });
                entityManager.SetComponentData(entity, new Translation() { Value = position });
                entityManager.SetComponentData(entity, new Scale() { Value = Random.Range(size.x, size.y) });
                entityManager.SetSharedComponentData(entity, new MeshRenderer() { mesh = mesh, material = material });
            }
        }
    }
}

namespace DotsConversion
{
    public struct Tornado : IComponentData
    {
        public float3 initialTranslation;
        public float spinRate;
        public float upwardSpeed;
        public float height;
    }
}
