using System.Collections;
using System.Collections.Generic;
using DotsConversion;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
public class DotsPointGenerator : MonoBehaviour {
	public Material barMaterial;
	public Mesh barMesh;
	[Range(0f,1f)]
	public float damping;
	[Range(0f,1f)]
	public float friction;
	public float breakResistance;
	public float expForce;
	[Range(0f,1f)]
	public float tornadoForce;
	public float tornadoMaxForceDist;
	public float tornadoHeight;
	public float tornadoUpForce;
	public float tornadoInwardForce;

	Point[] points;
	Bar[] bars;
	public int pointCount;

	bool generating;
	public static float tornadoX;
	public static float tornadoZ;

	float tornadoFader = 0f;

	Matrix4x4[][] matrices;
	MaterialPropertyBlock[] matProps;

	Transform cam;

	const int instancesPerBatch = 1023;

	private void Awake() {
		Time.timeScale = 0f;
	}
	IEnumerator Start()
	{
		cam = Camera.main.transform;
		yield return StartCoroutine(Generate());
	}

	public static float TornadoSway(float y) {
		return Mathf.Sin(y / 5f + Time.time/4f) * 3f;
	}

	void CreatePointEntity(ref Point point, bool active=true)
	{
		var entityManager = World.Active.EntityManager;
		ComponentType[] ct =
		{
			typeof(DotsConversion.Point)
		};

		var e = entityManager.CreateEntity(ct);
		var dPt = new DotsConversion.Point();
		dPt.active = active;
		dPt.anchor = point.anchor;
		dPt.position = new float3(point.x, point.y, point.z);
		dPt.previous = new float3(dPt.position);
		dPt.neighborCount = point.neighborCount;
		point.entity = e;
	}

