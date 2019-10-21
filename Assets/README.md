Types:

- Bar
	- Point[2]
	- Matrix (stored in PointManager.matrices)
	- Thickness
- Point

Constants

- Bar break resistance


Data             |  Type     | What for   | Updated       |
-----------------+-----------+------------+---------------+
Point            | Vector3   | Point      | Frequently    |
Bar              | Point[]   | Bars       | Frequently    |
Anchor           | tag       | Grounded   | Frequently    |
Neighbor Count   | int       | Connection | Infrequently  |
Tornado particle | tag       | Render     | Frequently    |

# Archetypes

## TornadoPosition

- Tornado
- Position

## Tornado Particle

- TornadoParticle
- Position
- Renderer

## Bar

- Bar
- BarThickness
- Renderer

## Bar (2)

- Bar
- BarThickness
- Anchor

---

# Systems

## InstantiateBars

- { [Read] BarSpawner }

## InstantiateTornadoParticles

- { [Read] TornadoParticleSpawner }

## TranslateTornado

- { [Read] Tornado, [Write] Position }

## TranslateTornadoParticles

- [ { [Read] Tornado, [Read] Position }, { TornadoParticle, [Write] Position } ]

## ApplyTornadoParticleSway

- { TornadoParticle, [Write] Position }

## ApplyBarPointTornadoSway

- [ { [Read] Tornado, [Read] Position },  { [Write] Bar, !BarAnchor } ]

## ApplyBarRotationAndScale

- { [Read] Bar, [Read] Thickness, [Write] Position, [Write] Rotation, [Write] Scale }

---

```
struct BarSpawner : ComponentData
{
	public int barCount;
	public Prefab bar;
	public Vector2 radius;
}

struct TornadoParticleSpawner : ComponentData
{
	public int particleCount;
	public Prefab particle;
}

struct TornadoParticle : ComponentData {}

struct Point { float3 position, float3 previous, int neighborCount; }

struct Bar : ComponentData
{
	Point a, b;
}

struct BarAnchor : ComponentData {}

struct BarThickness : ComponentData
{
	float thickness;
}
```