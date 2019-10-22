using System;
using Unity.Entities;

namespace DotsConversion
{
	[Serializable]
	public struct TornadoParticle : IComponentData
	{
		public float RadiusMultiplier;
	}
}
