using System;
using Unity.Entities;
using UnityEngine;

namespace DotsConversion
{
    [Serializable]
    struct DebrisGenerator : IComponentData
    {
        public int BuildingCount;
        public int BuildingMaxHeight;
        public int BuildingMinHeight;
        public int AdditionalPointCount;
    }

    [Serializable]
    struct PointInitialize : IComponentData { }

    [Serializable]
    struct BarInitialize : IComponentData { }
}

namespace DotsConversion.Authoring
{
    [DisallowMultipleComponent]
    [RequiresEntityConversion]
    public class DebrisGenerator : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int buildingCount = 32;
        public int buildingMinHeight = 4;
        public int buildingMaxHeight = 12;
        public int additionalPointCount = 300;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            EntityArchetype pointArchetype = dstManager.CreateArchetype(typeof(Point), typeof(PointInitialize));
            EntityArchetype barArchetype = dstManager.CreateArchetype(typeof(Bar), typeof(BarThickness), typeof(BarInitialize));

            dstManager.AddComponentData(entity, new DotsConversion.DebrisGenerator()
            {
                BuildingCount = buildingCount,
                BuildingMinHeight = buildingMinHeight,
                BuildingMaxHeight = buildingMaxHeight,
                AdditionalPointCount = additionalPointCount
            });

            int maxPointCount = buildingCount * buildingMaxHeight * 3 + (additionalPointCount + additionalPointCount % 2);

            for (int i = 0; i < maxPointCount; i++)
                dstManager.CreateEntity(pointArchetype);

            // Two bars for each point should be a large enough buffer with plenty left over for splitting
            for (int i = 0; i < maxPointCount * 2; i++)
                dstManager.CreateEntity(barArchetype);
        }
    }
}
