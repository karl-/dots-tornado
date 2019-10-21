using Unity.Entities;
using Unity.Mathematics;

namespace DotsConversion
{
	public struct Point
	{
		public float3 position, previous;
		public int neighborCount;
	}

	public struct Bar : IComponentData
	{
		public Point a, b;
	}
	public struct BarThickness : IComponentData { }
	public struct BarAnchor : IComponentData { }
}
