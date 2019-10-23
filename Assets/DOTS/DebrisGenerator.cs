﻿using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace DotsConversion
{
    [Serializable]
    struct DebrisGenerator : ISharedComponentData, System.IEquatable<DebrisGenerator>
    {
        public int BuildingCount;
        public int BuildingMaxHeight;
        public int BuildingMinHeight;
        public int AdditionalPointCount;

        public Mesh barMesh;
        public Material barMaterial;

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

        public Mesh mesh;
        public Material material;

        public Material barMaterial;
        public Mesh barMesh;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            EntityArchetype pointArchetype = dstManager.CreateArchetype(
                typeof(Point),
                typeof(PointInitialize)
//                typeof(DotsConversion.MeshRenderer),
//                typeof(LocalToWorld),
//                typeof(Translation)
                );

            dstManager.AddSharedComponentData(entity, new DotsConversion.DebrisGenerator()
            {
                BuildingCount = buildingCount,
                BuildingMinHeight = buildingMinHeight,
                BuildingMaxHeight = buildingMaxHeight,
                barMesh = barMesh,
                AdditionalPointCount = additionalPointCount,
                barMaterial = barMaterial
            });

            int maxPointCount = buildingCount * buildingMaxHeight * 3 + (additionalPointCount + 1);

            for (int i = 0; i < maxPointCount; i++)
            {
                var ent = dstManager.CreateEntity(pointArchetype);
//                dstManager.SetSharedComponentData(ent, new DotsConversion.MeshRenderer() { mesh = mesh, material = material} );
            }
        }
    }
}