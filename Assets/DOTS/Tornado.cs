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

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<DotsConversion.Tornado>(entity);
            dstManager.SetComponentData(entity, new DotsConversion.Tornado
            {
                id = GetInstanceID(),
                initialTranslation = transform.position,
                spinRate = spinRate,
                upwardSpeed = upwardSpeed,
                height = height
            });

            InstantiateParticles(GetInstanceID(), particleCount, radius, height, particleSizeRange);
        }

        static void InstantiateParticles(int tornadoId, int count, float radius, float height, float2 size)
        {
            EntityManager entityManager = World.Active.EntityManager;

            EntityArchetype particleArchetype = entityManager.CreateArchetype(
                typeof(TornadoParticle),
                typeof(Translation),
                typeof(Scale),
                typeof(RenderMesh),
                typeof(LocalToWorld));

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh mesh = cube.GetComponent<MeshFilter>().sharedMesh;
            Material material = cube.GetComponent<MeshRenderer>().sharedMaterial;

            for (int i = 1; i < count; i++)
            {
                var position = new float3(
                    Random.Range(-radius, radius),
                    Random.Range(0f, height),
                    Random.Range(-radius, radius));

                Entity entity = entityManager.CreateEntity(particleArchetype);
                entityManager.SetComponentData(entity, new TornadoParticle() { RadiusMultiplier = Random.value, tornadoId = tornadoId });
                entityManager.SetComponentData(entity, new Translation() { Value = position });
                entityManager.SetComponentData(entity, new Scale() { Value = Random.Range(size.x, size.y) });
                entityManager.SetSharedComponentData(entity, new RenderMesh() { mesh = mesh, material = material });
            }
        }
    }
}

namespace DotsConversion
{
    public struct Tornado : IComponentData
    {
        public int id; // todo Is there a better way to associate a tornado with it's particles?
        public float3 initialTranslation;
        public float spinRate;
        public float upwardSpeed;
        public float height;
    }
}
