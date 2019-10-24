using System;
using Unity.Entities;
using UnityEngine;

namespace DotsConversion
{
    [Serializable]
    struct DebrisGenerator : ISharedComponentData, IEquatable<DebrisGenerator>
    {
        public int BuildingCount;
        public int BuildingMaxHeight;
        public int BuildingMinHeight;
        public int AdditionalPointCount;

        public Mesh barMesh;
        public Material barMaterial;

        public bool UseRenderMesh;

        public bool Equals(DebrisGenerator other)
        {
            return BuildingCount == other.BuildingCount
                && BuildingMaxHeight == other.BuildingMaxHeight
                && BuildingMinHeight == other.BuildingMinHeight
                && AdditionalPointCount == other.AdditionalPointCount
                && Equals(barMesh, other.barMesh)
                && Equals(barMaterial, other.barMaterial);
        }

        public override bool Equals(object obj)
        {
            return obj is DebrisGenerator other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = BuildingCount;
                hashCode = (hashCode * 397) ^ BuildingMaxHeight;
                hashCode = (hashCode * 397) ^ BuildingMinHeight;
                hashCode = (hashCode * 397) ^ AdditionalPointCount;
                hashCode = (hashCode * 397) ^ (barMesh != null ? barMesh.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (barMaterial != null ? barMaterial.GetHashCode() : 0);
                return hashCode;
            }
        }
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

        public Material barMaterial;
        public Mesh barMesh;

        public bool useRenderMesh;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            EntityArchetype pointArchetype = dstManager.CreateArchetype(
                typeof(Point),
                typeof(PointInitialize));

            dstManager.AddSharedComponentData(entity, new DotsConversion.DebrisGenerator()
            {
                BuildingCount = buildingCount,
                BuildingMinHeight = buildingMinHeight,
                BuildingMaxHeight = buildingMaxHeight,
                barMesh = barMesh,
                AdditionalPointCount = additionalPointCount,
                barMaterial = barMaterial,
                UseRenderMesh = useRenderMesh
            });

            int maxPointCount = buildingCount * buildingMaxHeight * 3 + (additionalPointCount + 1);

            for (int i = 0; i < maxPointCount; i++)
                dstManager.CreateEntity(pointArchetype);
        }
    }
}
