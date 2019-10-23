using static Unity.Mathematics.math;

namespace DotsConversion
{
	static class TornadoUtility
	{
		public static float ApplySway(float height, float time)
		{
			return sin(height / 5f + time/4f) * 3f;
		}
	}
}
