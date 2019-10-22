using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsConversion.Authoring
{
    [DisallowMultipleComponent]
    [RequiresEntityConversion]
    public sealed class Tornado : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<DotsConversion.Tornado>(entity);
            dstManager.SetComponentData(entity, new DotsConversion.Tornado
            {
                initialTranslation = transform.position,
            });
        }
    }
}

namespace DotsConversion
{
    public struct Tornado : IComponentData
    {
        public float3 initialTranslation;
    }
}