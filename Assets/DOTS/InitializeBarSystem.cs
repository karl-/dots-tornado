using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace DotsConversion
{
    [UpdateAfter(typeof(InitializePointsSystem))]
    public class InitializeBarSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            EntityQuery findPoints = GetEntityQuery(typeof(Point), typeof(PointInitialize));
            NativeArray<Entity> pointEntities = findPoints.ToEntityArray(Allocator.TempJob);
            NativeArray<Point> pointData = findPoints.ToComponentDataArray<Point>(Allocator.TempJob);

            System.Collections.Generic.List<DebrisGenerator> settingsList = new System.Collections.Generic.List<DebrisGenerator>();
            EntityManager.GetAllUniqueSharedComponentData(settingsList);
            // why the actual data is stored in the second index is beyond me, but it works... so :)
            DebrisGenerator settings = settingsList[1];

            EntityArchetype barArchetype = EntityManager.CreateArchetype(
                typeof(Bar),
                typeof(BarThickness),
                typeof(LocalToWorld),
                typeof(MeshRenderer),
                typeof(ColorData));

            EntityArchetype pointArchetype = EntityManager.CreateArchetype(
                typeof(Point),
                typeof(PointInitialize));

            Random random = new Random(32);

            // Now go through the point list and connect adjacent points, forming "bars"
            for (int i = 0, c = pointEntities.Length; i < c; i++)
            {
                var a = pointData[i];

                for (int j = i + 1; j < c; j++)
                {
                    var b = pointData[j];

                    float length = math.distance(a.position, b.position);

                    if (length < 5f && length > .2f)
                    {
                        // for each point, create a connection to any point between .2f and 5f radius of self
                        Bar bar = new Bar()
                        {
                            barLength = length,
                            a = pointEntities[i],
                            b = pointEntities[j],
                            backupA = EntityManager.CreateEntity(pointArchetype),
                            backupB = EntityManager.CreateEntity(pointArchetype)
                        };

                        bar.a = pointEntities[i];
                        bar.b = pointEntities[j];

                        a.neighborCount++;
                        b.neighborCount++;

                        var barEntity = EntityManager.CreateEntity(barArchetype);

                        pointData[i] = a;
                        pointData[j] = b;

                        EntityManager.SetComponentData(barEntity, bar);
                        EntityManager.SetComponentData(barEntity, new BarThickness() { thickness = .4f });
                        EntityManager.SetComponentData(barEntity, new LocalToWorld());
                        float upDot = math.acos(math.abs(math.dot(new float3(0,1,0), math.normalize(b.position - a.position))))/math.PI;
                        EntityManager.SetComponentData(barEntity, new ColorData() { Value = new float4(upDot * random.NextFloat(.7f, 1f)) } );
                        EntityManager.SetSharedComponentData(barEntity,
                            new MeshRenderer() { mesh = settings.barMesh, material = settings.barMaterial });
                    }
                }
            }

            for (int i = 0; i < pointEntities.Length; i++)
            {
                Point p = pointData[i];
                EntityManager.SetComponentData(pointEntities[i], p);
                if(p.neighborCount < 1)
                    EntityManager.DestroyEntity(pointEntities[i]);
            }

            EntityManager.RemoveComponent<PointInitialize>(findPoints);
            pointEntities.Dispose();
            pointData.Dispose();
        }
    }
}
