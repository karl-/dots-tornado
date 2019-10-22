using DotsConversion;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class TornadoParticleSpawner : MonoBehaviour
{
    [Range(10, 100000)]
    public int particleCount = 1000;
    [Range(1, 500)]
    public float radius = 100f;
    [Range(1, 500)]
    public float height = 50f;

    public Vector2 size = new Vector2(.2f, .7f);

    public void Start()
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

        for (int i = 1; i < particleCount; i++)
        {
            var position = new float3(
                Random.Range(-radius * .5f, radius * .5f),
                Random.Range(0f, height),
                Random.Range(-radius * .5f, radius * .5f));

            Entity entity = entityManager.CreateEntity(particleArchetype);
            entityManager.SetComponentData(entity, new TornadoParticle() { RadiusMultiplier = Random.value });
            entityManager.SetComponentData(entity, new Translation() { Value = position });
            entityManager.SetComponentData(entity, new Scale() { Value = Random.Range(size.x, size.y) });
            entityManager.SetSharedComponentData(entity, new RenderMesh() { mesh = mesh, material = material });
        }
    }
}
