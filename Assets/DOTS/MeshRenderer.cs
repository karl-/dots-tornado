using System;
using Unity.Entities;
using UnityEngine;

namespace DotsConversion
{
    public struct MeshRenderer : ISharedComponentData, IEquatable<MeshRenderer>
    {
        public Mesh mesh;
        public Material material;

        public bool Equals(MeshRenderer other)
        {
            return mesh == other.mesh 
                   && material == other.material;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            if (!ReferenceEquals(mesh, null)) hash ^= mesh.GetHashCode();
            if (!ReferenceEquals(material, null)) hash ^= material.GetHashCode();
            return hash;
        }
    }
}