	void CreateBarEntity(ref Bar bar)
	{
		var entityManager = World.Active.EntityManager;

		ComponentType[] ct = 
		{
			typeof(DotsConversion.Bar), 
			typeof(DotsConversion.BarThickness),
			typeof(LocalToWorld), 
			typeof(RenderMesh)
		};
			
		var e = entityManager.CreateEntity(ct);
		if ( bar.point1.anchor|| bar.point2.anchor) 
			entityManager.AddComponentData(e, new BarAnchor());
		DotsConversion.Bar dotsBar = new DotsConversion.Bar();
		dotsBar.a = bar.point1.entity;
		dotsBar.b = bar.point2.entity;
		dotsBar.barLength = bar.length;
			
		entityManager.SetComponentData(e, dotsBar);
		var dotsBarThickness = new BarThickness();
		dotsBarThickness.thickness = bar.thickness;
		entityManager.SetComponentData(e, dotsBarThickness);
		
		var lw = entityManager.GetComponentData<LocalToWorld>(e);
		entityManager.SetComponentData(e, lw);

		var rm = new RenderMesh();
		rm.mesh = barMesh;
		rm.material = barMaterial;
		rm.castShadows = ShadowCastingMode.On;
		rm.subMesh = barMesh.subMeshCount;
			
		entityManager.SetSharedComponentData(e, rm);
	}
	IEnumerator Generate() {
		generating = true;

		List<Point> pointsList = new List<Point>();
		List<Bar> barsList = new List<Bar>();
		List<List<Matrix4x4>> matricesList = new List<List<Matrix4x4>>();
		matricesList.Add(new List<Matrix4x4>());


		// Create 35 buildings
		for (int i = 0; i < 35; i++)
		{
			// In random positions along the X and Z axes
			int height = UnityEngine.Random.Range(4, 12);
			Vector3 pos = new Vector3(UnityEngine.Random.Range(-45f, 45f), 0f, UnityEngine.Random.Range(-45f, 45f));
			float spacing = 2f;

			// Buildings are between 4 and 12 blocks tall
			for (int j = 0; j < height; j++)
			{
				// Buildings are composed of sets of 3 points forming a triangle. The first floor is marked as the ancho.
				// The anchor is used to
				Point point = new Point();
				point.x = pos.x + spacing;
				point.y = j * spacing;
				point.z = pos.z - spacing;
				point.oldX = point.x;
				point.oldY = point.y;
				point.oldZ = point.z;
				if (j == 0)
				{
					point.anchor = true;
				}
				pointsList.Add(point);
				
				point = new Point();
				point.x = pos.x - spacing;
				point.y = j * spacing;
				point.z = pos.z - spacing;
				point.oldX = point.x;
				point.oldY = point.y;
				point.oldZ = point.z;
				if (j == 0)
				{
					point.anchor = true;
				}
				pointsList.Add(point);

				point = new Point();
				point.x = pos.x + 0f;
				point.y = j * spacing;
				point.z = pos.z + spacing;
				point.oldX = point.x;
				point.oldY = point.y;
				point.oldZ = point.z;
				if (j == 0)
				{
					point.anchor = true;
				}
				pointsList.Add(point);
			}
		}

		// ground details
		// In addition to the buildings, also generate 300 connected points
		for (int i=0;i<600;i++)
		{
			Vector3 pos = new Vector3(UnityEngine.Random.Range(-55f,55f),0f,UnityEngine.Random.Range(-55f,55f));
			Point point = new Point();
			point.x = pos.x + Random.Range(-.2f,-.1f);
			point.y = pos.y+Random.Range(0f,3f);
			point.z = pos.z + Random.Range(.1f,.2f);
			point.oldX = point.x;
			point.oldY = point.y;
			point.oldZ = point.z;
			pointsList.Add(point);

			point = new Point();
			point.x = pos.x + Random.Range(.2f,.1f);
			point.y = pos.y + Random.Range(0f,.2f);
			point.z = pos.z + Random.Range(-.1f,-.2f);
			point.oldX = point.x;
			point.oldY = point.y;
			point.oldZ = point.z;
			if (Random.value<.1f) {
				point.anchor = true;
			}
			pointsList.Add(point);
		}

		int batch = 0;

		// Now go through the point list and connect adjacent points, forming "bars"
		for (int i = 0; i < pointsList.Count; i++)
		{
			for (int j = i + 1; j < pointsList.Count; j++)
			{
				// for each point, create a connection to any point between .2f and 5f radius of self
				Bar bar = new Bar();
				bar.AssignPoints(pointsList[i], pointsList[j]);
				if (bar.length < 5f && bar.length > .2f)
				{
					bar.point1.neighborCount++;
					bar.point2.neighborCount++;

					barsList.Add(bar);
					matricesList[batch].Add(bar.matrix);
					if (matricesList[batch].Count == instancesPerBatch)
					{
						batch++;
						matricesList.Add(new List<Matrix4x4>());
					}

					if (barsList.Count % 500 == 0)
					{
						yield return null;
					}
				}
			}
		}

		// pare down the initial pointList to only include points that have been associated with at least one bar
		points = new Point[barsList.Count * 2];
		pointCount = 0;
		for (int i = 0; i < pointsList.Count; i++)
		{
			// if the point is used in a bar, it gets added to the retained 'points' array
			if (pointsList[i].neighborCount > 0)
			{
				points[pointCount] = pointsList[i];
				pointCount++;
			}
		}
		Debug.Log(pointCount + " points, room for " + points.Length + " (" + barsList.Count + " bars)");

		for (int i = 0; i < points.Length; i++)
		{
			if ( i % 100 == 0) 
				Debug.Log( "Calling CreatePointEntity for " + i);
			if ( points[i] != null)
				CreatePointEntity(ref points[i]);
		}

		// Copy generated bars to an array that will be kept
		bars = barsList.ToArray();

		// generate matrices
		matrices = new Matrix4x4[matricesList.Count][];
		for (int i=0;i<matrices.Length;i++) {
			matrices[i] = matricesList[i].ToArray();
		}

		matProps = new MaterialPropertyBlock[barsList.Count];
		Vector4[] colors = new Vector4[instancesPerBatch];
		for (int i=0;i<barsList.Count;i++) {
			colors[i%instancesPerBatch] = barsList[i].color;
			if ((i + 1) % instancesPerBatch == 0 || i == barsList.Count - 1) {
				MaterialPropertyBlock block = new MaterialPropertyBlock();
				block.SetVectorArray("_Color",colors);
				matProps[i / instancesPerBatch] = block;
			}
		}

		
		for(int i = 0; i < bars.Length; ++i )
		{
			CreateBarEntity(ref bars[i] );
		}

		Point fPoint = new Point();
		fPoint.anchor = false;
		
		for (int i = 0; i < bars.Length*2; i++)
		{
			CreatePointEntity(ref fPoint, false);
		}

		pointsList = null;
		barsList = null;
		matricesList = null;
		System.GC.Collect();
		generating = false;
		Time.timeScale = 1f;
	}

}
