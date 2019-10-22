using System;
using Unity.Entities;

namespace DotsConversion
{
	[Serializable]
	public struct TornadoParticle : IComponentData
	{
		// int tornadoIndex;
		public float RadiusMultiplier;
	}
}
