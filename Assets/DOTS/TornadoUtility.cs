using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace DotsConversion
{
	static class TornadoUtility
	{
		public static float ApplySway(float height, float time)
		{
			return sin(height / 5f + time/4f) * 3f;
		}

		public static float Random(float3 position)
		{
			return frac(sin(dot(position.xy, new float2(12.9898f, 78.233f))) * 43758.5453f);
		}
	}
}
