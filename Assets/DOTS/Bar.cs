using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace DotsConversion
{
	public struct Point : IComponentData
	{
		public float3 position, previous;
		public bool active;
		public int neighborCount;
		public bool anchor;
	}

	public struct FreePointQueue
	{
		public NativeQueue<Entity> points;
	}

	public struct Bar : IComponentData
	{
		public Entity a, b;
		public float barLength;
		public float extraDist;
	}

	public struct BarThickness : IComponentData
	{
		public float thickness;
	}
	
	public struct BarAnchor : IComponentData { }
}
