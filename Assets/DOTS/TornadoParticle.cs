using System;
using Unity.Entities;

namespace DotsConversion
{
	[Serializable]
	public struct TornadoParticle : IComponentData
	{
		public int tornadoId;
		public float RadiusMultiplier;
	}
}
