using System;
using Unity.Entities;
using Unity.Mathematics;

namespace DotsConversion
{
	[Serializable]
	public struct Point : IComponentData
	{
		public float3 position, previous;
		public int neighborCount;
		public bool anchor;
	}

	public struct Bar : IComponentData
	{
		public Entity a, b, backupA, backupB;
		public float barLength;
		public float extraDist;
	}

	public struct BarThickness : IComponentData
	{
		public float thickness;
	}

	public struct BarAnchor : IComponentData { }
}
