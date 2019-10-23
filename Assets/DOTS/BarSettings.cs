using Unity.Entities;
using UnityEngine;

namespace DotsConversion.Authoring
{
    public sealed class BarSettings : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] float BreakDistance = 0.4f;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<DotsConversion.BarSettings>(entity);
            dstManager.SetComponentData(entity, new DotsConversion.BarSettings
            {
                BreakDistance = BreakDistance,
            });
        }
    }
}

namespace DotsConversion
{
    public struct BarSettings : IComponentData
    {
        public float BreakDistance;
    }
}